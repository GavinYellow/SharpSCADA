using ClientDriver;
using DatabaseLib;
using DataService;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Timers;
using System.Xml;

namespace BatchCoreService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, Namespace = "http://BatchCoreService")]
    public class DAService : IDataServer, IAlarmServer
    {
        const int PORT = 6543;

        const char SPLITCHAR = '.';
        const string SERVICELOGSOURCE = "DataProcess";
        const string SERVICELOGNAME = "DataProcess";
        const string PATH = @"C:\DataConfig\";
        const string FILENAME = "server.xml";


        //可配置参数，从XML文件读取
        int DELAY = 3000;
        int MAXHDACAP = 10000;
        int ALARMLIMIT = 1000;
        int CYCLE = 60000;
        int CYCLE2 = 600000;
        int SENDTIMEOUT = 60000;
        //int SENDSIZE = ushort.MaxValue;
        int HDALEN = 1024 * 1024;
        int MAXLOGSIZE = 1024;
        int HDADELAY = 3600 * 1000;
        int ALARMDELAY = 3600 * 1000;
        int ARCHIVEINTERVAL = 100;

        static EventLog Log;

        private System.Timers.Timer timer1 = new System.Timers.Timer();
        private System.Timers.Timer timer2 = new System.Timers.Timer();
        private System.Timers.Timer timer3 = new System.Timers.Timer();
        private DateTime _hdastart = DateTime.Now;
        private DateTime _alarmstart = DateTime.Now;

        #region DAServer（标签数据服务器）
        public ITag this[short id]
        {
            get
            {
                int index = GetItemProperties(id);
                if (index >= 0)
                {
                    return this[_list[index].Name];
                }
                return null;
            }
        }

        public ITag this[string name]
        {
            get
            {
                if (string.IsNullOrEmpty(name)) return null;
                ITag dataItem;
                _mapping.TryGetValue(name.ToUpper(), out dataItem);
                return dataItem;
            }
        }

        List<TagMetaData> _list;
        public IList<TagMetaData> MetaDataList
        {
            get
            {
                return _list;
            }
        }

        public IList<Scaling> ScalingList
        {
            get
            {
                return _scales;
            }
        }

        object _syncRoot;
        public object SyncRoot
        {
            get
            {
                if (this._syncRoot == null)
                {
                    Interlocked.CompareExchange(ref this._syncRoot, new object(), null);
                }
                return this._syncRoot;
            }
        }

        bool _hasHda = false;
        List<HistoryData> _hda;
        List<DriverArgumet> _arguments = new List<DriverArgumet>();
        Dictionary<short, ArchiveTime> _archiveTimes = new Dictionary<short, ArchiveTime>();

        Socket tcpServer = null;

        Dictionary<IPAddress, Socket> _socketThreadList;
        public Dictionary<IPAddress, Socket> SocketList
        {
            get
            {
                return _socketThreadList;
            }
        }

        Dictionary<string, ITag> _mapping;

        List<Scaling> _scales;

        SortedList<short, IDriver> _drivers;
        public IEnumerable<IDriver> Drivers
        {
            get { return _drivers.Values; }
        }

        CompareCondBySource _compare;

        ExpressionEval reval;
        public ExpressionEval Eval
        {
            get
            {
                return reval;
            }
        }

        private object _myLock = new object();
        Dictionary<short, string> _archiveList = null;//是否需要lock
        public Dictionary<short, string> ArchiveList
        {
            get
            {
                lock (_myLock)
                {
                    if (_archiveList == null)
                    {
                        var list = MetaDataList.Where(x => x.Archive).Select(y => y.ID);//&& x.DataType != DataType.BOOL
                        if (list != null && list.Count() > 0)
                        {
                            string sql = "SELECT TAGID,DESCRIPTION FROM META_TAG WHERE TAGID IN(" + string.Join(",", list) + ");";
                            using (var reader = DataHelper.Instance.ExecuteReader(sql))
                            {
                                if (reader != null)
                                {
                                    _archiveList = new Dictionary<short, string>();
                                    while (reader.Read())
                                    {
                                        _archiveList.Add(reader.GetInt16(0), reader.GetNullableString(1));
                                    }
                                }
                            }
                        }
                    }
                }
                return _archiveList;
            }
        }

        public DAService()
        {
            if (!EventLog.SourceExists(SERVICELOGSOURCE))
            {
                EventLog.CreateEventSource(SERVICELOGSOURCE, SERVICELOGNAME);
            }
            Log = new EventLog(SERVICELOGNAME);
            Log.Source = SERVICELOGSOURCE;
            InitServerByXml();
            if (Log.MaximumKilobytes != MAXLOGSIZE)
                Log.MaximumKilobytes = MAXLOGSIZE;
            if (Log.OverflowAction != OverflowAction.OverwriteAsNeeded)
            {
                // 當EventLog 滿了就把最早的那一筆log 蓋掉。
                Log.ModifyOverflowPolicy(OverflowAction.OverwriteAsNeeded, 7);
            }
            _scales = new List<Scaling>();
            _drivers = new SortedList<short, IDriver>();
            _alarmList = new List<AlarmItem>(ALARMLIMIT + 10);
            reval = new ExpressionEval(this);
            _hda = new List<HistoryData>();
            InitServerByDatabase();
            InitConnection();
            _socketThreadList = new Dictionary<IPAddress, Socket>();
            InitHost();

            timer1.Elapsed += timer1_Elapsed;
            timer2.Elapsed += timer2_Elapsed;
            timer3.Elapsed += timer3_Elapsed;
            timer1.Interval = CYCLE;
            timer1.Enabled = true;
            timer1.Start();
            timer2.Interval = CYCLE2;
            timer2.Enabled = true;
            timer2.Start();
            if (_hasHda)
            {
                foreach (var item in _archiveTimes.Values)
                {
                    if (item != null)
                    {
                        timer3.Interval = ARCHIVEINTERVAL;
                        timer3.Enabled = true;
                        timer3.Start();
                        return;
                    }
                }
            }
        }

        public void Dispose()
        {
            lock (this)
            {
                try
                {
                    if (timer1 != null)
                        timer1.Dispose();
                    if (timer2 != null)
                        timer2.Dispose();
                    if (timer3 != null)
                        timer3.Dispose();
                    if (_drivers != null)
                    {
                        foreach (var driver in Drivers)
                        {
                            driver.OnError -= this.reader_OnClose;
                            driver.Dispose();
                        }
                        foreach (var condition in _conditionList)
                        {
                            if (condition != null)
                                condition.AlarmActive -= cond_SendAlarm;
                        }

                        if (_hasHda)
                        {
                            Flush();
                            //hda.Clear();
                        }
                        SaveAlarm();
                        foreach (var socket in _socketThreadList.Values)
                        {
                            socket.Dispose();
                        }
                        if (tcpServer != null && tcpServer.Connected)
                            tcpServer.Disconnect(false);

                        _mapping.Clear();
                        _conditionList.Clear();
                        reval.Dispose();
                    }
                }
                catch (Exception e)
                {
                    AddErrorLog(e);
                }
            }
        }

        public void AddErrorLog(Exception e)
        {
            Log.WriteEntry(e.GetExceptionMsg(), EventLogEntryType.Error);
        }

        private void timer1_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (IDriver d in Drivers)
            {
                if (d.IsClosed)
                {
                    d.Connect();//t.IsAlive可加入判断；如线程异常，重新启动。
                }
            }
        }

        private void timer2_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (HDADELAY > 0 && _hda.Count > 0 && (DateTime.Now - _hdastart).TotalMilliseconds > HDADELAY)
            {
                lock (_hdaRoot)
                {
                    ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(this.SaveCachedData), _hda.ToArray());
                    _hda.Clear();
                }
            }
            if (ALARMDELAY > 0 && _alarmList.Count > 0 && (DateTime.Now - _alarmstart).TotalMilliseconds > ALARMDELAY)
                SaveAlarm();
            DateTime today = DateTime.Today;
            try
            {
                if (e.SignalTime > today.AddHours(2))
                {
                    DateTime startTime = DateTime.MinValue;
                    DateTime endTime = DateTime.MaxValue;
                    HDAIOHelper.GetRangeFromDatabase(null, ref startTime, ref endTime);
                    if (startTime >= today || startTime == DateTime.MinValue)
                    {
                        return;
                    }
                    bool success = true;
                    if (endTime < today && _hda.Count > 0 && _hda[0].TimeStamp < today)
                    {
                        success = SaveRange(endTime, today);
                    }
                    if (success)
                    {
                        startTime = startTime.Date.AddDays(1);
                        endTime = endTime.Date.AddDays(1);
                        if (endTime >= today) endTime = today;
                        while (startTime <= endTime)
                        {
                            HDAIOHelper.BackUpFile(startTime);
                            startTime = startTime.AddDays(1);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                AddErrorLog(err);
            }
        }

        private void timer3_Elapsed(object sender, ElapsedEventArgs e)
        {
            var now = e.SignalTime;
            List<HistoryData> tempData = new List<HistoryData>();
            foreach (var archive in _archiveTimes)
            {
                var archiveTime = archive.Value;
                if (archiveTime != null && (now - archiveTime.LastTime).TotalMilliseconds > archiveTime.Cycle)
                {
                    var tag = this[archive.Key];
                    if (tag != null && tag.TimeStamp > archiveTime.LastTime)
                    {
                        tempData.Add(new HistoryData(tag.ID, tag.Quality, tag.Value, now));
                        archive.Value.LastTime = now;
                    }
                }
            }
            if (tempData.Count > 0)
            {
                ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(this.OnUpdate), tempData);
            }
            //var result = from item in _archiveTimes where item.Value.Cycle > 0 && (now - item.Value.LastTime).Milliseconds > item.Value.Cycle select item.Key;
        }

        #region 初始化（标签数据服务器）
        void InitConnection()
        {
            foreach (IDriver reader in _drivers.Values)
            {
                reader.OnError += new IOErrorEventHandler(reader_OnClose);
                if (reader.IsClosed)
                {
                    //if (reader is IFileDriver)
                    reader.Connect();
                }
                foreach (IGroup grp in reader.Groups)
                {
                    grp.DataChange += new DataChangeEventHandler(grp_DataChange);
                    //可在此加入判断，如为ClientDriver发出，则变化数据毋须广播，只需归档。
                    grp.IsActive = grp.IsActive;
                }
            }
            //此处需改进,与Condition采用相同的处理方式，可配置
        }

        void InitServerByDatabase()
        {
            using (var dataReader = DataHelper.Instance.ExecuteProcedureReader("InitServer", DataHelper.CreateParam("@TYPE", SqlDbType.Int, 0)))
            {
                if (dataReader == null) return;// Stopwatch sw = Stopwatch.StartNew();
                while (dataReader.Read())
                {
                    _arguments.Add(new DriverArgumet(dataReader.GetInt16(0), dataReader.GetNullableString(1), dataReader.GetNullableString(2)));
                }
                dataReader.NextResult();
                while (dataReader.Read())
                {
                    AddDriver(dataReader.GetInt16(0), dataReader.GetNullableString(1), dataReader.GetNullableString(2),  dataReader.GetNullableString(3));
                }

                dataReader.NextResult();
                dataReader.Read();
                int count = dataReader.GetInt32(0);
                _list = new List<TagMetaData>(count);
                _mapping = new Dictionary<string, ITag>(count);
                dataReader.NextResult();
                while (dataReader.Read())
                {
                    var meta = new TagMetaData(dataReader.GetInt16(0), dataReader.GetInt16(1), dataReader.GetString(2), dataReader.GetString(3), (DataType)dataReader.GetByte(4),
                     (ushort)dataReader.GetInt16(5), dataReader.GetBoolean(6), dataReader.GetFloat(7), dataReader.GetFloat(8), dataReader.GetInt32(9));
                    _list.Add(meta);
                    if (meta.Archive)
                    {
                        _archiveTimes.Add(meta.ID, meta.Cycle == 0 ? null : new ArchiveTime(meta.Cycle, DateTime.MinValue));
                    }
                    //Advise(DDETOPIC, meta.Name);
                }
                _list.Sort();
                dataReader.NextResult();
                while (dataReader.Read())
                {
                    IDriver dv;
                    _drivers.TryGetValue(dataReader.GetInt16(0), out dv);
                    if (dv != null)
                    {
                        IGroup grp = dv.AddGroup(dataReader.GetString(1), dataReader.GetInt16(2), dataReader.GetInt32(3),
                               dataReader.GetFloat(4), dataReader.GetBoolean(5));
                        if (grp != null)
                            grp.AddItems(_list);
                    }
                }
                dataReader.NextResult();
                while (dataReader.Read())
                {
                    ITag tag = this[dataReader.GetNullableString(0)];
                    if (tag != null)
                    {
                        tag.ValueChanged += OnValueChanged;
                    }
                }
                dataReader.NextResult();
                _conditions = new List<ICondition>();
                _conditionList = new List<ICondition>();
                while (dataReader.Read())
                {
                    int id = dataReader.GetInt32(0);
                    AlarmType type = (AlarmType)dataReader.GetInt32(2);
                    ICondition cond;
                    string source = dataReader.GetString(1);
                    if (_conditions.Count > 0)
                    {
                        cond = _conditions[_conditions.Count - 1];
                        if (cond.ID == id)
                        {
                            cond.AddSubCondition(new SubCondition((SubAlarmType)dataReader.GetInt32(9), dataReader.GetFloat(10),
                                (Severity)dataReader.GetByte(11), dataReader.GetString(12), dataReader.GetBoolean(13)));
                            continue;
                        }
                    }
                    switch (type)
                    {
                        case AlarmType.Complex:
                            cond = new ComplexCondition(id, source, dataReader.GetString(6), dataReader.GetFloat(7), dataReader.GetInt32(8));
                            break;
                        case AlarmType.Level:
                            cond = new LevelAlarm(id, source, dataReader.GetString(6), dataReader.GetFloat(7), dataReader.GetInt32(8));
                            break;
                        case AlarmType.Dev:
                            cond = new DevAlarm(id, (ConditionType)dataReader.GetByte(4), source, dataReader.GetString(6),
                                dataReader.GetFloat(5), dataReader.GetFloat(7), dataReader.GetInt32(8));
                            break;
                        case AlarmType.ROC:
                            cond = new ROCAlarm(id, source, dataReader.GetString(6), dataReader.GetFloat(7), dataReader.GetInt32(8));
                            break;
                        case AlarmType.Quality:
                            cond = new QualitiesAlarm(id, source, dataReader.GetString(6));
                            break;
                        case AlarmType.WordDsc:
                            cond = new WordDigitAlarm(id, source, dataReader.GetString(6), dataReader.GetInt32(8));
                            break;
                        default:
                            cond = new DigitAlarm(id, source, dataReader.GetString(6), dataReader.GetInt32(8));
                            break;
                    }
                    cond.AddSubCondition(new SubCondition((SubAlarmType)dataReader.GetInt32(9), dataReader.GetFloat(10),
                               (Severity)dataReader.GetByte(11), dataReader.GetString(12), dataReader.GetBoolean(13)));

                    cond.IsEnabled = dataReader.GetBoolean(3);
                    var simpcond = cond as SimpleCondition;
                    if (simpcond != null)
                    {
                        simpcond.Tag = this[source];
                    }
                    else
                    {
                        var complexcond = cond as ComplexCondition;
                        if (complexcond != null)
                        {
                            var action = complexcond.SetFunction(reval.Eval(source));
                            if (action != null)
                            {
                                ValueChangedEventHandler handle = (s1, e1) => { action(); };
                                foreach (ITag tag in reval.TagList)
                                {
                                    tag.ValueChanged += handle;// tag.Refresh();
                                }
                            }
                        }
                    }
                    cond.AlarmActive += new AlarmEventHandler(cond_SendAlarm);
                    //_conditions.Add(cond);// UpdateCondition(cond);
                    _conditions.Add(cond);
                }
                dataReader.NextResult();
                while (dataReader.Read())
                {
                    _scales.Add(new Scaling(dataReader.GetInt16(0), (ScaleType)dataReader.GetByte(1),
                        dataReader.GetFloat(2), dataReader.GetFloat(3), dataReader.GetFloat(4), dataReader.GetFloat(5)));
                }
            }
            if (_archiveTimes.Count > 0)
            {
                _hasHda = true;
                _hda.Capacity = MAXHDACAP;
            }
            reval.Clear();
            _scales.Sort();
            _compare = new CompareCondBySource();
            _conditions.Sort(_compare);
        }

        void InitHost()
        {
            /*对关闭状态的判断，最好用心跳检测；冗余切换，可广播冗余命令，包含新主机名、数据库连接、IP地址等。
             * 服务启动时，向整个局域网UDP广播加密的主机名、连接字符串等信息
             */
            //socketThreadList = new Dictionary<IPAddress, Socket>();
            tcpServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint LocalPort = new IPEndPoint(IPAddress.Any, PORT);
            tcpServer.Bind(LocalPort);
            tcpServer.Listen(100);
            ThreadPool.QueueUserWorkItem(new WaitCallback(AcceptWorkThread));
        }

        void InitServerByXml()
        {
            string path = PATH + '\\' + FILENAME;
            if (File.Exists(path))
            {
                try
                {
                    using (var reader = XmlTextReader.Create(path))
                    {
                        while (reader.Read())
                        {
                            if (reader.NodeType == XmlNodeType.Element)
                            {
                                switch (reader.Name)
                                {
                                    case "Server":
                                        {
                                            if (reader.MoveToAttribute("MaxLogSize"))
                                                int.TryParse(reader.Value, out MAXLOGSIZE);
                                        }
                                        break;
                                    case "Data":
                                        {
                                            if (reader.MoveToAttribute("TestCycle"))
                                                int.TryParse(reader.Value, out CYCLE);
                                            if (reader.MoveToAttribute("SendTimeout"))
                                                int.TryParse(reader.Value, out SENDTIMEOUT);
                                        }
                                        break;
                                    case "Hda":
                                        {
                                            if (reader.MoveToAttribute("MaxHdaCap"))
                                            {
                                                int.TryParse(reader.Value, out MAXHDACAP);
                                            }
                                            if (reader.MoveToAttribute("HdaLen"))
                                                int.TryParse(reader.Value, out HDALEN);
                                            if (reader.MoveToAttribute("WriteCycle"))
                                                int.TryParse(reader.Value, out CYCLE2);
                                            if (reader.MoveToAttribute("Delay"))
                                                int.TryParse(reader.Value, out HDADELAY);
                                            if (reader.MoveToAttribute("Interval"))
                                                int.TryParse(reader.Value, out ARCHIVEINTERVAL);
                                        }
                                        break;
                                    case "Alarm":
                                        {
                                            if (reader.MoveToAttribute("AlarmLimit"))
                                                int.TryParse(reader.Value, out ALARMLIMIT);
                                            if (reader.MoveToAttribute("Delay"))
                                                int.TryParse(reader.Value, out ALARMDELAY);
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
                catch (Exception err)
                {
                    AddErrorLog(err);
                }
            }
        }
        #endregion

        void AcceptWorkThread(object state)
        {
            while (true)
            {
                //if (tcpServer.Poll(0, SelectMode.SelectRead))
                Socket s_Accept = tcpServer.Accept();
                //IPAddress addr = (s_Accept.RemoteEndPoint as IPEndPoint).Address;
                s_Accept.SendTimeout = SENDTIMEOUT;
                IPAddress addr = (s_Accept.RemoteEndPoint as IPEndPoint).Address;
                try
                {
                    if (!_socketThreadList.ContainsKey(addr))
                        _socketThreadList.Add(addr, s_Accept);
                }
                catch (Exception err)
                {
                    AddErrorLog(err);
                }
                ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(ReceiveWorkThread), s_Accept);
            }
        }

        void ReceiveWorkThread(object obj)
        {
            Socket s_Receive = (Socket)obj;
            IPAddress addr = null;
            try
            {
                addr = (s_Receive.RemoteEndPoint as IPEndPoint).Address;
            }
            catch (Exception err)
            {
                AddErrorLog(err);
                return;
            }
            byte[] buffer = new byte[s_Receive.ReceiveBufferSize];     // 创建接收缓冲
            while (true)
            {
                try
                {
                    if (addr == null || !_socketThreadList.ContainsKey(addr)) return;
                    /*if (!s_Receive.Connected) return;
                    关于数据传输协议：命令可分为：订单指令（订单类型，增删改标记可各用一个字段，路径ID用GUID，路径状态包括暂停、继续
                    、终止、启动）；可返回客户端一个可行的路径设备链、ERP交换数据指令（包含DATASET)，冗余切换指令等）
                     */
                    int ReceiveCount = s_Receive.Receive(buffer);

                    if (buffer[0] == FCTCOMMAND.fctHead)
                    {
                        //buffer[0]是协议头，1是指令号，2是读方式（缓存还是设备），3、4是ID，5是长度，后接变量值
                        byte command = buffer[1];
                        switch (command)
                        {
                            case FCTCOMMAND.fctReadSingle:
                                {
                                    //DataSource source = buffer[2] == 0 ? DataSource.Cache : DataSource.Device;
                                    short id = BitConverter.ToInt16(buffer, 3);
                                    byte length = buffer[5];
                                    byte[] send = new byte[5 + length];
                                    for (int i = 0; i < 5; i++)
                                    {
                                        send[i] = buffer[i];
                                    }
                                    ITag tag = this[id];
                                    if (tag != null)
                                    {
                                        Storage value = buffer[2] == 0 ? tag.Value : tag.Read(DataSource.Device);
                                        byte[] bt = tag.ToByteArray(value);
                                        for (int k = 0; k < bt.Length; k++)
                                        {
                                            send[5 + k] = bt[k];
                                        }
                                    }
                                    else
                                    {
                                        //出错处理,可考虑返回一个DATATYPE.NONE类型
                                    }
                                    s_Receive.Send(send);
                                }
                                break;
                            case FCTCOMMAND.fctReadMultiple:
                                {
                                    //buffer[0]是协议头，1是指令号，2是读方式（缓存还是设备），3、4是变量数，后接变量值
                                    //DataSource source = buffer[2] == 0 ? DataSource.Cache : DataSource.Device;
                                    byte[] send = new byte[s_Receive.SendBufferSize];
                                    send[0] = FCTCOMMAND.fctHead;
                                    short count = BitConverter.ToInt16(buffer, 3);//要读取的变量数
                                    int j = 5; int l = 5;
                                    if (buffer[2] == 0)
                                    {
                                        for (int i = 0; i < count; i++)
                                        {
                                            short id = BitConverter.ToInt16(buffer, l);
                                            send[j++] = buffer[l++];
                                            send[j++] = buffer[l++];
                                            ITag tag = this[id];
                                            if (tag != null)
                                            {
                                                byte[] bt = tag.ToByteArray();
                                                var length = (byte)bt.Length;
                                                send[j++] = length;
                                                for (int k = 0; k < length; k++)
                                                {
                                                    send[j + k] = bt[k];
                                                }
                                                j += length;
                                            }
                                            else
                                            {//类型后跟长度
                                                send[j++] = 0;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Dictionary<IGroup, List<ITag>> dict = new Dictionary<IGroup, List<ITag>>();
                                        for (int i = 0; i < count; i++)
                                        {
                                            short id = BitConverter.ToInt16(buffer, l);
                                            l += 2;
                                            ITag tag = this[id];
                                            if (tag != null)
                                            {
                                                IGroup grp = tag.Parent;
                                                if (!dict.ContainsKey(grp))
                                                    dict.Add(grp, new List<ITag> { tag });
                                                else
                                                    dict[grp].Add(tag);
                                            }
                                        }
                                        foreach (var dev in dict)
                                        {
                                            var list = dev.Value;
                                            var array = dev.Key.BatchRead(DataSource.Device, true, list.ToArray());
                                            if (array == null) continue;
                                            for (int i = 0; i < list.Count; i++)
                                            {
                                                byte[] bt = list[i].ToByteArray(array[i].Value);
                                                var length = (byte)bt.Length;
                                                send[j++] = length;
                                                for (int k = 0; k < bt.Length; k++)
                                                {
                                                    send[j + k] = bt[k];
                                                }
                                                j += length;
                                            }
                                        }
                                    }
                                    s_Receive.Send(send, 0, j, SocketFlags.None);
                                }
                                break;
                            case FCTCOMMAND.fctWriteSingle:
                                {
                                    //buffer[0]是协议头，1是指令号，2是写方式（缓存还是设备），3、4是ID，5是长度
                                    short id = BitConverter.ToInt16(buffer, 3);
                                    byte rs = 0;
                                    ITag tag = this[id];
                                    if (tag != null)//此处应考虑万一写失败，是否需要更新值
                                    {
                                        if (tag.Address.VarType == DataType.STR)
                                        {
                                            StringTag strTag = tag as StringTag;
                                            if (strTag != null)
                                            {
                                                string txt = Encoding.ASCII.GetString(buffer, 6, buffer[5]).Trim((char)0);
                                                rs = (byte)tag.Write(txt);
                                                if (rs == 0)
                                                    strTag.String = txt;
                                            }
                                        }
                                        else
                                        {
                                            Storage value = Storage.Empty;
                                            switch (tag.Address.VarType)
                                            {
                                                case DataType.BOOL:
                                                    value.Boolean = BitConverter.ToBoolean(buffer, 6);
                                                    break;
                                                case DataType.BYTE:
                                                    value.Byte = buffer[6];
                                                    break;
                                                case DataType.WORD:
                                                    value.Word = BitConverter.ToUInt16(buffer, 6);
                                                    break;
                                                case DataType.SHORT:
                                                    value.Int16 = BitConverter.ToInt16(buffer, 6);
                                                    break;
                                                case DataType.DWORD:
                                                    value.DWord = BitConverter.ToUInt32(buffer, 6);
                                                    break;
                                                case DataType.INT:
                                                    value.Int32 = BitConverter.ToInt32(buffer, 6);
                                                    break;
                                                case DataType.FLOAT:
                                                    value.Single = BitConverter.ToSingle(buffer, 6);
                                                    break;
                                                default:
                                                    break;
                                            }
                                            rs = (byte)tag.Write(value, false);
                                        }
                                    }
                                    else
                                    {
                                        rs = 0xFF;//此处长度应注意;如无此变量，应返回一个错误代码
                                    }
                                    s_Receive.Send(new byte[] { FCTCOMMAND.fctWriteSingle, rs }, 0, 2, SocketFlags.None);//应返回一个错误代码;
                                }
                                break;
                            case FCTCOMMAND.fctWriteMultiple:
                                {  //int BatchWrite(IDictionary<ITag, object> items, bool isSync = true);
                                    int count = BitConverter.ToInt16(buffer, 2);
                                    int j = 4; byte rs = 0;
                                    Dictionary<IGroup, SortedDictionary<ITag, object>> dict = new Dictionary<IGroup, SortedDictionary<ITag, object>>();
                                    for (int i = 0; i < count; i++)
                                    {
                                        short id = BitConverter.ToInt16(buffer, j);
                                        j += 2;
                                        byte length = buffer[j++];
                                        ITag tag = this[id];
                                        IGroup grp = tag.Parent;
                                        SortedDictionary<ITag, object> values;
                                        if (!dict.ContainsKey(grp))
                                        {
                                            values = new SortedDictionary<ITag, object>();
                                            dict.Add(grp, values);
                                        }
                                        else
                                            values = dict[grp];
                                        if (tag != null)
                                        {
                                            switch (tag.Address.VarType)
                                            {
                                                case DataType.BOOL:
                                                    values.Add(tag, BitConverter.ToBoolean(buffer, j));
                                                    break;
                                                case DataType.BYTE:
                                                    values.Add(tag, buffer[j]);
                                                    break;
                                                case DataType.WORD:
                                                    values.Add(tag, BitConverter.ToUInt16(buffer, j));
                                                    break;
                                                case DataType.SHORT:
                                                    values.Add(tag, BitConverter.ToInt16(buffer, j));
                                                    break;
                                                case DataType.DWORD:
                                                    values.Add(tag, BitConverter.ToUInt32(buffer, j));
                                                    break;
                                                case DataType.INT:
                                                    values.Add(tag, BitConverter.ToInt32(buffer, j));
                                                    break;
                                                case DataType.FLOAT:
                                                    values.Add(tag, BitConverter.ToSingle(buffer, j));
                                                    break;
                                                case DataType.STR:
                                                    values.Add(tag, Encoding.ASCII.GetString(buffer, j, length).Trim((char)0));
                                                    break;
                                            }
                                        }
                                        j += length;
                                    }
                                    foreach (var dev in dict)
                                    {
                                        if (dev.Key.BatchWrite(dev.Value) < 0) rs = 0xFF;
                                    }
                                    s_Receive.Send(new byte[] { FCTCOMMAND.fctWriteMultiple, rs }, 0, 2, SocketFlags.None);
                                }
                                break;
                            case FCTCOMMAND.fctAlarmRequest://刷新报警数据
                                {
                                    if (_alarmList.Count > 0)
                                    {
                                        long startTime = BitConverter.ToInt64(buffer, 2);
                                        long endTime = BitConverter.ToInt64(buffer, 10);
                                        if (_alarmstart > DateTime.FromFileTime(startTime) || DateTime.FromFileTime(endTime) > _alarmstart)
                                        {
                                            SaveAlarm();
                                        }
                                    }
                                    s_Receive.Send(new byte[] { FCTCOMMAND.fctAlarmRequest, 0 }, 0, 2, SocketFlags.None);
                                }
                                break;
                            case FCTCOMMAND.fctReset://重置连接
                                {
                                    byte[] iparry = new byte[4];
                                    Array.Copy(buffer, 2, iparry, 0, 4);
                                    IPAddress ipaddr = new IPAddress(iparry);
                                    if (_socketThreadList.Count > 0 && _socketThreadList.ContainsKey(ipaddr))
                                    {
                                        var scok = _socketThreadList[ipaddr];
                                        _socketThreadList.Remove(ipaddr);
                                        if (scok != null)
                                        {
                                            scok.Dispose();
                                        }
                                    }
                                }
                                break;
                            case FCTCOMMAND.fctHdaRequest:
                                {
                                    DateTime start = DateTime.FromFileTime(BitConverter.ToInt64(buffer, 2));
                                    DateTime end = DateTime.FromFileTime(BitConverter.ToInt64(buffer, 10));
                                    try
                                    {
                                        SendHData(GetHData(start, end), new byte[HDALEN], s_Receive);
                                    }
                                    catch (Exception err)
                                    {
                                        AddErrorLog(err);
                                    }
                                    s_Receive.Send(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                                    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF}, 24, SocketFlags.None);
                                }
                                break;
                            case FCTCOMMAND.fctHdaIdRequest://优先读取本地HDA文件夹下的二进制归档文件
                                {
                                    DateTime start = DateTime.FromFileTime(BitConverter.ToInt64(buffer, 2));
                                    DateTime end = DateTime.FromFileTime(BitConverter.ToInt64(buffer, 10));
                                    short ID = BitConverter.ToInt16(buffer, 18);
                                    try
                                    {
                                        SendHData(GetHData(start, end, ID), new byte[HDALEN], s_Receive, this[ID]);
                                    }
                                    catch (Exception err)
                                    {
                                        AddErrorLog(err);
                                    }
                                    s_Receive.Send(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                                    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF}, 24, SocketFlags.None);
                                }
                                break;
                        }
                    }
                }
                catch (SocketException ex)
                {
                    var err = ex.SocketErrorCode;
                    if (err == SocketError.ConnectionAborted || err == SocketError.HostDown || err == SocketError.NetworkDown || err == SocketError.Shutdown || err == SocketError.ConnectionReset)
                    {
                        s_Receive.Dispose();
                        if (addr != null)
                            _socketThreadList.Remove(addr);
                        //s_Receive.Dispose();
                    }
                    AddErrorLog(ex);
                }
                catch (Exception ex)
                {
                    AddErrorLog(ex);
                }
            }
        }

        #region 历史数据归档查询
        private IEnumerable<HistoryData> GetHData(DateTime start, DateTime end, short ID)
        {
            if (start > end) yield break;
            DateTime now = DateTime.Now;
            if (start > now) yield break;
            if (end > now) end = now;
            if (now.Date > start.Date)
            {
                DateTime tempstart = DateTime.MinValue;
                DateTime tempend = end;
                HDAIOHelper.GetRangeFromDatabase(ID, ref tempend, ref tempstart);
                if (tempend > end) tempend = end;
                if (tempend > start)
                {
                    int eyear = tempend.Year;
                    int syear = start.Year;
                    int emonth = tempend.Month;
                    int smonth = start.Month;
                    int year = syear;
                    while (year <= eyear)
                    {
                        int month = (year == syear ? smonth : 1);
                        while (month <= (year == eyear ? emonth : 12))
                        {
                            var result = HDAIOHelper.LoadFromFile((year == syear && month == smonth ? start : new DateTime(year, month, 1)),
                                (year == eyear && month == emonth ? tempend : new DateTime(year, month, 1).AddMonths(1).AddMilliseconds(-2)), ID);//考虑按月遍历
                            if (result != null)
                            {
                                foreach (var data in result)
                                {
                                    yield return data;
                                }
                            }
                            month++;
                        }
                        year++;
                    }
                }
            }
            var tempdata = _hda.ToArray();
            DateTime ftime = (tempdata.Length > 0 ? tempdata[0].TimeStamp : DateTime.Now);
            if (start < ftime)
            {
                var result = HDAIOHelper.LoadFromDatabase(start, ftime > end ? end : ftime, ID);//范围冲突
                if (result != null)
                {
                    foreach (var data in result)
                    {
                        yield return data;
                    }
                }
            }
            if (end > ftime)
            {
                var result = tempdata.Where(x => x.ID == ID && x.TimeStamp >= ftime && x.TimeStamp <= end);
                if (result != null)
                {
                    foreach (var data in result)
                    {
                        yield return data;
                    }
                }
            }
            yield break;
        }

        private IEnumerable<HistoryData> GetHData(DateTime start, DateTime end)
        {
            if (start > end) yield break;
            DateTime now = DateTime.Now;
            if (start > now) yield break;
            if (end > now) end = now;
            if (now.Date > start.Date)
            {
                DateTime tempstart = DateTime.MinValue;
                DateTime tempend = end;
                HDAIOHelper.GetRangeFromDatabase(null, ref tempend, ref tempstart);
                if (tempend > start)
                {
                    int eyear = tempend.Year;
                    int syear = start.Year;
                    int emonth = tempend.Month;
                    int smonth = start.Month;
                    int year = syear;
                    while (year <= eyear)
                    {
                        int month = (year == syear ? smonth : 1);
                        while (month <= (year == eyear ? emonth : 12))
                        {
                            var result = HDAIOHelper.LoadFromFile((year == syear && month == smonth ? start : new DateTime(year, month, 1)),
                                (year == eyear && month == emonth ? tempend : new DateTime(year, month, 1).AddMonths(1).AddMilliseconds(-2)));//考虑按月遍历
                            if (result != null)
                            {
                                foreach (var data in result)
                                {
                                    yield return data;
                                }
                            }
                            month++;
                        }
                        year++;
                    }
                }
            }
            var tempdata = _hda.ToArray();
            DateTime ftime = (tempdata.Length > 0 ? tempdata[0].TimeStamp : DateTime.Now);
            if (start < ftime)
            {
                var result = HDAIOHelper.LoadFromDatabase(start, ftime > end ? end : ftime);//范围冲突
                if (result != null)
                {
                    foreach (var data in result)
                    {
                        yield return data;
                    }
                }
            }
            if (end > ftime)
            {
                var result = tempdata.Where(x => x.TimeStamp >= ftime && x.TimeStamp <= end);
                if (result != null)
                {
                    foreach (var data in result)
                    {
                        yield return data;
                    }
                }
            }
            yield break;
        }

        private void SendHData(IEnumerable<HistoryData> result, byte[] buffer, Socket socket, ITag tag)
        {
            if (result == null || tag == null || socket == null || !socket.Connected) return;
            int index = 0;
            int len = buffer.Length;
            int size = tag.Address.DataSize;
            foreach (var data in result)
            {
                if (index + 8 + size >= len)
                {
                    //s_Receive.BeginSend(tempbuffer, 0, index, SocketFlags.None, null, null);
                    socket.Send(buffer, index, SocketFlags.None);
                    index = 0;
                }
                byte[] bits = tag.ToByteArray(data.Value);
                bits.CopyTo(buffer, index);
                index += size;
                bits = BitConverter.GetBytes(data.TimeStamp.ToFileTime());
                bits.CopyTo(buffer, index);
                index += 8;
            }
            socket.Send(buffer, index, SocketFlags.None);
        }

        private void SendHData(IEnumerable<HistoryData> result, byte[] buffer, Socket socket)
        {
            if (result == null || socket == null || !socket.Connected) return;
            int index = 0;
            int len = buffer.Length;
            short tempid = short.MinValue;
            ITag tag = null;
            byte[] idarray = new byte[2];
            foreach (var data in result)
            {
                if (tempid != data.ID)
                {
                    tempid = data.ID;
                    idarray = BitConverter.GetBytes(tempid);
                    tag = this[tempid];
                }
                if (tag == null) continue;
                int size = tag.Address.DataSize;
                if (index + 10 + size >= len)
                {
                    //s_Receive.BeginSend(tempbuffer, 0, index, SocketFlags.None, null, null);这里有一个同步的问题，发生ID号错位。
                    socket.Send(buffer, index, SocketFlags.None);
                    index = 0;
                }
                idarray.CopyTo(buffer, index);
                index += 2;
                byte[] bits = tag.ToByteArray(data.Value);
                bits.CopyTo(buffer, index);
                index += size;
                bits = BitConverter.GetBytes(data.TimeStamp.ToFileTime());
                bits.CopyTo(buffer, index);
                index += 8;
            }
            socket.Send(buffer, index, SocketFlags.None);
        }

        private object _hdaRoot = new object();
        public void Flush()
        {
            lock (_hdaRoot)
            {
                if (_hda.Count == 0) return;
                if (DataHelper.Instance.BulkCopy(new HDASqlReader(_hda, this), "Log_HData",
                      string.Format("DELETE FROM Log_HData WHERE [TIMESTAMP]>'{0}'", _hda[0].TimeStamp.ToString("yyyy/MM/dd HH:mm:ss"))))
                {
                    _hda.Clear();
                    _hdastart = DateTime.Now;
                }
            }
        }

        public bool SaveRange(DateTime startTime, DateTime endTime)
        {
            var tempdata = _hda.ToArray();
            if (tempdata.Length == 0) return true;
            return DataHelper.Instance.BulkCopy(new HDASqlReader(GetData(tempdata, startTime, endTime), this), "Log_HData",
                     string.Format("DELETE FROM Log_HData WHERE [TIMESTAMP] BETWEEN '{0}' AND '{1}'", startTime.ToString("yyyy/MM/dd HH:mm:ss"), endTime.ToString("yyyy/MM/dd HH:mm:ss")));
        }

        public void OnUpdate(object stateInfo)
        {
            lock (_hdaRoot)
            {
                var tempData = (List<HistoryData>)stateInfo;
                _hda.AddRange(tempData);
                if (_hda.Count >= MAXHDACAP)
                {
                    //Reverse(data);
                    DateTime start = _hda[0].TimeStamp;
                    //_array.CopyTo(data, 0);
                    if (DataHelper.Instance.BulkCopy(new HDASqlReader(_hda, this), "Log_HData",
                    string.Format("DELETE FROM Log_HData WHERE [TIMESTAMP]>'{0}'", start.ToString("yyyy/MM/dd HH:mm:ss"))))
                        _hdastart = DateTime.Now;
                    else ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(this.SaveCachedData), _hda.ToArray());
                    _hda.Clear();
                }
            }
        }

        public void SaveCachedData(object stateInfo)
        {
            var tempData = (HistoryData[])stateInfo;
            if (tempData.Length == 0) return;
            DateTime startTime = tempData[0].TimeStamp;
            DateTime endTime = tempData[tempData.Length - 1].TimeStamp;
            //Thread.Sleep(TimeSpan.FromMinutes(10));
            int count = 0;
            while (true)
            {
                if (count >= 5) return;
                if (DataHelper.Instance.BulkCopy(new HDASqlReader(tempData, this), "Log_HData",
                   string.Format("DELETE FROM Log_HData WHERE [TIMESTAMP] BETWEEN '{0}' AND '{1}'",
                    startTime.ToString("yyyy/MM/dd HH:mm:ss"), endTime.ToString("yyyy/MM/dd HH:mm:ss"))))
                {
                    stateInfo = null;
                    _hdastart = DateTime.Now;
                }
                count++;
                Thread.Sleep(CYCLE2);
            }
        }

        public IEnumerable<HistoryData> GetData(HistoryData[] hdaarray, DateTime startTime, DateTime endTime)
        {
            //if (hdaarray.Length == 0) yield break;
            foreach (var data in hdaarray)
            {
                if (data.TimeStamp >= startTime)
                {
                    if (data.TimeStamp <= endTime)
                        yield return data;
                    else
                        yield break;
                }
            }
        }
        #endregion

        void OnValueChanged(object sender, ValueChangedEventArgs e)
        {
            var tag = sender as ITag;
            DataHelper.Instance.ExecuteStoredProcedure("AddEventLog",
                DataHelper.CreateParam("@StartTime", SqlDbType.DateTime, tag.TimeStamp),
                DataHelper.CreateParam("@Source", SqlDbType.NVarChar, tag.ID.ToString(), 50),
                DataHelper.CreateParam("@Comment", SqlDbType.NVarChar, tag.ToString(), 50));
        }

        public HistoryData[] BatchRead(DataSource source, bool sync, params ITag[] itemArray)
        {
            int count = itemArray.Length;
            HistoryData[] data = new HistoryData[count];
            Dictionary<IGroup, List<ITag>> dict = new Dictionary<IGroup, List<ITag>>();
            for (int i = 0; i < count; i++)
            {
                short id = itemArray[i].ID;
                ITag tag = this[id];
                if (tag != null)
                {
                    IGroup grp = tag.Parent;
                    if (!dict.ContainsKey(grp))
                        dict.Add(grp, new List<ITag> { tag });
                    else
                        dict[grp].Add(tag);
                }
            }
            int j = 0;
            foreach (var dev in dict)
            {
                var list = dev.Value;
                var array = dev.Key.BatchRead(source, sync, list.ToArray());
                if (array == null) continue;
                Array.Copy(array, 0, data, j, array.Length);
                j += array.Length;
            }
            return data;
        }

        public int BatchWrite(Dictionary<string, object> tags, bool sync)
        {
            int rs = -1;
            Dictionary<IGroup, SortedDictionary<ITag, object>> dict = new Dictionary<IGroup, SortedDictionary<ITag, object>>();
            foreach (var item in tags)
            {
                var tag = this[item.Key];
                if (tag != null)
                {
                    IGroup grp = tag.Parent;
                    SortedDictionary<ITag, object> values;
                    if (!dict.ContainsKey(grp))
                    {
                        values = new SortedDictionary<ITag, object>();
                        if (tag.Address.VarType != DataType.BOOL && tag.Address.VarType != DataType.STR)
                        {
                            values.Add(tag, tag.ValueToScale(Convert.ToSingle(item.Value)));
                        }
                        else
                            values.Add(tag, item.Value);
                        dict.Add(grp, values);
                    }
                    else
                    {
                        values = dict[grp];
                        if (tag.Address.VarType != DataType.BOOL && tag.Address.VarType != DataType.STR)
                        {
                            values.Add(tag, tag.ValueToScale(Convert.ToSingle(item.Value)));
                        }
                        else
                            values.Add(tag, item.Value);
                    }
                }
                else Log.WriteEntry(string.Format("变量{0}不在变量表中，无法下载", item.Key), EventLogEntryType.Error);
            }
            foreach (var dev in dict)
            {
                rs = dev.Key.BatchWrite(dev.Value, sync);
            }
            return rs;
        }

        void grp_DataChange(object sender, DataChangeEventArgs e)
        {
            var data = e.Values;
            var now = DateTime.Now;
            if (_hasHda)
            {
                ArchiveTime archiveTime;
                List<HistoryData> tempData = new List<HistoryData>(20);
                for (int i = 0; i < data.Count; i++)
                {
                    if (_archiveTimes.TryGetValue(data[i].ID, out archiveTime) && archiveTime == null && data[i].TimeStamp != DateTime.MinValue)
                    {
                        tempData.Add(data[i]);
                    }
                }
                if (tempData.Count > 0)
                {
                    ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(this.OnUpdate), tempData);
                }
            }
            if (_socketThreadList != null && _socketThreadList.Count > 0)
            {
                IPAddress addr = null;
                var grp = sender as ClientGroup;
                if (grp != null)
                    addr = grp.RemoteAddress;
                ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(this.SendData), new TempCachedData(addr, data));
            }
        }
        //此处发生内存泄漏；需要试验CLRProfile确定泄漏原因；改回原方法测试；看是否解决队列堵塞问题。对于客户端Grp,要过滤掉
        private void SendData(object obj)
        {
            var tempdata = obj as TempCachedData;
            var data = tempdata.Data;
            byte[] sendBuffer = new byte[8192];
            sendBuffer[0] = FCTCOMMAND.fctHead;
            sendBuffer[1] = FCTCOMMAND.fctReadMultiple;
            //bytes[2] = 0;
            short j = 5;
            for (int i = 0; i < data.Count; i++)
            {
                short id = data[i].ID;
                var propid = GetItemProperties(id);
                if (propid >= 0 && propid < _list.Count)
                {
                    byte[] dt = BitConverter.GetBytes(id);
                    sendBuffer[j++] = dt[0];
                    sendBuffer[j++] = dt[1];
                    switch (_list[propid].DataType)
                    {
                        case DataType.BOOL:
                            sendBuffer[j++] = 1;
                            sendBuffer[j++] = data[i].Value.Boolean ? (byte)1 : (byte)0;
                            break;
                        case DataType.BYTE:
                            sendBuffer[j++] = 1;
                            sendBuffer[j++] = data[i].Value.Byte;
                            break;
                        case DataType.WORD:
                            {
                                sendBuffer[j++] = 2;
                                byte[] bt = BitConverter.GetBytes(data[i].Value.Word);
                                sendBuffer[j++] = bt[0];
                                sendBuffer[j++] = bt[1];
                            }
                            break;
                        case DataType.SHORT:
                            {
                                sendBuffer[j++] = 2;
                                byte[] bt = BitConverter.GetBytes(data[i].Value.Int16);
                                sendBuffer[j++] = bt[0];
                                sendBuffer[j++] = bt[1];
                            }
                            break;
                        case DataType.DWORD:
                            {
                                sendBuffer[j++] = 4;
                                byte[] bt = BitConverter.GetBytes(data[i].Value.DWord);
                                sendBuffer[j++] = bt[0];
                                sendBuffer[j++] = bt[1];
                                sendBuffer[j++] = bt[2];
                                sendBuffer[j++] = bt[3];
                            }
                            break;
                        case DataType.INT:
                            {
                                sendBuffer[j++] = 4;
                                byte[] bt = BitConverter.GetBytes(data[i].Value.Int32);
                                sendBuffer[j++] = bt[0];
                                sendBuffer[j++] = bt[1];
                                sendBuffer[j++] = bt[2];
                                sendBuffer[j++] = bt[3];
                            }
                            break;
                        case DataType.FLOAT:
                            {
                                sendBuffer[j++] = 4;
                                byte[] bt = BitConverter.GetBytes(data[i].Value.Single);
                                sendBuffer[j++] = bt[0];
                                sendBuffer[j++] = bt[1];
                                sendBuffer[j++] = bt[2];
                                sendBuffer[j++] = bt[3];
                            }
                            break;
                        case DataType.STR:
                            {
                                byte[] bt = Encoding.ASCII.GetBytes(this[data[i].ID].ToString());
                                sendBuffer[j++] = (byte)bt.Length;
                                for (int k = 0; k < bt.Length; k++)
                                {
                                    sendBuffer[j++] = bt[k];
                                }
                            }
                            break;
                        default:
                            break;
                    }
                    Array.Copy(BitConverter.GetBytes((data[i].TimeStamp == DateTime.MinValue ? DateTime.Now : data[i].TimeStamp).ToFileTime()), 0, sendBuffer, j, 8);
                    j += 8;
                }
            }
            byte[] dt1 = BitConverter.GetBytes(j);
            sendBuffer[3] = dt1[0];
            sendBuffer[4] = dt1[1];
            SocketError err;
            //bytes.CopyTo(bytes2, 0);
            List<Socket> sockets = new List<Socket>();
            foreach (var socket in _socketThreadList)
            {
                if (!socket.Key.Equals(tempdata.Address))
                    sockets.Add(socket.Value);
            }
            data = null;
            obj = null;
            tempdata = null;
            foreach (var socket in sockets)
            {
                try
                {
                    socket.Send(sendBuffer, 0, j, SocketFlags.None, out err);
                    if (err == SocketError.ConnectionAborted || err == SocketError.HostDown ||
                        err == SocketError.NetworkDown || err == SocketError.Shutdown)
                    {
                        _socketThreadList.Remove((socket.RemoteEndPoint as IPEndPoint).Address);
                    }
                }
                catch (Exception ex1)
                {
                    AddErrorLog(ex1);
                }
            }
        }

        public IDriver AddDriver(short id, string name, string assembly, string className)
        {
            if (_drivers.ContainsKey(id))
                return _drivers[id];
            IDriver dv = null;
            try
            {
                Assembly ass = Assembly.LoadFrom(assembly);
                var dvType = ass.GetType(className);
                if (dvType != null)
                {
                    dv = Activator.CreateInstance(dvType, new object[] { this, id, name }) as IDriver;
                    if (dv != null)
                    {
                        foreach (var arg in _arguments)
                        {
                            if (arg.DriverID == id)
                            {
                                var prop = dvType.GetProperty(arg.PropertyName);
                                if (prop != null)
                                {
                                    if (prop.PropertyType.IsEnum)
                                        prop.SetValue(dv, Enum.Parse(prop.PropertyType, arg.PropertyValue), null);
                                    else
                                        prop.SetValue(dv, Convert.ChangeType(arg.PropertyValue, prop.PropertyType, CultureInfo.CreateSpecificCulture("en-US")), null);
                                }
                            }
                        }
                        _drivers.Add(id, dv);
                    }
                }
            }
            catch (Exception e)
            {
                AddErrorLog(e);
            }
            return dv;
        }

        public bool RemoveDriver(IDriver device)
        {
            lock (SyncRoot)
            {
                if (_drivers.Remove(device.ID))
                {
                    device.Dispose();
                    device = null;
                    return true;
                }
                return false;
            }
        }

        void reader_OnClose(object sender, IOErrorEventArgs e)
        {
            Log.WriteEntry(e.Reason, EventLogEntryType.Error);
            //AddErrorLog(new Exception(e.shutdownReason));
        }

        public bool AddItemIndex(string key, ITag value)
        {
            key = key.ToUpper();
            if (_mapping.ContainsKey(key))
                return false;
            _mapping.Add(key, value);
            return true;
        }

        public bool RemoveItemIndex(string key)
        {
            return _mapping.Remove(key.ToUpper());
        }

        object _alarmsync = new object();

        string[] itemList = null;
        public IEnumerable<string> BrowseItems(BrowseType browseType, string tagName, DataType dataType)
        {
            lock (SyncRoot)
            {
                if (_list.Count == 0) yield break;
                int len = _list.Count;
                if (itemList == null)
                {
                    itemList = new string[len];
                    for (int i = 0; i < len; i++)
                    {
                        itemList[i] = _list[i].Name;
                    }
                    Array.Sort(itemList);
                }
                int ii = 0;
                bool hasTag = !string.IsNullOrEmpty(tagName);
                bool first = true;
                string str = hasTag ? tagName + SPLITCHAR : string.Empty;
                if (hasTag)
                {
                    ii = Array.BinarySearch(itemList, tagName);
                    if (ii < 0) first = false;
                    //int strLen = str.Length;
                    ii = Array.BinarySearch(itemList, str);
                    if (ii < 0) ii = ~ii;
                }
                //while (++i < len && temp.Length >= strLen && temp.Substring(0, strLen) == str)
                do
                {
                    if (first && hasTag)
                    {
                        first = false;
                        yield return tagName;
                    }
                    string temp = itemList[ii];
                    if (hasTag && !temp.StartsWith(str, StringComparison.Ordinal))
                        break;
                    if (dataType == DataType.NONE || _mapping[temp].Address.VarType == dataType)
                    {
                        bool b3 = true;
                        if (browseType != BrowseType.Flat)
                        {
                            string curr = temp + SPLITCHAR;
                            int index = Array.BinarySearch(itemList, ii, len - ii, curr);
                            if (index < 0) index = ~index;
                            b3 = itemList[index].StartsWith(curr, StringComparison.Ordinal);
                            if (browseType == BrowseType.Leaf)
                                b3 = !b3;
                        }
                        if (b3)
                            yield return temp;
                    }
                } while (++ii < len);
            }
        }

        public int GetScaleByID(short Id)
        {
            if (_scales == null || _scales.Count == 0) return -1;
            return _scales.BinarySearch(new Scaling { ID = Id });
        }

        public IGroup GetGroupByName(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            foreach (IDriver device in Drivers)
            {
                foreach (IGroup grp in device.Groups)
                {
                    if (grp.Name == name)
                        return grp;
                }
            }
            return null;
        }

        public void ActiveItem(bool active, params ITag[] items)
        {
            Dictionary<IGroup, List<short>> dict = new Dictionary<IGroup, List<short>>();
            for (int i = 0; i < items.Length; i++)
            {
                List<short> list = null;
                ITag item = items[i];
                dict.TryGetValue(item.Parent, out list);
                if (list != null)
                {
                    list.Add(item.ID);
                }
                else
                    dict.Add(item.Parent, new List<short> { item.ID });

            }
            foreach (var grp in dict)
            {
                grp.Key.SetActiveState(active, grp.Value.ToArray());
            }
        }

        public int GetItemProperties(short id)
        {
            return _list.BinarySearch(new TagMetaData { ID = id });
        }
        #endregion

        #region Condition & Alarm（报警和条件）
        List<ICondition> _conditions;
        List<ICondition> _conditionList;

        List<AlarmItem> _alarmList;
        public IEnumerable<AlarmItem> AlarmList
        {
            get
            {
                return _alarmList;
            }
        }

        public IList<ICondition> ActivedConditionList
        {
            get
            {
                return _conditionList;
            }
        }

        public IList<ICondition> ConditionList
        {
            get
            {
                return _conditions;
            }
        }

        void cond_SendAlarm(object sender, AlarmItem e)
        {
            lock (_alarmsync)
            {
                int index2 = _conditions.BinarySearch(new DigitAlarm(0, e.Source), _compare);
                if (index2 > -1)
                {
                    var cond = _conditions[index2];
                    _conditionList.Remove(cond);
                    if (e.SubAlarmType != SubAlarmType.None)
                    {
                        _conditionList.Add(cond);
                    }
                }

                if (_alarmList.Count < ALARMLIMIT)
                {
                    _alarmList.Add(e);
                }
                else
                {
                    SaveAlarm();
                    _alarmList.Add(e);
                }
            }
            /*应加入判断，是否需要更新数据库（if(Index>=ALARMLIMIT){Save(); Index=0;}else Index++;
             * 客户端查询前先发送一个查询报警(alarmQuery)请求，包含起始时间参数，服务器从判断是否要将缓存写入数据库，
             * 待服务器返回就绪后，客户端再从数据库查询报警记录。
             */
        }

        private bool SaveAlarm()
        {
            if (_alarmList.Count == 0) return true;
            if (DataHelper.Instance.BulkCopy(new AlarmDataReader(_alarmList), "Log_Alarm", null, SqlBulkCopyOptions.KeepIdentity))
            {
                _alarmList.Clear();
                _alarmstart = DateTime.Now;
                return true;
            }
            return false;
        }

        public ICondition GetCondition(string tagName, AlarmType type)
        {
            ITag tag = this[tagName];
            if (tag == null) return null;
            short id = tag.ID;
            int index = _conditions.BinarySearch(new DigitAlarm(0, tagName));
            if (index < 0) return null;
            int ind1 = index - 1;
            ICondition cond = _conditions[index];
            while (index < _conditions.Count && cond.Source == tagName)
            {
                cond = _conditions[index++];
                if (cond.AlarmType == type)
                {
                    return cond;
                }
            }
            while (ind1 >= 0 && cond.Source == tagName)
            {
                cond = _conditions[ind1--];
                if (cond.AlarmType == type)
                {
                    return cond;
                }
            }
            return null;
        }

        public IList<ICondition> QueryConditions(string sourceName)
        {
            if (_conditions == null || sourceName == null) return null;
            ITag tag = this[sourceName];
            if (tag == null) return null;
            int index = _conditions.BinarySearch(new DigitAlarm(0, sourceName));
            if (index < 0) return null;
            List<ICondition> condList = new List<ICondition>();
            ICondition cond = _conditions[index];
            int ind1 = index - 1;
            while (cond.Source == sourceName)
            {
                condList.Add(cond);
                if (++index < _conditions.Count)
                    cond = _conditions[index];
                else
                    break;
            }
            while (ind1 >= 0)
            {
                if (cond.Source == sourceName)
                    condList.Add(cond);
            }
            return condList;
        }

        public int DisableCondition(string sourceName, AlarmType type)
        {
            var cond = GetCondition(sourceName, type);
            if (cond != null)
            {
                cond.IsEnabled = false;
                return 1;
            }
            return -1;
        }

        public int EnableCondition(string sourceName, AlarmType type)
        {
            var cond = GetCondition(sourceName, type);
            if (cond != null)
            {
                cond.IsEnabled = true;
                return 1;
            }
            return -1;
        }

        public int RemoveConditon(string sourceName, AlarmType type)
        {
            var cond = GetCondition(sourceName, type);
            if (cond != null)
            {
                _conditions.Remove(cond);
                return 1;
            }
            return -1;
        }

        public int RemoveConditons(string sourceName)
        {
            ITag tag = this[sourceName];
            if (_conditions == null || tag == null) return -1;
            int index = _conditions.BinarySearch(new DigitAlarm(0, sourceName));
            if (index < 0) return index;
            int ind1 = index - 1;
            ICondition cond = _conditions[index];
            List<int> li = new List<int>();
            while (cond.Source == sourceName)
            {
                li.Add(index);
                if (++index < _conditions.Count)
                    cond = _conditions[index];
                else
                    break;
            }
            while (ind1 >= 0)
            {
                cond = _conditions[ind1--];
                if (cond.Source == sourceName)
                    li.Add(ind1);
            }
            if (li.Count == 0) return -1;
            for (int i = li.Count - 1; i >= 0; i--)
            {
                _conditions.RemoveAt(i);
            }
            return 1;
        }

        public int AckConditions(params ICondition[] conditions)
        {
            if (conditions == null || conditions.Length == 0) return -1;
            foreach (ICondition cond in conditions)
            {
                cond.IsAcked = true;
                cond.LastAckTime = DateTime.Now;
            }
            return 1;
        }

        public string Read(string id)
        {
            throw new NotImplementedException();
        }

        public bool ReadExpression(string expression)
        {
            throw new NotImplementedException();
        }

        public int Write(string id, string value)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> BatchRead(string[] tags)
        {
            throw new NotImplementedException();
        }

        public int BatchWrite(Dictionary<string, string> tags)
        {
            throw new NotImplementedException();
        }

        public Stream LoadMetaData()
        {
            throw new NotImplementedException();
        }

        public Stream LoadHdaBatch(DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public Stream LoadHdaSingle(DateTime start, DateTime end, short id)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    class DriverArgumet
    {
        public short DriverID;
        public string PropertyName;
        public string PropertyValue;

        public DriverArgumet(short id, string name, string value)
        {
            DriverID = id;
            PropertyName = name;
            PropertyValue = value;
        }
    }

    class TempCachedData
    {
        IPAddress _addr;
        public IPAddress Address
        {
            get { return _addr; }
        }

        IList<HistoryData> _data;
        public IList<HistoryData> Data
        {
            get { return _data; }
        }

        public TempCachedData(IPAddress addr, IList<HistoryData> data)
        {
            _addr = addr;
            _data = data;
        }
    }

    internal sealed class ArchiveTime
    {
        public int Cycle;
        public DateTime LastTime;
        public ArchiveTime(int cycle, DateTime last)
        {
            Cycle = cycle;
            LastTime = last;
        }
    }
}

using ClientDriver;
using DatabaseLib;
using DataService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace CoreTest
{
    public sealed class DAServer : IDataServer, IAlarmServer, IHDAServer
    {
        int ALARMLIMIT = 1000;
        int CYCLE = 60000;

        const char SPLITCHAR = '.';
        const string SERVICELOGSOURCE = "ClientService";
        const string SERVICELOGNAME = "ClientService";


        static EventLog Log;

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

        Timer timer;

        Dictionary<string, ITag> _mapping;

        List<Scaling> _scales;

        public IEnumerable<IDriver> Drivers
        {
            get { yield return reader; }
        }

        CompareCondBySource _compare;

        ClientReader reader = null;
        ClientGroup group = null;

        ExpressionEval reval;
        public ExpressionEval Eval
        {
            get
            {
                return reval;
            }
        }

        public DAServer()
        {
            /*hda = new HDAQueue(MAXHDACAP);历史数据均保存于服务器；本机如需查询，需复制历史数据文件到指定文件夹。
             * 历史数据在服务器上分包（1024字节）发送，客户端每接收一条记录，异步附加或yield return*/
            if (!EventLog.SourceExists(SERVICELOGSOURCE))
            {
                EventLog.CreateEventSource(SERVICELOGSOURCE, SERVICELOGNAME);
            }
            Log = new EventLog(SERVICELOGNAME);
            Log.Source = SERVICELOGSOURCE;
            ALARMLIMIT = ConfigCache.AlarmLimit;
            CYCLE = ConfigCache.Cycle;
            _scales = new List<Scaling>();
            _linkedList = new QueueCollection<AlarmItem>(ALARMLIMIT + 10);
            reval = new ExpressionEval(this);
            InitClient();
            InitServerByDatabase();
            InitConnection();
        }

        void InitClient()
        {
            string sLine = DataHelper.HostName;
            var cdrv = AddDriver(1, "Client1", null, null);
            cdrv.ServerName = string.IsNullOrEmpty(sLine) ? Environment.MachineName : sLine;
            cdrv.TimeOut = 20000;
            cdrv.Connect();
        }

        void InitConnection()
        {
            reader.OnError += new IOErrorEventHandler(reader_OnClose);
            if (reader.IsClosed)
            {
                reader.Connect();
            }
            if (group != null)
            {
                group.IsActive = true;
                timer = new Timer(OnTimer, null, CYCLE, CYCLE);
            }
        }

        void OnTimer(object o)
        {
            if (reader.IsClosed)
            {
                reader.Connect();
                group.IsActive = true;
            }
        }

        void InitServerByDatabase()
        {
            try
            {
                using (var dataReader = DataHelper.Instance.ExecuteProcedureReader("InitServer", DataHelper.CreateParam("@TYPE", System.Data.SqlDbType.Int, 1)))
                {
                    if (dataReader == null) Environment.Exit(0);
                    //dataReader.Read();
                    dataReader.Read();
                    int count = dataReader.GetInt32(0);
                    _list = new List<TagMetaData>(count);
                    _mapping = new Dictionary<string, ITag>(count);
                    dataReader.NextResult();
                    while (dataReader.Read())
                    {
                        _list.Add(new TagMetaData(dataReader.GetInt16(0), dataReader.GetInt16(1), dataReader.GetString(2), dataReader.GetString(3), (DataType)dataReader.GetByte(4),
                     (ushort)dataReader.GetInt16(5), dataReader.GetBoolean(6), dataReader.GetFloat(7), dataReader.GetFloat(8), dataReader.GetInt32(9)));
                        //_list[i].Description = dataReader.GetSqlString(6).Value;
                    }
                    _list.Sort();
                    if (reader != null && group == null)
                    {
                        group = reader.AddGroup("Group1", 1, 0, 0, true) as ClientGroup;
                        group.AddItems(_list);
                    }
                    dataReader.NextResult();
                    _conditions = new List<ICondition>();
                    _conditionList = new ObservableCollection<ICondition>();
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
                        cond.AlarmAck += new EventHandler(cond_AckAlarm);
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
                reval.Clear();
                _scales.Sort();
                _compare = new CompareCondBySource();
                _conditions.Sort(_compare);
            }
            catch (Exception e)
            {
                App.AddErrorLog(e);
                Environment.Exit(0);
            }
        }

        public HistoryData[] BatchRead(DataSource source, bool sync, params ITag[] itemArray)
        {
            if (group == null) return null;
            return group.BatchRead(source, sync, itemArray);
        }

        public int BatchWrite(Dictionary<string, object> tags, bool sync)
        {
            if (group == null) return -1;
            SortedDictionary<ITag, object> dict = new SortedDictionary<ITag, object>();
            foreach (var item in tags)
            {
                var tag = this[item.Key];
                if (tag != null)
                    dict.Add(tag, item.Value);
            }
            return group.BatchWrite(dict, sync);
        }

        public IDriver AddDriver(short id, string name, string assembly, string className)
        {
            if (reader == null)
            {
                reader = new ClientReader(this, id, name);//server应为远程主机名/IP，从本地字符串解析获取
            }
            return reader;
        }

        public bool RemoveDriver(IDriver device)
        {
            lock (SyncRoot)
            {
                if (device == reader)
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
            App.AddErrorLog(new Exception((e.Reason)));
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

        object _asyncAlarm = new object();
        void cond_SendAlarm(object sender, AlarmItem e)
        {
            lock (_asyncAlarm)
            {
                int index2 = _conditions.BinarySearch(new DigitAlarm(0, e.Source), _compare);
                if (index2 > -1)
                {
                    var cond = _conditions[index2];
                    if (_conditionList.Contains(cond) && App.Current != null)
                    {
                        App.Current.Dispatcher.BeginInvoke(new Action(delegate
                        {
                            try
                            {
                                _conditionList.Remove(cond);
                            }
                            catch (Exception err) { }
                        }));
                    }
                    if (e.SubAlarmType != SubAlarmType.None && App.Current != null)
                    {
                        App.Current.Dispatcher.BeginInvoke(new Action(delegate
                        {
                            try
                            {
                                _conditionList.Add(cond);
                            }
                            catch (Exception err) { }
                        }));
                    }
                }
                if (_linkedList.Count < ALARMLIMIT)
                {
                    _linkedList.Enqueue(e);
                }
                else
                {
                    _linkedList.Dequeue();
                    _linkedList.Enqueue(e);
                }
            }
        }

        void cond_AckAlarm(object sender, EventArgs e)
        {
            SystemLog.AddLog(new SystemLog(EventType.Simple, DateTime.Now, App.LogSource, "报警应答"));
        }

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
            return group;
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

        public IEnumerable<HistoryData> ReadAtTime(params DateTime[] timeStamps)
        {
            return null;//待定
        }

        public IEnumerable<HistoryData> ReadAtTime(short ID, params DateTime[] timeStamps)
        {
            return null;//待定
        }

        //考虑用异步Socket从服务器获取历史数据，命令包含起始时间和ID，以数组返回（要拆包）
        //对日期、时间如何分割？可返回任意时间段；合并；对于历史数据，如本地文件夹存在，则从本地读取；没有则当日从服务器读取，历史数据分离处理
        public IEnumerable<HistoryData> ReadRaw(DateTime start, DateTime end)
        {
            if (end < start) return null;
            try
            {
                if (HDAIOHelper.FindFile(end) && (DateTime.Today > end.Date))
                {
                    return ReadFromFile(start, end);
                }
                return group.SendHdaRequest(start, end);
            }
            catch (Exception exp)
            {
                App.AddErrorLog(exp);
                return null;
            }
        }

        public IEnumerable<HistoryData> ReadRaw(DateTime start, DateTime end, short ID)
        {
            if (end < start) return null;
            try
            {
                if (HDAIOHelper.FindFile(end) && (DateTime.Today > end.Date))
                {
                    return ReadFromFile(start, end, ID);
                }
                return group.SendHdaRequest(start, end, ID);
            }
            catch (Exception exp)
            {
                App.AddErrorLog(exp);
                return null;
            }
        }

        private IEnumerable<HistoryData> ReadFromFile(DateTime start, DateTime end, short ID)
        {
            int eyear = end.Year;
            int syear = start.Year;
            int emonth = end.Month;
            int smonth = start.Month;
            int year = syear;
            while (year <= eyear)
            {
                int month = (year == syear ? smonth : 1);
                while (month <= (year == eyear ? emonth : 12))
                {
                    var result = HDAIOHelper.LoadFromFile((year == syear && month == smonth ? start : new DateTime(year, month, 1)),
                        (year == eyear && month == emonth ? end : new DateTime(year, month, 1).AddMonths(1).AddMilliseconds(-2)), ID);//考虑按月遍历
                    if (result != null)
                    {
                        foreach (var data in result)
                            yield return data;
                    }
                    month++;
                }
                year++;
            }
        }

        private IEnumerable<HistoryData> ReadFromFile(DateTime start, DateTime end)
        {
            int eyear = end.Year;
            int syear = start.Year;
            int emonth = end.Month;
            int smonth = start.Month;
            int year = syear;
            while (year <= eyear)
            {
                int month = (year == syear ? smonth : 1);
                while (month <= (year == eyear ? emonth : 12))
                {
                    var result = HDAIOHelper.LoadFromFile((year == syear && month == smonth ? start : new DateTime(year, month, 1)),
                        (year == eyear && month == emonth ? end : new DateTime(year, month, 1).AddMonths(1).AddMilliseconds(-2)));//考虑按月遍历
                    if (result != null)
                    {
                        foreach (var data in result)
                            yield return data;
                    }
                    month++;
                }
                year++;
            }
        }

        public int SendAlarmRequest(DateTime? start, DateTime? end)
        {
            return group.SendAlarmRequest(start, end);
        }

        public void Dispose()
        {
            lock (this)
            {
                if (timer != null)
                    timer.Dispose();
                group.SendResetRequest();
                reader.OnError -= this.reader_OnClose;
                reader.Dispose();
                if (_conditionList != null)
                {
                    foreach (var condition in _conditionList)
                    {
                        if (condition != null)
                            condition.AlarmActive -= cond_SendAlarm;
                    }
                }
                reval.Dispose();
            }
        }

        List<ICondition> _conditions;
        ObservableCollection<ICondition> _conditionList;

        QueueCollection<AlarmItem> _linkedList;
        public IEnumerable<AlarmItem> AlarmList
        {
            get
            {
                return _linkedList;
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
            lock (_asyncAlarm)
            {
                foreach (ICondition cond in conditions)
                {
                    cond.IsAcked = true;
                    if (_conditionList.Contains(cond))
                        _conditionList.Remove(cond);
                }
            }
            return 1;
        }

        public int IgnoreConditions(params ICondition[] conditions)
        {
            if (conditions == null || conditions.Length == 0) return -1;
            lock (_asyncAlarm)
            {
                foreach (ICondition cond in conditions)
                {
                    if (_conditionList.Contains(cond))
                        _conditionList.Remove(cond);
                }
            }
            return 1;
        }

        Dictionary<short, string> _archiveList = null;
        public Dictionary<short, string> ArchiveList
        {
            get
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
                return _archiveList;
            }
        }

        public void GetMinMax(short id, out float max, out float min)
        {
            int index = GetItemProperties(id);
            if (index >= 0)
            {
                var metadata = MetaDataList[index];
                if (metadata.Maximum == metadata.Minimum && metadata.Maximum == 0)
                {
                    switch (metadata.DataType)
                    {
                        case DataType.BOOL:
                            max = 1; min = 0;
                            break;
                        case DataType.BYTE:
                            max = byte.MaxValue; min = byte.MinValue;
                            break;
                        case DataType.WORD:
                            max = ushort.MaxValue; min = ushort.MinValue;
                            break;
                        case DataType.SHORT:
                            max = short.MaxValue; min = short.MinValue;
                            break;
                        case DataType.DWORD:
                            max = uint.MaxValue; min = 0;
                            break;
                        case DataType.INT:
                            max = 100000; min = -100000;
                            break;
                        case DataType.FLOAT:
                            max = 100000; min = -100000;
                            break;
                        default:
                            max = min = 0f;
                            break;
                    }
                }
                else
                {
                    max = metadata.Maximum; min = metadata.Minimum;
                }
            }
            else
            {
                max = 100000; min = 0;
            }
        }
    }
}

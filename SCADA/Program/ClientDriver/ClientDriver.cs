using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using DataService;

namespace ClientDriver
{
    [Description("客户端驱动")]
    public class ClientReader : IDriver//客户端存在对TLV数据的字节序转换问题，需测试
    {
        short _id;
        public short ID
        {
            get
            {
                return _id;
            }
        }

        private int _timeout = 0;
        public int TimeOut
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        string _name;
        public string Name
        {
            get
            {
                return _name;
            }
        }

        string _ip;
        public string ServerName
        {
            get { return _ip; }
            set { _ip = value; }
        }

        internal Socket tcpSend;
        internal Socket tcpRecive;

        public bool IsClosed
        {
            get
            {
                //return tcpASynCl.Poll(-1, SelectMode.SelectRead);
                return tcpSend == null || tcpRecive == null || !tcpSend.Connected || !tcpRecive.Connected;
            }
        }

        List<IGroup> _grps = new List<IGroup>(1);
        public IEnumerable<IGroup> Groups
        {
            get { return _grps; }
        }

        IDataServer _server;
        public IDataServer Parent
        {
            get { return _server; }
        }

        public ClientReader(IDataServer server, short id, string name, string ip, int timeout)
        {
            _id = id;
            _server = server;
            _ip = ip;
            _name = name;
            _timeout = timeout;
            Connect();
        }

        public bool Connect()
        {
            int port = 6543;
            lock (this)
            {
                try
                {
                    if (tcpRecive != null)
                    {
                        tcpRecive.Dispose();
                    }
                    if (tcpSend != null)
                    {
                        tcpSend.Dispose();
                    }
                    tcpRecive = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    tcpRecive.Connect(_ip, port);
                    tcpRecive.SendTimeout = _timeout;
                    tcpRecive.ReceiveTimeout = -1;

                    tcpSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    tcpSend.Connect(_ip, port);
                    tcpSend.SendTimeout = _timeout;
                    tcpSend.ReceiveTimeout = _timeout;
                    return true;
                }
                catch (SocketException error)
                {
                    if (OnClose != null)
                        OnClose(this, new ShutdownRequestEventArgs(error.Message));
                    return false;
                }
            }
        }

        public IGroup AddGroup(string name, short id, int updateRate, float deadBand = 0f, bool active = false)
        {
            ClientGroup grp = new ClientGroup(id, name, updateRate, active, this);
            _grps.Add(grp);
            return grp;
        }

        public bool RemoveGroup(IGroup grp)
        {
            grp.IsActive = false;
            return _grps.Remove(grp);
        }

        public event ShutdownRequestEventHandler OnClose;

        public void Dispose()
        {
            foreach (IGroup grp in _grps)
            {
                grp.Dispose();
            }
            _grps.Clear();
            try
            {
                if (tcpRecive != null)
                {
                    if (tcpRecive.Connected)
                        tcpRecive.Shutdown(SocketShutdown.Both);
                    tcpRecive.Dispose();
                }
                if (tcpSend != null)
                {
                    if (tcpSend.Connected)
                        tcpSend.Shutdown(SocketShutdown.Both);
                    tcpSend.Dispose();
                }
            }
            catch (SocketException err)
            {
                if (OnClose != null)
                    OnClose(this, new ShutdownRequestEventArgs(err.Message));
            }
        }
    }

    public class ClientGroup : IGroup
    {
        bool _active = false;
        public bool IsActive
        {
            get
            {
                return _active;
            }
            set
            {
                _active = value;
                if (value)
                {
                    if (_plcReader != null && _plcReader.tcpSend != null)
                    {
                        this._tcpRecive = _plcReader.tcpRecive;
                        this._tcpSend = _plcReader.tcpSend;
                        try
                        {
                            _addr = (_tcpSend.RemoteEndPoint as IPEndPoint).Address;
                        }
                        catch { }
                        Thread workItem = new Thread(new ThreadStart(ReciveData));
                        workItem.Priority = ThreadPriority.Highest;
                        workItem.Start();
                        if (_updateRate > 0)
                            ThreadPool.QueueUserWorkItem(new WaitCallback(this.OnUpdate));
                        else
                            Init();
                    }
                }
            }
        }

        protected short _id;
        public short ID
        {
            get
            {
                return _id;
            }
        }

        protected int _updateRate;
        public int UpdateRate
        {
            get
            {
                return _updateRate;
            }
            set
            {
                _updateRate = value;
            }
        }

        protected DeviceAddress _start;
        public DeviceAddress Start
        {
            get
            {
                return _start;
            }
        }

        protected string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        protected float _deadband;
        public float DeadBand
        {
            get
            {
                return _deadband;
            }
            set
            {
                _deadband = value;
            }
        }

        protected ClientReader _plcReader;
        public IDriver Parent
        {
            get
            {
                return _plcReader;
            }
        }

        protected Dictionary<short, ITag> _items;
        public IEnumerable<ITag> Items
        {
            get { return _items.Values; }
        }

        IDataServer _server;
        public IDataServer Server
        {
            get
            {
                return _server;
            }
        }

        Socket _tcpSend, _tcpRecive;
        byte[] tcpBuffer;

        IPAddress _addr;
        public IPAddress RemoteAddress
        {
            get { return _addr; }
        }

        public ClientGroup(short id, string name, int updateRate, bool active, ClientReader plcReader)
        {
            this._id = id;
            this._name = name;
            this._updateRate = updateRate;
            this._active = active;
            this._plcReader = plcReader;
            this._server = plcReader.Parent;
            tcpBuffer = new byte[8192];
        }

        object sendasync = new object();
        private byte[] ReadSingleData(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            if (_tcpSend == null) return null;
            short ID = (short)address.Start;
            byte type = (byte)address.VarType;
            byte len = (byte)(address.DataSize);
            byte[] idbits = BitConverter.GetBytes(ID);
            byte[] write_data = new byte[6] { FCTCOMMAND.fctHead, FCTCOMMAND.fctReadSingle,
                source == DataSource.Cache?(byte)0:(byte)1, idbits[0], idbits[1] ,len};
            byte[] data = new byte[len];
            SocketError error;
            lock (sendasync)
            {
                _tcpSend.Send(write_data, 0, 6, SocketFlags.None, out error);
                int result = _tcpSend.Receive(tcpBuffer, 0, data.Length + 5, SocketFlags.None, out error);
            }
            Array.Copy(tcpBuffer, 5, data, 0, data.Length);
            if (error == SocketError.Success)
                return data;
            else
            {
                return null;
            }
        }

        private int WriteSingleData(DeviceAddress address, byte[] value)
        {
            if (_tcpSend == null) return -1;
            short ID = (short)address.Start;
            byte type = (byte)address.VarType;
            byte[] idbits = BitConverter.GetBytes(ID);
            byte[] write_data = new byte[6] { FCTCOMMAND.fctHead, FCTCOMMAND.fctWriteSingle, 1, idbits[0], idbits[1], (byte)(value.Length) };
            byte[] data = new byte[6 + value.Length];
            write_data.CopyTo(data, 0);
            value.CopyTo(data, 6);
            SocketError error;
            lock (sendasync)
            {
                _tcpSend.Send(data, 0, data.Length, SocketFlags.None, out error);
                _tcpSend.Receive(tcpBuffer, 0, 2, SocketFlags.None, out error);
            }
            if (error == SocketError.Success)
                return tcpBuffer[1];//可在此处返回一个错误号
            else
            {
                return (int)error;
            }
        }

        public void Init()
        {
            if (_items != null && _tcpSend != null)
            {
                int len = 0;
                List<ITag> tags = new List<ITag>();
                foreach (var tag in _items.Values)
                {
                    len += (3 + tag.Address.DataSize);
                    if (len >= tcpBuffer.Length - 10)
                    {
                        len = 0;
                        BatchRead(DataSource.Cache, true, tags.ToArray());
                        tags.Clear();
                    }
                    tags.Add(tag);
                }
                BatchRead(DataSource.Cache, true, tags.ToArray());
            }
        }

        private void ReciveData()
        {
            if (!_active || _plcReader.tcpRecive == null) return;
            List<HistoryData> historys = null;
            byte[] bytes = new byte[ushort.MaxValue];
            byte[] temp = new byte[_tcpRecive.ReceiveBufferSize];
            Storage value = Storage.Empty;
            int result = 0;
            int start = 0;
            SocketError error;
            do
            {
                if (!_tcpRecive.Connected) return;
                result = _tcpRecive.Receive(bytes, 0, bytes.Length, SocketFlags.None, out error);
                if (error == SocketError.Success)
                {
                    if (DataChange != null)
                        historys = new List<HistoryData>();
                    //DateTime time = DateTime.Now;//当前时间戳
                    if (start != 0 && temp[0] == FCTCOMMAND.fctHead)
                    {
                        int j = 3;
                        if (start < 0)
                        {
                            Array.Copy(bytes, 0, temp, -start, 5 + start);
                        }
                        short tc = BitConverter.ToInt16(temp, j);//总字节数
                        if (start < 0)
                            start += tc;
                        Array.Copy(bytes, 0, temp, tc - start, start);
                        j += 2;
                        while (j < tc)
                        {
                            short id = BitConverter.ToInt16(temp, j);//标签ID、数据长度、数据值（T,L,V)
                            j += 2;
                            byte length = temp[j++];
                            ITag tag;
                            if (_items.TryGetValue(id, out tag))
                            {
                                //数据类型
                                switch (tag.Address.VarType)
                                {
                                    case DataType.BOOL:
                                        value.Boolean = BitConverter.ToBoolean(temp, j);
                                        break;
                                    case DataType.BYTE:
                                        value.Byte = temp[j];
                                        break;
                                    case DataType.WORD:
                                    case DataType.SHORT:
                                        value.Int16 = BitConverter.ToInt16(temp, j);//需测试
                                        break;
                                    case DataType.INT:
                                        value.Int32 = BitConverter.ToInt32(temp, j);//需测试
                                        break;
                                    case DataType.FLOAT:
                                        value.Single = BitConverter.ToSingle(temp, j);
                                        break;
                                    case DataType.STR:
                                        StringTag strTag = tag as StringTag;
                                        if (strTag != null)
                                        {
                                            strTag.String = Encoding.ASCII.GetString(temp, j, length).Trim((char)0);
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                j += length;
                                DateTime time = DateTime.FromFileTime(BitConverter.ToInt64(temp, j));
                                j += 8;
                                tag.Update(value, time, QUALITIES.QUALITY_GOOD);
                                if (historys != null)
                                    historys.Add(new HistoryData(id, QUALITIES.QUALITY_GOOD, value, time));
                            }
                            else
                                j += length + 8;
                        }
                    }
                    byte head = bytes[start];
                    int count = start;
                    while (head == FCTCOMMAND.fctHead && result > count)
                    {
                        if (count + 5 > bytes.Length)
                        {
                            start = count - bytes.Length;
                            Array.Copy(bytes, count, temp, 0, -start);
                            break;
                        }
                        int j = count + 3;
                        short tc = BitConverter.ToInt16(bytes, j);//总标签数
                        count += tc;
                        if (count >= bytes.Length)
                        {
                            start = count - bytes.Length;
                            Array.Copy(bytes, count - tc, temp, 0, tc - start);
                            break;
                        }
                        else start = 0;
                        j += 2;
                        while (j < count)
                        {
                            short id = BitConverter.ToInt16(bytes, j);//标签ID、数据长度、数据值（T,L,V)
                            j += 2;
                            byte length = bytes[j++];
                            ITag tag;
                            if (_items.TryGetValue(id, out tag))
                            {
                                //数据类型
                                switch (tag.Address.VarType)
                                {
                                    case DataType.BOOL:
                                        value.Boolean = BitConverter.ToBoolean(bytes, j);
                                        break;
                                    case DataType.BYTE:
                                        value.Byte = bytes[j];
                                        break;
                                    case DataType.WORD:
                                    case DataType.SHORT:
                                        value.Int16 = BitConverter.ToInt16(bytes, j);//需测试
                                        break;
                                    case DataType.INT:
                                        value.Int32 = BitConverter.ToInt32(bytes, j);//需测试
                                        break;
                                    case DataType.FLOAT:
                                        value.Single = BitConverter.ToSingle(bytes, j);
                                        break;
                                    case DataType.STR:
                                        StringTag strTag = tag as StringTag;
                                        if (strTag != null)
                                        {
                                            strTag.String = Encoding.ASCII.GetString(bytes, j, length).Trim((char)0);
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                j += length;
                                DateTime time = DateTime.FromFileTime(BitConverter.ToInt64(bytes, j));
                                j += 8;
                                tag.Update(value, time, QUALITIES.QUALITY_GOOD);
                                if (historys != null)
                                    historys.Add(new HistoryData(id, QUALITIES.QUALITY_GOOD, value, time));
                            }
                            else
                                j += length + 8;
                        }
                        head = bytes[count];
                    }
                    if (DataChange != null && historys.Count > 0)
                        DataChange(this, new DataChangeEventArgs(1, historys));

                }
                else if (error == SocketError.ConnectionReset || error == SocketError.Interrupted
                    || error == SocketError.HostDown || error == SocketError.NetworkDown || error == SocketError.Shutdown)
                {
                    _tcpRecive.Dispose();
                    _active = false;
                    return;
                }
            }
            while (result > 0);
        }

        public void OnUpdate(object stateInfo)
        {
            while (true)
            {
                Thread.Sleep(_updateRate);
                lock (this)
                {
                    if (!_active)
                    {
                        return;
                    }
                    Init();
                }
            }
        }

        public bool AddItems(IList<TagMetaData> items)
        {
            int count = items.Count;
            if (_items == null) _items = new Dictionary<short, ITag>(count);
            lock (_server.SyncRoot)
            {
                for (int i = 0; i < count; i++)
                {
                    ITag dataItem = null;
                    TagMetaData meta = items[i];
                    DeviceAddress addr = new DeviceAddress(0, 0, 0, meta.ID, meta.Size, 0, meta.DataType);
                    switch (meta.DataType)
                    {
                        case DataType.BOOL:
                            dataItem = new BoolTag(meta.ID, addr, this);
                            break;
                        case DataType.BYTE:
                            dataItem = new ByteTag(meta.ID, addr, this);
                            break;
                        case DataType.WORD:
                        case DataType.SHORT:
                            dataItem = new ShortTag(meta.ID, addr, this);
                            break;
                        case DataType.TIME:
                        case DataType.INT:
                            dataItem = new IntTag(meta.ID, addr, this);
                            break;
                        case DataType.FLOAT:
                            dataItem = new FloatTag(meta.ID, addr, this);
                            break;
                        case DataType.STR:
                            dataItem = new StringTag(meta.ID, addr, this);
                            break;
                        default:
                            dataItem = new BoolTag(meta.ID, addr, this);
                            break;
                    }
                    _items.Add(meta.ID, dataItem);
                    _server.AddItemIndex(meta.Name, dataItem);
                }
            }
            //Init();
            return true;
        }

        public bool AddTags(IEnumerable<ITag> tags)
        {
            if (_items == null)
            {
                _items = new Dictionary<short, ITag>();
            }
            foreach (ITag tag in tags)
            {
                if (tag != null)
                {
                    _items.Add(tag.ID, tag);
                }
            }
            //Init();
            return true;
        }

        public bool RemoveItems(params ITag[] items)
        {
            foreach (var item in items)
            {
                _server.RemoveItemIndex(item.GetTagName());
                _items.Remove(item.ID);
            }
            return true;
        }

        public bool SetActiveState(bool active, params short[] items)
        {
            return true;
        }

        public ITag FindItemByAddress(DeviceAddress addr)
        {
            ITag tag;
            if (_items.TryGetValue((short)addr.Start, out tag))
                return tag;
            return _server[(short)addr.Start];
        }

        public HistoryData[] BatchRead(DataSource source, bool isSync, params ITag[] itemArray)
        {
            if (itemArray.Length == 0 || _tcpSend == null || !_tcpSend.Connected) return null;
            short len = (short)itemArray.Length;
            byte[] bt = new byte[2];
            byte[] data = new byte[5 + len * 2];
            int j = 0;
            data[j++] = FCTCOMMAND.fctHead;
            data[j++] = FCTCOMMAND.fctReadMultiple;
            data[j++] = source == DataSource.Cache ? (byte)0 : (byte)1;
            bt = BitConverter.GetBytes(len);
            data[j++] = bt[0];
            data[j++] = bt[1];
            for (int i = 0; i < len; i++)
            {
                bt = BitConverter.GetBytes(itemArray[i].ID);
                data[j++] = bt[0];
                data[j++] = bt[1];
            }
            SocketError error;
            lock (sendasync)
            {
                _tcpSend.Send(data, 0, data.Length, SocketFlags.None, out error);
                int result = _tcpSend.Receive(tcpBuffer, 0, tcpBuffer.Length, SocketFlags.None, out error);
            }
            //！！！！此处的协议应注意，如批量读入的数据量超过缓存，必须分批合并；在协议头应加入总字节数，以和result比较是否需要循环读入。
            j = 5;
            if (error == SocketError.Success)
            {
                DateTime time = DateTime.Now;
                HistoryData[] values = new HistoryData[len];
                for (int i = 0; i < len; i++)
                {
                    short id = BitConverter.ToInt16(tcpBuffer, j);
                    j += 2;
                    ITag tag = _server[id];
                    byte length = tcpBuffer[j++];
                    if (tag != null && length > 0)
                    {
                        switch (tag.Address.VarType)
                        {
                            case DataType.BOOL:
                                values[i].Value.Boolean = BitConverter.ToBoolean(tcpBuffer, j);
                                break;
                            case DataType.BYTE:
                                values[i].Value.Byte = tcpBuffer[j];
                                break;
                            case DataType.WORD:
                            case DataType.SHORT:
                                values[i].Value.Int16 = BitConverter.ToInt16(tcpBuffer, j);
                                break;
                            case DataType.INT:
                                values[i].Value.Int32 = BitConverter.ToInt32(tcpBuffer, j);
                                break;
                            case DataType.FLOAT:
                                values[i].Value.Single = BitConverter.ToSingle(tcpBuffer, j);
                                break;
                            case DataType.STR:
                                StringTag strTag = tag as StringTag;
                                if (strTag != null)
                                {
                                    strTag.String = Encoding.ASCII.GetString(tcpBuffer, j, length).Trim();
                                }
                                break;
                        }
                        if (values[i].Value != tag.Value)
                            tag.Update(values[i].Value, time, QUALITIES.QUALITY_GOOD);
                    }
                    j += length;
                }
                return values;
            }
            else
                return null;
        }

        public int BatchWrite(SortedDictionary<ITag, object> items, bool isSync = true)
        {
            if (_tcpSend == null || !_tcpSend.Connected) return -1;
            List<byte> list = new List<byte>(new byte[] { FCTCOMMAND.fctHead, FCTCOMMAND.fctWriteMultiple });
            list.AddRange(BitConverter.GetBytes((short)items.Count));
            foreach (var item in items)
            {
                ITag tag = item.Key;
                list.AddRange(BitConverter.GetBytes(tag.ID));
                var addr = tag.Address;
                if (addr.VarType != DataType.STR)
                    list.Add((byte)(addr.DataSize));//此处存疑
                switch (addr.VarType)
                {
                    case DataType.BOOL:
                        list.Add(Convert.ToBoolean(item.Value) ? (byte)1 : (byte)0);
                        break;
                    case DataType.BYTE:
                        list.Add(Convert.ToByte(item.Value));
                        break;
                    case DataType.WORD:
                    case DataType.SHORT:
                        list.AddRange(BitConverter.GetBytes(Convert.ToInt16(item.Value)));
                        break;
                    case DataType.INT:
                        list.AddRange(BitConverter.GetBytes(Convert.ToInt32(item.Value)));
                        break;
                    case DataType.FLOAT:
                        list.AddRange(BitConverter.GetBytes(Convert.ToSingle(item.Value)));
                        break;
                    case DataType.STR:
                        var bts = Encoding.ASCII.GetBytes(Convert.ToString(item.Value));
                        list.Add((byte)bts.Length);
                        list.AddRange(bts);
                        break;
                }
            }
            SocketError error;
            lock (sendasync)
            {
                _tcpSend.Send(list.ToArray(), 0, list.Count, SocketFlags.None, out error);
                _tcpSend.Receive(tcpBuffer, 0, 2, SocketFlags.None, out error);
            }
            if (error == SocketError.Success)
                return tcpBuffer[1];
            else
            {
                return (int)error;
            }
        }

        public IEnumerable<HistoryData> SendHdaRequest(DateTime start, DateTime end)
        {
            if (_tcpSend == null || !_tcpSend.Connected) yield break;
            byte[] hdaReq = new byte[18];
            hdaReq[0] = FCTCOMMAND.fctHead;
            hdaReq[1] = FCTCOMMAND.fctHdaRequest;
            byte[] startbuffer = BitConverter.GetBytes(start.ToFileTime());
            startbuffer.CopyTo(hdaReq, 2);
            byte[] endbuffer = BitConverter.GetBytes(end.ToFileTime());
            endbuffer.CopyTo(hdaReq, 10);
            SocketError error;
            HistoryData data = HistoryData.Empty;
            short tempid = short.MinValue;
            byte[] temp = new byte[14];
            ITag tag = null;
            int index = 0;
            int size = 0;
            int result = 0;
            short id = 0;
            lock (sendasync)
            {
                _tcpSend.Send(hdaReq);
                do
                {
                    result = _tcpSend.Receive(tcpBuffer, 0, tcpBuffer.Length, SocketFlags.None, out error);
                    if (error == SocketError.ConnectionReset || error == SocketError.Interrupted || error == SocketError.HostDown || error == SocketError.NetworkDown || error == SocketError.Shutdown)
                    {
                        _tcpSend.Dispose();
                        yield break;
                    }
                    if (index != 0)
                    {
                        Array.Copy(tcpBuffer, 0, temp, 14 - index, index);
                        id = BitConverter.ToInt16(temp, 0);
                        tempid = id;
                        tag = _server[id];
                        if (tag == null) yield break;
                        data.ID = id;
                        size = tag.Address.DataSize;
                        index -= (4 - size);
                        switch (tag.Address.VarType)
                        {
                            case DataType.BOOL:
                                data.Value.Boolean = BitConverter.ToBoolean(temp, 2);
                                break;
                            case DataType.BYTE:
                                data.Value.Byte = tcpBuffer[index];
                                break;
                            case DataType.WORD:
                            case DataType.SHORT:
                                data.Value.Int16 = BitConverter.ToInt16(temp, 2);
                                break;
                            case DataType.TIME:
                            case DataType.INT:
                                data.Value.Int32 = BitConverter.ToInt32(temp, 2);
                                break;
                            case DataType.FLOAT:
                                data.Value.Single = BitConverter.ToSingle(temp, 2);
                                break;
                        }
                        long fileTime = BitConverter.ToInt64(temp, 2 + size);
                        if (fileTime == -1) yield break;
                        data.TimeStamp = DateTime.FromFileTime(fileTime);
                        yield return data;
                    }
                    while (result >= index + 2)
                    {
                        id = BitConverter.ToInt16(tcpBuffer, index);
                        if (tempid != id)
                        {
                            tempid = id;
                            tag = _server[id];
                            if (tag == null) yield break;
                            size = tag.Address.DataSize;
                        }
                        if (index + 10 + size > result)
                            break;
                        data.ID = id;
                        index += 2;
                        switch (tag.Address.VarType)
                        {
                            case DataType.BOOL:
                                data.Value.Boolean = BitConverter.ToBoolean(tcpBuffer, index);
                                break;
                            case DataType.BYTE:
                                data.Value.Byte = tcpBuffer[index];
                                break;
                            case DataType.WORD:
                            case DataType.SHORT:
                                data.Value.Int16 = BitConverter.ToInt16(tcpBuffer, index);
                                break;
                            case DataType.TIME:
                            case DataType.INT:
                                data.Value.Int32 = BitConverter.ToInt32(tcpBuffer, index);
                                break;
                            case DataType.FLOAT:
                                data.Value.Single = BitConverter.ToSingle(tcpBuffer, index);
                                break;
                        }
                        index += size;
                        long fileTime = BitConverter.ToInt64(tcpBuffer, index);
                        if (fileTime == -1) yield break;
                        data.TimeStamp = DateTime.FromFileTime(fileTime);
                        index += 8;
                        yield return data;
                    }
                    if (index == result)
                        index = 0;
                    else
                    {
                        Array.Copy(tcpBuffer, index, temp, 0, result - index);
                        index += 14 - result;
                    }
                } while (result > 0);
            }
            yield break;
        }

        public IEnumerable<HistoryData> SendHdaRequest(DateTime start, DateTime end, short id)
        {
            if (_tcpSend == null || !_tcpSend.Connected) yield break;
            ITag tag = _server[id];
            if (tag == null) yield break;
            byte[] hdaReq = new byte[20];
            hdaReq[0] = FCTCOMMAND.fctHead;
            hdaReq[1] = FCTCOMMAND.fctHdaIdRequest;
            byte[] startbuffer = BitConverter.GetBytes(start.ToFileTime());
            startbuffer.CopyTo(hdaReq, 2);
            byte[] endbuffer = BitConverter.GetBytes(end.ToFileTime());
            endbuffer.CopyTo(hdaReq, 10);
            byte[] idbuffer = BitConverter.GetBytes(id);
            idbuffer.CopyTo(hdaReq, 18);
            SocketError error;
            int index = 0;
            HistoryData data = HistoryData.Empty;
            data.ID = id;
            int result = 0;
            lock (sendasync)
            {
                _tcpSend.Send(hdaReq);
                switch (tag.Address.VarType)
                {
                    case DataType.FLOAT:
                        do
                        {
                            result = _tcpSend.Receive(tcpBuffer, 0, tcpBuffer.Length, SocketFlags.None, out error);
                            if (error == SocketError.ConnectionReset || error == SocketError.Interrupted || error == SocketError.HostDown || error == SocketError.NetworkDown || error == SocketError.Shutdown)
                            {
                                _tcpSend.Dispose();
                                yield break;
                            }
                            while (index + 12 <= result)
                            {
                                data.Value.Single = BitConverter.ToSingle(tcpBuffer, index);//未来可考虑量程转换和其他数据类型
                                index += 4;
                                long fileTime = BitConverter.ToInt64(tcpBuffer, index);
                                if (fileTime == -1) yield break;
                                data.TimeStamp = DateTime.FromFileTime(fileTime);
                                index += 8;
                                yield return data;
                            }
                            if (index == result)
                                index = 0;
                            else
                                index += 12 - result;//丢弃一个值
                        } while (result > 0);
                        break;
                    case DataType.INT:
                        do
                        {
                            result = _tcpSend.Receive(tcpBuffer, 0, tcpBuffer.Length, SocketFlags.None, out error);
                            if (error == SocketError.ConnectionReset || error == SocketError.Interrupted || error == SocketError.HostDown || error == SocketError.NetworkDown || error == SocketError.Shutdown)
                            {
                                _tcpSend.Dispose();
                                yield break;
                            }
                            while (index + 12 <= result)
                            {
                                data.Value.Int32 = BitConverter.ToInt32(tcpBuffer, index);//未来可考虑量程转换和其他数据类型
                                index += 4;
                                long fileTime = BitConverter.ToInt64(tcpBuffer, index);
                                if (fileTime == -1) yield break;
                                data.TimeStamp = DateTime.FromFileTime(fileTime);
                                index += 8;
                                yield return data;
                            }
                            if (index == result)
                                index = 0;
                            else
                                index += 12 - result;//丢弃一个值
                        } while (result > 0);
                        break;
                    case DataType.BOOL://布尔量一个都不能少
                        {
                            byte[] temp = new byte[9];
                            do
                            {
                                result = _tcpSend.Receive(tcpBuffer, 0, tcpBuffer.Length, SocketFlags.None, out error);
                                if (error == SocketError.ConnectionReset || error == SocketError.Interrupted || error == SocketError.HostDown || error == SocketError.NetworkDown || error == SocketError.Shutdown)
                                {
                                    _tcpSend.Dispose();
                                    yield break;
                                }
                                if (index != 0)
                                {
                                    Array.Copy(tcpBuffer, 0, temp, 9 - index, index);
                                    data.Value.Boolean = BitConverter.ToBoolean(temp, 0);
                                    long fileTime = BitConverter.ToInt64(temp, 1);
                                    if (fileTime == -1) yield break;
                                    data.TimeStamp = DateTime.FromFileTime(fileTime);
                                    yield return data;
                                }
                                while (index + 9 <= result)
                                {
                                    data.Value.Boolean = BitConverter.ToBoolean(tcpBuffer, index);
                                    index += 1;
                                    long fileTime = BitConverter.ToInt64(tcpBuffer, index);
                                    if (fileTime == -1) yield break;
                                    data.TimeStamp = DateTime.FromFileTime(fileTime);
                                    index += 8;
                                    yield return data;
                                }
                                if (index == result)
                                    index = 0;
                                else
                                {
                                    Array.Copy(tcpBuffer, index, temp, 0, result - index);
                                    index += 9 - result;
                                }
                            } while (result > 0);
                        }
                        break;
                }
            }
        }

        public int SendResetRequest()
        {
            if (_tcpSend != null && _tcpSend.Connected)
            {
                var ipaddr = (_tcpSend.LocalEndPoint as IPEndPoint).Address;
                byte[] resetReq = new byte[6];
                resetReq[0] = FCTCOMMAND.fctHead;
                resetReq[1] = FCTCOMMAND.fctReset;
                ipaddr.GetAddressBytes().CopyTo(resetReq, 2);
                lock (sendasync)
                {
                    return _tcpSend.Send(resetReq);
                }
            }
            return -1;
        }

        public int SendAlarmRequest(DateTime? start, DateTime? end)
        {
            if (_tcpSend != null && _tcpSend.Connected)
            {
                byte[] alarmReq = new byte[18];
                alarmReq[0] = FCTCOMMAND.fctHead;
                alarmReq[1] = FCTCOMMAND.fctAlarmRequest;
                if (start.HasValue)
                {
                    byte[] startbuffer = BitConverter.GetBytes(start.Value.ToFileTime());
                    startbuffer.CopyTo(alarmReq, 2);
                }
                if (end.HasValue)
                {
                    byte[] endbuffer = BitConverter.GetBytes(end.Value.ToFileTime());
                    endbuffer.CopyTo(alarmReq, 10);
                }
                SocketError error;
                lock (sendasync)
                {
                    _tcpSend.Send(alarmReq);
                    _tcpSend.Receive(tcpBuffer, 0, 2, SocketFlags.None, out error);
                }
                return (int)error;
            }
            return -1;
        }

        public ItemData<int> ReadInt32(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            var data = ReadSingleData(address, source);
            return data == null ? new ItemData<int>(0, 0, QUALITIES.QUALITY_BAD) :
                new ItemData<int>(BitConverter.ToInt32(data, 0), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<short> ReadInt16(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            var data = ReadSingleData(address, source);
            return data == null ? new ItemData<short>(0, 0, QUALITIES.QUALITY_BAD) :
                new ItemData<short>(BitConverter.ToInt16(data, 0), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<byte> ReadByte(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            var data = ReadSingleData(address, source);
            return data == null ? new ItemData<byte>(0, 0, QUALITIES.QUALITY_BAD) :
                new ItemData<byte>(data[0], 0, QUALITIES.QUALITY_GOOD);
        }

        public unsafe ItemData<float> ReadFloat(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            var data = ReadSingleData(address, source);
            if (data == null)
                return new ItemData<float>(0.0f, 0, QUALITIES.QUALITY_BAD);
            else
            {
                int value = BitConverter.ToInt32(data, 0);
                return new ItemData<float>(*(((float*)&value)), 0, QUALITIES.QUALITY_GOOD);
            }
        }

        public ItemData<bool> ReadBool(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            var data = ReadSingleData(address, source);
            return data == null ? new ItemData<bool>(false, 0, QUALITIES.QUALITY_BAD) :
                new ItemData<bool>(BitConverter.ToBoolean(data, address.Bit), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<string> ReadString(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            var data = ReadSingleData(address, source);
            return data == null ? new ItemData<string>(string.Empty, 0, QUALITIES.QUALITY_BAD) :
                new ItemData<string>(Encoding.ASCII.GetString(data, 0, Math.Min((int)address.DataSize, 254)).Trim((char)0), 0, QUALITIES.QUALITY_GOOD);
        }

        public int WriteInt32(DeviceAddress address, int value)
        {
            return WriteSingleData(address, BitConverter.GetBytes(value));
        }

        public int WriteInt16(DeviceAddress address, short value)
        {
            return WriteSingleData(address, BitConverter.GetBytes(value));
        }

        public int WriteFloat(DeviceAddress address, float value)
        {
            return WriteSingleData(address, BitConverter.GetBytes(value));
        }

        public int WriteString(DeviceAddress address, string value)
        {
            return WriteSingleData(address, Encoding.ASCII.GetBytes(value));
        }

        public int WriteBit(DeviceAddress address, bool value)
        {
            return WriteSingleData(address, new byte[] { (byte)(value ? 1 : 0) });
        }

        public int WriteBits(DeviceAddress address, byte value)
        {
            return WriteSingleData(address, new byte[] { value });
        }

        public event DataChangeEventHandler DataChange;

        public void Dispose()
        {
            if (_items != null)
            {
                _items.Clear();
            }
            _items = null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using DataService;

namespace DDEDriver
{
    [Description("DDE 通讯协议")]
    public class DDEReader : IDriver
    {
        //DDE用常量定义#region DDE用常量定义
        /**/
        /// <summary>
        /// ///////////////////////////////////////////////////////////////////////////
        /// /***** conversation states (usState) *****/
        /**/
        /// </summary>
        private int dwDDEInst;    // DDE Instance value
        private IntPtr hconvCurrent;
        //private bool bAutoConnect;

        //public string LinkDesc;
        private DdeCallback _Callback;

        short _id;
        public string LinkName;

        public short ID
        {
            get
            {
                return _id;
            }
        }

        string _name;
        public string Name
        {
            get
            {
                return _name;
            }
        }

        string _server;
        public string ServerName
        {
            get { return _server; }
            set { _server = value; }
        }

        string _topic;
        public string Topic
        {
            get
            {
                return _topic;
            }
            set
            {
                _topic = value;
            }
        }

        internal bool _connected;
        public bool IsClosed
        {
            get { return !_connected; }
        }

        int _timeOut;
        public int TimeOut
        {
            get { return _timeOut; }
            set { _timeOut = value; }
        }

        List<IGroup> _groups = new List<IGroup>(3);
        public IEnumerable<IGroup> Groups
        {
            get { return _groups; }
        }

        IDataServer _parent;
        public IDataServer Parent
        {
            get { return _parent; }
        }

        public DDEValueChangeEventHandler DDEValueChange;

        public DDEReader(IDataServer parent, short id, string name, string server, int timeOut = 2000, string topic = null, string spare2 = null)
        {
            _id = id;
            _parent = parent;
            _name = name;
            _server = server;
            _timeOut = timeOut;
            _topic = topic;
            LinkName = _server + "|" + _topic;
            _Callback = DdeClientCallback;
        }

        public unsafe bool Connect()
        {
            IntPtr hConv, hszService, hszTopic;

            if (dwDDEInst != 0)
            {
                Ddeml.DdeUninitialize(dwDDEInst);
                dwDDEInst = 0;
            }
            if (hconvCurrent != IntPtr.Zero)
            {
                Ddeml.DdeDisconnect(hconvCurrent);
                hconvCurrent = IntPtr.Zero;
            };
            //如果是第一次，则什么也不做
            Ddeml.DdeInitialize(ref dwDDEInst, _Callback, 0x3f000, 0);
            // Connect to the server
            hszTopic = Ddeml.DdeCreateStringHandle(dwDDEInst, _topic, Ddeml.CP_WINANSI);
            hszService = Ddeml.DdeCreateStringHandle(dwDDEInst, _server, Ddeml.CP_WINANSI);
            CONVCONTEXT cc = new CONVCONTEXT();
            cc.cb = sizeof(CONVCONTEXT);
            hConv = Ddeml.DdeConnect(dwDDEInst, hszService, hszTopic, ref cc);
            //int DdeErrcode = Win32.DdeGetLastError(dwDDEInst);

            Ddeml.DdeFreeStringHandle(dwDDEInst, hszTopic);
            Ddeml.DdeFreeStringHandle(dwDDEInst, hszService);

            if (hConv != IntPtr.Zero)
            {
                if (hconvCurrent != IntPtr.Zero)
                {
                    Ddeml.DdeDisconnect(hconvCurrent);
                }
                hconvCurrent = hConv;
                _connected = true;
                return true;
            }
            else
            {
                _connected = false;
                return false;
            }
        }

        private IntPtr DdeClientCallback(int uType, ConversionFormat uFmt, IntPtr hConv, IntPtr hsz1, IntPtr hsz2, IntPtr hData, uint dwData1, uint dwData2)
        {
            int dwLength = 0;
            //DateTime time = DateTime.Now;
            unsafe
            {
                switch (uType)
                {
                    case Ddeml.XTYP_ADVDATA:
                        try
                        {
                            sbyte* pData = (sbyte*)Ddeml.DdeAccessData(hData, ref dwLength);
                            if (pData != null)
                            {
                                sbyte* pSZ = stackalloc sbyte[0xFF];
                                Ddeml.DdeQueryString(dwDDEInst, hsz2, pSZ, 0xFF, Ddeml.CP_WINANSI);
                                if (DDEValueChange != null)
                                    DDEValueChange(this, new DDEValueChangeEventArgs(new string(pSZ).ToUpper(), new string(pData, 0, dwLength)));
                            }
                        }
                        catch { }
                        finally
                        {
                            if (hData != IntPtr.Zero) Ddeml.DdeUnaccessData(hData);
                        }
                        break;
                    case Ddeml.XTYP_DISCONNECT:
                        if (OnError != null)
                            OnError(this, new IOErrorEventArgs("XTYP_DISCONNECT"));
                        _connected = false;
                        break;
                    case Ddeml.XTYP_XACT_COMPLETE:
                        break;
                    default:
                        break;
                }
            }
            return new IntPtr(Ddeml.DDE_FACK);
        }

        //与Server建立Hoot连接
        public unsafe bool TransactItem(string name)
        {
            IntPtr hszItem, hDDEData;
            int dwResult = 0;
            if (string.IsNullOrEmpty(name)) return false;
            if (hconvCurrent != IntPtr.Zero)
            {
                hszItem = Ddeml.DdeCreateStringHandle(dwDDEInst, name, Ddeml.CP_WINANSI);

                hDDEData = Ddeml.DdeClientTransaction(null,
                    0,
                    hconvCurrent,
                    hszItem,
                    ConversionFormat.TEXT,//CF_TEXT,
                    Ddeml.XTYP_ADVSTART,
                    Ddeml.TIMEOUT_ASYNC, // ms timeout
                    ref dwResult);

                Ddeml.DdeFreeStringHandle(dwDDEInst, hszItem);

                if (hDDEData != IntPtr.Zero)
                {
                    try
                    {
                        int dwLength = 0;
                        sbyte* pData = (sbyte*)Ddeml.DdeAccessData(hDDEData, ref dwLength);
                        if ((pData != null) && (dwLength != 0))
                        {
                            if (DDEValueChange != null)
                                DDEValueChange(this, new DDEValueChangeEventArgs(name, new string(pData, 0, dwLength)));
                        }
                    }
                    catch { }
                    finally
                    {
                        Ddeml.DdeUnaccessData(hDDEData);
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
                return false;
        }

        public int Execute(string command)
        {
            int res = 0;
            byte[] data = Encoding.ASCII.GetBytes(command);
            Ddeml.DdeClientTransaction(data, data.Length, hconvCurrent, IntPtr.Zero, ConversionFormat.NONE, Ddeml.XTYP_EXECUTE, _timeOut, ref res);
            return res;
        }

        //必须设置item.ItemName和item.ItemValue后调用
        public int Poke(string name, string value)
        {
            IntPtr hszItem, hDDEData;
            int dwResult = 0;
            if (string.IsNullOrEmpty(name)) return -2;
            if (hconvCurrent != IntPtr.Zero)
            {
                hszItem = Ddeml.DdeCreateStringHandle(dwDDEInst, name, Ddeml.CP_WINANSI);
                if (!string.IsNullOrEmpty(name))
                {
                    //int errcode=Win32.DdeGetLastError(dwDDEInst);
                    byte[] ByteArray = Encoding.ASCII.GetBytes(value);
                    IntPtr hd = Ddeml.DdeCreateDataHandle(dwDDEInst, ByteArray, ByteArray.Length, 0, hszItem, ConversionFormat.TEXT, 0);
                    hDDEData = Ddeml.DdeClientTransaction(
                                                    ByteArray, ByteArray.Length,
                                                    hconvCurrent, hszItem,
                                                    ConversionFormat.TEXT,//CF_TEXT,
                                                    Ddeml.XTYP_POKE,
                                                     Ddeml.TIMEOUT_ASYNC, // ms timeout
                                                    ref dwResult);
                    Ddeml.DdeFreeStringHandle(dwDDEInst, hszItem);

                    if (hDDEData != IntPtr.Zero)
                    {
                        return 1;
                    }
                }
                return -1;
            }
            else
                return -1;
        }

        //  [10/24/2003]
        //主动请求Item的值
        public unsafe string Request(string name)
        {
            IntPtr hszItem, hDDEData;
            int dwResult = 0;
            int dwLength = 0;
            if (name != null && hconvCurrent != IntPtr.Zero)
            {
                hszItem = Ddeml.DdeCreateStringHandle(dwDDEInst, name, Ddeml.CP_WINANSI);

                hDDEData = Ddeml.DdeClientTransaction(null,
                    0,
                    hconvCurrent,
                    hszItem,
                    ConversionFormat.TEXT,//CF_TEXT,
                    (int)Ddeml.XTYP_REQUEST,
                    5000, // ms timeout
                    ref dwResult);
                Ddeml.DdeFreeStringHandle(dwDDEInst, hszItem);

                if (hDDEData != IntPtr.Zero)
                {
                    sbyte* pData = (sbyte*)Ddeml.DdeAccessData(hDDEData, ref dwLength);
                    {
                        if ((pData != null) && (dwLength != 0))
                        {
                            return new string(pData, 0, dwLength);
                        }
                    }
                }
            }
            return null;
        }


        public IGroup AddGroup(string name, short id, int updateRate, float deadBand = 0f, bool active = false)
        {
            if (_groups.Count > 0)
                return null;
            DDEGroup grp = new DDEGroup(id, name, updateRate, active, this);
            _groups.Add(grp);
            return grp;
        }

        public bool RemoveGroup(IGroup grp)
        {
            grp.IsActive = false;
            return _groups.Remove(grp);
        }

        public void Dispose()
        {
            foreach (IGroup grp in _groups)
            {
                grp.Dispose();
            }
            _groups.Clear();
            Ddeml.DdeDisconnect(hconvCurrent);
        }

        public event IOErrorEventHandler OnError;
    }

    public class DDEGroup : IGroup
    {
        bool _isActive;
        public bool IsActive
        {
            get
            {
                return _isActive;
            }
            set
            {
                _isActive = value;
            }
        }

        short _id;
        public short ID
        {
            get
            {
                return _id;
            }
        }

        int _updateRate;
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

        string _name;
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

        float deadband;
        public float DeadBand
        {
            get
            {
                return deadband;
            }
            set
            {
                deadband = value;
            }
        }

        DDEReader _plcReader;
        public IDriver Parent
        {
            get
            {
                return _plcReader;
            }
        }


        List<short> _changedList;
        public List<short> ChangedList
        {
            get
            {
                return _changedList;
            }
        }


        List<ITag> _items;
        public IEnumerable<ITag> Items
        {
            get { return _items; }
        }

        IDataServer _server;
        public IDataServer Server
        {
            get
            {
                return _server;
            }
        }

        Dictionary<short, string> _mapping;

        public DDEGroup(short id, string address, int updateRate, bool active, DDEReader plcReader)
        {
            this._id = id;
            this._updateRate = updateRate;
            this._isActive = active;
            this._plcReader = plcReader;
            this._name = address;
            this._server = _plcReader.Parent;
            this._changedList = new List<short>(100);
            plcReader.DDEValueChange += new DDEValueChangeEventHandler(plcReader_DDEValueChange);
        }

        void plcReader_DDEValueChange(object sender, DDEValueChangeEventArgs e)
        {
            string name = e.Name;
            string data = e.Data;
            ITag tag = _server[name];
            if (tag != null)
            {
                Storage value = Storage.Empty;
                switch (tag.Address.VarType)
                {
                    case DataType.BOOL:
                        value.Boolean = bool.Parse(data);
                        break;
                    case DataType.BYTE:
                        value.Byte = byte.Parse(data);
                        break;
                    case DataType.WORD:
                    case DataType.SHORT:
                        value.Int16 = short.Parse(data);
                        break;
                    case DataType.INT:
                        value.Int32 = int.Parse(data);
                        break;
                    case DataType.STR:
                        var stag = tag as StringTag;
                        if (stag != null)
                        {
                            stag.String = data;
                        }
                        break;
                }
                tag.Update(value, DateTime.Now, QUALITIES.QUALITY_GOOD);
                if (DataChange != null)
                    DataChange(this, new DataChangeEventArgs(1, new HistoryData[]{
                        new HistoryData(tag.ID,QUALITIES.QUALITY_GOOD,tag.Value,tag.TimeStamp)}));
            }
        }


        public bool AddItems(IList<TagMetaData> items)
        {
            int count = items.Count;
            if (_items == null)
            {
                _items = new List<ITag>(count);
                _mapping = new Dictionary<short, string>(count);
            }
            lock (_server.SyncRoot)
            {
                for (int i = 0; i < count; i++)
                {
                    ITag dataItem = null;
                    TagMetaData meta = items[i];
                    if (meta.GroupID == this._id)
                    {
                        DeviceAddress addr = new DeviceAddress { Start = meta.ID, DataSize = meta.Size, VarType = meta.DataType };
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
                            case DataType.DWORD:
                            case DataType.INT:
                                dataItem = new IntTag(meta.ID, addr, this);
                                break;
                            case DataType.FLOAT:
                                dataItem = new FloatTag(meta.ID, addr, this);
                                break;
                            case DataType.STR:
                                dataItem = new StringTag(meta.ID, addr, this);
                                break;
                        }
                        if (dataItem != null)
                        {
                            //dataItem.Active = meta.Active;
                            _items.Add(dataItem);
                            _plcReader.TransactItem(meta.Name);
                            _mapping.Add(meta.ID, meta.Address);
                            _server.AddItemIndex(meta.Name, dataItem);
                        }
                    }
                }
                return true;
            }
        }

        public bool AddTags(IEnumerable<ITag> tags)
        {
            if (_items == null)
            {
                _items = new List<ITag>();
            }
            foreach (ITag tag in tags)
            {
                if (tag != null)
                {
                    _items.Add(tag);
                }
            }
            return true;
        }

        public bool RemoveItems(params ITag[] items)
        {
            foreach (var item in items)
            {
                _server.RemoveItemIndex(item.GetTagName());
                _mapping.Remove(item.ID);
                _items.Remove(item);
            }
            return true;
        }

        public bool SetActiveState(bool active, params short[] items)
        {
            return true;
        }

        public ITag FindItemByAddress(DeviceAddress addr)
        {
            return null;
        }

        public ITag GetItemByID(short id)
        {
            return _server[id];
        }

        public HistoryData[] BatchRead(DataSource source, bool isSync, params ITag[] itemArray)
        {
            return ExtMethods.BatchRead(source, itemArray);
        }

        public int BatchWrite(SortedDictionary<ITag, object> items, bool isSync = true)
        {
            int rev = ExtMethods.BatchWrite(items);
            if (DataChange != null && rev >= 0)
            {
                int len = items.Count;
                HistoryData[] data = new HistoryData[len];
                int i = 0;
                foreach (var item in items)
                {
                    ITag tag = item.Key;
                    data[i].ID = tag.ID;
                    data[i].TimeStamp = tag.TimeStamp;
                    data[i].Quality = tag.Quality;
                    //data[i].Value = item.Value.ToStorge();
                    i++;
                }
                foreach (DataChangeEventHandler deleg in DataChange.GetInvocationList())
                {
                    deleg.BeginInvoke(this, new DataChangeEventArgs(1, data), null, null);
                }
            }
            return rev;
        }

        public ItemData<int> ReadInt32(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            string data = _plcReader.Request(_mapping[(short)address.Start]);
            return string.IsNullOrEmpty(data) ? new ItemData<int>(0, 0, QUALITIES.QUALITY_BAD) :
            new ItemData<int>(int.Parse(data), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<short> ReadInt16(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            string data = _plcReader.Request(_mapping[(short)address.Start]);
            return string.IsNullOrEmpty(data) ? new ItemData<short>(0, 0, QUALITIES.QUALITY_BAD) :
            new ItemData<short>(short.Parse(data), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<uint> ReadUInt32(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            string data = _plcReader.Request(_mapping[(short)address.Start]);
            return string.IsNullOrEmpty(data) ? new ItemData<uint>(0, 0, QUALITIES.QUALITY_BAD) :
            new ItemData<uint>(uint.Parse(data), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<ushort> ReadUInt16(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            string data = _plcReader.Request(_mapping[(short)address.Start]);
            return string.IsNullOrEmpty(data) ? new ItemData<ushort>(0, 0, QUALITIES.QUALITY_BAD) :
            new ItemData<ushort>(ushort.Parse(data), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<byte> ReadByte(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            string data = _plcReader.Request(_mapping[(short)address.Start]);
            return string.IsNullOrEmpty(data) ? new ItemData<byte>(0, 0, QUALITIES.QUALITY_BAD) :
            new ItemData<byte>(byte.Parse(data), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<float> ReadFloat(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            string data = _plcReader.Request(_mapping[(short)address.Start]);
            return string.IsNullOrEmpty(data) ? new ItemData<float>(0, 0, QUALITIES.QUALITY_BAD) :
            new ItemData<float>(float.Parse(data), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<bool> ReadBool(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            string data = _plcReader.Request(_mapping[(short)address.Start]);
            return string.IsNullOrEmpty(data) ? new ItemData<bool>(false, 0, QUALITIES.QUALITY_BAD) :
            new ItemData<bool>(bool.Parse(data), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<string> ReadString(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            string data = _plcReader.Request(_mapping[(short)address.Start]);
            return string.IsNullOrEmpty(data) ? new ItemData<string>(null, 0, QUALITIES.QUALITY_BAD) :
            new ItemData<string>(data, 0, QUALITIES.QUALITY_GOOD);
        }

        public int WriteInt32(DeviceAddress address, int value)
        {
            return _plcReader.Poke(_mapping[(short)address.Start], value.ToString());
        }

        public int WriteInt16(DeviceAddress address, short value)
        {
            return _plcReader.Poke(_mapping[(short)address.Start], value.ToString());
        }

        public int WriteUInt32(DeviceAddress address, uint value)
        {
            return _plcReader.Poke(_mapping[(short)address.Start], value.ToString());
        }

        public int WriteUInt16(DeviceAddress address, ushort value)
        {
            return _plcReader.Poke(_mapping[(short)address.Start], value.ToString());
        }

        public int WriteFloat(DeviceAddress address, float value)
        {
            return _plcReader.Poke(_mapping[(short)address.Start], value.ToString());
        }

        public int WriteString(DeviceAddress address, string value)
        {
            return _plcReader.Poke(_mapping[(short)address.Start], value);
        }

        public int WriteBit(DeviceAddress address, bool value)
        {
            return _plcReader.Poke(_mapping[(short)address.Start], value.ToString());
        }

        public int WriteBits(DeviceAddress address, byte value)
        {
            return _plcReader.Poke(_mapping[(short)address.Start], value.ToString());
        }

        public event DataChangeEventHandler DataChange;

        public void Dispose()
        {
            _plcReader.DDEValueChange -= new DDEValueChangeEventHandler(plcReader_DDEValueChange);
            _items.Clear();
            _mapping.Clear();
            _items = null;
            _mapping = null;
        }
    }

    public delegate void DDEValueChangeEventHandler(object sender, DDEValueChangeEventArgs e);

    public class DDEValueChangeEventArgs : EventArgs
    {
        public DDEValueChangeEventArgs(string name, string data)
        {
            this.Name = name;
            this.Data = data;
        }

        public string Name;
        public string Data;
    }
}

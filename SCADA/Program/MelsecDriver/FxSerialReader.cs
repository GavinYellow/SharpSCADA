using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Timers;
using DataService;
using HslCommunication.Profinet.Melsec;

namespace MelsecDriver
{
    [Description("三菱Fx系列串口")]
    //ModbusRTUReader : IPLCDriver         IPLCDriver : IDriver, IReaderWriter              IDriver : IDisposable
    public sealed class FxSerialReader : IPLCDriver
    {
        #region : IDisposable

        public void Dispose()
        {
            foreach (IGroup grp in _grps)
            {
                grp.Dispose();
            }

            _grps.Clear();
            _fxSerial.Close();
        }

        #endregion

        //从站地址
        short _id;

        public short ID
        {
            get { return _id; }
        }

        string _name;

        public string Name
        {
            get { return _name; }
        }

        private string _serverName = "unknown";

        public string ServerName
        {
            get { return _serverName; }
            set { _serverName = value; }
        }

        public bool IsClosed
        {
            get { return _fxSerial == null || _fxSerial.IsOpen == false; }
        }

        IDataServer _server;

        public IDataServer Parent
        {
            get { return _server; }
        }


        private string _port = "COM1";

        [Category("串口设置"), Description("串口号")]
        public string PortName
        {
            get { return _port; }
            set { _port = value; }
        }

        private int _timeOut = 3000;

        [Category("串口设置"), Description("通迅超时时间")]
        public int TimeOut
        {
            get { return _timeOut; }
            set { _timeOut = value; }
        }

        private int _baudRate = 9600;

        [Category("串口设置"), Description("波特率")]
        public int BaudRate
        {
            get { return _baudRate; }
            set { _baudRate = value; }
        }

        private int _dataBits = 8;

        [Category("串口设置"), Description("数据位")]
        public int DataBits
        {
            get { return _dataBits; }
            set { _dataBits = value; }
        }

        private StopBits _stopBits = StopBits.One;

        [Category("串口设置"), Description("停止位")]
        public StopBits StopBits
        {
            get { return _stopBits; }
            set { _stopBits = value; }
        }

        private Parity _parity = Parity.None;

        [Category("串口设置"), Description("奇偶校验")]
        public Parity Parity
        {
            get { return _parity; }
            set { _parity = value; }
        }

        private MelsecFxSerial _fxSerial;

        public bool Connect()
        {
            _fxSerial = new MelsecFxSerial();
            try
            {
                _fxSerial.SerialPortInni(sp =>
                {
                    sp.PortName = _port;
                    sp.BaudRate = _baudRate;
                    sp.DataBits = _dataBits;
                    sp.StopBits = _stopBits;
                    sp.Parity = _parity;
                });
                _fxSerial.Open();

                return true;
            }
            catch (Exception ex)
            {
                if (OnError != null)
                    OnError(this, new IOErrorEventArgs(ex.Message));
                return false;
            }
        }

        List<IGroup> _grps = new List<IGroup>();

        public event IOErrorEventHandler OnError;

        public IEnumerable<IGroup> Groups
        {
            get { return _grps; }
        }

        public IGroup AddGroup(string name, short id, int updateRate, float deadBand = 0, bool active = false)
        {
            FxShortGroup grp = new FxShortGroup(id, name, updateRate, active, this);
            _grps.Add(grp);
            return grp;
        }

        public bool RemoveGroup(IGroup grp)
        {
            grp.IsActive = false;
            return _grps.Remove(grp);
        }

        //自定义构造函数3
        public FxSerialReader(IDataServer server, short id, string name)
        {
            _id = id;
            _name = name;
            _server = server;
        }

        object _async = new object();


        public byte[] ReadBytes(DeviceAddress address, ushort size)
        {
            string addr;
            addr = GetAddress(address);
            try
            {
                lock (_async)
                {
                    if (address.DBNumber < 3)
                    {
                        var read = _fxSerial.ReadBool(addr, (ushort) (size * 16));
                        if (read.IsSuccess)
                        {
                            byte[] retBytes = new byte[size * 2];
                            Buffer.BlockCopy(read.Content,0,retBytes,0,size * 2);
                            return retBytes;
                        }
                        else
                        {
                            if (OnError != null)
                                OnError(this, new IOErrorEventArgs(read.Message));
                            return null;
                        }
                    }
                    else
                    {
                        var read = _fxSerial.Read(addr, size);
                        if (read.IsSuccess)
                        {
                            return read.Content;
                        }
                        else
                        {
                            if (OnError != null)
                                OnError(this, new IOErrorEventArgs(read.Message));
                            return null;
                        }
                    }
                    
                }
            }
            catch (Exception e)
            {
                if (OnError != null)
                    OnError(this, new IOErrorEventArgs(e.Message));
                return null;
            }
        }

        public ItemData<uint> ReadUInt32(DeviceAddress address)
        {
            byte[] bit = ReadBytes(address, 2);
            return bit == null ? new ItemData<uint>(0, 0, QUALITIES.QUALITY_BAD) :
                new ItemData<uint>(BitConverter.ToUInt32(bit, 0), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<int> ReadInt32(DeviceAddress address)
        {
            byte[] bit = ReadBytes(address, 2);
            return bit == null ? new ItemData<int>(0, 0, QUALITIES.QUALITY_BAD) :
                new ItemData<int>(BitConverter.ToInt32(bit, 0), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<ushort> ReadUInt16(DeviceAddress address)
        {
            byte[] bit = ReadBytes(address, 1);
            return bit == null ? new ItemData<ushort>(0, 0, QUALITIES.QUALITY_BAD) :
                new ItemData<ushort>(BitConverter.ToUInt16(bit, 0), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<short> ReadInt16(DeviceAddress address)
        {
            byte[] bit = ReadBytes(address, 1);
            return bit == null ? new ItemData<short>(0, 0, QUALITIES.QUALITY_BAD) :
                new ItemData<short>(BitConverter.ToInt16(bit, 0), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<byte> ReadByte(DeviceAddress address)
        {
            byte[] bit = ReadBytes(address, 1);
            return bit == null ? new ItemData<byte>(0, 0, QUALITIES.QUALITY_BAD) :
                new ItemData<byte>(bit[0], 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<string> ReadString(DeviceAddress address, ushort size)
        {
            byte[] bit = ReadBytes(address, size);
            return bit == null ? new ItemData<string>(string.Empty, 0, QUALITIES.QUALITY_BAD) :
                new ItemData<string>(Encoding.ASCII.GetString(bit), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<float> ReadFloat(DeviceAddress address)
        {
            byte[] bit = ReadBytes(address, 2);
            return bit == null ? new ItemData<float>(0f, 0, QUALITIES.QUALITY_BAD) :
                new ItemData<float>(BitConverter.ToSingle(bit, 0), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<bool> ReadBit(DeviceAddress address)
        {
            byte[] bit = ReadBytes(address, 1);
            return bit == null ? new ItemData<bool>(false, 0, QUALITIES.QUALITY_BAD) :
                new ItemData<bool>((bit[0] & (1 << (address.Bit))) > 0, 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<object> ReadValue(DeviceAddress address)
        {
            return this.ReadValueEx(address);
        }

        public int WriteBytes(DeviceAddress address, byte[] bit)
        {
            return Writes(address, bit);
        }

        public int WriteBit(DeviceAddress address, bool bit)
        {
            try
            {
                lock (_async)
                {

                    string addr = GetAddress(address);
                    var ret = _fxSerial.Write(addr, bit);
                    if (ret.IsSuccess)
                    {
                        return 0;
                    }
                    else
                    {
                        if (OnError != null)
                        {
                            OnError(this, new IOErrorEventArgs(ret.Message));
                        }

                        return -1;
                    }
                }
            }
            catch (Exception e)
            {
                if (OnError != null)
                {
                    OnError(this, new IOErrorEventArgs(e.Message));
                }

                return -1;
            }

        }

        public int WriteBits(DeviceAddress address, byte bits)
        {
            return Writes(address, new byte[] {bits});
        }

        public int WriteInt16(DeviceAddress address, short value)
        {
            return Writes(address, BitConverter.GetBytes(value));
        }

        public int WriteUInt16(DeviceAddress address, ushort value)
        {
            return Writes(address, BitConverter.GetBytes(value));
        }

        public int WriteInt32(DeviceAddress address, int value)
        {
            return Writes(address, BitConverter.GetBytes(value)); 
        }

        public int WriteUInt32(DeviceAddress address, uint value)
        {
            return Writes(address, BitConverter.GetBytes(value));
        }

        public int WriteFloat(DeviceAddress address, float value)
        {
            return Writes(address, BitConverter.GetBytes(value));
        }

        public int WriteString(DeviceAddress address, string str)
        {
            return Writes(address, Encoding.ASCII.GetBytes(str));
        }

        private int Writes(DeviceAddress address, byte[] bytes)
        {
            try
            {
                lock (_async)
                {
                    string addr = GetAddress(address);
                    var ret = _fxSerial.Write(addr, bytes);
                    if (ret.IsSuccess)
                    {
                        return 0;
                    }
                    else
                    {
                        if (OnError != null)
                        {
                            OnError(this, new IOErrorEventArgs(ret.Message));
                        }
                        return -1;
                    }
                }
            }
            catch (Exception e)
            {
                if (OnError != null)
                {
                    OnError(this, new IOErrorEventArgs(e.Message));
                }
                return -1;
            }
            
        }


        public int WriteValue(DeviceAddress address, object value)
        {
            return this.WriteValueEx(address, value);
        }

        public int PDU { get; }

        //将三菱PLC的地址解析为modbus地址格式，参考bcnet的地址解析方式
        //T/C暂存器分为bool类型和word类型，采用地址的第二码区分，第二码A表示bool，第二码为B表示word, 默认则为读取word值
        private string ParseAddress(TagMetaData meta)
        {
            string address = meta.Address;
            int m, n;
            string newaddress = "";
            switch (address[0])
            {
                case 'Y':
                case 'y':
                    int.TryParse(address.Substring(1, address.Length - 2), out m);
                    int.TryParse(address.Substring(address.Length - 1), out n);
                    newaddress = '0' + (m * 8 + n).ToString().PadLeft(5, '0');
                    break;
                case 'M':
                case 'm':
                    int.TryParse(address.Substring(1, address.Length - 1), out m);
                    if (m >= 8000)
                    {
                        newaddress = '0' + (5001 + (m - 8000)).ToString().PadLeft(5, '0');
                    }
                    else
                    {
                        newaddress = '0' + (10001 + m).ToString().PadLeft(5, '0');
                    }
                    break;
                case 'T':
                case 't':
                    if (meta.DataType == DataType.BOOL)
                    {
                        int.TryParse(address.Substring(1, address.Length - 1), out m);
                        newaddress = '0' + (6001 + m).ToString().PadLeft(5, '0');
                    }
                    else
                    {
                        int.TryParse(address.Substring(1, address.Length - 1), out m);
                        newaddress = '4' + (1 + m).ToString().PadLeft(5, '0');
                    }
                    break;
                case 'C':
                case 'c':
                    if (meta.DataType == DataType.BOOL)
                    {
                        int.TryParse(address.Substring(1, address.Length - 1), out m);
                        newaddress = '0' + (7001 + m).ToString().PadLeft(5, '0');
                    }
                    else
                    {
                        int.TryParse(address.Substring(1, address.Length - 1), out m);
                        newaddress = '4' + (1001 + m).ToString().PadLeft(5, '0');
                    }
                    break;
                case 'S':
                case 's':
                    int.TryParse(address.Substring(1, address.Length - 1), out m);
                    newaddress = '0' + (30001 + m).ToString().PadLeft(5, '0');
                    break;
                case 'X':
                case 'x':
                    int.TryParse(address.Substring(1, address.Length - 2), out m);
                    int.TryParse(address.Substring(address.Length - 1), out n);
                    newaddress = '1' + (1 + m * 8 + n).ToString().PadLeft(5, '0');
                    break;
                case 'D':
                case 'd':
                    int.TryParse(address.Substring(1, address.Length - 1), out m);
                    if (m >= 8000)
                    {
                        newaddress = '4' + (1301 + m - 8000).ToString().PadLeft(5, '0');
                    }
                    else
                    {
                        newaddress = '4' + (2001 + m).ToString().PadLeft(5, '0');
                    }
                    break;

            }
            return newaddress;
        }


        public DeviceAddress GetDeviceAddress(string address)
        {
            DeviceAddress dv = DeviceAddress.Empty;

            if (string.IsNullOrEmpty(address))
                return dv;
            
            if (string.IsNullOrEmpty(address))
                return dv;

            switch (address[0])
            {
                case '0':
                    {
                        dv.DBNumber = Fx.fctReadCoil;
                        int st;
                        int.TryParse(address, out st);
                        st--;
                        dv.Bit = (byte)(st % 16);
                        st /= 16;
                        dv.Start = st;
                        //dv.Bit--;
                    }
                    break;
                case '1':
                    {
                        dv.DBNumber = Fx.fctReadDiscreteInputs;
                        int st;
                        int.TryParse(address.Substring(1), out st);
                        st--;
                        dv.Bit = (byte)(st % 16);
                        st /= 16;
                        dv.Start = st;
                        //dv.Bit--;
                    }
                    break;
                case '4':
                    {
                        int index = address.IndexOf('.');
                        dv.DBNumber = Fx.fctReadHoldingRegister;
                        if (index > 0)
                        {
                            dv.Start = int.Parse(address.Substring(1, index - 1));
                            dv.Bit = byte.Parse(address.Substring(index + 1));
                        }
                        else
                            dv.Start = int.Parse(address.Substring(1));
                        dv.Start--;
                        dv.Bit--;
                        dv.ByteOrder = ByteOrder.BigEndian;
                    }
                    break;
                case '3':
                    
                    break;
            }
            return dv;
        }

        public DeviceAddress GetDeviceAddress(TagMetaData meta)
        {
            string newaddress = ParseAddress(meta);
            return GetDeviceAddress(newaddress);
        }

        public string GetAddress(DeviceAddress dv)
        {
            string address = "", newaddress = "";
            int addr;
            switch (dv.DBNumber)
            {
                case Fx.fctReadCoil:
                    address = '0' + (dv.Start * 16 + dv.Bit + 1).ToString().PadLeft(5, '0');

                    int.TryParse(address, out addr);
                    if (addr >= 30001)
                    {
                        newaddress = "S" + (addr - 30001).ToString();
                    }
                    else if (addr >= 10001)
                    {
                        newaddress = "M" + (addr - 10001).ToString();
                    }
                    else if (addr >= 7001)
                    {
                        newaddress = "C" + (addr - 7001).ToString();
                    }
                    else if (addr >= 6001)
                    {
                        newaddress = "T" + (addr - 6001).ToString();
                    }
                    else if (addr >= 5001)
                    {
                        newaddress = "M8" + (addr - 5001).ToString().PadLeft(3, '0');
                    }
                    else
                    {
                        newaddress = "Y" + Convert.ToString(addr - 1, 8);
                    }
                    break;
                case Fx.fctReadDiscreteInputs:
                    address = "1" + (dv.Start * 16 + dv.Bit + 1).ToString().PadLeft(5, '0');
                    int.TryParse(address, out addr);
                    newaddress = "X" + Convert.ToString(addr - 100001, 8);
                    break;
                case Fx.fctReadHoldingRegister:
                    address = "4" + (dv.Start * 16 + 1).ToString().PadLeft(5, '0');
                    int.TryParse(address, out addr);
                    if (addr >= 420001)
                    {
                        newaddress = "R" + (addr - 420001).ToString();
                    }
                    else if (addr >= 402001)
                    {
                        newaddress = "D" + (addr - 402001).ToString();
                    }
                    else if (addr >= 401301)
                    {
                        newaddress = "D8" + (addr - 401301).ToString().PadLeft(3, '0');
                    }
                    else if (addr >= 401001)
                    {
                        newaddress = "C" + (addr - 401001).ToString();
                    }
                    else if (addr >= 400001)
                    {
                        newaddress = "T" + (addr - 400001).ToString();
                    }
                    break;
                case Fx.fctReadInputRegister:

                    break;
            }
            return newaddress;
        }
    }

    public sealed class Fx
    {
        public const byte fctReadCoil = 1;
        public const byte fctReadDiscreteInputs = 2;
        public const byte fctReadHoldingRegister = 3;
        public const byte fctReadInputRegister = 4;
        public const byte fctWriteSingleCoil = 5;
        public const byte fctWriteSingleRegister = 6;
        public const byte fctWriteMultipleCoils = 15;
        public const byte fctWriteMultipleRegister = 16;
        public const byte fctReadWriteMultipleRegister = 23;
    }


    public class FxShortGroup : IGroup
    {
        protected Timer _timer;

        protected bool _isActive;
        public virtual bool IsActive
        {
            get
            {
                return _isActive;
            }
            set
            {
                _isActive = value;
                if (value)
                {
                    if (_updateRate <= 0) _updateRate = 100;
                    _timer.Interval = _updateRate;
                    _timer.Elapsed += new ElapsedEventHandler(timer_Timer);
                    _timer.Start();
                }
                else
                {
                    _timer.Elapsed -= new ElapsedEventHandler(timer_Timer);
                    _timer.Stop();
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


        protected ICache _cacheReader;

        protected IPLCDriver _plcReader;
        public IDriver Parent
        {
            get
            {
                return _plcReader;
            }
        }

        protected List<int> _changedList;
        public List<int> ChangedList
        {
            get
            {
                return _changedList;
            }
        }


        protected List<ITag> _items;
        public IEnumerable<ITag> Items
        {
            get { return _items; }
        }

        protected IDataServer _server;
        public IDataServer Server
        {
            get
            {
                return _server;
            }
        }

        protected List<PDUArea> _rangeList = new List<PDUArea>();

        protected FxShortGroup()
        {
        }

        public FxShortGroup(short id, string name, int updateRate, bool active, IPLCDriver plcReader)
        {
            this._id = id;
            this._name = name;
            this._updateRate = updateRate;
            this._isActive = active;
            this._plcReader = plcReader;
            this._server = _plcReader.Parent;
            this._timer = new Timer();
            this._changedList = new List<int>();
            this._cacheReader = new ShortCacheReader();
        }

        public bool AddItems(IList<TagMetaData> items)
        {
            FxSerialReader fxReader = _plcReader as FxSerialReader;
            if (fxReader == null) return false;


            int count = items.Count;
            if (_items == null) _items = new List<ITag>(count);
            lock (_server.SyncRoot)
            {
                for (int i = 0; i < count; i++)
                {
                    ITag dataItem = null;
                    TagMetaData meta = items[i];
                    if (meta.GroupID == this._id)
                    {
                        DeviceAddress addr = fxReader.GetDeviceAddress(meta);
                        if (addr.DataSize == 0) addr.DataSize = meta.Size;
                        if (addr.VarType == DataType.NONE) addr.VarType = meta.DataType;
                        if (addr.VarType != DataType.BOOL) addr.Bit = 0;
                        switch (meta.DataType)
                        {
                            case DataType.BOOL:
                                dataItem = new BoolTag(meta.ID, addr, this);
                                break;
                            case DataType.BYTE:
                                dataItem = new ByteTag(meta.ID, addr, this);
                                break;
                            case DataType.WORD:
                                dataItem = new UShortTag(meta.ID, addr, this);
                                break;
                            case DataType.SHORT:
                                dataItem = new ShortTag(meta.ID, addr, this);
                                break;
                            case DataType.DWORD:
                                dataItem = new UIntTag(meta.ID, addr, this);
                                break;
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
                            _server.AddItemIndex(meta.Name, dataItem);
                        }
                    }
                }
            }
            _items.TrimExcess();
            _items.Sort((x, y) => x.Address.CompareTo(y.Address));
            UpdatePDUArea();
            return true;
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
            _items.TrimExcess();
            _items.Sort((x, y) => x.Address.CompareTo(y.Address));
            UpdatePDUArea();
            return true;
        }

        public bool RemoveItems(params ITag[] items)
        {
            foreach (var item in items)
            {
                _server.RemoveItemIndex(item.GetTagName());
                _items.Remove(item);
            }
            UpdatePDUArea();
            return true;
        }

        protected void UpdatePDUArea()
        {
            int count = _items.Count;
            if (count > 0)
            {
                DeviceAddress _start = _items[0].Address;
                _start.Bit = 0;
                int bitCount = _cacheReader.ByteCount;
                if (count > 1)
                {
                    int cacheLength = 0;//缓冲区的大小
                    int cacheIndexStart = 0;
                    int startIndex = 0;
                    DeviceAddress segmentEnd = DeviceAddress.Empty;
                    DeviceAddress tagAddress = DeviceAddress.Empty;
                    DeviceAddress segmentStart = _start;
                    for (int j = 1, i = 1; i < count; i++, j++)
                    {
                        tagAddress = _items[i].Address;//当前变量地址 
                        int offset1 = _cacheReader.GetOffset(tagAddress, segmentStart);
                        if (offset1 > (_plcReader.PDU - tagAddress.DataSize) / bitCount)
                        {
                            segmentEnd = _items[i - 1].Address;
                            int len = _cacheReader.GetOffset(segmentEnd, segmentStart);
                            len += segmentEnd.DataSize <= bitCount ? 1 : segmentEnd.DataSize / bitCount;
                            tagAddress.CacheIndex = (ushort)(cacheIndexStart + len);
                            _items[i].Address = tagAddress;
                            _rangeList.Add(new PDUArea(segmentStart, len, startIndex, j));
                            startIndex += j; j = 0;
                            cacheLength += len;//更新缓存长度
                            cacheIndexStart = cacheLength;
                            segmentStart = tagAddress;//更新数据片段的起始地址
                        }
                        else
                        {
                            tagAddress.CacheIndex = (ushort)(cacheIndexStart + offset1);
                            _items[i].Address = tagAddress;
                        }
                        if (i == count - 1)
                        {
                            segmentEnd = _items[i].Address;
                            int segmentLength = _cacheReader.GetOffset(segmentEnd, segmentStart);
                            if (segmentLength > (_plcReader.PDU - segmentEnd.DataSize) / bitCount)
                            {
                                segmentEnd = _items[i - 1].Address;
                                segmentLength = segmentEnd.DataSize <= bitCount ? 1 : segmentEnd.DataSize / bitCount;
                            }
                            tagAddress.CacheIndex = (ushort)(cacheIndexStart + segmentLength);
                            _items[i].Address = tagAddress;
                            segmentLength += segmentEnd.DataSize <= bitCount ? 1 : segmentEnd.DataSize / bitCount;
                            _rangeList.Add(new PDUArea(segmentStart, segmentLength, startIndex, j + 1));
                            cacheLength += segmentLength;
                        }
                    }
                    _cacheReader.Size = cacheLength;
                }
                else
                {
                    var size = _start.DataSize <= bitCount ? 1 : _start.DataSize / bitCount;
                    _rangeList.Add(new PDUArea(_start, size, 0, 1));
                    _cacheReader.Size = size;//改变Cache的Size属性值将创建Cache的内存区域
                }
            }
        }

        public ITag FindItemByAddress(DeviceAddress addr)
        {
            int index = _items.BinarySearch(new BoolTag(0, addr, null));
            return index < 0 ? null : _items[index];
        }

        public bool SetActiveState(bool active, params short[] items)
        {
            return false;
        }

        object sync = new object();

        public void Flush()
        {
            lock (sync)
            {
                _changedList.Clear();
                Poll();
                if (_changedList.Count > 0)
                    Update();
            }
        }

        protected void timer_Timer(object sender, EventArgs e)
        {
            //Modified Kevin
            if (_isActive && !_plcReader.IsClosed)
            {
                lock (sync)
                {
                    _changedList.Clear();
                    Poll();
                    if (_changedList.Count > 0)
                        Update();
                }
            }
            else
                return;
        }

        protected virtual unsafe void Poll()
        {
            short[] cache = (short[])_cacheReader.Cache;
            int k = 0;
            foreach (PDUArea area in _rangeList)
            {
                byte[] rcvBytes = _plcReader.ReadBytes(area.Start, (ushort)area.Len);//从PLC读取数据  
                if (rcvBytes == null)
                {
                    k += (area.Len + 1) / 2;
                    continue;
                }
                else
                {
                    int len = rcvBytes.Length / 2;
                    fixed (byte* p1 = rcvBytes)
                    {
                        short* prcv = (short*)p1;
                        int index = area.StartIndex;//index指向_items中的Tag元数据
                        int count = index + area.Count;
                        while (index < count)
                        {
                            DeviceAddress addr = _items[index].Address;
                            int iShort = addr.CacheIndex;
                            int iShort1 = iShort - k;
                            if (addr.VarType == DataType.BOOL)
                            {
                                int tmp = prcv[iShort1] ^ cache[iShort];
                                DeviceAddress next = addr;
                                if (tmp != 0)
                                {
                                    while (addr.Start == next.Start)
                                    {
                                        if ((tmp & (1 << next.Bit)) > 0) _changedList.Add(index);
                                        if (++index < count)
                                            next = _items[index].Address;
                                        else
                                            break;
                                    }
                                }
                                else
                                {
                                    while (addr.Start == next.Start && ++index < count)
                                    {
                                        next = _items[index].Address;
                                    }
                                }
                            }
                            else
                            {
                                if (addr.ByteOrder.HasFlag(ByteOrder.BigEndian))
                                {
                                    for (int i = 0; i < addr.DataSize / 2; i++)
                                    {
                                        prcv[iShort1 + i] = IPAddress.HostToNetworkOrder(prcv[iShort1 + i]);
                                    }
                                }
                                if (addr.DataSize <= 2)
                                {
                                    if (prcv[iShort1] != cache[iShort]) _changedList.Add(index);
                                }
                                else
                                {
                                    int size = addr.DataSize / 2;
                                    for (int i = 0; i < size; i++)
                                    {
                                        if (prcv[iShort1 + i] != cache[iShort + i])
                                        {
                                            _changedList.Add(index);
                                            break;
                                        }
                                    }
                                }
                                index++;
                            }
                        }
                        short[] prcvShorts = new short[len];
                        for (int i = 0; i < len; i++)
                        {
                            prcvShorts[i] = prcv[i];
                        }

                        //改成Array.Copy 由于线程安全的问题
                        Array.Copy(prcvShorts, 0, cache, k, len);
                        /*
                        for (int j = 0; j < len; j++)
                        {
                            cache[j + offset] = prcv[j];
                        }//将PLC读取的数据写入到CacheReader中
                        */
                    }
                    k += len;
                }
            }
        
    }

        protected void Update()
        {
            DateTime dt = DateTime.Now;
            if (DataChange != null)
            {
                HistoryData[] values = new HistoryData[_changedList.Count];
                int i = 0;
                foreach (int index in _changedList)
                {
                    ITag item = _items[index];
                    var itemData = item.Read();
                    if (item.Active)
                        item.Update(itemData, dt, QUALITIES.QUALITY_GOOD);
                    if (_deadband == 0 || (item.Address.VarType == DataType.FLOAT &&
                        (Math.Abs(itemData.Single / item.Value.Single - 1) > _deadband)))
                    {
                        values[i].ID = item.ID;
                        values[i].Quality = item.Quality;
                        values[i].Value = itemData;
                        values[i].TimeStamp = item.TimeStamp;
                        i++;
                    }
                }
                foreach (DataChangeEventHandler deleg in DataChange.GetInvocationList())
                {
                    deleg.BeginInvoke(this, new DataChangeEventArgs(1, values), null, null);
                }
            }
            else
            {
                foreach (int index in _changedList)
                {
                    ITag item = _items[index];
                    if (item.Active)
                        item.Update(item.Read(), dt, QUALITIES.QUALITY_GOOD);
                }
            }
        }

        public void Dispose()
        {
            if (_timer != null)
                _timer.Dispose();
            //if (_items != null)
            //    _items.Clear();
        }

        public virtual HistoryData[] BatchRead(DataSource source, bool isSync, params ITag[] itemArray)
        {
            int len = itemArray.Length;
            HistoryData[] values = new HistoryData[len];
            if (source == DataSource.Device)
            {
                IMultiReadWrite multi = _plcReader as IMultiReadWrite;
                if (multi != null)
                {
                    Array.Sort(itemArray);
                    var itemArr = multi.ReadMultiple(Array.ConvertAll(itemArray, x => x.Address));
                    for (int i = 0; i < len; i++)
                    {
                        values[i].ID = itemArray[i].ID;
                        values[i].Value = itemArr[i].Value;
                        values[i].TimeStamp = itemArr[i].TimeStamp.ToDateTime();
                        itemArray[i].Update(itemArr[i].Value, values[i].TimeStamp, itemArr[i].Quality);
                    }
                }
                else
                {
                    for (int i = 0; i < len; i++)
                    {
                        itemArray[i].Refresh(source);
                        values[i].ID = itemArray[i].ID;
                        values[i].Value = itemArray[i].Value;
                        values[i].Quality = itemArray[i].Quality;
                        values[i].TimeStamp = itemArray[i].TimeStamp;
                    }
                }
            }
            else
            {
                for (int i = 0; i < len; i++)
                {
                    values[i].ID = itemArray[i].ID;
                    values[i].Value = itemArray[i].Value;
                    values[i].Quality = itemArray[i].Quality;
                    values[i].TimeStamp = itemArray[i].TimeStamp;
                }
            }
            return values;
        }

        public virtual int BatchWrite(SortedDictionary<ITag, object> items, bool isSync = true)
        {
            int len = items.Count;
            int rev = 0;
            IMultiReadWrite multi = _plcReader as IMultiReadWrite;
            if (multi != null)
            {
                DeviceAddress[] addrs = new DeviceAddress[len];
                object[] objs = new object[len];
                int i = 0;
                foreach (var item in items)
                {
                    addrs[i] = item.Key.Address;
                    objs[i] = item.Value;
                    i++;
                }
                rev = multi.WriteMultiple(addrs, objs);
            }
            else
            {
                foreach (var tag in items)
                {
                    if (tag.Key.Write(tag.Value) < 0)
                        rev = -1;
                }
            }
            if (DataChange != null && rev >= 0)
            {
                HistoryData[] data = new HistoryData[len];
                int i = 0;
                foreach (var item in items)
                {
                    ITag tag = item.Key;
                    data[i].ID = tag.ID;
                    data[i].TimeStamp = tag.TimeStamp;
                    data[i].Quality = tag.Quality;
                    data[i].Value = item.Key.ToStorage(item.Value);
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
            return source == DataSource.Cache ? _cacheReader.ReadInt32(address) : _plcReader.ReadInt32(address);
        }

        public ItemData<uint> ReadUInt32(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            return source == DataSource.Cache ? _cacheReader.ReadUInt32(address) : _plcReader.ReadUInt32(address);
        }

        public ItemData<ushort> ReadUInt16(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            return source == DataSource.Cache ? _cacheReader.ReadUInt16(address) : _plcReader.ReadUInt16(address);
        }

        public ItemData<short> ReadInt16(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            return source == DataSource.Cache ? _cacheReader.ReadInt16(address) : _plcReader.ReadInt16(address);
        }

        public ItemData<byte> ReadByte(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            return source == DataSource.Cache ? _cacheReader.ReadByte(address) : _plcReader.ReadByte(address);
        }

        public ItemData<float> ReadFloat(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            return source == DataSource.Cache ? _cacheReader.ReadFloat(address) : _plcReader.ReadFloat(address);
        }

        public ItemData<bool> ReadBool(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            return source == DataSource.Cache ? _cacheReader.ReadBit(address) : _plcReader.ReadBit(address);
        }

        public ItemData<string> ReadString(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            ushort siz = address.DataSize;
            return source == DataSource.Cache ? _cacheReader.ReadString(address, siz) :
                _plcReader.ReadString(address, siz);
        }

        public int WriteInt32(DeviceAddress address, int value)
        {
            int rs = _plcReader.WriteInt32(address, value);
            if (rs >= 0)
            {
                if (DataChange != null)
                {
                    ITag tag = GetTagByAddress(address);
                    if (tag != null)
                        DataChange(this, new DataChangeEventArgs(1, new HistoryData[1]
                {
                    new HistoryData(tag.ID,QUALITIES.QUALITY_GOOD,new Storage{Int32=value}, DateTime.Now)
                }));
                }
            }
            return rs;
        }

        public int WriteUInt32(DeviceAddress address, uint value)
        {
            int rs = _plcReader.WriteUInt32(address, value);
            if (rs >= 0)
            {
                if (DataChange != null)
                {
                    ITag tag = GetTagByAddress(address);
                    if (tag != null)
                        DataChange(this, new DataChangeEventArgs(1, new HistoryData[1]
                {
                    new HistoryData(tag.ID,QUALITIES.QUALITY_GOOD,new Storage{DWord=value}, DateTime.Now)
                }));
                }
            }
            return rs;
        }

        public int WriteUInt16(DeviceAddress address, ushort value)
        {
            int rs = _plcReader.WriteUInt16(address, value);
            if (rs >= 0)
            {
                if (DataChange != null)
                {
                    ITag tag = GetTagByAddress(address);
                    if (tag != null)
                        DataChange(this, new DataChangeEventArgs(1, new HistoryData[1]
                {
                    new HistoryData(tag.ID,QUALITIES.QUALITY_GOOD,new Storage{Word=value}, DateTime.Now)
                }));
                }
            }
            return rs;
        }

        public int WriteInt16(DeviceAddress address, short value)
        {
            int rs = _plcReader.WriteInt16(address, value);
            if (rs >= 0)
            {
                if (DataChange != null)
                {
                    ITag tag = GetTagByAddress(address);
                    if (tag != null)
                        DataChange(this, new DataChangeEventArgs(1, new HistoryData[1]
                {
                    new HistoryData(tag.ID,QUALITIES.QUALITY_GOOD,new Storage{Int16=value}, DateTime.Now)
                }));
                }
            }
            return rs;
        }

        public int WriteFloat(DeviceAddress address, float value)
        {
            int rs = _plcReader.WriteFloat(address, value);
            if (rs >= 0)
            {
                if (DataChange != null)
                {
                    ITag tag = GetTagByAddress(address);
                    if (tag != null)
                        DataChange(this, new DataChangeEventArgs(1, new HistoryData[1]
                {
                    new HistoryData(tag.ID,QUALITIES.QUALITY_GOOD,new Storage{Single=value}, DateTime.Now)
                }));
                }
            }
            return rs;
        }

        public int WriteString(DeviceAddress address, string value)
        {
            int rs = _plcReader.WriteString(address, value);
            if (rs >= 0)
            {
                if (DataChange != null)
                {
                    ITag tag = GetTagByAddress(address);
                    if (tag != null)
                        DataChange(this, new DataChangeEventArgs(1, new HistoryData[1]
                {
                    new HistoryData(tag.ID,QUALITIES.QUALITY_GOOD,Storage.Empty, DateTime.Now)
                }));
                }
            }
            return rs;
        }

        public int WriteBit(DeviceAddress address, bool value)
        {
            int rs = _plcReader.WriteBit(address, value);
            if (rs >= 0)
            {
                if (DataChange != null)
                {
                    ITag tag = GetTagByAddress(address);
                    if (tag != null)
                        DataChange(this, new DataChangeEventArgs(1, new HistoryData[1]
                {
                    new HistoryData(tag.ID,QUALITIES.QUALITY_GOOD,new Storage{Boolean=value}, DateTime.Now)
                }));
                }
            }
            return rs;
        }

        public int WriteBits(DeviceAddress address, byte value)
        {
            int rs = _plcReader.WriteBits(address, value);
            if (rs >= 0)
            {
                if (DataChange != null)
                {
                    ITag tag = GetTagByAddress(address);
                    if (tag != null)
                        DataChange(this, new DataChangeEventArgs(1, new HistoryData[1]
                {
                    new HistoryData(tag.ID,QUALITIES.QUALITY_GOOD,new Storage{Byte=value}, DateTime.Now)
                }));
                }
            }
            return rs;
        }

        private ITag GetTagByAddress(DeviceAddress addr)
        {
            int index = _items.BinarySearch(new BoolTag(0, addr, null));
            return index < 0 ? null : _items[index];
        }

        public event DataChangeEventHandler DataChange;
    }

    
}

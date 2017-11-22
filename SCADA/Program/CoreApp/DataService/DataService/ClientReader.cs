using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

namespace DataService
{
    public class ClientReader : IDevice
    {
        string _ip;
        public string ServerName
        {
            get { return _ip; }
        }

        internal Socket tcpSynCl;
        internal Socket tcpASynCl;

        public bool IsClosed
        {
            get
            {
                //return tcpASynCl.Poll(-1, SelectMode.SelectRead);
                return !tcpSynCl.Connected || !tcpASynCl.Connected;
            }
        }

        private ushort _timeout = 0;
        public int TimeOut
        {
            get { return _timeout; }
        }


        List<ClientGroup> _grps = new List<ClientGroup>(1);
        public IEnumerable<IGroup> Groups
        {
            get { return _grps; }
        }

        IDataServer _server;
        public IDataServer Parent
        {
            get { return _server; }
        }

        public ClientReader(IDataServer server, string ip)
        {
            _server = server;
            _ip = ip;
        }

        public bool Connect()
        {
            try
            {
                int port = 1000;
                IPAddress ip = IPAddress.Parse(_ip);
                tcpASynCl = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                tcpASynCl.Connect(new IPEndPoint(ip, port));
                tcpASynCl.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, _timeout);
                tcpASynCl.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, _timeout);

                tcpSynCl = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                tcpSynCl.Connect(new IPEndPoint(ip, port));
                tcpSynCl.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, _timeout);
                tcpSynCl.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, _timeout);
                return true;
            }
            catch (SocketException error)
            {
                if (OnClose != null)
                    OnClose(this, new ShutdownRequestEventArgs(error.Message));
                return false;
            }
        }

        public IGroup AddGroup(string name, ushort id, int updateRate, int timeOut = 0, float deadBand = 0f, bool active = false)
        {
            ClientGroup grp = new ClientGroup(id, name, updateRate, active, this);
            _grps.Add(grp);
            return grp;
        }

        public int RemoveAllGroup()
        {
            foreach (IGroup grp in _grps)
            {
                grp.Dispose();
            }
            _grps.Clear();
            return 1;
        }

        public event ShutdownRequestEventHandler OnClose;

        public void Dispose()
        {
            if (tcpSynCl != null)
            {
                if (tcpSynCl.Connected)
                {
                    try
                    {
                        tcpASynCl.Shutdown(SocketShutdown.Both);
                        tcpSynCl.Shutdown(SocketShutdown.Both);
                    }
                    catch { }
                    if (OnClose != null)
                        OnClose(this, new ShutdownRequestEventArgs("SHUTDOWN"));
                    tcpSynCl.Close();
                    tcpASynCl.Close();
                }
                tcpSynCl = null;
                tcpASynCl = null;
            }
            RemoveAllGroup();
        }
    }

    public class ClientGroup : IGroup
    {
        public const byte fctHead = 0xAB;
        public const byte fctReadSingle = 1;
        public const byte fctReadMultiple = 2;
        public const byte fctWriteSingle = 5;
        public const byte fctWriteMultiple = 15;

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
                    ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(ReciveData), _tcpASynCl);
                }
            }
        }

        protected ushort _id;
        public ushort ID
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

        public int Size
        {
            get
            {
                return _items == null ? 0 : _items.Length;
            }
        }

        protected string _name;
        public string Name
        {
            get
            {
                return _name;
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
        public IDevice Parent
        {
            get
            {
                return _plcReader;
            }
        }

        protected ITag[] _items;
        public IEnumerable<ITag> Items
        {
            get { return _items; }
        }

        IDataServer _server;
        Socket _tcpSynCl, _tcpASynCl;

        byte[] tcpSynClBuffer;

        public ClientGroup(ushort id, string name, int updateRate, bool active, ClientReader plcReader)
        {
            this._id = id;
            this._name = name;
            this._updateRate = updateRate;
            this._active = active;
            this._plcReader = plcReader;
            this._server = plcReader.Parent;
            this._tcpASynCl = plcReader.tcpASynCl;
            this._tcpSynCl = plcReader.tcpASynCl;
            tcpSynClBuffer = new byte[_tcpASynCl.ReceiveBufferSize];
        }

        private byte[] ReadSingleData(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            short ID = (short)address.Start;
            byte type = (byte)address.VarType;
            byte[] idbits = BitConverter.GetBytes(ID);
            byte[] write_data = new byte[6] { fctHead, fctReadSingle,
                source == DataSource.Cache?(byte)0:(byte)1, idbits[0], idbits[1], type };
            byte[] data = new byte[type < 4 ? 1 : type < 6 ? 2 : 4];
            SocketError error;
            _tcpSynCl.Send(write_data, 0, 6, SocketFlags.None, out error);
            int result = _tcpSynCl.Receive(tcpSynClBuffer, 0, data.Length + 3, SocketFlags.None, out error);
            Array.Copy(tcpSynClBuffer, 3, data, 0, data.Length);
            if (error == SocketError.Success)
                return data;
            else
            {
                throw new SocketException((int)error);
            }
        }


        private int WriteSingleData(DeviceAddress address, byte[] value)
        {
            short ID = (short)address.Start;
            byte type = (byte)address.VarType;
            byte[] idbits = BitConverter.GetBytes(ID);
            byte[] write_data = new byte[6] { fctHead, fctWriteSingle, 1, idbits[0], idbits[1], type };
            byte[] data = new byte[6 + value.Length];
            write_data.CopyTo(data, 0);
            value.CopyTo(data, 6);
            SocketError error;
            _tcpSynCl.Send(data, 0, data.Length, SocketFlags.None, out error);
            int result = _tcpSynCl.Receive(tcpSynClBuffer, 0, 2, SocketFlags.None, out error);
            if (error == SocketError.Success)
                return tcpSynClBuffer[1];
            else
            {
                throw new SocketException((int)error);
            }
        }


        public void Init()
        {
            if (_items != null)
            {
                for (int i = 0; i < _items.Length; i++)
                {
                    _items[i].Value = _items[i].Read(DataSource.Cache);//DataSource.Device
                }
            }
        }

        private void ReciveData(object state)
        {
            if (state == null || !_active) return;
            byte[] bytes = new byte[_tcpASynCl.ReceiveBufferSize];
            int result = 0;
            SocketError error;
            do
            {
                result = _tcpASynCl.Receive(bytes, 0, bytes.Length, SocketFlags.None, out error);
                if (result > 5 && bytes[0] == 0xAB)
                {
                    short len = BitConverter.ToInt16(bytes, 1);
                    short count = BitConverter.ToInt16(bytes, 3);
                    int j = 5;
                    DateTime time = DateTime.UtcNow;
                    Storage value = Storage.Empty;
                    for (int i = 0; i < count; i++)
                    {
                        short id = BitConverter.ToInt16(bytes, j);
                        j += 2;
                        ITag tag = GetItemByID(id);
                        if (tag != null)
                        {
                            DataType type = (DataType)bytes[j++];
                            switch (type)
                            {
                                case DataType.BOOL:
                                    value.Boolean = BitConverter.ToBoolean(bytes, j++);
                                    break;
                                case DataType.BYTE:
                                    value.Byte = bytes[j++];
                                    break;
                                case DataType.SHORT:
                                    value.Int16 = BitConverter.ToInt16(bytes, j);
                                    j += 2;
                                    break;
                                case DataType.INT:
                                    value.Int32 = BitConverter.ToInt32(bytes, j);
                                    j += 4;
                                    break;
                                case DataType.FLOAT:
                                    value.Single = BitConverter.ToSingle(bytes, j);
                                    j += 4;
                                    break;
                            }
                            tag.Update(value, time, QUALITIES.QUALITY_GOOD);
                        }
                        else
                        {
                            byte type = bytes[j];
                            j += (type < 4 ? 2 : type < 6 ? 3 : 5);
                        }
                    }
                    //Array.Clear(bytes, 0, count);
                }
            }
            while (result > 0);
        }

        public bool AddItems(ItemMetaData[] items)
        {
            int count = items.Length;
            if (_items == null) _items = new ITag[count];
            for (int i = 0; i < count; i++)
            {
                ITag dataItem = null;
                ItemMetaData meta = items[i];
                DeviceAddress addr = new DeviceAddress(0, 0, meta.ID, meta.Size, 0, meta.DataType);
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
                _items[i] = dataItem;
                _server.AddItemIndex(meta.Name, dataItem);
            }
            Array.Sort<ITag>(_items);
            Init();
            return true;
        }

        public bool RemoveAll()
        {
            Array.Clear(_items, 0, _items.Length);
            return true;
        }

        public bool SetActiveState(bool active, params short[] items)
        {
            return true;
        }

        public int FindItemByAddress(DeviceAddress addr)
        {
            return Array.BinarySearch<ITag>(_items, new BoolTag(0, addr, null));
        }

        public ITag GetItemByID(short id)
        {
            return _server[id];
        }

        public int BatchRead(DataSource source, bool isSync, params ITag[] itemArray)
        {
            if (itemArray == null) return -1;
            int len = itemArray.Length;
            byte[] bt = new byte[4];
            byte[] data = new byte[3 + len * 2];
            int j=0;
            data[j++] = fctHead;
            data[j++] = fctReadMultiple;
            data[j++] = source == DataSource.Cache ? (byte)0 : (byte)1;
            bt = BitConverter.GetBytes(itemArray.Length);
            data[j++] = bt[0];
            data[j++] = bt[1];
            data[j++] = bt[2];
            data[j++] = bt[3];
            for (int i = 0; i < len; i++)
            {
                ITag tag = itemArray[i];
                bt = BitConverter.GetBytes(tag.ID);
                data[j++] = bt[0];
                data[j++] = bt[1];
                data[j++] = (byte)(tag.Address.DataSize >> 3);
            }
            SocketError error;
            _tcpSynCl.Send(data, 0, data.Length, SocketFlags.None, out error);
            int result = _tcpSynCl.Receive(tcpSynClBuffer, 0, tcpSynClBuffer.Length, SocketFlags.None, out error);
            j = 2;
            if (error == SocketError.Success)
            {
                DateTime time=DateTime.UtcNow;
                Storage value=Storage.Empty;
                for (int i = 0; i < len; i++)
                {
                    ITag tag = itemArray[i];
                    switch (tag.Address.VarType)
                    {
                        case DataType.BOOL:
                            value.Boolean = BitConverter.ToBoolean(tcpSynClBuffer, j++);
                            break;
                        case DataType.BYTE:
                            value.Byte = tcpSynClBuffer[j++];
                            break;
                        case DataType.SHORT:
                            value.Int16 = BitConverter.ToInt16(tcpSynClBuffer, j);
                            j += 2;
                            break;
                        case DataType.INT:
                            value.Int32 = BitConverter.ToInt32(tcpSynClBuffer, j);
                            j += 4;
                            break;
                        case DataType.FLOAT:
                            value.Single = BitConverter.ToSingle(tcpSynClBuffer, j);
                            j += 4;
                            break;
                    }
                    tag.Update(value, time, QUALITIES.QUALITY_GOOD);
                }
                return 0;
            }
            else
            {
                throw new SocketException((int)error);
            }
        }

        public int BatchWrite(IDictionary<ITag, object> items, bool isSync = true)
        {
            List<byte> list = new List<byte>(new byte[] { fctHead, fctWriteMultiple });
            list.AddRange(BitConverter.GetBytes(items.Count));
            foreach (var item in items)
            {
                ITag tag = item.Key;
                list.AddRange(BitConverter.GetBytes(tag.ID));
                switch (tag.Address.VarType)
                {
                    case DataType.BOOL:
                        list.Add((bool)item.Value ? (byte)1 : (byte)0);
                        break;
                    case DataType.BYTE:
                        list.Add((byte)item.Value);
                        break;
                    case DataType.SHORT:
                        list.AddRange(BitConverter.GetBytes((short)item.Value));
                        break;
                    case DataType.INT:
                        list.AddRange(BitConverter.GetBytes((int)item.Value));
                        break;
                    case DataType.FLOAT:
                        list.AddRange(BitConverter.GetBytes((float)item.Value));
                        break;
                }
            }
            SocketError error;
            _tcpSynCl.Send(list.ToArray(), 0, list.Count, SocketFlags.None, out error);
            int result = _tcpSynCl.Receive(tcpSynClBuffer, 0, 2, SocketFlags.None, out error);
            if (error == SocketError.Success)
                return tcpSynClBuffer[1];
            else
            {
                throw new SocketException((int)error);
            }
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
                new ItemData<string>(Encoding.Default.GetString(data, 0, address.DataSize), 0, QUALITIES.QUALITY_GOOD);
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
            RemoveAll();
            _items = null;
        }
    }
}

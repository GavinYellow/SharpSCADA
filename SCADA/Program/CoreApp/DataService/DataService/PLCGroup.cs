using System;
using System.Collections.Generic;
using System.Timers;

namespace DataService
{
    public class PLCGroup : IGroup
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
                    //_timer.Start((uint)_updateRate, true);
                    //_timer.Timer += new EventHandler(timer_Timer);
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

        protected PLCGroup()
        {
        }

        public PLCGroup(short id, string name, int updateRate, bool active, IPLCDriver plcReader)
        {
            this._id = id;
            this._name = name;
            this._updateRate = updateRate;
            this._isActive = active;
            this._plcReader = plcReader;
            this._server = _plcReader.Parent;
            this._changedList = new List<int>();
            this._cacheReader = new ByteCacheReader();
            this._timer = new Timer();
        }

        public bool AddItems(IList<TagMetaData> items)
        {
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
                        DeviceAddress addr = _plcReader.GetDeviceAddress(meta.Address);
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
            _items.Sort();
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
            _items.Sort();
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
                    _cacheReader.Size = _start.DataSize <= bitCount ? 1 : _start.DataSize / bitCount;//改变Cache的Size属性值将创建Cache的内存区域
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
        protected void timer_Timer(object sender, EventArgs e)
        {
            if (_isActive)
            {
                lock (sync)
                {
                    Poll();
                    if (_changedList.Count > 0)
                        Update();
                }
            }
            else
                return;
        }

        protected virtual int Poll()
        {
            if (_plcReader.IsClosed) return -1;
            byte[] cache = (byte[])_cacheReader.Cache;
            int offset = 0;
            foreach (PDUArea area in _rangeList)
            {
                byte[] rcvBytes = _plcReader.ReadBytes(area.Start, (ushort)area.Len);//从PLC读取数据                
                if (rcvBytes == null)
                {
                    //_plcReader.Connect();
                    return -1;
                }
                else
                {
                    int index = area.StartIndex;//index指向_items中的Tag元数据
                    int count = index + area.Count;
                    while (index < count)
                    {
                        DeviceAddress addr = _items[index].Address;
                        int iByte = addr.CacheIndex;
                        int iByte1 = iByte - offset;
                        if (addr.VarType == DataType.BOOL)
                        {
                            int tmp = rcvBytes[iByte1] ^ cache[iByte];
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
                            ushort size = addr.DataSize;
                            for (int i = 0; i < size; i++)
                            {
                                if (rcvBytes[iByte1 + i] != cache[iByte + i])
                                {
                                    _changedList.Add(index);
                                    break;
                                }
                            }
                            index++;
                        }
                    }
                    for (int j = 0; j < rcvBytes.Length; j++)
                        cache[j + offset] = rcvBytes[j];//将PLC读取的数据写入到CacheReader中
                }
                offset += rcvBytes.Length;
            }
            return 1;
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
            _changedList.Clear();
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


    public struct PDUArea
    {
        public DeviceAddress Start;
        public int Len;
        public int StartIndex;
        public int Count;

        public PDUArea(DeviceAddress start, int len, int startIndex, int count)
        {
            Start = start;
            Len = len;
            StartIndex = startIndex;
            Count = count;
        }
    }
}

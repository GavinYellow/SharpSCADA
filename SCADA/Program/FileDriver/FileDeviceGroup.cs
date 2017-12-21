using System;
using System.Collections.Generic;
using System.Threading;
using DataService;

namespace FileDriver
{
    public class FileDeviceGroup : IGroup
    {
        //可采用定期轮询和消息通知改变两种方式
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
                if (value)
                {
                    ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(this.OnUpdate), 1);
                }
                else
                    Update();
            }
        }

        private void OnUpdate(object stateInfo)
        {
            while (true)
            {
                Thread.Sleep(_updateRate);
                lock (this)
                {
                    if (!_isActive)
                    {
                        return;
                    }
                    Update();
                }
            }
        }

        public void Update()
        {
            DateTime dt = DateTime.Now;
            FileData[] list = _fileReader.ReadAll(_id);
            if (list != null)
            {
                if (DataChange != null)
                {
                    List<HistoryData> hdata = new List<HistoryData>();
                    foreach (ITag tag in _items)
                    {
                        int index = Array.BinarySearch(list, new FileData { ID = tag.ID });
                        if (index >= 0)
                        {
                            FileData data = list[index];
                            if (tag.Address.VarType == DataType.STR)
                            {
                                if (data.Text != tag.ToString())
                                {
                                    (tag as StringTag).String = data.Text;
                                    tag.Update(Storage.Empty, dt, QUALITIES.QUALITY_GOOD);
                                }
                                continue;
                            }
                            if (tag.Value != data.Value)
                            {
                                tag.Update(data.Value, dt, QUALITIES.QUALITY_GOOD);
                                hdata.Add(new HistoryData(data.ID, QUALITIES.QUALITY_GOOD, data.Value, dt));
                            }
                        }
                    }
                    foreach (DataChangeEventHandler deleg in DataChange.GetInvocationList())
                    {
                        deleg.BeginInvoke(this, new DataChangeEventArgs(1, hdata), null, null);
                    }
                }
                else
                {
                    foreach (ITag tag in _items)
                    {
                        int index = Array.BinarySearch(list, new FileData { ID = tag.ID });
                        if (index >= 0)
                        {
                            if (tag.Value != list[index].Value)
                            {
                                tag.Update(list[index].Value, dt, QUALITIES.QUALITY_GOOD);
                            }
                        }
                    }
                }
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


        IFileDriver _fileReader;
        public IDriver Parent
        {
            get
            {
                return _fileReader;
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

        public FileDeviceGroup(short id, string name, int updateRate, bool active, IFileDriver plcReader)
        {
            this._id = id;
            this._updateRate = updateRate;
            this._isActive = active;
            this._fileReader = plcReader;
            this._name = name;
            this._server = _fileReader.Parent;
        }

        public bool AddItems(IList<TagMetaData> items)
        {
            int count = items.Count;
            if (_items == null) _items = new List<ITag>();
            lock (_server.SyncRoot)
            {
                int j = 0;
                for (int i = 0; i < count; i++)
                {
                    ITag dataItem = null;
                    TagMetaData meta = items[i];
                    if (meta.GroupID == this._id)
                    {
                        DeviceAddress addr = new DeviceAddress(-1, (ushort)meta.GroupID, (ushort)meta.ID, j++, meta.Size, 0, meta.DataType);
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
                            default:
                                dataItem = new BoolTag(meta.ID, addr, this);
                                break;
                        }
                        _items.Add(dataItem);
                        _server.AddItemIndex(meta.Name, dataItem);
                    }
                }
            }
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
            return true;
        }

        public bool RemoveItems(params ITag[] items)
        {
            foreach (var item in items)
            {
                _server.RemoveItemIndex(item.GetTagName());
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
            int id = addr.CacheIndex;
            for (int i = 0; i < _items.Count; i++)
            {
                if (id == _items[i].Address.CacheIndex)
                    return _items[i];
            }
            return null;
        }

        public virtual HistoryData[] BatchRead(DataSource source, bool isSync, params ITag[] itemArray)
        {
            int len = itemArray.Length;
            HistoryData[] values = new HistoryData[len];
            if (source == DataSource.Device)
            {
                IMultiReadWrite multi = _fileReader as IMultiReadWrite;
                if (multi != null)
                {
                    var itemArr = multi.ReadMultiple(Array.ConvertAll(itemArray, x => x.Address));
                    for (int i = 0; i < len; i++)
                    {
                        values[i].ID = itemArray[i].ID;
                        values[i].Value = itemArr[i].Value;
                        values[i].Quality = itemArr[i].Quality;
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
            IMultiReadWrite multi = _fileReader as IMultiReadWrite;
            if (DataChange == null)
            {
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
                    return multi.WriteMultiple(addrs, objs);
                }
                else
                {
                    foreach (var tag in items)
                    {
                        tag.Key.Write(tag.Value);
                    }
                    return 0;
                }
            }
            else
            {
                HistoryData[] data = new HistoryData[len];
                int i = 0;
                if (multi != null)
                {
                    DeviceAddress[] addrs = new DeviceAddress[len];
                    object[] objs = new object[len];

                    foreach (var item in items)
                    {
                        ITag tag = item.Key;
                        addrs[i] = tag.Address;
                        objs[i] = item.Value;
                        data[i].ID = tag.ID;
                        data[i].TimeStamp = tag.TimeStamp;
                        data[i].Quality = tag.Quality;
                        data[i].Value = item.Key.ToStorage(item.Value);
                        i++;
                    }
                    rev = multi.WriteMultiple(addrs, objs);
                }
                else
                {
                    foreach (var item in items)
                    {
                        ITag tag = item.Key;
                        if (tag.Write(tag.Value) >= 0)
                        {
                            data[i].ID = tag.ID;
                            data[i].TimeStamp = tag.TimeStamp;
                            data[i].Quality = tag.Quality;
                            data[i].Value = item.Key.ToStorage(item.Value);
                            i++;
                        }
                    }
                }
                foreach (DataChangeEventHandler deleg in DataChange.GetInvocationList())
                {
                    deleg.BeginInvoke(this, new DataChangeEventArgs(1, data), null, null);
                }
            }
            return rev;
        }

        public bool RecieveData(string data)//when sendmessge arrived WM_COPYDATA
        {
            int index = data.IndexOf('#');//格式：GroupID#TagID:Value|TagID:Value|TagID:Value
            if (index > 0 && data.Substring(0, index) == _id.ToString())
            {
                string[] strs = data.Right(index).Split('|');
                HistoryData[] hdata = new HistoryData[strs.Length];
                DateTime date = DateTime.Now;
                int i = 0;
                foreach (string d in strs)
                {
                    int ind = d.IndexOf(':');
                    if (ind > 0)
                    {
                        short tid;
                        if (short.TryParse(data.Substring(0, ind), out tid))
                        {
                            ITag tag = _server[tid];
                            if (tag != null)
                            {
                                Storage value = Storage.Empty;
                                DataType type = tag.Address.VarType;
                                switch (type)
                                {
                                    case DataType.BOOL:
                                        value.Boolean = Convert.ToBoolean(d.Right(ind));
                                        break;
                                    case DataType.BYTE:
                                        value.Byte = Convert.ToByte(d.Right(ind));
                                        break;
                                    case DataType.WORD:
                                        value.Word = Convert.ToUInt16(d.Right(ind));
                                        break;
                                    case DataType.SHORT:
                                        value.Int16 = Convert.ToInt16(d.Right(ind));
                                        break;
                                    case DataType.DWORD:
                                        value.DWord = Convert.ToUInt32(d.Right(ind));
                                        break;
                                    case DataType.INT:
                                        value.Int32 = Convert.ToInt32(d.Right(ind));
                                        break;
                                    case DataType.FLOAT:
                                        value.Single = Convert.ToSingle(d.Right(ind));
                                        break;
                                    case DataType.STR:
                                        break;
                                }
                                tag.Update(value, date, QUALITIES.QUALITY_GOOD);//也可以不传值，tag自身refresh
                                hdata[i].ID = tag.ID;
                                hdata[i].Value = value;
                                hdata[i].TimeStamp = date;
                                hdata[i].Quality = QUALITIES.QUALITY_GOOD;
                            }
                        }
                    }
                    i++;
                }
                if (DataChange != null)
                {
                    foreach (DataChangeEventHandler deleg in DataChange.GetInvocationList())
                    {
                        deleg.BeginInvoke(this, new DataChangeEventArgs(1, hdata), null, null);
                    }
                }
                return true;
            }
            return false;
        }

        public ItemData<int> ReadInt32(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            if (source == DataSource.Device) return _fileReader.ReadInt32(address);
            else
            {
                ITag tag = _items[address.Start];
                return new ItemData<int>(tag.Value.Int32, 0, tag.Quality);
            }
        }

        public ItemData<uint> ReadUInt32(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            if (source == DataSource.Device) return _fileReader.ReadUInt32(address);
            else
            {
                ITag tag = _items[address.Start];
                return new ItemData<uint>(tag.Value.DWord, 0, tag.Quality);
            }
        }

        public ItemData<ushort> ReadUInt16(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            if (source == DataSource.Device) return _fileReader.ReadUInt16(address);
            else
            {
                ITag tag = _items[address.Start];
                return new ItemData<ushort>(tag.Value.Word, 0, tag.Quality);
            }
        }

        public ItemData<short> ReadInt16(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            if (source == DataSource.Device) return _fileReader.ReadInt16(address);
            else
            {
                ITag tag = _items[address.Start];
                return new ItemData<short>(tag.Value.Int16, 0, tag.Quality);
            }
        }

        public ItemData<byte> ReadByte(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            if (source == DataSource.Device) return _fileReader.ReadByte(address);
            else
            {
                ITag tag = _items[address.Start];
                return new ItemData<byte>(tag.Value.Byte, 0, tag.Quality);
            }
        }

        public ItemData<float> ReadFloat(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            if (source == DataSource.Device) return _fileReader.ReadFloat(address);
            else
            {
                ITag tag = _items[address.Start];
                return new ItemData<float>(tag.Value.Single, 0, tag.Quality);
            }
        }

        public ItemData<bool> ReadBool(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            if (source == DataSource.Device) return _fileReader.ReadBit(address);
            else
            {
                ITag tag = _items[address.Start];
                return new ItemData<bool>(tag.Value.Boolean, 0, tag.Quality);
            }
        }

        public ItemData<string> ReadString(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            if (source == DataSource.Device) return _fileReader.ReadString(address, address.DataSize);
            else
            {
                StringTag tag = _items[address.Start] as StringTag;
                return tag == null ? new ItemData<string>(null, 0, QUALITIES.QUALITY_BAD) :
                    new ItemData<string>(tag.String, 0, tag.Quality);
            }
        }

        public int WriteInt32(DeviceAddress address, int value)
        {
            int rs = _fileReader.WriteInt32(address, value);
            if (rs >= 0)
            {
                Storage stor = new Storage { Int32 = value };
                _items[address.Start].Update(stor, DateTime.Now, QUALITIES.QUALITY_GOOD);
                if (DataChange != null)
                {
                    DataChange(this, new DataChangeEventArgs(1, new HistoryData[1]
                {
                    new HistoryData( (short)address.CacheIndex,QUALITIES.QUALITY_GOOD,stor, DateTime.Now)
                }));
                }
            }
            return rs;
        }

        public int WriteUInt32(DeviceAddress address, uint value)
        {
            int rs = _fileReader.WriteUInt32(address, value);
            if (rs >= 0)
            {
                Storage stor = new Storage { DWord = value };
                _items[address.Start].Update(stor, DateTime.Now, QUALITIES.QUALITY_GOOD);
                if (DataChange != null)
                {
                    DataChange(this, new DataChangeEventArgs(1, new HistoryData[1]
                {
                    new HistoryData( (short)address.CacheIndex,QUALITIES.QUALITY_GOOD,stor, DateTime.Now)
                }));
                }
            }
            return rs;
        }

        public int WriteUInt16(DeviceAddress address, ushort value)
        {
            int rs = _fileReader.WriteUInt16(address, value);
            if (rs >= 0)
            {
                Storage stor = new Storage { Word = value };
                _items[address.Start].Update(stor, DateTime.Now, QUALITIES.QUALITY_GOOD);
                if (DataChange != null)
                {
                    DataChange(this, new DataChangeEventArgs(1, new HistoryData[1]
                {
                    new HistoryData( (short)address.CacheIndex,QUALITIES.QUALITY_GOOD,stor, DateTime.Now)
                }));
                }
            }
            return rs;
        }

        public int WriteInt16(DeviceAddress address, short value)
        {
            int rs = _fileReader.WriteInt16(address, value);
            if (rs >= 0)
            {
                Storage stor = new Storage { Int16 = value };
                _items[address.Start].Update(stor, DateTime.Now, QUALITIES.QUALITY_GOOD);
                if (DataChange != null)
                {
                    DataChange(this, new DataChangeEventArgs(1, new HistoryData[1]
                {
                    new HistoryData( (short)address.CacheIndex,QUALITIES.QUALITY_GOOD,stor, DateTime.Now)
                }));
                }
            }
            return rs;
        }

        public int WriteFloat(DeviceAddress address, float value)
        {
            int rs = _fileReader.WriteFloat(address, value);
            if (rs >= 0)
            {
                Storage stor = new Storage { Single = value };
                _items[address.Start].Update(stor, DateTime.Now, QUALITIES.QUALITY_GOOD);
                if (DataChange != null)
                {
                    DataChange(this, new DataChangeEventArgs(1, new HistoryData[1]
                {
                    new HistoryData( (short)address.CacheIndex,QUALITIES.QUALITY_GOOD,stor, DateTime.Now)
                }));
                }
            }
            return rs;
        }

        public int WriteString(DeviceAddress address, string value)
        {
            int rs = _fileReader.WriteString(address, value);
            if (rs >= 0)
            {
                StringTag tag = _items[address.Start] as StringTag;
                if (tag != null && tag.String != value)
                {
                    tag.String = value;
                    tag.Update(Storage.Empty, DateTime.Now, QUALITIES.QUALITY_GOOD);
                    if (DataChange != null)
                    {
                        DataChange(this, new DataChangeEventArgs(1, new HistoryData[1]
                        {
                            new HistoryData( (short)address.CacheIndex,tag.Quality,Storage.Empty, DateTime.Now)
                        }));
                    }
                }
            }
            return rs;
        }

        public int WriteBit(DeviceAddress address, bool value)
        {
            int rs = _fileReader.WriteBit(address, value);
            if (rs >= 0)
            {
                Storage stor = new Storage { Boolean = value };
                _items[address.Start].Update(stor, DateTime.Now, QUALITIES.QUALITY_GOOD);
                if (DataChange != null)
                {
                    DataChange(this, new DataChangeEventArgs(1, new HistoryData[1]
                {
                    new HistoryData( (short)address.CacheIndex,QUALITIES.QUALITY_GOOD,stor, DateTime.Now)
                }));
                }
            }
            return rs;
        }

        public int WriteBits(DeviceAddress address, byte value)
        {
            int rs = _fileReader.WriteBits(address, value);
            if (rs >= 0)
            {
                Storage stor = new Storage { Byte = value };
                _items[address.Start].Update(stor, DateTime.Now, QUALITIES.QUALITY_GOOD);
                if (DataChange != null)
                {
                    DataChange(this, new DataChangeEventArgs(1, new HistoryData[1]
                {
                    new HistoryData( (short)address.CacheIndex,QUALITIES.QUALITY_GOOD,stor, DateTime.Now)
                }));
                }
            }
            return rs;
        }

        public void Dispose()
        {
            _items = null;
        }

        public event DataChangeEventHandler DataChange;
    }
}

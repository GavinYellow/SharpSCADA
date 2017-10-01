using System.Collections.Generic;
using System.Threading;

namespace DataService
{
    public class TagListGroup : IGroup
    {
        bool _isActive, _isDisposed;
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

        IPLCDriver _reader;
        public IDriver Parent
        {
            get { return _reader; }
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

        HashSet<short> _activeList;
        CompareItemByID rc = new CompareItemByID();

        public TagListGroup(short id, string name, int updateRate, int size, bool active, IPLCDriver plcReader)
        {
            this._id = id;
            this._updateRate = updateRate;
            this._isActive = active;
            this._reader = plcReader;
            this._name = name;
            this._server = _reader.Parent;
        }


        public bool AddItems(IList<TagMetaData> items)
        {
            int count = items.Count;
            if (_items == null)
            {
                _items = new List<ITag>(count);
                _activeList = new HashSet<short>();
            }
            lock (_server.SyncRoot)
            {
                for (int i = 0; i < count; i++)
                {
                    ITag dataItem = null;
                    TagMetaData meta = items[i];
                    if (meta.GroupID == this._id)
                    {
                        switch (meta.DataType)
                        {
                            case DataType.BOOL:
                                dataItem = new BoolTag(meta.ID, _reader.GetDeviceAddress(meta.Address), this);
                                break;
                            case DataType.BYTE:
                                dataItem = new ByteTag(meta.ID, _reader.GetDeviceAddress(meta.Address), this);
                                break;
                            case DataType.WORD:
                            case DataType.SHORT:
                                dataItem = new ShortTag(meta.ID, _reader.GetDeviceAddress(meta.Address), this);
                                break;
                            case DataType.TIME:
                            case DataType.INT:
                                dataItem = new IntTag(meta.ID, _reader.GetDeviceAddress(meta.Address), this);
                                break;
                            case DataType.FLOAT:
                                dataItem = new FloatTag(meta.ID, _reader.GetDeviceAddress(meta.Address), this);
                                break;
                            case DataType.STR:
                                dataItem = new StringTag(meta.ID, _reader.GetDeviceAddress(meta.Address), this);
                                break;
                        }
                        if (dataItem != null)
                        {
                            _items.Add(dataItem);
                            _server.AddItemIndex(meta.Name, dataItem);
                            _activeList.Add(meta.ID);
                        }
                    }
                }
            }
            _items.Sort(rc);
            _items.TrimExcess();
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
                    if (tag.Active)
                    {
                        _activeList.Add(tag.ID);
                    }
                }
            }
            _items.Sort(rc);
            _items.TrimExcess();
            return true;
        }

        public bool RemoveItems(params ITag[] items)
        {
            foreach (var item in items)
            {
                _server.RemoveItemIndex(item.GetTagName());
                _activeList.Remove(item.ID);
                _items.Remove(item);
            }
            return true;
        }


        public bool SetActiveState(bool active, params short[] items)
        {
            if (active)
            {
                lock (_activeList)
                {
                    _activeList.UnionWith(items);
                };
            }
            else
            {
                lock (_activeList)
                {
                    _activeList.ExceptWith(items);
                };
            }
            return true;
        }

        public ITag FindItemByAddress(DeviceAddress addr)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                DeviceAddress address = _items[i].Address;
                if (addr.Area == address.Area
                    && addr.DBNumber == address.DBNumber
                    && addr.Start == address.Start
                    && addr.Bit == address.Bit)
                    return _items[i];
            }
            return null;
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
            return _reader.ReadInt32(address);
        }

        public ItemData<short> ReadInt16(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            return _reader.ReadInt16(address);
        }

        public ItemData<byte> ReadByte(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            return _reader.ReadByte(address);
        }

        public ItemData<float> ReadFloat(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            return _reader.ReadFloat(address);
        }

        public ItemData<bool> ReadBool(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            return _reader.ReadBit(address);
        }

        public ItemData<string> ReadString(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            return new ItemData<string>(string.Empty, 0, QUALITIES.QUALITY_UNCERTAIN);
        }

        public int WriteInt32(DeviceAddress address, int value)
        {
            return _reader.WriteInt32(address, value);
        }

        public int WriteInt16(DeviceAddress address, short value)
        {
            return _reader.WriteInt16(address, value);
        }

        public int WriteFloat(DeviceAddress address, float value)
        {
            return _reader.WriteFloat(address, value);
        }

        public int WriteString(DeviceAddress address, string value)
        {
            return _reader.WriteString(address, value);
        }

        public int WriteBit(DeviceAddress address, bool value)
        {
            return _reader.WriteBit(address, value);
        }

        public int WriteBits(DeviceAddress address, byte value)
        {
            return _reader.WriteBits(address, value);
        }

        public void Dispose()
        {
            _items.Clear();
            _activeList.Clear();
            _isDisposed = true;
            //Dictionary<int, string> dict = new Dictionary<int, string>();
        }


        public void OnUpdate(object stateInfo)
        {
            while (true)
            {
                Thread.Sleep(_updateRate);
                lock (this)
                {
                    if (!_isActive || _isDisposed)
                    {
                        return;
                    }
                    List<HistoryData> hdalist = new List<HistoryData>();
                    foreach (short id in _activeList)
                    {
                        ITag item = _server[id];
                        if (item != null)
                        {
                            if (item.Refresh() && DataChange != null)
                                hdalist.Add(new HistoryData(item.ID, item.Quality, item.Value, item.TimeStamp));
                        }
                        if (DataChange != null)
                            DataChange.BeginInvoke(this, new DataChangeEventArgs(1, hdalist), null, null);
                    }
                }
            }
        }

        public event DataChangeEventHandler DataChange;
    }

    public class CompareItemByID : IComparer<ITag>
    {
        public int Compare(ITag x, ITag y)
        {
            if (x == null)
            {
                if (y == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (y == null)
                {
                    return 1;
                }
                else
                {
                    return x.ID.CompareTo(y.ID);
                }
            }
        }
    }
}

using System.Collections.Generic;
using System.ComponentModel;
using DataService;
using System;

namespace FileDriver
{
    [Description("标签直接读写")]
    public sealed class TagDriver : IFileDriver
    {
        string _fileName;
        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                _fileName = value;
            }
        }

        short _id;//内存及文件中的地址与ID相关
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
            get
            {
                return _server;
            }
            set
            {
                _server = value;
            }
        }

        public bool IsClosed
        {
            get { return false; }
        }

        int _timeOut;
        public int TimeOut
        {
            get
            {
                return _timeOut;
            }
            set
            {
                _timeOut = value;
            }
        }

        List<IGroup> _groups = new List<IGroup>();
        public IEnumerable<IGroup> Groups
        {
            get { return _groups; }
        }

        IDataServer _parent;
        public IDataServer Parent
        {
            get { return _parent; }
        }

        public TagDriver(IDataServer parent, short id, string name)
        {
            _parent = parent;
            _id = id;
            _name = name;
        }

        public bool Connect()
        {
            return true;
        }

        public IGroup AddGroup(string name, short id, int updateRate, float deadBand = 0f, bool active = false)
        {
            FileDeviceGroup grp = new FileDeviceGroup(id, name, updateRate, active, this);
            _groups.Add(grp);
            return grp;
        }

        public bool RemoveGroup(IGroup group)
        {
            return _groups.Remove(group);
        }

        public event IOErrorEventHandler OnError;

        public void Dispose()
        {
            foreach (IGroup grp in _groups)
            {
                grp.Dispose();
            }
            _groups.Clear();
        }

        public byte[] ReadBytes(DeviceAddress address, ushort size)
        {
            var tag = _parent[(short)address.CacheIndex];
            return tag == null ? null : tag.ToByteArray();
        }

        public ItemData<int> ReadInt32(DeviceAddress address)
        {
            var tag = _parent[(short)address.CacheIndex];
            return tag == null ? new ItemData<int>(0, 0, QUALITIES.QUALITY_BAD) : new ItemData<int>(tag.Value.Int32, 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<uint> ReadUInt32(DeviceAddress address)
        {
            var tag = _parent[(short)address.CacheIndex];
            return tag == null ? new ItemData<uint>(0, 0, QUALITIES.QUALITY_BAD) : new ItemData<uint>(tag.Value.DWord, 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<ushort> ReadUInt16(DeviceAddress address)
        {
            var tag = _parent[(short)address.CacheIndex];
            return tag == null ? new ItemData<ushort>(0, 0, QUALITIES.QUALITY_BAD) : new ItemData<ushort>(tag.Value.Word, 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<short> ReadInt16(DeviceAddress address)
        {
            var tag = _parent[(short)address.CacheIndex];
            return tag == null ? new ItemData<short>(0, 0, QUALITIES.QUALITY_BAD) : new ItemData<short>(tag.Value.Int16, 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<byte> ReadByte(DeviceAddress address)
        {
            var tag = _parent[(short)address.CacheIndex];
            return tag == null ? new ItemData<byte>(0, 0, QUALITIES.QUALITY_BAD) : new ItemData<byte>(tag.Value.Byte, 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<string> ReadString(DeviceAddress address, ushort size)
        {
            var tag = _parent[(short)address.CacheIndex];
            return tag == null ? new ItemData<string>(null, 0, QUALITIES.QUALITY_BAD) : new ItemData<string>(tag.ToString(), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<float> ReadFloat(DeviceAddress address)
        {
            var tag = _parent[(short)address.CacheIndex];
            return tag == null ? new ItemData<float>(0, 0, QUALITIES.QUALITY_BAD) : new ItemData<float>(tag.Value.Single, 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<bool> ReadBit(DeviceAddress address)
        {
            var tag = _parent[(short)address.CacheIndex];
            return tag == null ? new ItemData<bool>(false, 0, QUALITIES.QUALITY_BAD) : new ItemData<bool>(tag.Value.Boolean, 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<object> ReadValue(DeviceAddress address)
        {
            return this.ReadValueEx(address);
        }

        public int WriteBytes(DeviceAddress address, byte[] bit)
        {
            return 0;
        }

        public int WriteBit(DeviceAddress address, bool bit)
        {
            var tag = _parent[(short)address.CacheIndex];
            if (tag != null)
            {
                Storage v = tag.Value;
                v.Boolean = bit;
                tag.Update(v, DateTime.Now, QUALITIES.QUALITY_GOOD);
                return 0;
            }
            return -1;
        }

        public int WriteBits(DeviceAddress address, byte bits)
        {
            var tag = _parent[(short)address.CacheIndex];
            if (tag != null)
            {
                Storage v = tag.Value;
                v.Byte = bits;
                tag.Update(v, DateTime.Now, QUALITIES.QUALITY_GOOD);
                return 0;
            }
            return -1;
        }

        public int WriteInt16(DeviceAddress address, short value)
        {
            var tag = _parent[(short)address.CacheIndex];
            if (tag != null)
            {
                Storage v = tag.Value;
                v.Int16 = value;
                tag.Update(v, DateTime.Now, QUALITIES.QUALITY_GOOD);
                return 0;
            }
            return -1;
        }

        public int WriteUInt16(DeviceAddress address, ushort value)
        {
            var tag = _parent[(short)address.CacheIndex];
            if (tag != null)
            {
                Storage v = tag.Value;
                v.Word = value;
                tag.Update(v, DateTime.Now, QUALITIES.QUALITY_GOOD);
                return 0;
            }
            return -1;
        }

        public int WriteUInt32(DeviceAddress address, uint value)
        {
            var tag = _parent[(short)address.CacheIndex];
            if (tag != null)
            {
                Storage v = tag.Value;
                v.DWord = value;
                tag.Update(v, DateTime.Now, QUALITIES.QUALITY_GOOD);
                return 0;
            }
            return -1;
        }

        public int WriteInt32(DeviceAddress address, int value)
        {
            var tag = _parent[(short)address.CacheIndex];
            if (tag != null)
            {
                Storage v = tag.Value;
                v.Int32 = value;
                tag.Update(v, DateTime.Now, QUALITIES.QUALITY_GOOD);
                return 0;
            }
            return -1;
        }

        public int WriteFloat(DeviceAddress address, float value)
        {
            var tag = _parent[(short)address.CacheIndex];
            if (tag != null)
            {
                Storage v = tag.Value;
                v.Single = value;
                tag.Update(v, DateTime.Now, QUALITIES.QUALITY_GOOD);
                return 0;
            }
            return -1;
        }

        public int WriteString(DeviceAddress address, string str)
        {
            return 0;
        }

        public int WriteValue(DeviceAddress address, object value)
        {
            return 0;
        }

        public FileData[] ReadAll(short groupId)
        {
            return null;
        }
    }
}

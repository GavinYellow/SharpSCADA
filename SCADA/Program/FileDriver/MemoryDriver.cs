using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using DataService;

namespace FileDriver
{
    [Description("内存映射文件")]
    public class MemoryReader : IFileDriver
    {
        internal struct MemoryVar
        {
            public DataType VarType;
            public ushort Size;

            public MemoryVar(DataType varType, ushort size)
            {
                VarType = varType;
                Size = size;
            }
        }
        //共享内存如何释放？还是长久保存在内存中
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
            get { return mapp == null; }
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

        int _count = 0;
        MemoryMappedFile mapp = null;
        MemoryMappedViewAccessor accessor = null;
        SortedList<short, int> psList = new SortedList<short, int>();

        public MemoryReader(IDataServer parent, short id, string name)
        {
            _parent = parent;
            _id = id;
            _name = name;
        }

        public bool Connect()
        {
            try
            {
                if (File.Exists(_fileName))
                {
                    using (FileStream stream = File.Open(_fileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
                    {
                        mapp = MemoryMappedFile.CreateFromFile(stream, _name, 0, MemoryMappedFileAccess.ReadWrite,
                                 new MemoryMappedFileSecurity(), HandleInheritability.Inheritable, false);
                        using (var reader = new BinaryReader(stream))
                        {
                            _count = reader.ReadInt32();
                            for (int i = 0; i < _count; i++)
                            {
                                psList.Add(reader.ReadInt16(), reader.ReadInt32());
                            }
                        }
                    }
                }
                else
                {
                    int len = 0;
                    List<MemoryVar> hdata = new List<MemoryVar>();
                    foreach (IGroup grp in _groups)
                    {
                        foreach (ITag tag in grp.Items)
                        {
                            _count++;
                            psList.Add(tag.ID, len);
                            ushort size = tag.Address.DataSize;
                            len += 3 + size;
                            hdata.Add(new MemoryVar(tag.Address.VarType, size));
                        }
                    }
                    if (string.IsNullOrEmpty(_fileName))
                    {
                        mapp = MemoryMappedFile.CreateOrOpen(_name, 4 + _count * 6 + len);
                        using (MemoryMappedViewStream stream = mapp.CreateViewStream())
                        {
                            using (BinaryWriter writer = new BinaryWriter(stream))
                            {
                                writer.Write(_count);
                                foreach (var item in psList)
                                {
                                    writer.Write(item.Key);
                                    writer.Write(item.Value);
                                }
                                for (int i = 0; i < _count; i++)
                                {
                                    writer.Write((byte)hdata[i].VarType);
                                    writer.Write(hdata[i].Size);
                                    writer.Seek(hdata[i].Size, SeekOrigin.Current);
                                }
                            }
                        }
                    }
                    else
                    {
                        using (FileStream stream = File.Create(_fileName))
                        {
                            using (BinaryWriter writer = new BinaryWriter(stream))
                            {
                                writer.Write(_count);
                                foreach (var item in psList)
                                {
                                    writer.Write(item.Key);
                                    writer.Write(item.Value);
                                }
                                for (int i = 0; i < _count; i++)
                                {
                                    writer.Write((byte)hdata[i].VarType);
                                    writer.Write(hdata[i].Size);
                                    writer.Seek(hdata[i].Size, SeekOrigin.Current);
                                }
                            }
                        }
                        mapp = MemoryMappedFile.CreateFromFile(_fileName, FileMode.Open, _name);
                    }
                }
                if (mapp != null)
                    accessor = mapp.CreateViewAccessor(0, 0);
                return true;
            }
            catch (Exception err)
            {
                if (OnError != null)
                    OnError(this, new IOErrorEventArgs(err.Message));
                return false;
            }
        }

        long FindPosition(DeviceAddress addr)
        {
            int offset = psList[(short)addr.CacheIndex];
            return offset + 6 * _count + 7;
        }

        public IGroup AddGroup(string name, short id, int updateRate, float deadBand = 0f, bool active = false)
        {
            FileDeviceGroup grp = new FileDeviceGroup(id, name, updateRate, active, this);
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
            if (mapp != null)
            {
                accessor.Dispose();
                mapp.Dispose();
                mapp = null;
                if (OnError != null)
                    OnError(this, new IOErrorEventArgs("mapp file closed"));
            }
        }

        public event IOErrorEventHandler OnError;

        public byte[] ReadBytes(DeviceAddress address, ushort size)
        {
            byte[] bytes = new byte[size];
            try
            {
                int result = accessor.ReadArray(FindPosition(address), bytes, 0, size);
                return bytes;
            }
            catch { return null; }
        }

        public ItemData<int> ReadInt32(DeviceAddress address)
        {
            try
            {
                return new ItemData<int>(accessor.ReadInt32(FindPosition(address)), 0, QUALITIES.QUALITY_GOOD);
            }
            catch { return new ItemData<int>(0, 0, QUALITIES.QUALITY_BAD); }
        }

        public ItemData<uint> ReadUInt32(DeviceAddress address)
        {
            try
            {
                return new ItemData<uint>(accessor.ReadUInt32(FindPosition(address)), 0, QUALITIES.QUALITY_GOOD);
            }
            catch { return new ItemData<uint>(0, 0, QUALITIES.QUALITY_BAD); }
        }

        public ItemData<ushort> ReadUInt16(DeviceAddress address)
        {
            try
            {
                return new ItemData<ushort>(accessor.ReadUInt16(FindPosition(address)), 0, QUALITIES.QUALITY_GOOD);
            }
            catch { return new ItemData<ushort>(0, 0, QUALITIES.QUALITY_BAD); }
        }

        public ItemData<short> ReadInt16(DeviceAddress address)
        {
            try
            {
                return new ItemData<short>(accessor.ReadInt16(FindPosition(address)), 0, QUALITIES.QUALITY_GOOD);
            }
            catch { return new ItemData<short>(0, 0, QUALITIES.QUALITY_BAD); }
        }

        public ItemData<byte> ReadByte(DeviceAddress address)
        {
            try
            {
                return new ItemData<byte>(accessor.ReadByte(FindPosition(address)), 0, QUALITIES.QUALITY_GOOD);
            }
            catch { return new ItemData<byte>(0, 0, QUALITIES.QUALITY_BAD); }
        }

        public ItemData<string> ReadString(DeviceAddress address, ushort size)
        {
            try
            {
                byte[] bytes = new byte[size];
                int result = accessor.ReadArray(FindPosition(address), bytes, 0, bytes.Length);
                return new ItemData<string>(Encoding.ASCII.GetString(bytes), 0, QUALITIES.QUALITY_GOOD);
            }
            catch { return new ItemData<string>(null, 0, QUALITIES.QUALITY_BAD); }
        }

        public ItemData<float> ReadFloat(DeviceAddress address)
        {
            try
            {
                return new ItemData<float>(accessor.ReadSingle(FindPosition(address)), 0, QUALITIES.QUALITY_GOOD);
            }
            catch { return new ItemData<float>(0f, 0, QUALITIES.QUALITY_BAD); }
        }

        public ItemData<bool> ReadBit(DeviceAddress address)
        {
            try
            {
                return new ItemData<bool>(accessor.ReadBoolean(FindPosition(address)), 0, QUALITIES.QUALITY_GOOD);
            }
            catch { return new ItemData<bool>(false, 0, QUALITIES.QUALITY_BAD); }
        }

        public ItemData<object> ReadValue(DeviceAddress address)
        {
            return this.ReadValueEx(address);
        }

        public int WriteBytes(DeviceAddress address, byte[] bit)
        {
            try
            {
                accessor.WriteArray(FindPosition(address), bit, 0, bit.Length);
                return 0;
            }
            catch { return -1; }
        }

        public int WriteBit(DeviceAddress address, bool bit)
        {
            try
            {
                accessor.Write(FindPosition(address), bit);
                return 0;
            }
            catch { return -1; }
        }

        public int WriteBits(DeviceAddress address, byte bits)
        {
            try
            {
                accessor.Write(FindPosition(address), bits);
                return 0;
            }
            catch { return -1; }
        }

        public int WriteInt16(DeviceAddress address, short value)
        {
            try
            {
                accessor.Write(FindPosition(address), value);
                return 0;
            }
            catch { return -1; }
        }

        public int WriteUInt16(DeviceAddress address, ushort value)
        {
            try
            {
                accessor.Write(FindPosition(address), value);
                return 0;
            }
            catch { return -1; }
        }

        public int WriteUInt32(DeviceAddress address, uint value)
        {
            try
            {
                accessor.Write(FindPosition(address), value);
                return 0;
            }
            catch { return -1; }
        }

        public int WriteInt32(DeviceAddress address, int value)
        {
            try
            {
                accessor.Write(FindPosition(address), value);
                return 0;
            }
            catch { return -1; }
        }

        public int WriteFloat(DeviceAddress address, float value)
        {
            try
            {
                accessor.Write(FindPosition(address), value);
                return 0;
            }
            catch { return -1; }
        }

        public int WriteString(DeviceAddress address, string str)
        {
            try
            {
                return WriteBytes(address, Encoding.ASCII.GetBytes(str));
            }
            catch { return -1; }
        }

        public int WriteValue(DeviceAddress address, object value)
        {
            try
            {
                return this.WriteValueEx(address, value);
            }
            catch { return -1; }
        }

        public int Limit
        {
            get { return _count; }
        }

        public FileData[] ReadAll(short groupId)
        {
            if (mapp == null) return null;
            FileData[] hdata = new FileData[_count];
            for (int i = 0; i < _count; i++)
            {
                int pos = 4 + i * 6;
                hdata[i].ID = accessor.ReadInt16(pos);
                pos = accessor.ReadInt32(pos + 2);
                DataType type = (DataType)accessor.ReadByte(pos);
                pos++;
                byte len = accessor.ReadByte(pos);
                pos++;
                switch (type)
                {
                    case DataType.BOOL:
                        hdata[i].Value.Boolean = accessor.ReadBoolean(pos);
                        break;
                    case DataType.BYTE:
                        hdata[i].Value.Byte = accessor.ReadByte(pos);
                        break;
                    case DataType.WORD:
                        hdata[i].Value.Word = accessor.ReadUInt16(pos);
                        break;
                    case DataType.SHORT:
                        hdata[i].Value.Int16 = accessor.ReadInt16(pos);
                        break;
                    case DataType.DWORD:
                        hdata[i].Value.DWord = accessor.ReadUInt32(pos);
                        break;
                    case DataType.INT:
                        hdata[i].Value.Int32 = accessor.ReadInt32(pos);
                        break;
                    case DataType.FLOAT:
                        hdata[i].Value.Single = accessor.ReadSingle(pos);
                        break;
                    case DataType.STR:
                        byte[] bytes = new byte[len];
                        accessor.ReadArray(pos, bytes, 0, len);
                        hdata[i].Text = Encoding.ASCII.GetString(bytes);
                        break;
                }
            }
            return hdata;
        }
    }
}

using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace DataService
{
    public sealed class ByteCacheReader : ICache
    {
        byte[] _cache;
        public Array Cache { get { return _cache; } }

        public int ByteCount
        {
            get { return 1; }
        }

        int _size;
        public int Size
        {
            get { return _size; }
            set { _size = value; this._cache = new byte[_size]; }
        }

        public ByteCacheReader() { }

        public ByteCacheReader(int size)
        {
            this.Size = size;
        }

        public int GetOffset(DeviceAddress start, DeviceAddress end)
        {
            return start.Area == end.Area && start.DBNumber == end.DBNumber ? start.Start - end.Start : ushort.MaxValue;
        }

        public ItemData<bool> ReadBit(DeviceAddress address)
        {
            return new ItemData<bool>((_cache[address.CacheIndex] & (1 << address.Bit)) != 0, 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<int> ReadInt32(DeviceAddress address)
        {
            return new ItemData<int>(BitConverter.ToInt32(_cache, address.CacheIndex), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<uint> ReadUInt32(DeviceAddress address)
        {
            return new ItemData<uint>(BitConverter.ToUInt32(_cache, address.CacheIndex), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<ushort> ReadUInt16(DeviceAddress address)
        {
            return new ItemData<ushort>(BitConverter.ToUInt16(_cache, address.CacheIndex), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<short> ReadInt16(DeviceAddress address)
        {
            return new ItemData<short>(BitConverter.ToInt16(_cache, address.CacheIndex), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<byte> ReadByte(DeviceAddress address)
        {
            return new ItemData<byte>(_cache[address.CacheIndex], 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<string> ReadString(DeviceAddress address, ushort size = 0xFF)
        {
            return new ItemData<string>(Encoding.ASCII.GetString(_cache, address.CacheIndex, size), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<float> ReadFloat(DeviceAddress address)
        {
            return new ItemData<float>(BitConverter.ToSingle(_cache, address.CacheIndex), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<object> ReadValue(DeviceAddress address)
        {
            return this.ReadValueEx(address);
        }

        public int WriteBit(DeviceAddress address, bool bit)
        {
            _cache[address.CacheIndex] |= (byte)(1 << address.Bit);
            return 0;
        }

        public int WriteBits(DeviceAddress address, byte bits)
        {
            _cache[address.CacheIndex] = bits;
            return 0;
        }

        public unsafe int WriteInt16(DeviceAddress address, short value)
        {
            fixed (byte* p1 = _cache)
            {
                Marshal.WriteInt16((IntPtr)(p1 + address.CacheIndex), value);
            }
            return 0;
        }

        public unsafe int WriteUInt16(DeviceAddress address, ushort value)
        {
            fixed (byte* p1 = _cache)
            {
                Marshal.WriteInt16((IntPtr)(p1 + address.CacheIndex), (short)value);
            }
            return 0;
        }

        public unsafe int WriteUInt32(DeviceAddress address, uint value)
        {
            fixed (byte* p1 = _cache)
            {
                Marshal.WriteInt32((IntPtr)(p1 + address.CacheIndex), (int)value);
            }
            return 0;
        }

        public unsafe int WriteInt32(DeviceAddress address, int value)
        {
            fixed (byte* p1 = _cache)
            {
                Marshal.WriteInt32((IntPtr)(p1 + address.CacheIndex), value);
            }
            return 0;
        }

        public unsafe int WriteFloat(DeviceAddress address, float value)
        {
            fixed (byte* p1 = _cache)
            {
                Marshal.WriteInt32((IntPtr)(p1 + address.CacheIndex), *(int*)&value);
            }
            return 0;
        }

        public int WriteString(DeviceAddress address, string str)
        {
            byte[] b = Encoding.ASCII.GetBytes(str);
            int index = address.CacheIndex;
            Array.Copy(_cache, index, b, 0, b.Length);
            return 0;
        }

        public int WriteValue(DeviceAddress address, object value)
        {
            return this.WriteValueEx(address, value);
        }

        public byte[] ReadBytes(DeviceAddress address, ushort size)
        {
            byte[] bytes = new byte[size];
            Array.Copy(_cache, address.CacheIndex, bytes, 0, size);
            return bytes;
        }

        public int WriteBytes(DeviceAddress address, byte[] bit)
        {
            if (bit != null && bit.Length > 0)
            {
                Array.Copy(bit, 0, _cache, address.CacheIndex, bit.Length);
                return 0;
            }
            return -1;
        }
    }

    public sealed class NetByteCacheReader : ICache
    {
        byte[] _cache;
        public Array Cache { get { return _cache; } }

        public int ByteCount
        {
            get { return 1; }
        }

        int _size;
        public int Size
        {
            get { return _size; }
            set { _size = value; this._cache = new byte[_size]; }
        }

        public NetByteCacheReader() { }

        public NetByteCacheReader(int size)
        {
            this.Size = size;
        }

        public int GetOffset(DeviceAddress start, DeviceAddress end)
        {
            return start.Area == end.Area && start.DBNumber == end.DBNumber ? start.Start - end.Start : ushort.MaxValue;
        }

        public ItemData<bool> ReadBit(DeviceAddress address)
        {
            return new ItemData<bool>((_cache[address.CacheIndex] & (1 << address.Bit)) != 0, 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<int> ReadInt32(DeviceAddress address)
        {
            return new ItemData<int>(Utility.NetToInt32(_cache, address.CacheIndex), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<uint> ReadUInt32(DeviceAddress address)
        {
            return new ItemData<uint>((uint)Utility.NetToInt32(_cache, address.CacheIndex), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<ushort> ReadUInt16(DeviceAddress address)
        {
            return new ItemData<ushort>((ushort)Utility.NetToInt16(_cache, address.CacheIndex), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<short> ReadInt16(DeviceAddress address)
        {
            return new ItemData<short>(Utility.NetToInt16(_cache, address.CacheIndex), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<byte> ReadByte(DeviceAddress address)
        {
            return new ItemData<byte>(_cache[address.CacheIndex], 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<string> ReadString(DeviceAddress address, ushort size = 0xFF)
        {
            return new ItemData<string>(Utility.ConvertToString(_cache, address.CacheIndex, size), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<float> ReadFloat(DeviceAddress address)
        {
            return new ItemData<float>(Utility.NetToSingle(_cache, address.CacheIndex), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<object> ReadValue(DeviceAddress address)
        {
            return this.ReadValueEx(address);
        }

        public int WriteBit(DeviceAddress address, bool bit)
        {
            _cache[address.CacheIndex] |= (byte)(1 << address.Bit);
            return 0;
        }

        public int WriteBits(DeviceAddress address, byte bits)
        {
            _cache[address.CacheIndex] = bits;
            return 0;
        }

        public unsafe int WriteInt16(DeviceAddress address, short value)
        {
            fixed (byte* p1 = _cache)
            {
                Marshal.WriteInt16((IntPtr)(p1 + address.CacheIndex), IPAddress.HostToNetworkOrder(value));
            }
            return 0;
        }

        public unsafe int WriteUInt16(DeviceAddress address, ushort value)
        {
            fixed (byte* p1 = _cache)
            {
                Marshal.WriteInt16((IntPtr)(p1 + address.CacheIndex), IPAddress.HostToNetworkOrder((short)value));
            }
            return 0;
        }

        public unsafe int WriteUInt32(DeviceAddress address, uint value)
        {
            fixed (byte* p1 = _cache)
            {
                Marshal.WriteInt32((IntPtr)(p1 + address.CacheIndex), IPAddress.HostToNetworkOrder((int)value));
            }
            return 0;
        }

        public unsafe int WriteInt32(DeviceAddress address, int value)
        {
            fixed (byte* p1 = _cache)
            {
                Marshal.WriteInt32((IntPtr)(p1 + address.CacheIndex), IPAddress.HostToNetworkOrder(value));
            }
            return 0;
        }

        public unsafe int WriteFloat(DeviceAddress address, float value)
        {
            fixed (byte* p1 = _cache)
            {
                Marshal.WriteInt32((IntPtr)(p1 + address.CacheIndex), IPAddress.HostToNetworkOrder(*(int*)(&value)));
            }
            return 0;
        }

        public int WriteString(DeviceAddress address, string str)
        {
            byte[] b = Encoding.ASCII.GetBytes(str);
            int index = address.CacheIndex;
            Array.Copy(_cache, index, b, 0, b.Length);
            return 0;
        }

        public int WriteValue(DeviceAddress address, object value)
        {
            return this.WriteValueEx(address, value);
        }

        public byte[] ReadBytes(DeviceAddress address, ushort size)
        {
            byte[] bytes = new byte[size];
            Array.Copy(_cache, address.CacheIndex, bytes, 0, size);
            return bytes;
        }

        public int WriteBytes(DeviceAddress address, byte[] bit)
        {
            if (bit != null && bit.Length > 0)
            {
                Array.Copy(bit, 0, _cache, address.CacheIndex, bit.Length);
                return 0;
            }
            return -1;
        }

    }

    public sealed class ShortCacheReader : ICache
    {
        short[] _cache;
        public Array Cache
        {
            get
            {
                return _cache;
            }
        }

        public int ByteCount
        {
            get { return 2; }
        }

        int _size;
        public int Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
                this._cache = new short[_size];
            }
        }

        public ShortCacheReader()
        {
        }

        public ShortCacheReader(int size)
        {
            this.Size = size;
        }

        public int GetOffset(DeviceAddress start, DeviceAddress end)
        {
            return start.Area == end.Area && start.DBNumber == end.DBNumber ? start.Start - end.Start : ushort.MaxValue;
        }

        public unsafe ItemData<bool> ReadBit(DeviceAddress address)
        {
            return new ItemData<bool>((_cache[address.CacheIndex] & (1 << address.Bit)) != 0, 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<int> ReadInt32(DeviceAddress address)
        {
            int startIndex = address.CacheIndex;
            int result;
            if (startIndex == _cache.Length - 1)
            {
                result = _cache[startIndex];
            }
            else
            {
                result = (_cache[startIndex + 1] << 16) | ((ushort)_cache[startIndex]);
            }
            return new ItemData<int>(result, 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<uint> ReadUInt32(DeviceAddress address)
        {
            return new ItemData<uint>((uint)ReadInt32(address).Value, 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<short> ReadInt16(DeviceAddress address)
        {
            return new ItemData<short>(_cache[address.CacheIndex], 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<ushort> ReadUInt16(DeviceAddress address)
        {
            return new ItemData<ushort>((ushort)_cache[address.CacheIndex], 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<byte> ReadByte(DeviceAddress address)
        {
            return new ItemData<byte>((byte)_cache[address.CacheIndex], 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<string> ReadString(DeviceAddress address, ushort size)
        {
            byte[] buffer = new byte[size];
            Buffer.BlockCopy(_cache, 2 * address.CacheIndex, buffer, 0, size);
            return new ItemData<string>(Encoding.ASCII.GetString(buffer).Trim(), 0, QUALITIES.QUALITY_GOOD);
        }

        public unsafe ItemData<float> ReadFloat(DeviceAddress address)
        {
            int startIndex = address.CacheIndex;
            int result;
            if (startIndex == _cache.Length - 1)
            {
                result = _cache[startIndex];
            }
            else
            {
                result = (_cache[startIndex] << 16) | ((ushort)_cache[startIndex + 1]);
            }
            return new ItemData<float>(*(((float*)&result)), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<object> ReadValue(DeviceAddress address)
        {
            return this.ReadValueEx(address);
        }

        public int WriteBit(DeviceAddress address, bool bit)
        {
            _cache[address.CacheIndex] |= (short)(1 << address.Bit);
            return 0;
        }

        public int WriteBits(DeviceAddress address, byte bits)
        {
            _cache[address.CacheIndex] = bits;
            return 0;
        }

        public int WriteInt16(DeviceAddress address, short value)
        {
            _cache[address.CacheIndex] = value;
            return 0;
        }

        public int WriteUInt16(DeviceAddress address, ushort value)
        {
            _cache[address.CacheIndex] = (short)value;
            return 0;
        }

        public unsafe int WriteUInt32(DeviceAddress address, uint value)
        {
            fixed (short* p1 = _cache)
            {
                Marshal.WriteInt32((IntPtr)(p1 + address.CacheIndex), (int)value);
            }
            return 0;
        }

        public unsafe int WriteInt32(DeviceAddress address, int value)
        {
            fixed (short* p1 = _cache)
            {
                Marshal.WriteInt32((IntPtr)(p1 + address.CacheIndex), value);
            }
            return 0;
        }

        public unsafe int WriteFloat(DeviceAddress address, float value)
        {
            fixed (short* p1 = _cache)
            {
                Marshal.WriteInt32((IntPtr)(p1 + address.CacheIndex), *(int*)&value);
            }
            return 0;
        }

        public int WriteString(DeviceAddress address, string str)
        {
            byte[] b = Encoding.ASCII.GetBytes(str);
            int index = address.CacheIndex;
            Buffer.BlockCopy(_cache, index, b, 0, b.Length);
            return 0;
        }

        public int WriteValue(DeviceAddress address, object value)
        {
            return this.WriteValueEx(address, value);
        }

        public byte[] ReadBytes(DeviceAddress address, ushort size)
        {
            byte[] bytes = new byte[2 * size];
            Buffer.BlockCopy(_cache, address.CacheIndex, bytes, 0, bytes.Length);
            return bytes;
        }

        public int WriteBytes(DeviceAddress address, byte[] bit)
        {
            if (bit != null && bit.Length > 0)
            {
                Buffer.BlockCopy(bit, 0, _cache, address.CacheIndex, bit.Length);
                return 0;
            }
            return -1;
        }
    }

    public sealed class NetShortCacheReader : ICache
    {
        short[] _cache;
        public Array Cache
        {
            get
            {
                return _cache;
            }
        }

        public int ByteCount
        {
            get { return 2; }
        }

        int _size;
        public int Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
                this._cache = new short[_size];
            }
        }

        public NetShortCacheReader()
        {
        }

        public NetShortCacheReader(int size)
        {
            this.Size = size;
        }

        public int GetOffset(DeviceAddress start, DeviceAddress end)
        {
            return start.Area == end.Area && start.DBNumber == end.DBNumber ? start.Start - end.Start : ushort.MaxValue;
        }

        public unsafe ItemData<bool> ReadBit(DeviceAddress address)
        {
            return new ItemData<bool>((_cache[address.CacheIndex] & (1 << address.Bit)) != 0, 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<int> ReadInt32(DeviceAddress address)
        {
            int startIndex = address.CacheIndex;
            int result;
            if (startIndex == _cache.Length - 1)
            {
                result = _cache[startIndex];
            }
            else
            {
                result = (IPAddress.HostToNetworkOrder(_cache[startIndex]) << 16) | ((ushort)IPAddress.HostToNetworkOrder(_cache[startIndex + 1]));
            }
            return new ItemData<int>(result, 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<uint> ReadUInt32(DeviceAddress address)
        {
            return new ItemData<uint>((uint)ReadInt32(address).Value, 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<ushort> ReadUInt16(DeviceAddress address)
        {
            return new ItemData<ushort>((ushort)IPAddress.HostToNetworkOrder(_cache[address.CacheIndex]), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<short> ReadInt16(DeviceAddress address)
        {
            return new ItemData<short>(IPAddress.HostToNetworkOrder(_cache[address.CacheIndex]), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<byte> ReadByte(DeviceAddress address)
        {
            return new ItemData<byte>((byte)IPAddress.HostToNetworkOrder(_cache[address.CacheIndex]), 0, QUALITIES.QUALITY_GOOD);
        }

        public unsafe ItemData<string> ReadString(DeviceAddress address, ushort size)
        {
            short[] sarray = new short[size / 2];
            int index = address.CacheIndex;
            for (int i = 0; i < sarray.Length; i++)
            {
                sarray[i] = IPAddress.HostToNetworkOrder(_cache[index + i]);
            }
            byte[] buffer = new byte[size];
            Buffer.BlockCopy(sarray, 0, buffer, 0, size);
            return new ItemData<string>(Encoding.ASCII.GetString(buffer).Trim(), 0, QUALITIES.QUALITY_GOOD);
        }

        public unsafe ItemData<float> ReadFloat(DeviceAddress address)
        {
            int startIndex = address.CacheIndex;
            int result;
            if (startIndex == _cache.Length - 1)
            {
                result = _cache[startIndex];
            }
            else
            {
                result = (IPAddress.HostToNetworkOrder(_cache[startIndex]) << 16) | ((ushort)IPAddress.HostToNetworkOrder(_cache[startIndex + 1]));
            }
            return new ItemData<float>(*(((float*)&result)), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<object> ReadValue(DeviceAddress address)
        {
            return this.ReadValueEx(address);
        }

        public int WriteBit(DeviceAddress address, bool bit)
        {
            _cache[address.CacheIndex] |= (short)(1 << address.Bit);
            return 0;
        }

        public int WriteBits(DeviceAddress address, byte bits)
        {
            _cache[address.CacheIndex] = bits;
            return 0;
        }

        public int WriteInt16(DeviceAddress address, short value)
        {
            _cache[address.CacheIndex] = value;
            return 0;
        }

        public int WriteUInt16(DeviceAddress address, ushort value)
        {
            _cache[address.CacheIndex] = (short)value;
            return 0;
        }

        public unsafe int WriteInt32(DeviceAddress address, int value)
        {
            fixed (short* p1 = _cache)
            {
                Marshal.WriteInt32((IntPtr)(p1 + address.CacheIndex), IPAddress.HostToNetworkOrder(value));
            }
            return 0;
        }

        public unsafe int WriteUInt32(DeviceAddress address, uint value)
        {
            fixed (short* p1 = _cache)
            {
                Marshal.WriteInt32((IntPtr)(p1 + address.CacheIndex), IPAddress.HostToNetworkOrder((int)value));
            }
            return 0;
        }

        public unsafe int WriteFloat(DeviceAddress address, float value)
        {
            fixed (short* p1 = _cache)
            {
                Marshal.WriteInt32((IntPtr)(p1 + address.CacheIndex), IPAddress.HostToNetworkOrder(*(int*)&value));
            }
            return 0;
        }

        public int WriteString(DeviceAddress address, string str)
        {
            byte[] b = Encoding.ASCII.GetBytes(str);
            int index = address.CacheIndex;
            Buffer.BlockCopy(_cache, index, b, 0, b.Length);
            return 0;
        }

        public int WriteValue(DeviceAddress address, object value)
        {
            return this.WriteValueEx(address, value);
        }

        public byte[] ReadBytes(DeviceAddress address, ushort size)
        {
            byte[] bytes = new byte[2 * size];
            Buffer.BlockCopy(_cache, address.CacheIndex, bytes, 0, bytes.Length);
            return bytes;
        }

        public int WriteBytes(DeviceAddress address, byte[] bit)
        {
            if (bit != null && bit.Length > 0)
            {
                Buffer.BlockCopy(bit, 0, _cache, address.CacheIndex, bit.Length);
                return 0;
            }
            return -1;
        }
    }

    public sealed class IntCacheReader : ICache
    {
        int[] _cache;

        public Array Cache
        {
            get
            {
                return _cache;
            }
        }

        public int ByteCount
        {
            get { return 4; }
        }

        int _size;
        public int Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
                this._cache = new int[_size];
            }
        }

        public IntCacheReader()
        {
        }

        public IntCacheReader(int size)
        {
            this.Size = size;
        }

        public int GetOffset(DeviceAddress start, DeviceAddress end)
        {
            return start.Area == end.Area && start.DBNumber == end.DBNumber ? start.Start - end.Start : ushort.MaxValue;
        }

        public unsafe ItemData<bool> ReadBit(DeviceAddress address)
        {
            return new ItemData<bool>((_cache[address.CacheIndex] & (1 << address.Bit)) != 0, 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<int> ReadInt32(DeviceAddress address)
        {
            return new ItemData<int>(_cache[address.CacheIndex], 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<uint> ReadUInt32(DeviceAddress address)
        {
            return new ItemData<uint>((uint)_cache[address.CacheIndex], 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<ushort> ReadUInt16(DeviceAddress address)
        {
            return new ItemData<ushort>((ushort)_cache[address.CacheIndex], 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<short> ReadInt16(DeviceAddress address)
        {
            return new ItemData<short>((short)(_cache[address.CacheIndex]), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<byte> ReadByte(DeviceAddress address)
        {
            return new ItemData<byte>((byte)_cache[address.CacheIndex], 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<string> ReadString(DeviceAddress address, ushort size)
        {
            byte[] buffer = new byte[size];
            Buffer.BlockCopy(_cache, 4 * address.CacheIndex, buffer, 0, size);
            return new ItemData<string>(Encoding.ASCII.GetString(buffer).Trim(), 0, QUALITIES.QUALITY_GOOD);
        }

        public unsafe ItemData<float> ReadFloat(DeviceAddress address)
        {
            int result = _cache[address.CacheIndex];
            return new ItemData<float>(*(((float*)&result)), 0, QUALITIES.QUALITY_GOOD);//强制将4字节转换为浮点格式
        }

        public ItemData<object> ReadValue(DeviceAddress address)
        {
            return this.ReadValueEx(address);
        }

        public int WriteBit(DeviceAddress address, bool bit)
        {
            _cache[address.CacheIndex] |= (1 << address.Bit);
            return 0;
        }

        public int WriteBits(DeviceAddress address, byte bits)
        {
            _cache[address.CacheIndex] = bits;
            return 0;
        }

        public unsafe int WriteInt16(DeviceAddress address, short value)
        {
            _cache[address.CacheIndex] = value;
            return 0;
        }

        public unsafe int WriteUInt16(DeviceAddress address, ushort value)
        {
            _cache[address.CacheIndex] = value;
            return 0;
        }

        public unsafe int WriteUInt32(DeviceAddress address, uint value)
        {
            _cache[address.CacheIndex] = (int)value;
            return 0;
        }

        public unsafe int WriteInt32(DeviceAddress address, int value)
        {
            _cache[address.CacheIndex] = value;
            return 0;
        }

        public unsafe int WriteFloat(DeviceAddress address, float value)
        {
            fixed (int* p1 = _cache)
            {
                Marshal.WriteInt32((IntPtr)(p1 + address.CacheIndex), *(int*)&value);
            }
            return 0;
        }

        public int WriteString(DeviceAddress address, string str)
        {
            byte[] b = Encoding.ASCII.GetBytes(str);
            int index = address.CacheIndex;
            Buffer.BlockCopy(_cache, index, b, 0, b.Length);
            return 0;
        }

        public int WriteValue(DeviceAddress address, object value)
        {
            return this.WriteValueEx(address, value);
        }

        public byte[] ReadBytes(DeviceAddress address, ushort size)
        {
            byte[] bytes = new byte[4 * size];
            Buffer.BlockCopy(_cache, address.CacheIndex, bytes, 0, bytes.Length);
            return bytes;
        }

        public int WriteBytes(DeviceAddress address, byte[] bit)
        {
            if (bit != null && bit.Length > 0)
            {
                Buffer.BlockCopy(bit, 0, _cache, address.CacheIndex, bit.Length);
                return 0;
            }
            return -1;
        }
    }

    public sealed class FloatCacheReader : ICache
    {
        float[] _cache;

        public Array Cache
        {
            get
            {
                return _cache;
            }
        }

        public int ByteCount
        {
            get { return 4; }
        }

        int _size;
        public int Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
                this._cache = new float[_size];
            }
        }

        public FloatCacheReader()
        {
        }

        public FloatCacheReader(int size)
        {
            this.Size = size;
        }

        public int GetOffset(DeviceAddress start, DeviceAddress end)
        {
            return start.Area == end.Area && start.DBNumber == end.DBNumber ? start.Start - end.Start : int.MaxValue;
        }

        public unsafe ItemData<bool> ReadBit(DeviceAddress address)
        {
            return new ItemData<bool>(((int)_cache[address.CacheIndex] & (1 << address.Bit)) != 0, 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<int> ReadInt32(DeviceAddress address)
        {

            return new ItemData<int>((int)_cache[address.CacheIndex], 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<uint> ReadUInt32(DeviceAddress address)
        {
            return new ItemData<uint>((uint)_cache[address.CacheIndex], 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<ushort> ReadUInt16(DeviceAddress address)
        {
            return new ItemData<ushort>((ushort)_cache[address.CacheIndex], 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<short> ReadInt16(DeviceAddress address)
        {
            return new ItemData<short>((short)(_cache[address.CacheIndex]), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<byte> ReadByte(DeviceAddress address)
        {
            return new ItemData<byte>((byte)_cache[address.CacheIndex], 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<string> ReadString(DeviceAddress address, ushort size)
        {
            byte[] buffer = new byte[size];
            Buffer.BlockCopy(_cache, 4 * address.CacheIndex, buffer, 0, size);
            return new ItemData<string>(Encoding.ASCII.GetString(buffer).Trim(), 0, QUALITIES.QUALITY_GOOD);
        }

        public unsafe ItemData<float> ReadFloat(DeviceAddress address)
        {
            return new ItemData<float>(_cache[address.CacheIndex], 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<object> ReadValue(DeviceAddress address)
        {
            return this.ReadValueEx(address);
        }

        public int WriteBit(DeviceAddress address, bool bit)
        {
            _cache[address.CacheIndex] = (int)_cache[address.CacheIndex] | (1 << address.Bit);
            return 0;
        }

        public int WriteBits(DeviceAddress address, byte bits)
        {
            _cache[address.CacheIndex] = bits;
            return 0;
        }

        public int WriteInt16(DeviceAddress address, short value)
        {
            _cache[address.CacheIndex] = value;
            return 0;
        }

        public int WriteUInt16(DeviceAddress address, ushort value)
        {
            _cache[address.CacheIndex] = value;
            return 0;
        }

        public int WriteUInt32(DeviceAddress address, uint value)
        {
            _cache[address.CacheIndex] = value;
            return 0;
        }

        public int WriteInt32(DeviceAddress address, int value)
        {
            _cache[address.CacheIndex] = value;
            return 0;
        }

        public int WriteFloat(DeviceAddress address, float value)
        {
            _cache[address.CacheIndex] = value;
            return 0;
        }

        public int WriteString(DeviceAddress address, string str)
        {
            byte[] b = Encoding.ASCII.GetBytes(str);
            int index = address.CacheIndex;
            Buffer.BlockCopy(_cache, index, b, 0, b.Length);
            return 0;
        }

        public int WriteValue(DeviceAddress address, object value)
        {
            return this.WriteValueEx(address, value);
        }

        public byte[] ReadBytes(DeviceAddress address, ushort size)
        {
            byte[] bytes = new byte[4 * size];
            Buffer.BlockCopy(_cache, address.CacheIndex, bytes, 0, bytes.Length);
            return bytes;
        }

        public int WriteBytes(DeviceAddress address, byte[] bit)
        {
            if (bit != null && bit.Length > 0)
            {
                Buffer.BlockCopy(bit, 0, _cache, address.CacheIndex, bit.Length);
                return 0;
            }
            return -1;
        }
    }
}

using System;
using System.Runtime.InteropServices;

namespace DataService
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DeviceAddress : IComparable<DeviceAddress>
    {
        public int Area;
        public int Start;
        public ushort DBNumber;
        public ushort DataSize;
        public ushort CacheIndex;
        public byte Bit;
        public DataType VarType;

        public DeviceAddress(int area, ushort dbnumber, ushort cIndex, int start, ushort size, byte bit, DataType type)
        {
            Area = area;
            DBNumber = dbnumber;
            CacheIndex = cIndex;
            Start = start;
            DataSize = size;
            Bit = bit;
            VarType = type;
        }

        public static readonly DeviceAddress Empty = new DeviceAddress(0, 0, 0, 0, 0, 0, DataType.NONE);

        public int CompareTo(DeviceAddress other)
        {
            return this.Area > other.Area ? 1 :
                this.Area < other.Area ? -1 :
                this.DBNumber > other.DBNumber ? 1 :
                this.DBNumber < other.DBNumber ? -1 :
                this.Start > other.Start ? 1 :
                this.Start < other.Start ? -1 :
                this.Bit > other.Bit ? 1 :
                this.Bit < other.Bit ? -1 : 0;
        }
    }

}

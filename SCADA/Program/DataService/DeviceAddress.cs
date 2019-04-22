﻿using System;
using System.Runtime.InteropServices;

namespace DataService {
    [StructLayout(LayoutKind.Sequential)]
    public struct DeviceAddress : IComparable<DeviceAddress> {
        public int Area;
        public int Start;
        public ushort DBNumber;
        public ushort DataSize;
        public ushort CacheIndex;
        public byte Bit;
        public DataType VarType;

        public DeviceAddress(int area, ushort dbnumber, ushort cIndex, int start, ushort size, byte bit,
            DataType type) {
            Area = area;
            DBNumber = dbnumber;
            CacheIndex = cIndex;
            Start = start;
            DataSize = size;
            Bit = bit;
            VarType = type;
        }

        public static readonly DeviceAddress Empty = new DeviceAddress(0, 0, 0, 0, 0, 0, DataType.NONE);

        public int CompareTo(DeviceAddress other) {
            return Area > other.Area ? 1 :
                Area < other.Area ? -1 :
                DBNumber > other.DBNumber ? 1 :
                DBNumber < other.DBNumber ? -1 :
                Start > other.Start ? 1 :
                Start < other.Start ? -1 :
                Bit > other.Bit ? 1 :
                Bit < other.Bit ? -1 : 0;
        }
    }
}
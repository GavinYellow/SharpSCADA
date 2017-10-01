using System.Runtime.InteropServices;
using System;

namespace DataService
{
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public struct Storage
    {
        // Fields
        [FieldOffset(0)]
        public bool Boolean;
        [FieldOffset(0)]
        public byte Byte;
        [FieldOffset(0)]
        public short Int16;
        [FieldOffset(0)]
        public int Int32;
        [FieldOffset(0)]
        public float Single;

        public static readonly Storage Empty ;

        static Storage()
        {
            Empty = new Storage();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Type type = obj.GetType();
            if (type == typeof(Storage))
                return this.Int32 == ((Storage)obj).Int32;
            else
            {
                if (type == typeof(Int32))
                    return this.Int32 == (Int32)obj;
                if (type == typeof(Int16))
                    return this.Int16 == (Int16)obj;
                if (type == typeof(Byte))
                    return this.Byte == (Byte)obj;
                if (type == typeof(Boolean))
                    return this.Boolean == (Boolean)obj;
                if (type == typeof(Single))
                    return this.Single == (Single)obj;
                if (type == typeof(String))
                    return this.ToString() == obj.ToString();
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Int32.GetHashCode();
        }

        public static bool operator ==(Storage x, Storage y)
        {
            return x.Int32 == y.Int32;
        }

        public static bool operator !=(Storage x, Storage y)
        {
            return x.Int32 != y.Int32;
        } 
    }

    public enum QUALITIES : short
    {
        // Fields
        LIMIT_CONST = 3,
        LIMIT_HIGH = 2,
        LIMIT_LOW = 1,
        //LIMIT_MASK = 3,
        //LIMIT_OK = 0,
        QUALITY_BAD = 0,
        QUALITY_COMM_FAILURE = 0x18,
        QUALITY_CONFIG_ERROR = 4,
        QUALITY_DEVICE_FAILURE = 12,
        QUALITY_EGU_EXCEEDED = 0x54,
        QUALITY_GOOD = 0xc0,
        QUALITY_LAST_KNOWN = 20,
        QUALITY_LAST_USABLE = 0x44,
        QUALITY_LOCAL_OVERRIDE = 0xd8,
        QUALITY_MASK = 0xc0,
        QUALITY_NOT_CONNECTED = 8,
        QUALITY_OUT_OF_SERVICE = 0x1c,
        QUALITY_SENSOR_CAL = 80,
        QUALITY_SENSOR_FAILURE = 0x10,
        QUALITY_SUB_NORMAL = 0x58,
        QUALITY_UNCERTAIN = 0x40,
        QUALITY_WAITING_FOR_INITIAL_DATA = 0x20,
        STATUS_MASK = 0xfc,
    }
}
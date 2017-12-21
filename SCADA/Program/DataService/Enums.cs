using System;

namespace DataService
{

    public enum DataSource
    {
        Cache = 1,
        Device = 2
    }

    public enum DataType : byte
    {
        NONE = 0,
        BOOL = 1,
        BYTE = 3,
        SHORT = 4,
        WORD = 5,
        DWORD = 6,
        INT = 7,
        FLOAT = 8,
        SYS = 9,
        STR = 11
    }

    [Flags]
    public enum ByteOrder : byte
    {
        None = 0,
        BigEndian = 1,
        LittleEndian = 2,
        Network = 4,
        Host = 8
    }


    [Flags]
    public enum AlarmType
    {
        None = 0,
        Level = 1,
        Dev = 2,
        Dsc = 4,
        ROC = 8,
        Quality = 16,
        Complex = 32,
        WordDsc = 64
    }

    [Flags]
    public enum SubAlarmType
    {
        None = 0,
        LoLo = 1,
        Low = 2,
        High = 4,
        HiHi = 8,
        MajDev = 16,
        MinDev = 32,
        Dsc = 64,

        BadPV = 128,
        MajROC = 256,
        MinROC = 512
    }

    public enum Severity
    {
        Error = 7,
        High = 6,
        MediumHigh = 5,
        Medium = 4,
        MediumLow = 3,
        Low = 2,
        Information = 1,
        Normal = 0
    }

    [Flags]
    public enum ConditionState : byte
    {
        Acked = 4,
        Actived = 2,
        Enabled = 1
    }

    public enum EventType : byte
    {
        Simple = 1,
        TraceEvent = 2,
        ConditionEvent = 4,
    }

    public enum ConditionType : byte
    {
        Absolute = 0,
        Percent = 1
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

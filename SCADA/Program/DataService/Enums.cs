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
        TIME = 6,
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
}

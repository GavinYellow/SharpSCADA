using System.Runtime.InteropServices;

namespace Tuxeip
{
    public enum Plc_Type { Unknow, PLC5, SLC500, LGX }

    public enum DHP_Channel { Channel_A = 0x01, Channel_B } 

    public enum Data_Type { UNKNOW, BIT, SINT, TUXINT, DINT, REAL, TIMER, COUNTER } 

    public enum PLC_Data_Type
    {
        PLC_BIT = 1,
        PLC_BIT_STRING,
        PLC_BYTE_STRING,
        PLC_INTEGER,
        PLC_TIMER,
        PLC_COUNTER,
        PLC_CONTROL,
        PLC_FLOATING,
        PLC_ARRAY,
        PLC_ADRESS = 15,
        PLC_BCD
    } ;

    public enum LGX_Data_Type
    {
        LGX_BOOL = 0xC1,
        LGX_BITARRAY = 0xD3,
        LGX_SINT = 0xC2,
        LGX_INT = 0xC3,
        LGX_DINT = 0xC4,
        LGX_REAL = 0xCA
    }



    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct Eip_Session
    {
        [FieldOffset(0)]
        int sock;
        [FieldOffset(4)]
        uint Session_Handle;
        [FieldOffset(8)]
        uint Sender_ContextL;
        [FieldOffset(12)]
        uint Sender_ContextH;
        [FieldOffset(16)]
        int timeout;
        [FieldOffset(20)]
        int References;
        [FieldOffset(24)]
        void* Data;
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct Eip_Connection
    {
        /* Connected send parameters */
        [FieldOffset(0)]
        Eip_Session* Session;
        [FieldOffset(4)]
        int References;
        [FieldOffset(8)]
        void* Data;
        [FieldOffset(12)]
        ushort ConnectionSerialNumber;
        [FieldOffset(14)]
        ushort OriginatorVendorID;
        [FieldOffset(16)]
        uint OriginatorSerialNumber;

        [FieldOffset(20)]
        uint OT_ConnID; //originator's CIP Produced session ID

        [FieldOffset(24)]
        uint TO_ConnID; //originator's CIP consumed session ID
        [FieldOffset(28)]
        short packet;
        [FieldOffset(30)]
        byte Path_size;//
        //BYTE Path[0];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct PLC_Read
    {
        public PLC_Data_Type type;
        public int Varcount;
        public int totalsize;
        public int elementsize;
        public uint mask;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct DHP_Header
    {
        public ushort Dest_link;
        public ushort Dest_adress;
        public ushort Src_link;
        public ushort Src_adress;
        //BYTE data; // the PCCC request
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct LGX_Read
    {
        public LGX_Data_Type type;
        public int Varcount;
        public int totalsize;
        public int elementsize;
        public uint mask;
    }
}

using System.Runtime.InteropServices;

namespace Tuxeip
{
    public unsafe class Tuxeip_Class
    {
        public const int MAX_MSG_LEN = 1024; // a verifier

        public const int EIP_PORT = 0xAF12;//44818
        public const int ENCAP_PROTOCOL = 0x0001;

        /* Ethernet IP commands */
        public const int EIP_NOP = 0x0000;
        public const int EIP_LISTTARGETS = 0x0001; // Reserved for legacy RA
        public const int EIP_LISTSERVICES = 0x0004;
        public const int EIP_LISTIDENTITY = 0x0063;
        public const int EIP_LISTINTERFACES = 0x0064;
        public const int EIP_REGISTERSESSION = 0x0065;
        public const int EIP_UNREGISTERSESSION = 0x0066;
        public const int EIP_SENDRRDATA = 0x006F;
        public const int EIP_SENDUNITDATA = 0x0070;
        public const int EIP_INDICATESTATUS = 0x0072;
        public const int EIP_CANCEL = 0x0073;

        // Ethernet IP status code
        public const int EIP_SUCCESS = 0x0000;
        public const int EIP_INVALID_COMMAND = 0x0001;
        public const int EIP_MEMORY = 0x0002;
        public const int EIP_INCORRECT_DATA = 0x0003;
        public const int EIP_INVALID_SESSION_HANDLE = 0x0064;
        public const int EIP_INVALID_LENGTH = 0x0065;
        public const int EIP_UNSUPPORTED_PROTOCOL = 0x0069;

        // Ethernet IP Services Class
        public const int EIP_SC_COMMUNICATION = 0x0100;
        public const int EIP_VERSION = 0x01;
        public const int CIP_DEFAULT_TIMEOUT = 1000;

        private const ushort _OriginatorVendorID = 0xFFFE;
        private const uint _OriginatorSerialNumber = 0x12345678;
        private const byte _Priority = 0x07;
        private const sbyte _TimeOut_Ticks = 0x3f;
        private const short _Parameters = 0x43f8, _TO_Parameters = 0x43f8;
        private const byte _Transport = 0xa3;
        private const sbyte _TimeOutMultiplier = 0x01;

        const string PATH = @"tuxeipAB.dll";
        [DllImport(PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern Eip_Session* _OpenSession(string serveur, int port, int buffer_len, int timeout);
        public static Eip_Session* OpenSession(string serveur)
        {
            return _OpenSession(serveur, EIP_PORT, MAX_MSG_LEN, CIP_DEFAULT_TIMEOUT);
        }

        [DllImport(PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern void CloseSession(Eip_Session* session);

        [DllImport(PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int _RegisterSession(Eip_Session* session);

        [DllImport(PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int _UnRegisterSession(Eip_Session* session);

        [DllImport(PATH, CallingConvention = CallingConvention.Cdecl)]
        private static extern Eip_Connection* _ConnectPLCOverCNET(
        Eip_Session* session,
        Plc_Type Plc,
        byte Priority,
        sbyte TimeOut_Ticks,
        uint TO_ConnID, //originator's CIP consumed session ID
        ushort ConnSerialNumber,// session serial number
        ushort OriginatorVendorID,
        uint OriginatorSerialNumber,
        sbyte TimeOutMultiplier,
        uint RPI,// originator to target packet rate in msec
        byte Transport,
        byte[] path, byte pathsize);
        public static Eip_Connection* ConnectPLCOverCNET(Eip_Session* session, Plc_Type Plc, byte[] path)
        {
            return _ConnectPLCOverCNET(session, Plc, _Priority, _TimeOut_Ticks, 0x12345678, 0x6789,
                _OriginatorVendorID, _OriginatorSerialNumber, _TimeOutMultiplier, 5000, _Transport, path, (byte)path.Length);
        }

        [DllImport(PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern Eip_Connection* _ConnectPLCOverDHP(
        Eip_Session* session,
        Plc_Type Plc,
        byte Priority,
        sbyte TimeOut_Ticks,
        uint TO_ConnID, //originator's CIP consumed session ID
        ushort ConnSerialNumber,// session serial number
        ushort OriginatorVendorID,
        uint OriginatorSerialNumber,
        sbyte TimeOutMultiplier,
        uint RPI,// originator to target packet rate in msec
        byte Transport,
        DHP_Channel channel,
        byte[] path, byte pathsize);
        public static Eip_Connection* ConnectPLCOverDHP(Eip_Session* session, Plc_Type Plc, DHP_Channel channel, byte[] path)
        {
            return _ConnectPLCOverDHP(session, Plc, _Priority, _TimeOut_Ticks, 0x12345678, 0x6789,
                _OriginatorVendorID, _OriginatorSerialNumber, _TimeOutMultiplier, 5000, _Transport, channel, path, (byte)path.Length);
        }

        [DllImport(PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern PLC_Read* _ReadPLCData(
        Eip_Session* session,
        Eip_Connection* connection,
        DHP_Header* dhp,
        byte* routepath, byte routepathsize,
        Plc_Type type, short tns,
        string address, short number);

        [DllImport(PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int _WritePLCData(
        Eip_Session* session,
        Eip_Connection* connection,
        DHP_Header* dhp,
        byte* routepath, byte routepathsize,
        Plc_Type type, short tns,
        string address,
        PLC_Data_Type datatype,
        void* data,
        short number);

        [DllImport(PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int _PCCC_GetValueAsBoolean(PLC_Read* reply, int index);

        [DllImport(PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern float _PCCC_GetValueAsFloat(PLC_Read* reply, int index);

        [DllImport(PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int _PCCC_GetValueAsInteger(PLC_Read* reply, int index);

        [DllImport(PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int _GetLGXValueAsInteger(LGX_Read* reply, int index);

        [DllImport(PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern float _GetLGXValueAsFloat(LGX_Read* reply, int index);

        [DllImport(PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int _Forward_Close(Eip_Connection* connection);

        [DllImport(PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern LGX_Read* _ReadLgxData(
        Eip_Session* session,
        Eip_Connection* connection,
        string adress,
        ushort number);

        [DllImport(PATH, CallingConvention = CallingConvention.Cdecl)]
        public static extern int _WriteLgxData(
        Eip_Session* session,
        Eip_Connection* connection,
        string adress,
        LGX_Data_Type datatype,
        void* data,
            //int datasize,
        short number);

    }
}

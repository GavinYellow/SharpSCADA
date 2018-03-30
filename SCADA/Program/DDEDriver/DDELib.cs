using System;
using System.Runtime.InteropServices;

namespace DDEDriver
{
    public delegate IntPtr DdeCallback(
       int uType, ConversionFormat uFmt, IntPtr hConv, IntPtr hsz1, IntPtr hsz2, IntPtr hData, uint dwData1, uint dwData2);

    public static class Ddeml
    {
        public const int MAX_STRING_SIZE = 255;

        public const int APPCMD_CLIENTONLY = unchecked((int)0x00000010);
        public const int APPCMD_FILTERINITS = unchecked((int)0x00000020);
        public const int APPCMD_MASK = unchecked((int)0x00000FF0);
        public const int APPCLASS_STANDARD = unchecked((int)0x00000000);
        public const int APPCLASS_MONITOR = unchecked((int)0x00000001);
        public const int APPCLASS_MASK = unchecked((int)0x0000000F);

        public const int CBR_BLOCK = unchecked((int)0xFFFFFFFF);

        public const int CBF_FAIL_SELFCONNECTIONS = unchecked((int)0x00001000);
        public const int CBF_FAIL_CONNECTIONS = unchecked((int)0x00002000);
        public const int CBF_FAIL_ADVISES = unchecked((int)0x00004000);
        public const int CBF_FAIL_EXECUTES = unchecked((int)0x00008000);
        public const int CBF_FAIL_POKES = unchecked((int)0x00010000);
        public const int CBF_FAIL_REQUESTS = unchecked((int)0x00020000);
        public const int CBF_FAIL_ALLSVRXACTIONS = unchecked((int)0x0003f000);
        public const int CBF_SKIP_CONNECT_CONFIRMS = unchecked((int)0x00040000);
        public const int CBF_SKIP_REGISTRATIONS = unchecked((int)0x00080000);
        public const int CBF_SKIP_UNREGISTRATIONS = unchecked((int)0x00100000);
        public const int CBF_SKIP_DISCONNECTS = unchecked((int)0x00200000);
        public const int CBF_SKIP_ALLNOTIFICATIONS = unchecked((int)0x003c0000);

        public const int CF_TEXT = 1;

        public const int CP_WINANSI = 1004;
        public const int CP_WINUNICODE = 1200;

        public const int DDE_FACK = unchecked((int)0x8000);
        public const int DDE_FBUSY = unchecked((int)0x4000);
        public const int DDE_FDEFERUPD = unchecked((int)0x4000);
        public const int DDE_FACKREQ = unchecked((int)0x8000);
        public const int DDE_FRELEASE = unchecked((int)0x2000);
        public const int DDE_FREQUESTED = unchecked((int)0x1000);
        public const int DDE_FAPPSTATUS = unchecked((int)0x00ff);
        public const int DDE_FNOTPROCESSED = unchecked((int)0x0000);

        public const int DMLERR_NO_ERROR = unchecked((int)0x0000);
        public const int DMLERR_FIRST = unchecked((int)0x4000);
        public const int DMLERR_ADVACKTIMEOUT = unchecked((int)0x4000);
        public const int DMLERR_BUSY = unchecked((int)0x4001);
        public const int DMLERR_DATAACKTIMEOUT = unchecked((int)0x4002);
        public const int DMLERR_DLL_NOT_INITIALIZED = unchecked((int)0x4003);
        public const int DMLERR_DLL_USAGE = unchecked((int)0x4004);
        public const int DMLERR_EXECACKTIMEOUT = unchecked((int)0x4005);
        public const int DMLERR_INVALIDPARAMETER = unchecked((int)0x4006);
        public const int DMLERR_LOW_MEMORY = unchecked((int)0x4007);
        public const int DMLERR_MEMORY_ERROR = unchecked((int)0x4008);
        public const int DMLERR_NOTPROCESSED = unchecked((int)0x4009);
        public const int DMLERR_NO_CONV_ESTABLISHED = unchecked((int)0x400A);
        public const int DMLERR_POKEACKTIMEOUT = unchecked((int)0x400B);
        public const int DMLERR_POSTMSG_FAILED = unchecked((int)0x400C);
        public const int DMLERR_REENTRANCY = unchecked((int)0x400D);
        public const int DMLERR_SERVER_DIED = unchecked((int)0x400E);
        public const int DMLERR_SYS_ERROR = unchecked((int)0x400F);
        public const int DMLERR_UNADVACKTIMEOUT = unchecked((int)0x4010);
        public const int DMLERR_UNFOUND_QUEUE_ID = unchecked((int)0x4011);
        public const int DMLERR_LAST = unchecked((int)0x4011);

        public const int DNS_REGISTER = unchecked((int)0x0001);
        public const int DNS_UNREGISTER = unchecked((int)0x0002);
        public const int DNS_FILTERON = unchecked((int)0x0004);
        public const int DNS_FILTEROFF = unchecked((int)0x0008);

        public const int EC_ENABLEALL = unchecked((int)0x0000);
        public const int EC_ENABLEONE = unchecked((int)0x0080);
        public const int EC_DISABLE = unchecked((int)0x0008);
        public const int EC_QUERYWAITING = unchecked((int)0x0002);

        public const int HDATA_APPOWNED = unchecked((int)0x0001);

        public const int MF_HSZ_INFO = unchecked((int)0x01000000);
        public const int MF_SENDMSGS = unchecked((int)0x02000000);
        public const int MF_POSTMSGS = unchecked((int)0x04000000);
        public const int MF_CALLBACKS = unchecked((int)0x08000000);
        public const int MF_ERRORS = unchecked((int)0x10000000);
        public const int MF_LINKS = unchecked((int)0x20000000);
        public const int MF_CONV = unchecked((int)0x40000000);

        public const int MH_CREATE = 1;
        public const int MH_KEEP = 2;
        public const int MH_DELETE = 3;
        public const int MH_CLEANUP = 4;

        public const int QID_SYNC = unchecked((int)0xFFFFFFFF);
        public const int TIMEOUT_ASYNC = unchecked((int)0xFFFFFFFF);

        public const int XTYPF_NOBLOCK = unchecked((int)0x0002);
        public const int XTYPF_NODATA = unchecked((int)0x0004);
        public const int XTYPF_ACKREQ = unchecked((int)0x0008);
        public const int XCLASS_MASK = unchecked((int)0xFC00);
        public const int XCLASS_BOOL = unchecked((int)0x1000);
        public const int XCLASS_DATA = unchecked((int)0x2000);
        public const int XCLASS_FLAGS = unchecked((int)0x4000);
        public const int XCLASS_NOTIFICATION = unchecked((int)0x8000);
        public const int XTYP_ERROR = unchecked((int)(0x0000 | XCLASS_NOTIFICATION | XTYPF_NOBLOCK));
        public const int XTYP_ADVDATA = unchecked((int)(0x0010 | XCLASS_FLAGS));
        public const int XTYP_ADVREQ = unchecked((int)(0x0020 | XCLASS_DATA | XTYPF_NOBLOCK));
        public const int XTYP_ADVSTART = unchecked((int)(0x0030 | XCLASS_BOOL));
        public const int XTYP_ADVSTOP = unchecked((int)(0x0040 | XCLASS_NOTIFICATION));
        public const int XTYP_EXECUTE = unchecked((int)(0x0050 | XCLASS_FLAGS));
        public const int XTYP_CONNECT = unchecked((int)(0x0060 | XCLASS_BOOL | XTYPF_NOBLOCK));
        public const int XTYP_CONNECT_CONFIRM = unchecked((int)(0x0070 | XCLASS_NOTIFICATION | XTYPF_NOBLOCK));
        public const int XTYP_XACT_COMPLETE = unchecked((int)(0x0080 | XCLASS_NOTIFICATION));
        public const int XTYP_POKE = unchecked((int)(0x0090 | XCLASS_FLAGS));
        public const int XTYP_REGISTER = unchecked((int)(0x00A0 | XCLASS_NOTIFICATION | XTYPF_NOBLOCK));
        public const int XTYP_REQUEST = unchecked((int)(0x00B0 | XCLASS_DATA));
        public const int XTYP_DISCONNECT = unchecked((int)(0x00C0 | XCLASS_NOTIFICATION | XTYPF_NOBLOCK));
        public const int XTYP_UNREGISTER = unchecked((int)(0x00D0 | XCLASS_NOTIFICATION | XTYPF_NOBLOCK));
        public const int XTYP_WILDCONNECT = unchecked((int)(0x00E0 | XCLASS_DATA | XTYPF_NOBLOCK));
        public const int XTYP_MONITOR = unchecked((int)(0x00F0 | XCLASS_NOTIFICATION | XTYPF_NOBLOCK));
        public const int XTYP_ADVSTARTNODATA = unchecked(XTYP_ADVSTART | XTYPF_NODATA);
        public const int XTYP_ADVSTARTACKREQ = unchecked(XTYP_ADVSTART | XTYPF_ACKREQ);
        public const int XTYP_MASK = unchecked((int)0x00F0);
        public const int XTYP_SHIFT = unchecked((int)0x0004);

        [DllImport("user32.dll", EntryPoint = "DdeAbandonTransaction", CharSet = CharSet.Ansi)]
        public static extern bool DdeAbandonTransaction(int idInst, IntPtr hConv, int idTransaction);

        [DllImport("user32.dll", EntryPoint = "DdeAccessData", CharSet = CharSet.Ansi)]
        public static extern IntPtr DdeAccessData(IntPtr hData, ref int pcbDataSize);

        [DllImport("user32.dll", EntryPoint = "DdeAddData", CharSet = CharSet.Ansi)]
        public static extern IntPtr DdeAddData(IntPtr hData, byte[] pSrc, int cb, int cbOff);

        [DllImport("user32.dll", EntryPoint = "DdeClientTransaction", CharSet = CharSet.Ansi)]
        public static extern IntPtr DdeClientTransaction(
            byte[] pData, int cbData, IntPtr hConv, IntPtr hszItem, ConversionFormat wFmt, int wType, int dwTimeout, ref int pdwResult);

        [DllImport("user32.dll", EntryPoint = "DdeCmpStringHandles", CharSet = CharSet.Ansi)]
        public static extern int DdeCmpStringHandles(IntPtr hsz1, IntPtr hsz2);

        [DllImport("user32.dll", EntryPoint = "DdeConnect", CharSet = CharSet.Ansi)]
        public static extern IntPtr DdeConnect(int idInst, IntPtr hszService, IntPtr hszTopic, ref CONVCONTEXT pCC);

        [DllImport("user32.dll", EntryPoint = "DdeConnectList", CharSet = CharSet.Ansi)]
        public static extern IntPtr DdeConnectList(int idInst, IntPtr hszService, IntPtr hszTopic, IntPtr hConvList, ref CONVCONTEXT pCC);

        [DllImport("user32.dll", EntryPoint = "DdeCreateDataHandle", CharSet = CharSet.Ansi)]
        public static extern IntPtr DdeCreateDataHandle(int idInst, byte[] pSrc, int cb, int cbOff, IntPtr hszItem, ConversionFormat wFmt, int afCmd);

        [DllImport("user32.dll", EntryPoint = "DdeCreateStringHandle", CharSet = CharSet.Ansi)]
        public static extern IntPtr DdeCreateStringHandle(int idInst, string psz, int iCodePage);

        [DllImport("user32.dll", EntryPoint = "DdeDisconnect", CharSet = CharSet.Ansi)]
        public static extern bool DdeDisconnect(IntPtr hConv);

        [DllImport("user32.dll", EntryPoint = "DdeDisconnectList", CharSet = CharSet.Ansi)]
        public static extern bool DdeDisconnectList(IntPtr hConvList);

        [DllImport("user32.dll", EntryPoint = "DdeEnableCallback", CharSet = CharSet.Ansi)]
        public static extern bool DdeEnableCallback(int idInst, IntPtr hConv, int wCmd);

        [DllImport("user32.dll", EntryPoint = "DdeFreeDataHandle", CharSet = CharSet.Ansi)]
        public static extern bool DdeFreeDataHandle(IntPtr hData);

        [DllImport("user32.dll", EntryPoint = "DdeFreeStringHandle", CharSet = CharSet.Ansi)]
        public static extern bool DdeFreeStringHandle(int idInst, IntPtr hsz);

        [DllImport("user32.dll", EntryPoint = "DdeGetData", CharSet = CharSet.Ansi)]
        public unsafe static extern int DdeGetData(IntPtr hData, [Out] byte* pDst, int cbMax, int cbOff);

        [DllImport("user32.dll", EntryPoint = "DdeGetLastError", CharSet = CharSet.Ansi)]
        public static extern int DdeGetLastError(int idInst);

        [DllImport("user32.dll", EntryPoint = "DdeImpersonateClient", CharSet = CharSet.Ansi)]
        public static extern bool DdeImpersonateClient(IntPtr hConv);

        [DllImport("user32.dll", EntryPoint = "DdeInitialize", CharSet = CharSet.Ansi)]
        public static extern int DdeInitialize(ref int pidInst, DdeCallback pfnCallback, int afCmd, int ulRes);

        [DllImport("user32.dll", EntryPoint = "DdeKeepStringHandle", CharSet = CharSet.Ansi)]
        public static extern bool DdeKeepStringHandle(int idInst, IntPtr hsz);

        [DllImport("user32.dll", EntryPoint = "DdeNameService", CharSet = CharSet.Ansi)]
        public static extern IntPtr DdeNameService(int idInst, IntPtr hsz1, IntPtr hsz2, int afCmd);

        [DllImport("user32.dll", EntryPoint = "DdePostAdvise", CharSet = CharSet.Ansi)]
        public static extern bool DdePostAdvise(int idInst, IntPtr hszTopic, IntPtr hszItem);

        [DllImport("user32.dll", EntryPoint = "DdeQueryConvInfo", CharSet = CharSet.Ansi)]
        public static extern int DdeQueryConvInfo(IntPtr hConv, int idTransaction, IntPtr pConvInfo);

        [DllImport("user32.dll", EntryPoint = "DdeQueryNextServer", CharSet = CharSet.Ansi)]
        public static extern IntPtr DdeQueryNextServer(IntPtr hConvList, IntPtr hConvPrev);

        [DllImport("user32.dll", EntryPoint = "DdeQueryString", CharSet = CharSet.Ansi)]
        public unsafe static extern int DdeQueryString(int idInst, IntPtr hsz, sbyte* psz, int cchMax, int iCodePage);

        [DllImport("user32.dll", EntryPoint = "DdeReconnect", CharSet = CharSet.Ansi)]
        public static extern IntPtr DdeReconnect(IntPtr hConv);

        [DllImport("user32.dll", EntryPoint = "DdeSetUserHandle", CharSet = CharSet.Ansi)]
        public static extern bool DdeSetUserHandle(IntPtr hConv, int id, IntPtr hUser);

        [DllImport("user32.dll", EntryPoint = "DdeUnaccessData", CharSet = CharSet.Ansi)]
        public static extern bool DdeUnaccessData(IntPtr hData);

        [DllImport("user32.dll", EntryPoint = "DdeUninitialize", CharSet = CharSet.Ansi)]
        public static extern bool DdeUninitialize(int idInst);

        public static String DDEGetErrorMsg(int error)
        {
            switch (error)
            {
                case DMLERR_NO_ERROR:
                    return "no DDE error.";
                case DMLERR_ADVACKTIMEOUT:
                    return "a request for a synchronous advise transaction has timed out.";

                case DMLERR_BUSY:
                    return "the response to the transaction caused the DDE_FBUSY bit to be set.";

                case DMLERR_DATAACKTIMEOUT:
                    return "a request for a synchronous data transaction has timed out.";

                case DMLERR_DLL_NOT_INITIALIZED:
                    return "a DDEML function was called without first calling the DdeInitialize function,\nor an invalid instance identifier\nwas passed to a DDEML function.";

                case DMLERR_DLL_USAGE:
                    return "an application initialized as APPCLASS_MONITOR has\nattempted to perform a DDE transaction,\nor an application initialized as APPCMD_CLIENTONLY has \nattempted to perform server transactions.";

                case DMLERR_EXECACKTIMEOUT:
                    return "a request for a synchronous execute transaction has timed out.";

                case DMLERR_INVALIDPARAMETER:
                    return "a parameter failed to be validated by the DDEML.";

                case DMLERR_LOW_MEMORY:
                    return "a DDEML application has created a prolonged race condition.";

                case DMLERR_MEMORY_ERROR:
                    return "a memory allocation failed.";

                case DMLERR_NO_CONV_ESTABLISHED:
                    return "a client's attempt to establish a conversation has failed.";

                case DMLERR_NOTPROCESSED:
                    return "a transaction failed.";

                case DMLERR_POKEACKTIMEOUT:
                    return "a request for a synchronous poke transaction has timed out.";

                case DMLERR_POSTMSG_FAILED:
                    return "an internal call to the PostMessage function has failed. ";

                case DMLERR_REENTRANCY:
                    return "reentrancy problem.";

                case DMLERR_SERVER_DIED:
                    return "a server-side transaction was attempted on a conversation\nthat was terminated by the client, or the server\nterminated before completing a transaction.";

                case DMLERR_SYS_ERROR:
                    return "an internal error has occurred in the DDEML.";

                case DMLERR_UNADVACKTIMEOUT:
                    return "a request to end an advise transaction has timed out.";

                case DMLERR_UNFOUND_QUEUE_ID:
                    return "an invalid transaction identifier was passed to a DDEML function.\nOnce the application has returned from an XTYP_XACT_COMPLETE callback,\nthe transaction identifier for that callback is no longer valid.";

                default:
                    return "Unknown DDE error %08x";
            }
        }

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HSZPAIR
    {
        public IntPtr hszSvc;
        public IntPtr hszTopic;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SECURITY_QUALITY_OF_SERVICE
    { // sqos  
        public ushort Length;
        public int ImpersonationLevel; //SECURITY_IMPERSONATION_LEVEL 
        public int ContextTrackingMode; //SECURITY_CONTEXT_TRACKING_MODE 
        public bool EffectiveOnly;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CONVINFO
    {
        public int cb;
        public IntPtr hUser;
        public IntPtr hConvPartner;
        public IntPtr hszSvcPartner;
        public IntPtr hszServiceReq;
        public IntPtr hszTopic;
        public IntPtr hszItem;
        public int wFmt;
        public int wType;
        public int wStatus;
        public int wConvst;
        public int wLastError;
        public IntPtr hConvList;
        public CONVCONTEXT ConvCtxt;
        public IntPtr hwnd;
        public IntPtr hwndPartner;

    } // struct

    [StructLayout(LayoutKind.Sequential)]
    public struct CONVCONTEXT
    {
        public int cb;
        public int wFlags;
        public int wCountryID;
        public int iCodePage;
        public int dwLangID;
        public int dwSecurity;
        public SECURITY_QUALITY_OF_SERVICE qos;

    } // struct

    [StructLayout(LayoutKind.Sequential)]
    public struct MONCBSTRUCT
    {
        public int cb;
        public int dwTime;
        public IntPtr hTask;
        public IntPtr dwRet;
        public int wType;
        public int wFmt;
        public IntPtr hConv;
        public IntPtr hsz1;
        public IntPtr hsz2;
        public IntPtr hData;
        public uint dwData1;
        public uint dwData2;
        public CONVCONTEXT cc;
        public int cbData;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] Data;

    } // struct

    [StructLayout(LayoutKind.Sequential)]
    public struct MONCONVSTRUCT
    {
        public int cb;
        public bool fConnect;
        public int dwTime;
        public IntPtr hTask;
        public IntPtr hszSvc;
        public IntPtr hszTopic;
        public IntPtr hConvClient;
        public IntPtr hConvServer;

    } // struct

    [StructLayout(LayoutKind.Sequential)]
    public struct MONERRSTRUCT
    {
        public int cb;
        public int wLastError;
        public int dwTime;
        public IntPtr hTask;

    } // struct

    [StructLayout(LayoutKind.Sequential)]
    public struct MONHSZSTRUCT
    {
        public int cb;
        public int fsAction;
        public int dwTime;
        public IntPtr hsz;
        public IntPtr hTask;
        public IntPtr str;

    } // struct

    [StructLayout(LayoutKind.Sequential)]
    public struct MONLINKSTRUCT
    {
        public int cb;
        public int dwTime;
        public IntPtr hTask;
        public bool fEstablished;
        public bool fNoData;
        public IntPtr hszSvc;
        public IntPtr hszTopic;
        public IntPtr hszItem;
        public int wFmt;
        public bool fServer;
        public IntPtr hConvClient;
        public IntPtr hConvServer;

    } // struct

    [StructLayout(LayoutKind.Sequential)]
    public struct MONMSGSTRUCT
    {
        public int cb;
        public IntPtr hwndTo;
        public int dwTime;
        public IntPtr hTask;
        public int wMsg;
        public IntPtr wParam;
        public IntPtr lParam;
        public DDEML_MSG_HOOK_DATA dmhd;

    } // struct

    [StructLayout(LayoutKind.Sequential)]
    public struct DDEML_MSG_HOOK_DATA
    {
        public IntPtr uiLo;
        public IntPtr uiHi;
        public int cbData;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] Data;

    } // struct

    public enum ConversionFormat : uint
    {
        NONE = 0,
        TEXT = 1,
        BITMAP = 2,
        METAFILEPICT = 3,
        SYLK = 4,
        DIF = 5,
        TIFF = 6,
        OEMTEXT = 7,
        DIB = 8,
        PALETTE = 9,
        PENDATA = 10,
        RIFF = 11,
        WAVE = 12,
        UNICODETEXT = 13,
        ENHMETAFILE = 14,
        HDROP = 15,
        LOCALE = 16,
        DIBV5 = 17,
        OWNERDISPLAY = 0x0080,
        DSPTEXT = 0x0081,
        DSPBITMAP = 0x0082,
        DSPMETAFILEPICT = 0x0083,
        DSPENHMETAFILE = 0x008E,
        // "Private" formats don't get GlobalFree()'d
        PRIVATEFIRST = 0x0200,
        PRIVATELAST = 0x02FF,
        // "GDIOBJ" formats do get DeleteObject()'d
        GDIOBJFIRST = 0x0300,
        GDIOBJLAST = 0x03FF
    }

    [Flags]
    public enum DDEResult : uint
    {
        FACK = 0x8000U,
        FBUSY = 0x4000U,
        FDEFERUPD = 0x4000,
        FACKREQ = 0x8000,
        FRELEASE = 0x2000,
        FREQUESTED = 0x1000,
        FAPPSTATUS = 0x00ff,
        FNOTPROCESSED = 0x0,
        FACKRESERVED = (~(FACK | FBUSY | FAPPSTATUS)),
        FADVRESERVED = (~(FACKREQ | FDEFERUPD)),
        FDATRESERVED = (~(FACKREQ | FRELEASE | FREQUESTED)),
        FPOKRESERVED = (~(FRELEASE))
    }
}

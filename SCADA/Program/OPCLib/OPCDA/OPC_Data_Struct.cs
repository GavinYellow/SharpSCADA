/*=====================================================================
  File:      OPC_Data.cs

  Summary:   OPC DA custom interface

-----------------------------------------------------------------------
  This file is part of the Viscom OPC Code Samples.

  Copyright(c) 2001 Viscom (www.viscomvisual.com) All rights reserved.

THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
PARTICULAR PURPOSE.
======================================================================*/

/*
Notes:
	An interface declared with ComImport can expose HRESULTs to C#,
	this is done by [PreserveSig]

	midl attribute 'pointer_unique' is simulated by passing an array[1]


*/

using System;
using System.Runtime.InteropServices;
using OPC.Data.Enum;


namespace OPC.Data.Struct
{



    // ------------------ SERVER level structs ------------------

    [StructLayout(LayoutKind.Sequential, Pack = 2, CharSet = CharSet.Unicode)]
    public class SERVERSTATUS
    {
        public long ftStartTime;
        public long ftCurrentTime;
        public long ftLastUpdateTime;

        [MarshalAs(UnmanagedType.U4)]
        public OPCSERVERSTATE dwServerState;

        public int dwGroupCount;
        public int dwBandWidth;
        public short wMajorVersion;
        public short wMinorVersion;
        public short wBuildNumber;
        public short wReserved;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string szVendorInfo;
    };




    [StructLayout(LayoutKind.Sequential, Pack = 4), ComConversionLoss]
    public struct OPCITEMDEF
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string szAccessPath;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string szItemID;
        public bool bActive;
        public int hClient;
        public int dwBlobSize;
        [ComConversionLoss]
        public IntPtr pBlob;
        public short vtRequestedDataType;
        public short wReserved;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4), ComConversionLoss]
    public struct OPCITEMRESULT
    {
        public int hServer;
        public short vtCanonicalDataType;
        public short wReserved;
        public int dwAccessRights;
        public int dwBlobSize;
        [ComConversionLoss]
        public IntPtr pBlob;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct OPCITEMSTATE
    {
        public int hClient;
        public System.Runtime.InteropServices.ComTypes.FILETIME ftTimeStamp;
        public short wQuality;
        public short wReserved;
        [MarshalAs(UnmanagedType.Struct)]
        public object vDataValue;
    }




    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct OPCITEMPROPERTY
    {
        public short vtDataType;
        public short wReserved;
        public int dwPropertyID;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string szItemID;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string szDescription;
        [MarshalAs(UnmanagedType.Struct)]
        public object vValue;
        public int hrErrorID;
        public int dwReserved;
    }



    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct OPCITEMVQT
    {
        [MarshalAs(UnmanagedType.Struct)]
        public object vDataValue;
        public int bQualitySpecified;
        public short wQuality;
        public short wReserved;
        public int bTimeStampSpecified;
        public int dwReserved;
        public FILETIME ftTimeStamp;
    }




    public struct OPCGroupState
    {
        public bool Public;
        public int UpdateRate;
        public bool Active;
        public int TimeBias;
        public float Deadband;
        public int LocaleId;
        public int ClientId;
    }






    // ------------------ INTERNAL item level structs ------------------

    [StructLayout(LayoutKind.Sequential, Pack = 2, CharSet = CharSet.Unicode)]
    public class OPCITEMDEFINTERN
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string szAccessPath;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string szItemID;

        [MarshalAs(UnmanagedType.Bool)]
        public bool bActive;

        public int hClient;
        public int dwBlobSize;
        public IntPtr pBlob;

        public short vtRequestedDataType;

        public short wReserved;
    };




    // ------------- managed side only structs ----------------------

    [StructLayout(LayoutKind.Sequential, Pack = 4), ComConversionLoss]
    public struct OPCITEMATTRIBUTES
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string szAccessPath;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string szItemID;
        public int bActive;
        public int hClient;
        public int hServer;
        public int dwAccessRights;
        public int dwBlobSize;
        [ComConversionLoss]
        public IntPtr pBlob;
        public short vtRequestedDataType;
        public short vtCanonicalDataType;
        public OPCEUTYPE dwEUType;
        [MarshalAs(UnmanagedType.Struct)]
        public object vEUInfo;
    }


}

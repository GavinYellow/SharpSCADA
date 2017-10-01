/*=====================================================================
  File:      OPC_Data_Grp.cs

  Summary:   OPC DA group interfaces wrapper class

-----------------------------------------------------------------------
  This file is part of the Viscom OPC Code Samples.

  Copyright(c) 2001 Viscom (www.viscomvisual.com) All rights reserved.

THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
PARTICULAR PURPOSE.
======================================================================*/




namespace OPC.Data.Class
{
   

    public class HRESULTS
    {
        public static bool Failed(int hresultcode)
        { return (hresultcode < 0); }

        public static bool Succeeded(int hresultcode)
        { return (hresultcode >= 0); }

        public const int S_OK = 0x00000000;
        public const int S_FALSE = 0x00000001;

        public const int E_NOTIMPL = unchecked((int)0x80004001);		// winerror.h
        public const int E_NOINTERFACE = unchecked((int)0x80004002);
        public const int E_ABORT = unchecked((int)0x80004004);
        public const int E_FAIL = unchecked((int)0x80004005);
        public const int E_OUTOFMEMORY = unchecked((int)0x8007000E);
        public const int E_INVALIDARG = unchecked((int)0x80070057);

        public const int CONNECT_E_NOCONNECTION = unchecked((int)0x80040200);		// olectl.h
        public const int CONNECT_E_ADVISELIMIT = unchecked((int)0x80040201);

        public const int OPC_E_INVALIDHANDLE = unchecked((int)0xC0040001);		// opcerror.h
        public const int OPC_E_BADTYPE = unchecked((int)0xC0040004);
        public const int OPC_E_PUBLIC = unchecked((int)0xC0040005);
        public const int OPC_E_BADRIGHTS = unchecked((int)0xC0040006);
        public const int OPC_E_UNKNOWNITEMID = unchecked((int)0xC0040007);
        public const int OPC_E_INVALIDITEMID = unchecked((int)0xC0040008);
        public const int OPC_E_INVALIDFILTER = unchecked((int)0xC0040009);
        public const int OPC_E_UNKNOWNPATH = unchecked((int)0xC004000A);
        public const int OPC_E_RANGE = unchecked((int)0xC004000B);
        public const int OPC_E_DUPLICATENAME = unchecked((int)0xC004000C);
        public const int OPC_S_UNSUPPORTEDRATE = unchecked((int)0x0004000D);
        public const int OPC_S_CLAMP = unchecked((int)0x0004000E);
        public const int OPC_S_INUSE = unchecked((int)0x0004000F);
        public const int OPC_E_INVALIDCONFIGFILE = unchecked((int)0xC0040010);
        public const int OPC_E_NOTFOUND = unchecked((int)0xC0040011);
        public const int OPC_E_INVALID_PID = unchecked((int)0xC0040203);

    }	// class HRESULTS


    public class Constants
    {
        public const int OPC_BROWSE_HASCHILDREN = 1;
        public const int OPC_BROWSE_ISITEM = 2;
        public const string OPC_CATEGORY_DESCRIPTION_DA10 = "OPC Data Access Servers Version 1.0";
        public const string OPC_CATEGORY_DESCRIPTION_DA20 = "OPC Data Access Servers Version 2.0";
        public const string OPC_CATEGORY_DESCRIPTION_DA30 = "OPC Data Access Servers Version 3.0";
        public const string OPC_CATEGORY_DESCRIPTION_XMLDA10 = "OPC XML Data Access Servers Version 1.0";
        public const string OPC_CONSISTENCY_WINDOW_NOT_CONSISTENT = "Not Consistent";
        public const string OPC_CONSISTENCY_WINDOW_UNKNOWN = "Unknown";
        public const int OPC_READABLE = 1;
        public const string OPC_TYPE_SYSTEM_OPCBINARY = "OPCBinary";
        public const string OPC_TYPE_SYSTEM_XMLSCHEMA = "XMLSchema";
        public const string OPC_WRITE_BEHAVIOR_ALL_OR_NOTHING = "All or Nothing";
        public const string OPC_WRITE_BEHAVIOR_BEST_EFFORT = "Best Effort";
        public const int OPC_WRITEABLE = 2;
    }
}
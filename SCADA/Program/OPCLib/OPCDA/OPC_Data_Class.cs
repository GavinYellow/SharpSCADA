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

using System;
using System.Runtime.InteropServices;



namespace OPC.Data.Class
{
    public abstract class Constants
    {
        // Fields
        public const int OPC_BROWSE_HASCHILDREN = 1;
        public const int OPC_BROWSE_ISITEM = 2;
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_CATEGORY_DESCRIPTION_DA10 = "OPC Data Access Servers Version 1.0";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_CATEGORY_DESCRIPTION_DA20 = "OPC Data Access Servers Version 2.0";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_CATEGORY_DESCRIPTION_DA30 = "OPC Data Access Servers Version 3.0";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_CATEGORY_DESCRIPTION_XMLDA10 = "OPC XML Data Access Servers Version 1.0";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_CONSISTENCY_WINDOW_NOT_CONSISTENT = "Not Consistent";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_CONSISTENCY_WINDOW_UNKNOWN = "Unknown";
        public const int OPC_READABLE = 1;
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_TYPE_SYSTEM_OPCBINARY = "OPCBinary";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_TYPE_SYSTEM_XMLSCHEMA = "XMLSchema";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_WRITE_BEHAVIOR_ALL_OR_NOTHING = "All or Nothing";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_WRITE_BEHAVIOR_BEST_EFFORT = "Best Effort";
        public const int OPC_WRITEABLE = 2;
    }

 

 

    public abstract class Qualities
    {
        // Fields
        public const short OPC_LIMIT_CONST = 3;
        public const short OPC_LIMIT_HIGH = 2;
        public const short OPC_LIMIT_LOW = 1;
        public const short OPC_LIMIT_MASK = 3;
        public const short OPC_LIMIT_OK = 0;
        public const short OPC_QUALITY_BAD = 0;
        public const short OPC_QUALITY_COMM_FAILURE = 0x18;
        public const short OPC_QUALITY_CONFIG_ERROR = 4;
        public const short OPC_QUALITY_DEVICE_FAILURE = 12;
        public const short OPC_QUALITY_EGU_EXCEEDED = 0x54;
        public const short OPC_QUALITY_GOOD = 0xc0;
        public const short OPC_QUALITY_LAST_KNOWN = 20;
        public const short OPC_QUALITY_LAST_USABLE = 0x44;
        public const short OPC_QUALITY_LOCAL_OVERRIDE = 0xd8;
        public const short OPC_QUALITY_MASK = 0xc0;
        public const short OPC_QUALITY_NOT_CONNECTED = 8;
        public const short OPC_QUALITY_OUT_OF_SERVICE = 0x1c;
        public const short OPC_QUALITY_SENSOR_CAL = 80;
        public const short OPC_QUALITY_SENSOR_FAILURE = 0x10;
        public const short OPC_QUALITY_SUB_NORMAL = 0x58;
        public const short OPC_QUALITY_UNCERTAIN = 0x40;
        public const short OPC_QUALITY_WAITING_FOR_INITIAL_DATA = 0x20;
        public const short OPC_STATUS_MASK = 0xfc;
    }

    public abstract class Properties
    {
        // Fields
        public const int OPC_PROPERTY_ACCESS_RIGHTS = 5;
        public const int OPC_PROPERTY_ALARM_AREA_LIST = 0x12e;
        public const int OPC_PROPERTY_ALARM_QUICK_HELP = 0x12d;
        public const int OPC_PROPERTY_CHANGE_RATE_LIMIT = 0x137;
        public const int OPC_PROPERTY_CLOSE_LABEL = 0x6a;
        public const int OPC_PROPERTY_CONDITION_LOGIC = 0x130;
        public const int OPC_PROPERTY_CONDITION_STATUS = 300;
        public const int OPC_PROPERTY_CONSISTENCY_WINDOW = 0x25d;
        public const int OPC_PROPERTY_DATA_FILTER_VALUE = 0x261;
        public const int OPC_PROPERTY_DATATYPE = 1;
        public const int OPC_PROPERTY_DEADBAND = 0x132;
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_ACCESS_RIGHTS = "Item Access Rights";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_ALARM_AREA_LIST = "Alarm Area List";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_ALARM_QUICK_HELP = "Alarm Quick Help";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_CHANGE_RATE_LIMIT = "Rate of Change Limit";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_CLOSE_LABEL = "Contact Close Label";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_CONDITION_LOGIC = "Condition Logic";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_CONDITION_STATUS = "Condition Status";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_CONSISTENCY_WINDOW = "Consistency Window";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_DATA_FILTER_VALUE = "Data Filter Value";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_DATATYPE = "Item Canonical Data Type";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_DEADBAND = "Deadband";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_DESCRIPTION = "Item Description";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_DEVIATION_LIMIT = "Deviation Limit";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_DICTIONARY = "Dictionary";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_DICTIONARY_ID = "Dictionary ID";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_EU_INFO = "Item EU Info";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_EU_TYPE = "Item EU Type";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_EU_UNITS = "EU Units";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_HI_LIMIT = "Hi Limit";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_HIGH_EU = "High EU";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_HIGH_IR = "High Instrument Range";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_HIHI_LIMIT = "HiHi Limit";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_LIMIT_EXCEEDED = "Limit Exceeded";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_LO_LIMIT = "Lo Limit";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_LOLO_LIMIT = "LoLo Limit";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_LOW_EU = "Low EU";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_LOW_IR = "Low Instrument Range";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_OPEN_LABEL = "Contact Open Label";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_PRIMARY_ALARM_AREA = "Primary Alarm Area";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_QUALITY = "Item Quality";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_SCAN_RATE = "Server Scan Rate";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_SOUND_FILE = "Sound File";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_TIMESTAMP = "Item Timestamp";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_TIMEZONE = "Item Timezone";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_TYPE_DESCRIPTION = "Type Description";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_TYPE_ID = "Type ID";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_TYPE_SYSTEM_ID = "Type System ID";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_UNCONVERTED_ITEM_ID = "Unconverted Item ID";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_UNFILTERED_ITEM_ID = "Unfiltered Item ID";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_VALUE = "Item Value";
        [MarshalAs(UnmanagedType.LPWStr)]
        public const string OPC_PROPERTY_DESC_WRITE_BEHAVIOR = "Write Behavior";
        public const int OPC_PROPERTY_DESCRIPTION = 0x65;
        public const int OPC_PROPERTY_DEVIATION_LIMIT = 0x138;
        public const int OPC_PROPERTY_DICTIONARY = 0x25b;
        public const int OPC_PROPERTY_DICTIONARY_ID = 0x259;
        public const int OPC_PROPERTY_EU_INFO = 8;
        public const int OPC_PROPERTY_EU_TYPE = 7;
        public const int OPC_PROPERTY_EU_UNITS = 100;
        public const int OPC_PROPERTY_HI_LIMIT = 0x134;
        public const int OPC_PROPERTY_HIGH_EU = 0x66;
        public const int OPC_PROPERTY_HIGH_IR = 0x68;
        public const int OPC_PROPERTY_HIHI_LIMIT = 0x133;
        public const int OPC_PROPERTY_LIMIT_EXCEEDED = 0x131;
        public const int OPC_PROPERTY_LO_LIMIT = 0x135;
        public const int OPC_PROPERTY_LOLO_LIMIT = 310;
        public const int OPC_PROPERTY_LOW_EU = 0x67;
        public const int OPC_PROPERTY_LOW_IR = 0x69;
        public const int OPC_PROPERTY_OPEN_LABEL = 0x6b;
        public const int OPC_PROPERTY_PRIMARY_ALARM_AREA = 0x12f;
        public const int OPC_PROPERTY_QUALITY = 3;
        public const int OPC_PROPERTY_SCAN_RATE = 6;
        public const int OPC_PROPERTY_SOUND_FILE = 0x139;
        public const int OPC_PROPERTY_TIMESTAMP = 4;
        public const int OPC_PROPERTY_TIMEZONE = 0x6c;
        public const int OPC_PROPERTY_TYPE_DESCRIPTION = 0x25c;
        public const int OPC_PROPERTY_TYPE_ID = 0x25a;
        public const int OPC_PROPERTY_TYPE_SYSTEM_ID = 600;
        public const int OPC_PROPERTY_UNCONVERTED_ITEM_ID = 0x25f;
        public const int OPC_PROPERTY_UNFILTERED_ITEM_ID = 0x260;
        public const int OPC_PROPERTY_VALUE = 2;
        public const int OPC_PROPERTY_WRITE_BEHAVIOR = 0x25e;
    }


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

}
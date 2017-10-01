/*=====================================================================
  File:      OPC_Common.cs

  Summary:   OPC common custom interface

-----------------------------------------------------------------------
  This file is part of the Viscom OPC Code Samples.

  Copyright(c) 2001 Viscom (www.viscomvisual.com) All rights reserved.

THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
PARTICULAR PURPOSE.
======================================================================*/

using System;

namespace OPC.Data.Enum
{

    public enum OPCDATASOURCE
    {
        OPC_DS_CACHE = 1,
        OPC_DS_DEVICE = 2
    }

    public enum OPCBROWSETYPE
    {
        OPC_BRANCH = 1,
        OPC_LEAF = 2,
        OPC_FLAT = 3
    }

    public enum OPCNAMESPACETYPE
    {
        OPC_NS_HIERARCHIAL = 1,
        OPC_NS_FLAT = 2
    }

    public enum OPCBROWSEDIRECTION
    {
        OPC_BROWSE_UP = 1,
        OPC_BROWSE_DOWN = 2,
        OPC_BROWSE_TO = 3
    }

    [Flags]
    public enum OPCACCESSRIGHTS
    {
        OPC_READABLE = 1,
        OPC_WRITEABLE = 2
    }

    public enum OPCEUTYPE
    {
        OPC_NOENUM = 0,
        OPC_ANALOG = 1,
        OPC_ENUMERATED = 2
    }

    public enum OPCSERVERSTATE
    {
        OPC_STATUS_RUNNING = 1,
        OPC_STATUS_FAILED = 2,
        OPC_STATUS_NOCONFIG = 3,
        OPC_STATUS_SUSPENDED = 4,
        OPC_STATUS_TEST = 5
    }

    public enum OPCENUMSCOPE
    {
        OPC_ENUM_PRIVATE_CONNECTIONS = 1,
        OPC_ENUM_PUBLIC_CONNECTIONS = 2,
        OPC_ENUM_ALL_CONNECTIONS = 3,
        OPC_ENUM_PRIVATE = 4,
        OPC_ENUM_PUBLIC = 5,
        OPC_ENUM_ALL = 6
    }





    //****************************************************
    // OPC Quality flags
    [Flags]
    public enum OPC_QUALITY_MASKS : short
    {
        LIMIT_MASK = 0x0003,
        STATUS_MASK = 0x00FC,
        MASTER_MASK = 0x00C0,
    }

    [Flags]
    public enum OPC_QUALITY_MASTER : short
    {
        QUALITY_BAD = 0x0000,
        QUALITY_UNCERTAIN = 0x0040,
        ERROR_QUALITY_VALUE = 0x0080,		// non standard!
        QUALITY_GOOD = 0x00C0,
    }

    [Flags]
    public enum OPC_QUALITY_STATUS : short
    {
        BAD = 0x0000,	// STATUS_MASK Values for Quality = BAD
        CONFIG_ERROR = 0x0004,
        NOT_CONNECTED = 0x0008,
        DEVICE_FAILURE = 0x000c,
        SENSOR_FAILURE = 0x0010,
        LAST_KNOWN = 0x0014,
        COMM_FAILURE = 0x0018,
        OUT_OF_SERVICE = 0x001C,

        UNCERTAIN = 0x0040,	// STATUS_MASK Values for Quality = UNCERTAIN
        LAST_USABLE = 0x0044,
        SENSOR_CAL = 0x0050,
        EGU_EXCEEDED = 0x0054,
        SUB_NORMAL = 0x0058,

        OK = 0x00C0,	// STATUS_MASK Value for Quality = GOOD
        LOCAL_OVERRIDE = 0x00D8
    }

    [Flags]
    public enum OPC_QUALITY_LIMIT
    {
        LIMIT_OK = 0x0000,
        LIMIT_LOW = 0x0001,
        LIMIT_HIGH = 0x0002,
        LIMIT_CONST = 0x0003
    }


    public enum OPC_PROPS
    {
        OPC_PROP_CDT = 1,
        OPC_PROP_VALUE = 2,
        OPC_PROP_QUALITY = 3,
        OPC_PROP_TIME = 4,
        OPC_PROP_RIGHTS = 5,
        OPC_PROP_SCANRATE = 6,

        OPC_PROP_UNIT = 100,
        OPC_PROP_DESC = 101,
        OPC_PROP_HIEU = 102,
        OPC_PROP_LOEU = 103,
        OPC_PROP_HIRANGE = 104,
        OPC_PROP_LORANGE = 105,
        OPC_PROP_CLOSE = 106,
        OPC_PROP_OPEN = 107,
        OPC_PROP_TIMEZONE = 108,

        OPC_PROP_FGC = 200,
        OPC_PROP_BGC = 201,
        OPC_PROP_BLINK = 202,
        OPC_PROP_BMP = 203,
        OPC_PROP_SND = 204,
        OPC_PROP_HTML = 205,
        OPC_PROP_AVI = 206,

        OPC_PROP_ALMSTAT = 300,
        OPC_PROP_ALMHELP = 301,
        OPC_PROP_ALMAREAS = 302,
        OPC_PROP_ALMPRIMARYAREA = 303,
        OPC_PROP_ALMCONDITION = 304,
        OPC_PROP_ALMLIMIT = 305,
        OPC_PROP_ALMDB = 306,
        OPC_PROP_ALMHH = 307,
        OPC_PROP_ALMH = 308,
        OPC_PROP_ALML = 309,
        OPC_PROP_ALMLL = 310,
        OPC_PROP_ALMROC = 311,
        OPC_PROP_ALMDEV = 312
    }

}

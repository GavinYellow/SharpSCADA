/*=====================================================================
  File:      OPC_Data_Srv.cs

  Summary:   OPC DA server interfaces wrapper class

-----------------------------------------------------------------------
  This file is part of the Viscom OPC Code Samples.

  Copyright(c) 2001 Viscom (www.viscomvisual.com) All rights reserved.

THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
PARTICULAR PURPOSE.
======================================================================*/

//using System.Runtime.InteropServices;

using OPC.Data.Struct;
using System.Runtime.InteropServices;
using System;
using OPC.Data.Enum;


namespace OPC.Data.Interface
{

    [ComImport, InterfaceType((short)1), Guid("63D5F430-CFE4-11D1-B2C8-0060083BA1FB")]
    public interface CATID_OPCDAServer10
    {
    }


    [ComImport, InterfaceType((short)1), Guid("63D5F432-CFE4-11D1-B2C8-0060083BA1FB")]
    public interface CATID_OPCDAServer20
    {
    }

    [ComImport, InterfaceType((short)1), Guid("CC603642-66D7-48F1-B69A-B625E73652D7")]
    public interface CATID_OPCDAServer30
    {
    }

    [ComImport, InterfaceType((short)1), Guid("3098EDA4-A006-48B2-A27F-247453959408")]
    public interface CATID_XMLDAServer10
    {
    }



    // ----------------------------------------------------------------- OPC common
    [ComVisible(true), ComImport,
    Guid("F31DFDE2-07B6-11d2-B2D8-0060083BA1FB"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOPCCommon
    {
        void SetLocaleID(
            [In]								int dwLcid);

        void GetLocaleID(
            [Out]							out int pdwLcid);

        [PreserveSig]
        int QueryAvailableLocaleIDs(
            [Out]							out int pdwCount,
            [Out]							out	IntPtr pdwLcid);

        void GetErrorString(
            [In]											int dwError,
            [Out, MarshalAs(UnmanagedType.LPWStr)]		out	string ppString);

        void SetClientName(
            [In, MarshalAs(UnmanagedType.LPWStr)]			string szName);
    }



    // ----------------------------------------------------------------- Common callback
    [ComVisible(true), ComImport,
    Guid("F31DFDE1-07B6-11d2-B2D8-0060083BA1FB"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOPCShutdown
    {
        void ShutdownRequest(
            [In, MarshalAs(UnmanagedType.LPWStr)]		string szReason);
    }



    // ----------------------------------------------------------------- Server List enum
    [ComVisible(true), ComImport,
    Guid("13486D50-4821-11D2-A494-3CB306C10000"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOPCServerList
    {
        void EnumClassesOfCategories(
            [In]											int cImplemented,	// WARNING ONLY 1!!
            [In]										ref Guid catidImpl,		// WARNING ONLY 1!!
            [In]											int cRequired,		// WARNING ONLY 1!!
            [In]										ref Guid catidReq,		// WARNING ONLY 1!!
            [Out, MarshalAs(UnmanagedType.IUnknown)]	out	object ppUnk);

        void GetClassDetails(
            [In]										ref Guid clsid,
            [Out, MarshalAs(UnmanagedType.LPWStr)]		out	string ppszProgID,
            [Out, MarshalAs(UnmanagedType.LPWStr)]		out	string ppszUserType);

        void CLSIDFromProgID(
            [In, MarshalAs(UnmanagedType.LPWStr)]			string szProgId,
            [Out]										out Guid clsid);
    }	// interface IOPCServerList


    [ComVisible(true), ComImport,
    Guid("9DD0B56C-AD9E-43EE-8305-487F3188BF7A"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOPCServerList2
    {
        void EnumClassesOfCategories(
            [In]											int cImplemented,	// WARNING ONLY 1!!
            [In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct, SizeParamIndex = 0)] Guid[] catidImpl,		// WARNING ONLY 1!!
            [In]											int cRequired,		// WARNING ONLY 1!!
            [In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct, SizeParamIndex = 2)] Guid[] catidReq,		// WARNING ONLY 1!!
            [Out, MarshalAs(UnmanagedType.Interface)] out IOPCEnumGUID ppUnk);

        void GetClassDetails(
            [In]										ref Guid clsid,
            [Out, MarshalAs(UnmanagedType.LPWStr)]		out	string ppszProgID,
            [Out, MarshalAs(UnmanagedType.LPWStr)]		out	string ppszUserType,
            [Out, MarshalAs(UnmanagedType.LPWStr)]      out string ppszVerIndProgID);

        void CLSIDFromProgID(
            [In, MarshalAs(UnmanagedType.LPWStr)]			string szProgId,
            [Out]										out Guid clsid);
    }	// interface IOPCServerList


    // ----------------------------------------------------------------- Enum GUIDs
    [ComVisible(true), ComImport,
    Guid("0002E000-0000-0000-C000-000000000046"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumGUID
    {
        void Next(
            [In]											int celt,
            [In]											IntPtr rgelt,				// ptr to Out-Values!!
            [Out]										out int pceltFetched);

        void Skip(
            [In]											int celt);

        void Reset();

        void Clone(
            [Out, MarshalAs(UnmanagedType.IUnknown)]	out	object ppUnk);

    }

    [ComVisible(true), ComImport,
    Guid("00000101-0000-0000-C000-000000000046"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumString
    {
        void RemoteNext(
            [In]											int celt,
            [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] rgelt,				// ptr to Out-Values!!
            [Out]										out int pceltFetched);

        void Skip(
            [In]											int celt);

        void Reset();

        void Clone(
            [Out, MarshalAs(UnmanagedType.Interface)] out IEnumString ppUnk);

    }

    [ComVisible(true), ComImport,
    Guid("55C382C8-21C7-4E88-96C1-BECFB1E3F483"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOPCEnumGUID
    {
        void Next(
            [In]											int celt,
            [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct)] Guid[] rgelt,				// ptr to Out-Values!!
            [Out]										out int pceltFetched);

        void Skip(
            [In]											int celt);

        void Reset();

        void Clone(
            [Out, MarshalAs(UnmanagedType.Interface)]	out	IOPCEnumGUID ppUnk);

    }


    // ----------------------------------------------------------------- SERVER
    [ComVisible(true), ComImport,
    Guid("39c13a4d-011e-11d0-9675-0020afd8adb3"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOPCServer
    {
        [PreserveSig]
        int AddGroup(
            [In, MarshalAs(UnmanagedType.LPWStr)]					string szName,
            [In, MarshalAs(UnmanagedType.Bool)]					bool bActive,
            [In]													int dwRequestedUpdateRate,
            [In]													int hClientGroup,
            [In]		                                         IntPtr pTimeBias,
            [In]		                                         IntPtr pPercentDeadband,
            [In]													int dwLCID,
            [Out]													out	int phServerGroup,
            [Out]													out	int pRevisedUpdateRate,
            [In]													ref Guid riid,
            [Out, MarshalAs(UnmanagedType.IUnknown)]				out	object ppUnk);

        void GetErrorString(
            [In]											int dwError,
            [In]											int dwLocale,
            [Out, MarshalAs(UnmanagedType.LPWStr)]		out	string ppString);

        [PreserveSig]
        int GetGroupByName(
            [In, MarshalAs(UnmanagedType.LPWStr)]			string szName,
            [In]										ref Guid riid,
            [Out, MarshalAs(UnmanagedType.IUnknown)]	out	object ppUnk);

        [PreserveSig]
        int GetStatus(
            [Out, MarshalAs(UnmanagedType.LPStruct)]	out	SERVERSTATUS ppServerStatus);

        [PreserveSig]
        int RemoveGroup(
            [In]											int hServerGroup,
            [In, MarshalAs(UnmanagedType.Bool)]			bool bForce);

        [PreserveSig]
        int CreateGroupEnumerator(										// may return S_FALSE
            [In]											int dwScope,
            [In]										ref Guid riid,
            [Out, MarshalAs(UnmanagedType.IUnknown)]	out	object ppUnk);

    }	// interface IOPCServer




    // ----------------------------------------------------------------- Public Groups
    [ComVisible(true), ComImport,
    Guid("39c13a4e-011e-11d0-9675-0020afd8adb3"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOPCServerPublicGroups
    {
        void GetPublicGroupByName(
            [In, MarshalAs(UnmanagedType.LPWStr)]			string szName,
            [In]										ref Guid riid,
            [Out, MarshalAs(UnmanagedType.IUnknown)]	out	object ppUnk);

        void RemovePublicGroup(
            [In]											int hServerGroup,
            [In, MarshalAs(UnmanagedType.Bool)]			bool bForce);

    }

    // ----------------------------------------------------------------- ServerAddressSpace Browsing
    [ComVisible(true), ComImport,
    Guid("39c13a4f-011e-11d0-9675-0020afd8adb3"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOPCBrowseServerAddressSpace
    {
        void QueryOrganization(
            [Out, MarshalAs(UnmanagedType.U4)]			out	OPCNAMESPACETYPE pNameSpaceType);

        void ChangeBrowsePosition(
            [In, MarshalAs(UnmanagedType.U4)]				OPCBROWSEDIRECTION dwBrowseDirection,
            [In, MarshalAs(UnmanagedType.LPWStr)]			string szName);

        [PreserveSig]
        int BrowseOPCItemIDs(
            [In, MarshalAs(UnmanagedType.U4)]				OPCBROWSETYPE dwBrowseFilterType,
            [In, MarshalAs(UnmanagedType.LPWStr)]			string szFilterCriteria,
            [In, MarshalAs(UnmanagedType.U2)]				short vtDataTypeFilter,
            [In, MarshalAs(UnmanagedType.U4)]				OPCACCESSRIGHTS dwAccessRightsFilter,
            [Out, MarshalAs(UnmanagedType.Interface)]       out IEnumString ppUnk);

        void GetItemID(
            [In, MarshalAs(UnmanagedType.LPWStr)]			string szItemDataID,
            [Out, MarshalAs(UnmanagedType.LPWStr)]		out	string szItemID);

        [PreserveSig]
        int BrowseAccessPaths(
            [In, MarshalAs(UnmanagedType.LPWStr)]			string szItemID,
            [Out, MarshalAs(UnmanagedType.IUnknown)]	out	object ppUnk);
    }


    // ----------------------------------------------------------------- Item Properties
    [ComVisible(true), ComImport,
    Guid("39c13a72-011e-11d0-9675-0020afd8adb3"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOPCItemProperties
    {
        void QueryAvailableProperties(
            [In, MarshalAs(UnmanagedType.LPWStr)]			string szItemID,
            [Out]										out int dwCount,
            [Out]										out IntPtr ppPropertyIDs,
            [Out]										out IntPtr ppDescriptions,
            [Out]										out	IntPtr ppvtDataTypes);

        [PreserveSig]
        int GetItemProperties(
            [In, MarshalAs(UnmanagedType.LPWStr)]						string szItemID,
            [In]														int dwCount,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]	int[] pdwPropertyIDs,
            [Out]													out IntPtr ppvData,
            [Out]													out	IntPtr ppErrors);

        [PreserveSig]
        int LookupItemIDs(
            [In, MarshalAs(UnmanagedType.LPWStr)]						string szItemID,
            [In]														int dwCount,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]	int[] pdwPropertyIDs,
            [Out]													out IntPtr ppszNewItemIDs,
            [Out]													out	IntPtr ppErrors);
    }



    // ----------------------------------------------------------------- GroupStateMgt
    [ComVisible(true),
    Guid("39c13a50-011e-11d0-9675-0020afd8adb3"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOPCGroupStateMgt
    {
        void GetState(
            [Out]										out	int pUpdateRate,
            [Out, MarshalAs(UnmanagedType.Bool)]		out	bool pActive,
            [Out, MarshalAs(UnmanagedType.LPWStr)]		out	string ppName,
            [Out]										out	int pTimeBias,
            [Out]										out	float pPercentDeadband,
            [Out]										out	int pLCID,
            [Out]										out	int phClientGroup,
            [Out]										out	int phServerGroup);

        void SetState(
            [In, MarshalAs(UnmanagedType.LPArray, SizeConst = 1)]											int[] pRequestedUpdateRate,
            [Out]																					    out	int pRevisedUpdateRate,
            [In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Bool, SizeConst = 1)]	   bool[] pActive,
            [In, MarshalAs(UnmanagedType.LPArray, SizeConst = 1)]											int[] pTimeBias,
            [In, MarshalAs(UnmanagedType.LPArray, SizeConst = 1)]		                                  float[] pPercentDeadband,
            [In, MarshalAs(UnmanagedType.LPArray, SizeConst = 1)]		int[] pLCID,
            [In, MarshalAs(UnmanagedType.LPArray, SizeConst = 1)]		int[] phClientGroup);

        void SetName(
            [In, MarshalAs(UnmanagedType.LPWStr)]			string szName);

        void CloneGroup(
            [In, MarshalAs(UnmanagedType.LPWStr)]			string szName,
            [In]										ref Guid riid,
            [Out, MarshalAs(UnmanagedType.IUnknown)]	out	object ppUnk);

    }	// interface IOPCGroupStateMgt


    [ComVisible(true),
   Guid("8E368666-D72E-4F78-87ED-647611C61C9F"),
   InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOPCGroupStateMgt2
    {
        void GetState(
            [Out]										out	int pUpdateRate,
            [Out, MarshalAs(UnmanagedType.Bool)]		out	bool pActive,
            [Out, MarshalAs(UnmanagedType.LPWStr)]		out	string ppName,
            [Out]										out	int pTimeBias,
            [Out]										out	float pPercentDeadband,
            [Out]										out	int pLCID,
            [Out]										out	int phClientGroup,
            [Out]										out	int phServerGroup);

        void SetState(
            [In]										IntPtr pRequestedUpdateRate,
            [Out]									out	int pRevisedUpdateRate,
            [In]	                                    IntPtr pActive,
            [In]										IntPtr pTimeBias,
            [In]		                                IntPtr pPercentDeadband,
            [In]		                                IntPtr pLCID,
            [In]		                                IntPtr phClientGroup);

        void SetName(
            [In, MarshalAs(UnmanagedType.LPWStr)]			string szName);

        void CloneGroup(
            [In, MarshalAs(UnmanagedType.LPWStr)]			string szName,
            [In]										ref Guid riid,
            [Out, MarshalAs(UnmanagedType.IUnknown)]	out	object ppUnk);

        void SetKeepAlive(
            [In]                                        int dwKeepAliveTime,
            [Out]                                   out int pdwRevisedKeepAliveTime);

        void GetKeepAlive(
            [Out]                                   out int pdwKeepAliveTime);


    }	// interface IOPCGroupStateMgt


    // ----------------------------------------------------------------- Public Group StateMgt
    [ComVisible(true),
    Guid("39c13a51-011e-11d0-9675-0020afd8adb3"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOPCPublicGroupStateMgt
    {
        void GetState(
            [Out, MarshalAs(UnmanagedType.Bool)]		out	bool pPublic);

        void MoveToPublic();
    }







    // ----------------------------------------------------------------- Item Mgmt
    [ComVisible(true), ComImport,
    Guid("39c13a54-011e-11d0-9675-0020afd8adb3"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOPCItemMgt
    {
        [PreserveSig]
        int AddItems(
            [In]											int dwCount,
            [In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct, SizeParamIndex = 0)] OPCITEMDEF[] pItemArray,
            [Out]										out IntPtr ppAddResults,
            [Out]										out	IntPtr ppErrors);

        [PreserveSig]
        int ValidateItems(
            [In]											int dwCount,
            [In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct, SizeParamIndex = 0)] OPCITEMDEF[] pItemArray,
            [In, MarshalAs(UnmanagedType.Bool)]			bool bBlobUpdate,
            [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct, SizeParamIndex = 0)]out	OPCITEMRESULT[] ppValidationResults,
            [Out]                                       out IntPtr ppErrors);

        [PreserveSig]
        int RemoveItems(
            [In]														int dwCount,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]	int[] phServer,
            [Out]													out	IntPtr ppErrors);

        [PreserveSig]
        int SetActiveState(
            [In]														int dwCount,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]	int[] phServer,
            [In, MarshalAs(UnmanagedType.Bool)]						bool bActive,
            [Out]													out	IntPtr ppErrors);

        [PreserveSig]
        int SetClientHandles(
            [In]														int dwCount,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]	int[] phServer,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]	int[] phClient,
            [Out]													out	IntPtr ppErrors);

        [PreserveSig]
        int SetDatatypes(
            [In]														int dwCount,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]	int[] phServer,
            [In]														IntPtr pRequestedDatatypes,
            [Out]													out	IntPtr ppErrors);

        [PreserveSig]
        int CreateEnumerator(
            [In]										ref Guid riid,
            [Out, MarshalAs(UnmanagedType.IUnknown)]	out	object ppUnk);

    }	// interface IOPCItemMgt



    // ----------------------------------------------------------------- Sync IO
    [ComVisible(true), ComImport,
    Guid("39c13a52-011e-11d0-9675-0020afd8adb3"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOPCSyncIO
    {
        [PreserveSig]
        int Read(
            [In, MarshalAs(UnmanagedType.U4)]							OPCDATASOURCE dwSource,
            [In]														int dwCount,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]	int[] phServer,
            [Out]													out IntPtr ppItemValues,
            [Out]													out	IntPtr ppErrors);

        [PreserveSig]
        int Write(
            [In]														int dwCount,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]	int[] phServer,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]	object[] pItemValues,
            [Out]													out	IntPtr ppErrors);

    }	// interface IOPCSyncIO

    [ComVisible(true), ComImport,
    Guid("730F5F0F-55B1-4C81-9E18-FF8A0904E1FA"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOPCSyncIO2
    {
        [PreserveSig]
        int Read(
            [In, MarshalAs(UnmanagedType.U4)]							OPCDATASOURCE dwSource,
            [In]														int dwCount,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]	int[] phServer,
            [Out]													out IntPtr ppItemValues,
            [Out]													out	IntPtr ppErrors);

        [PreserveSig]
        int Write(
            [In]														int dwCount,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]	int[] phServer,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]	object[] pItemValues,
            [Out]													out	IntPtr ppErrors);

        [PreserveSig]
        int ReadMaxAge(
            [In] int dwCount,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] phServer,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] pdwMaxAge,
            [Out]                                               out IntPtr ppvValues,
            [Out]                                               out IntPtr ppwQualities,
            [Out]                                               out IntPtr ppftTimeStamps,
            [Out]                                               out IntPtr ppErrors);

        [PreserveSig]
        int WriteVQT(
           [In]                                                       int dwCount,
           [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] phServer,
           [In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct, SizeParamIndex = 0)] OPCITEMVQT[] pItemVQT,
           [Out]                                               out IntPtr ppErrors);


    }	// interface IOPCSyncIO 

    // ----------------------------------------------------------------- Async IO
    [ComVisible(true), ComImport,
    Guid("39c13a71-011e-11d0-9675-0020afd8adb3"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOPCAsyncIO2
    {
        [PreserveSig]
        int Read(
            [In]														int dwCount,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]	int[] phServer,
            [In]														int dwTransactionID,
            [Out]													out int pdwCancelID,
            [Out]													out	IntPtr ppErrors);

        [PreserveSig]
        int Write(
            [In]														int dwCount,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]	int[] phServer,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]	object[] pItemValues,
            [In]														int dwTransactionID,
            [Out]													out int pdwCancelID,
            [Out]													out	IntPtr ppErrors);

        void Refresh2(
            [In, MarshalAs(UnmanagedType.U4)]				OPCDATASOURCE dwSource,
            [In]											int dwTransactionID,
            [Out]										out int pdwCancelID);

        void Cancel2(
            [In]											int dwCancelID);

        void SetEnable(
            [In, MarshalAs(UnmanagedType.Bool)]			bool bEnable);

        void GetEnable(
            [Out, MarshalAs(UnmanagedType.Bool)]		out	bool pbEnable);

    }	// interface IOPCAsyncIO2


    [ComVisible(true), ComImport,
    Guid("0967B97B-36EF-423E-B6F8-6BFF1E40D39D"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOPCAsyncIO3
    {
        [PreserveSig]
        int Read(
            [In]														int dwCount,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]	int[] phServer,
            [In]														int dwTransactionID,
            [Out]													out int pdwCancelID,
            [Out]													out	IntPtr ppErrors);

        [PreserveSig]
        int Write(
            [In]														int dwCount,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]	int[] phServer,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]	object[] pItemValues,
            [In]														int dwTransactionID,
            [Out]													out int pdwCancelID,
            [Out]													out	IntPtr ppErrors);

        void Refresh2(
            [In, MarshalAs(UnmanagedType.U4)]				OPCDATASOURCE dwSource,
            [In]											int dwTransactionID,
            [Out]										out int pdwCancelID);

        void Cancel2(
            [In]											int dwCancelID);

        void SetEnable(
            [In, MarshalAs(UnmanagedType.Bool)]			bool bEnable);

        void GetEnable(
            [Out, MarshalAs(UnmanagedType.Bool)]		out	bool pbEnable);

        void ReadMaxAge(
            [In] int dwCount,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] phServer,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] pdwMaxAge,
            [In] int dwTransactionID,
            [Out]out int pdwCancelID,
            [Out]out IntPtr ppErrors);

        void WriteVQT(
            [In] int dwCount,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] int[] phServer,
            [In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Struct, SizeParamIndex = 0)] OPCITEMVQT[] pItemVQT,
            [In]                                                         int dwTransactionID,
            [Out]                                                    out int pdwCancelID,
            [Out]                                                 out IntPtr ppErrors);

        void RefreshMaxAge(
            [In]                                                        int dwMaxAge,
            [In]                                                        int dwTransactionID,
            [Out]                                                   out int pdwCancelID);



    }	// interface IOPCAsyncIO2






    // ----------------------------------------------------------------- Async Callback
    [ComVisible(true), ComImport,
    Guid("39c13a70-011e-11d0-9675-0020afd8adb3"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOPCDataCallback
    {
        void OnDataChange(
            [In]											int dwTransid,
            [In]											int hGroup,
            [In]											int hrMasterquality,
            [In]											int hrMastererror,
            [In]											int dwCount,
            [In]											IntPtr phClientItems,
            [In]											IntPtr pvValues,
            [In]											IntPtr pwQualities,
            [In]											IntPtr pftTimeStamps,
            [In]											IntPtr ppErrors);

        void OnReadComplete(
            [In]											int dwTransid,
            [In]											int hGroup,
            [In]											int hrMasterquality,
            [In]											int hrMastererror,
            [In]											int dwCount,
            [In]											IntPtr phClientItems,
            [In]											IntPtr pvValues,
            [In]											IntPtr pwQualities,
            [In]											IntPtr pftTimeStamps,
            [In]											IntPtr ppErrors);

        void OnWriteComplete(
            [In]											int dwTransid,
            [In]											int hGroup,
            [In]											int hrMastererr,
            [In]											int dwCount,
            [In]											IntPtr pClienthandles,
            [In]											IntPtr ppErrors);

        void OnCancelComplete(
            [In]											int dwTransid,
            [In]											int hGroup);

    }	// interface IOPCDataCallback




    // ----------------------------------------------------------------- Enum Item Attributes
    [ComVisible(true), ComImport,
    Guid("39c13a55-011e-11d0-9675-0020afd8adb3"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumOPCItemAttributes
    {
        void Next(
            [In]											int celt,
            [Out]										out	IntPtr ppItemArray,
            [Out]										out int pceltFetched);

        void Skip(
            [In]											int celt);

        void Reset();

        void Clone(
            [Out, MarshalAs(UnmanagedType.Interface)]	out	IEnumOPCItemAttributes ppUnk);

    }	// interface IEnumOPCItemAttributes

}

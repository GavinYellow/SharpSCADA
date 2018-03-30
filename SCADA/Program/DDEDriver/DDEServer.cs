using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace DDEDriver
{
    public abstract class DdemlServer : IDisposable
    {
        private int _InstanceId;                                           // DDEML instance identifier
        private string _Service;                                          // DDEML service name
        private IntPtr _ServiceHandle = IntPtr.Zero;                                 // DDEML service handle

        private bool _Disposed = false;
        private HashSet<IntPtr> _ConversationTable = new HashSet<IntPtr>();
        private DdeCallback _Callback;

        public DdemlServer()
        {
            _Callback = OnDdeCallback;
        }

        ~DdemlServer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                _Disposed = true;
                if (IsRegistered)
                {
                    // Unregister the service name.
                    Unregister();
                    if (disposing)
                    {
                        _ServiceHandle = IntPtr.Zero;
                        _InstanceId = 0;
                    }
                }
            }
        }

        public string Service
        {
            get { return _Service; }
        }

        public bool IsRegistered
        {
            get { return _ServiceHandle != IntPtr.Zero; }
        }

        internal bool IsDisposed
        {
            get { return _Disposed; }
        }

        public int Initialize(int afCmd)
        {
            int instanceId = 0;
            Ddeml.DdeInitialize(ref instanceId, _Callback, afCmd, 0);
            return instanceId;
        }

        public void Uninitialize()
        {
            Ddeml.DdeUninitialize(_InstanceId);
        }

        public string Register(string service)
        {
            if (IsRegistered || _InstanceId != 0)
            {
                throw new InvalidOperationException("AlreadyRegisteredMessage");
            }
            if (service == null || service.Length > Ddeml.MAX_STRING_SIZE)
            {
                throw new ArgumentNullException("service");
            }
            _Service = service;
            _ConversationTable.Clear();
            _InstanceId = Initialize(Ddeml.APPCLASS_STANDARD);
            _ServiceHandle = Ddeml.DdeCreateStringHandle(_InstanceId, _Service, Ddeml.CP_WINANSI);
            // Register the service name.
            if (Ddeml.DdeNameService(_InstanceId, _ServiceHandle, IntPtr.Zero, Ddeml.DNS_REGISTER) == IntPtr.Zero)
            {
                Ddeml.DdeFreeStringHandle(_InstanceId, _ServiceHandle);
                _ServiceHandle = IntPtr.Zero;
            }
            // If the service handle is null then the service name could not be registered.
            if (_ServiceHandle == IntPtr.Zero)
            {
                int error = Ddeml.DdeGetLastError(_InstanceId);
                return Ddeml.DDEGetErrorMsg(error);
            }
            return null;
        }

        public void Unregister()
        {
            Ddeml.DdeNameService(_InstanceId, _ServiceHandle, IntPtr.Zero, Ddeml.DNS_UNREGISTER);
            // Free the service string handle.    
            Ddeml.DdeFreeStringHandle(_InstanceId, _ServiceHandle);
            // Indicate that the service name is no longer registered.
            _ServiceHandle = IntPtr.Zero;
            _InstanceId = 0;
        }

        private IntPtr OnDdeCallback(int uType, ConversionFormat uFmt, IntPtr hConv, IntPtr hsz1, IntPtr hsz2, IntPtr hData, uint dwData1, uint dwData2)
        {
            // Create a new transaction object that will be dispatched to a DdemlClient, DdemlServer, or ITransactionFilter.
            // Dispatch the transaction.
            switch (uType)
            {
                case Ddeml.XTYP_MONITOR:
                    switch (dwData2)
                    {
                        case Ddeml.MF_CALLBACKS:
                            {
                                // Get the MONCBSTRUCT object.
                                int length = 0;
                                IntPtr phData = Ddeml.DdeAccessData(hData, ref length);
                                MONCBSTRUCT mon = (MONCBSTRUCT)Marshal.PtrToStructure(phData, typeof(MONCBSTRUCT));
                                Ddeml.DdeUnaccessData(hData);
                                OnCallback(mon);
                                return IntPtr.Zero;
                            }
                        case Ddeml.MF_CONV:
                            {
                                // Get the MONCONVSTRUCT object.
                                int length = 0;
                                IntPtr phData = Ddeml.DdeAccessData(hData, ref length);
                                MONCONVSTRUCT mon = (MONCONVSTRUCT)Marshal.PtrToStructure(phData, typeof(MONCONVSTRUCT));
                                Ddeml.DdeUnaccessData(hData);
                                OnConversation(mon);
                                return IntPtr.Zero;
                            }
                        case Ddeml.MF_ERRORS:
                            {
                                // Get the MONERRSTRUCT object.
                                int length = 0;
                                IntPtr phData = Ddeml.DdeAccessData(hData, ref length);
                                MONERRSTRUCT mon = (MONERRSTRUCT)Marshal.PtrToStructure(phData, typeof(MONERRSTRUCT));
                                Ddeml.DdeUnaccessData(hData);
                                OnError(mon);
                                return IntPtr.Zero;
                            }
                        case Ddeml.MF_HSZ_INFO:
                            {
                                // Get the MONHSZSTRUCT object.
                                int length = 0;
                                IntPtr phData = Ddeml.DdeAccessData(hData, ref length);
                                MONHSZSTRUCT mon = (MONHSZSTRUCT)Marshal.PtrToStructure(phData, typeof(MONHSZSTRUCT));
                                Ddeml.DdeUnaccessData(hData);
                                OnString(mon);
                                return IntPtr.Zero;
                            }
                        case Ddeml.MF_LINKS:
                            {
                                // Get the MONLINKSTRUCT object.
                                int length = 0;
                                IntPtr phData = Ddeml.DdeAccessData(hData, ref length);
                                MONLINKSTRUCT mon = (MONLINKSTRUCT)Marshal.PtrToStructure(phData, typeof(MONLINKSTRUCT));
                                Ddeml.DdeUnaccessData(hData);
                                OnLink(mon);
                                return IntPtr.Zero;
                            }
                        case Ddeml.MF_POSTMSGS:
                            {
                                // Get the MONMSGSTRUCT object.
                                int length = 0;
                                IntPtr phData = Ddeml.DdeAccessData(hData, ref length);
                                MONMSGSTRUCT mon = (MONMSGSTRUCT)Marshal.PtrToStructure(phData, typeof(MONMSGSTRUCT));
                                Ddeml.DdeUnaccessData(hData);
                                OnPost(mon);
                                return IntPtr.Zero;
                            }
                        case Ddeml.MF_SENDMSGS:
                            {
                                // Get the MONMSGSTRUCT object.
                                int length = 0;
                                IntPtr phData = Ddeml.DdeAccessData(hData, ref length);
                                MONMSGSTRUCT mon = (MONMSGSTRUCT)Marshal.PtrToStructure(phData, typeof(MONMSGSTRUCT));
                                Ddeml.DdeUnaccessData(hData);
                                OnSend(mon);
                                return IntPtr.Zero;
                            }
                    }
                    break;
                case Ddeml.XTYP_ADVDATA:
                    unsafe
                    {
                        sbyte* pSZ = stackalloc sbyte[Ddeml.MAX_STRING_SIZE];
                        int len = Ddeml.DdeQueryString(_InstanceId, hsz1, pSZ, Ddeml.MAX_STRING_SIZE, Ddeml.CP_WINANSI);
                        string topic = new string(pSZ);
                        len = Ddeml.DdeQueryString(_InstanceId, hsz2, pSZ, Ddeml.MAX_STRING_SIZE, Ddeml.CP_WINANSI);
                        string item = new string(pSZ);
                        byte* bt = stackalloc byte[Ddeml.MAX_STRING_SIZE];
                        len = Ddeml.DdeGetData(hData, bt, Ddeml.MAX_STRING_SIZE, 0);
                        byte[] bytes = new byte[len]; for (int i = 0; i < len; i++) { bytes[i] = *bt++; };
                        if (hData != IntPtr.Zero) Ddeml.DdeUnaccessData(hData);
                        return new IntPtr((int)OnAdvData(uFmt, topic, item, bytes));
                    }
                case Ddeml.XTYP_ADVREQ:
                    unsafe
                    {
                        sbyte* pSZ = stackalloc sbyte[Ddeml.MAX_STRING_SIZE];
                        int len = Ddeml.DdeQueryString(_InstanceId, hsz1, pSZ, Ddeml.MAX_STRING_SIZE, Ddeml.CP_WINANSI);
                        string topic = new string(pSZ);
                        len = Ddeml.DdeQueryString(_InstanceId, hsz2, pSZ, Ddeml.MAX_STRING_SIZE, Ddeml.CP_WINANSI);
                        string item = new string(pSZ);
                        byte[] data = OnAdvReq(uFmt, topic, item);
                        // Create and return the data handle representing the data being advised.
                        if (data != null && data.Length > 0)
                        {
                            return Ddeml.DdeCreateDataHandle(_InstanceId, data, data.Length, 0, hsz2, uFmt, 0); ;
                        }
                        // This transaction could not be Ddeml.DDE_FACK here.
                        return IntPtr.Zero;
                    }
                case Ddeml.XTYP_ADVSTART:
                    unsafe
                    {
                        // Get the item name from the hsz2 string handle.
                        sbyte* pSZ = stackalloc sbyte[Ddeml.MAX_STRING_SIZE];
                        int len = Ddeml.DdeQueryString(_InstanceId, hsz1, pSZ, Ddeml.MAX_STRING_SIZE, Ddeml.CP_WINANSI);
                        string topic = new string(pSZ);
                        len = Ddeml.DdeQueryString(_InstanceId, hsz2, pSZ, Ddeml.MAX_STRING_SIZE, Ddeml.CP_WINANSI);
                        string item = new string(pSZ);
                        // Get a value indicating whether an advise loop should be initiated from the subclass.
                        //AdvStart(hConv, item, uFmt, Ddeml.XTYPF_ACKREQ);
                        return OnAdvStart(uFmt, topic, item) ? new IntPtr(1) : IntPtr.Zero;
                    }
                case Ddeml.XTYP_ADVSTOP:
                    unsafe
                    {
                        // Get the item name from the hsz2 string handle.
                        sbyte* pSZ = stackalloc sbyte[Ddeml.MAX_STRING_SIZE];
                        int len = Ddeml.DdeQueryString(_InstanceId, hsz1, pSZ, Ddeml.MAX_STRING_SIZE, Ddeml.CP_WINANSI);
                        string topic = new string(pSZ);
                        len = Ddeml.DdeQueryString(_InstanceId, hsz2, pSZ, Ddeml.MAX_STRING_SIZE, Ddeml.CP_WINANSI);
                        string item = new string(pSZ);
                        // Inform the subclass that the advise loop has been terminated.
                        //AdvStop(hConv, item, uFmt);
                        OnAdvStop(uFmt, topic, item);
                        break;
                    }
                case Ddeml.XTYP_CONNECT:
                    unsafe
                    {
                        sbyte* pSZ = stackalloc sbyte[Ddeml.MAX_STRING_SIZE];
                        int len = Ddeml.DdeQueryString(_InstanceId, hsz1, pSZ, Ddeml.MAX_STRING_SIZE, Ddeml.CP_WINANSI);
                        string topic = new string(pSZ);
                        // Get a value from the subclass indicating whether the connection should be allowed.
                        return OnConnect(topic, new CONVCONTEXT(), true) ? new IntPtr(1) : IntPtr.Zero;
                    }
                case Ddeml.XTYP_CONNECT_CONFIRM:
                    unsafe
                    {
                        sbyte* pSZ = stackalloc sbyte[Ddeml.MAX_STRING_SIZE];
                        int len = Ddeml.DdeQueryString(_InstanceId, hsz1, pSZ, Ddeml.MAX_STRING_SIZE, Ddeml.CP_WINANSI);
                        string topic = new string(pSZ);
                        // Create a Conversation object and add it to the hConv table.
                        _ConversationTable.Add(hConv);
                        // Inform the subclass that a hConv has been established.
                        OnConnectConfirm(topic, true);
                        break;
                    }
                case Ddeml.XTYP_DISCONNECT:
                    {
                        // Remove the Conversation from the hConv table.
                        _ConversationTable.Remove(hConv);
                        // Inform the subclass that the hConv has been disconnected.
                        OnDisconnect(true);
                        // Return zero to indicate that there are no problems.
                        return IntPtr.Zero;
                    }
                case Ddeml.XTYP_EXECUTE:
                    unsafe
                    {
                        // Get the command from the data handle.
                        sbyte* pSZ = stackalloc sbyte[Ddeml.MAX_STRING_SIZE];
                        int len = Ddeml.DdeQueryString(_InstanceId, hsz1, pSZ, Ddeml.MAX_STRING_SIZE, Ddeml.CP_WINANSI);
                        string topic = new string(pSZ);
                        byte* bt = stackalloc byte[Ddeml.MAX_STRING_SIZE];
                        len = Ddeml.DdeGetData(hData, bt, Ddeml.MAX_STRING_SIZE, 0);
                        string command = new string((sbyte*)bt);
                        // Send the command to the subclass and get the resul
                        return new IntPtr((int)OnExecute(topic, command.TrimEnd('\0')));
                    }
                case Ddeml.XTYP_POKE:
                    unsafe
                    {
                        // Get the item name from the hsz2 string handle.
                        sbyte* pSZ = stackalloc sbyte[Ddeml.MAX_STRING_SIZE];
                        int len = Ddeml.DdeQueryString(_InstanceId, hsz1, pSZ, Ddeml.MAX_STRING_SIZE, Ddeml.CP_WINANSI);
                        string topic = new string(pSZ);
                        len = Ddeml.DdeQueryString(_InstanceId, hsz2, pSZ, Ddeml.MAX_STRING_SIZE, Ddeml.CP_WINANSI);
                        string item = new string(pSZ);
                        byte* bt = stackalloc byte[Ddeml.MAX_STRING_SIZE];
                        len = Ddeml.DdeGetData(hData, bt, Ddeml.MAX_STRING_SIZE, 0);
                        byte[] data = new byte[len]; for (int i = 0; i < len; i++) { data[i] = *bt++; };
                        // Send the data to the subclass and get the resul
                        return new IntPtr((int)OnPoke(uFmt, topic, item, data));
                    }
                case Ddeml.XTYP_REQUEST:
                    unsafe
                    {
                        // Get the item name from the hsz2 string handle.
                        sbyte* pSZ = stackalloc sbyte[Ddeml.MAX_STRING_SIZE];
                        int len = Ddeml.DdeQueryString(_InstanceId, hsz1, pSZ, Ddeml.MAX_STRING_SIZE, Ddeml.CP_WINANSI);
                        string topic = new string(pSZ);
                        len = Ddeml.DdeQueryString(_InstanceId, hsz2, pSZ, Ddeml.MAX_STRING_SIZE, Ddeml.CP_WINANSI);
                        string item = new string(pSZ);
                        // Send the request to the subclass and get the resul
                        var result = OnRequest(uFmt, topic, item);
                        // Return a data handle if the subclass Ddeml.DDE_FACK the request successfully.
                        if (result != null)
                        {
                            return Ddeml.DdeCreateDataHandle(_InstanceId, result, result.Length, 0, hsz2, uFmt, 0);
                        }
                        // Return DDE_FDdeml.DDE_FNOTDdeml.DDE_FACK if the subclass did not process the command.
                        return new IntPtr(Ddeml.DDE_FNOTPROCESSED);
                    }
                case Ddeml.XTYP_XACT_COMPLETE:
                    unsafe
                    {
                        sbyte* pSZ = stackalloc sbyte[Ddeml.MAX_STRING_SIZE];
                        int len = Ddeml.DdeQueryString(_InstanceId, hsz1, pSZ, Ddeml.MAX_STRING_SIZE, Ddeml.CP_WINANSI);
                        string topic = new string(pSZ);
                        len = Ddeml.DdeQueryString(_InstanceId, hsz2, pSZ, Ddeml.MAX_STRING_SIZE, Ddeml.CP_WINANSI);
                        string item = new string(pSZ);
                        OnXactComplete(uFmt, topic, item, hData, dwData1);
                        break;
                    }
                case Ddeml.XTYP_WILDCONNECT:
                    {
                        // This library does not support wild connects.
                        return IntPtr.Zero;
                    }

                case Ddeml.XTYP_ERROR:
                    {
                        // Get the error code, but do nothing with it at this time.
                        return IntPtr.Zero;
                    }
                case Ddeml.XTYP_REGISTER:
                    unsafe
                    {
                        // Get the service name from the hsz1 string handle.
                        sbyte* pSZ = stackalloc sbyte[Ddeml.MAX_STRING_SIZE];
                        int len = Ddeml.DdeQueryString(_InstanceId, hsz1, pSZ, Ddeml.MAX_STRING_SIZE, Ddeml.CP_WINANSI);
                        string bas = new string(pSZ);
                        len = Ddeml.DdeQueryString(_InstanceId, hsz2, pSZ, Ddeml.MAX_STRING_SIZE, Ddeml.CP_WINANSI);
                        string inst = new string(pSZ);
                        OnRegister(bas, inst);
                        return IntPtr.Zero;
                    }

                case Ddeml.XTYP_UNREGISTER:
                    unsafe
                    {
                        sbyte* pSZ = stackalloc sbyte[Ddeml.MAX_STRING_SIZE];
                        int len = Ddeml.DdeQueryString(_InstanceId, hsz1, pSZ, Ddeml.MAX_STRING_SIZE, Ddeml.CP_WINANSI);
                        string bas = new string(pSZ);
                        len = Ddeml.DdeQueryString(_InstanceId, hsz2, pSZ, Ddeml.MAX_STRING_SIZE, Ddeml.CP_WINANSI);
                        string inst = new string(pSZ);
                        OnUnRegister(bas, inst);
                        return IntPtr.Zero;
                    }
            }
            return IntPtr.Zero;
        }

        public void Advise(string topic, string item)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().ToString());
            }
            if (!IsRegistered)
            {
                throw new InvalidOperationException("NotRegisteredMessage");
            }
            if (topic == null || topic.Length > Ddeml.MAX_STRING_SIZE)
            {
                throw new ArgumentNullException("topic");
            }
            if (item == null || item.Length > Ddeml.MAX_STRING_SIZE)
            {
                throw new ArgumentNullException("item");
            }
            // Assume the topic name and item name are wild.
            IntPtr topicHandle = topic != "*" ? Ddeml.DdeCreateStringHandle(_InstanceId, topic, Ddeml.CP_WINANSI) : IntPtr.Zero;
            IntPtr itemHandle = item != "*" ? Ddeml.DdeCreateStringHandle(_InstanceId, item, Ddeml.CP_WINANSI) : IntPtr.Zero;
            // Check the result to see if the post failed.
            if (!Ddeml.DdePostAdvise(_InstanceId, topicHandle, itemHandle))
            {
                int error = Ddeml.DdeGetLastError(_InstanceId);
                var msg = Ddeml.DDEGetErrorMsg(error); if (msg != null) { }
            }
            Ddeml.DdeFreeStringHandle(_InstanceId, itemHandle);
            Ddeml.DdeFreeStringHandle(_InstanceId, topicHandle);
        }

        public void Pause(IntPtr hConv)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().ToString());
            }
            if (!IsRegistered)
            {
                throw new InvalidOperationException("NotRegisteredMessage");
            }
            // Check the result to see if the DDEML callback was disabled.
            if (!Ddeml.DdeEnableCallback(_InstanceId, hConv, Ddeml.EC_DISABLE))
            {
                int error = Ddeml.DdeGetLastError(_InstanceId);
                Ddeml.DDEGetErrorMsg(error);
            }
        }

        public void Resume(IntPtr hConv)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().ToString());
            }
            if (!IsRegistered)
            {
                throw new InvalidOperationException("NotRegisteredMessage");
            }
            // Check the result to see if the DDEML callback was enabled.
            if (!Ddeml.DdeEnableCallback(_InstanceId, hConv, Ddeml.EC_ENABLEALL))
            {
                int error = Ddeml.DdeGetLastError(_InstanceId);
                Ddeml.DDEGetErrorMsg(error);
            }
        }

        public void Disconnect(IntPtr hConv)
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().ToString());
            }
            if (!IsRegistered)
            {
                throw new InvalidOperationException("NotRegisteredMessage");
            }
            if (_ConversationTable.Contains(hConv))
            {
                // Terminate the hConv.
                Ddeml.DdeDisconnect(hConv);
                // Remove the Conversation from the hConv table.
                _ConversationTable.Remove(hConv);
            }
        }

        public void Disconnect()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().ToString());
            }
            if (!IsRegistered)
            {
                throw new InvalidOperationException("NotRegisteredMessage");
            }
            // Terminate all conversations.
            foreach (IntPtr hConv in _ConversationTable)
            {
                Ddeml.DdeDisconnect(hConv);
            }
            // clear the hConv table.
            _ConversationTable.Clear();
        }

        // Démarre une transaction Advise.
        public bool AdvStart(IntPtr hConv, string item, ConversionFormat wFormat, int wFlag)
        {
            if (!IsRegistered)
                return false;
            int res = 0;
            // Création de la chaîne DDE de l'élément.
            IntPtr hszItem = Ddeml.DdeCreateStringHandle(_InstanceId, item, Ddeml.CP_WINANSI);
            if ((hszItem == IntPtr.Zero) && (item.Length != 0))
                return false;
            // Exécution de la transaction.
            Ddeml.DdeClientTransaction(null, 0, hConv, hszItem, wFormat, (wFlag == Ddeml.XTYPF_ACKREQ) ?
                (Ddeml.XTYP_ADVSTARTACKREQ) : ((wFlag == Ddeml.XTYPF_NODATA) ? (Ddeml.XTYP_ADVSTARTNODATA) : (Ddeml.XTYP_ADVSTART)), Ddeml.TIMEOUT_ASYNC, ref res);
            // Libération de la chaîne DDE.
            if (hszItem != IntPtr.Zero)
                Ddeml.DdeFreeStringHandle(_InstanceId, hszItem);
            return res != 0;
        }

        // Arrête une transaction Advise.
        public void AdvStop(IntPtr hConv, string item, ConversionFormat wFormat)
        {
            if (!IsRegistered)
                return;
            // Création de la chaîne DDE de l'élément.
            IntPtr hszItem = Ddeml.DdeCreateStringHandle(_InstanceId, item, Ddeml.CP_WINANSI);
            if ((hszItem == IntPtr.Zero) && (item.Length != 0))
                return;
            // Exécution de la transaction.
            int res = 0;
            Ddeml.DdeClientTransaction(null, 0, hConv, hszItem, wFormat, Ddeml.XTYP_ADVSTOP, Ddeml.TIMEOUT_ASYNC, ref res);
            // Libération de la chaîne DDE.
            if (hszItem != IntPtr.Zero)
                Ddeml.DdeFreeStringHandle(_InstanceId, hszItem);
        }


        protected virtual DDEResult OnAdvData(ConversionFormat uFormat, string topic, string item, byte[] data)
        {
            return DDEResult.FNOTPROCESSED;
        }

        protected virtual byte[] OnAdvReq(ConversionFormat uFormat, string topic, string item)
        {
            return null;
        }

        protected virtual bool OnAdvStart(ConversionFormat uFormat, string topic, string item)
        {
            return true;
        }

        protected virtual void OnAdvStop(ConversionFormat uFormat, string topic, string item)
        {
        }

        protected virtual bool OnConnect(string topic, CONVCONTEXT context, bool sameInstance)
        {
            return true;
        }

        protected virtual void OnConnectConfirm(string topic, bool sameInstance)
        {
        }

        protected virtual void OnDisconnect(bool sameInstance)
        {
        }

        protected virtual void OnError(ushort errorCode)
        {
        }

        protected virtual DDEResult OnExecute(string topic, string command)
        {
            return DDEResult.FNOTPROCESSED;
        }

        protected virtual byte[] OnRequest(ConversionFormat ufmt, string topic, string item)
        {
            return null;
        }

        protected virtual DDEResult OnPoke(ConversionFormat uFormat, string topic, string item, byte[] data)
        {
            return DDEResult.FNOTPROCESSED;
        }

        protected virtual void OnRegister(string baseServiceName, string instanceServiceName)
        {
        }

        protected virtual void OnUnRegister(string baseServiceName, string instanceServiceName)
        {
        }

        protected virtual void OnXactComplete(ConversionFormat uFormat, string topic, string item, IntPtr data, uint transactionID)
        {
        }

        protected virtual void OnCallback(MONCBSTRUCT mon)
        {
        }

        protected virtual void OnConversation(MONCONVSTRUCT mon)
        {
        }

        protected virtual void OnError(MONERRSTRUCT mon)
        {
        }

        protected virtual void OnString(MONHSZSTRUCT mon)
        {
        }

        protected virtual void OnLink(MONLINKSTRUCT mon)
        {
        }

        protected virtual void OnPost(MONMSGSTRUCT mon)
        {
        }

        protected virtual void OnSend(MONMSGSTRUCT mon)
        {
        }
        /// <summary>
        /// This class is needed to dispose of DDEML resources correctly since the DDEML is thread specific.

    } // class

}

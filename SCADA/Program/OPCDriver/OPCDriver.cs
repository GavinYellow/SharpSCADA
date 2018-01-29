using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using DataService;
using OPC.Data.Class;
using OPC.Data.Enum;
using OPC.Data.Interface;
using OPC.Data.Struct;

namespace OPCDriver
{
    public class OPCGroup : IGroup, IOPCDataCallback
    {
        bool _isActive;
        public bool IsActive
        {
            get
            {
                return _isActive;
            }
            set
            {
                //SetActive(value);
                //_isActive = GetState().Active;
                _isActive = value;//OPC 该接口因COM组件的问题导致不能用！
            }
        }

        int _Id;
        public short ID
        {
            get
            {
                return (short)_Id;
            }
        }

        int _updateRate;
        public int UpdateRate
        {
            get
            {
                return GetState().UpdateRate;
            }
            set
            {
                SetUpdateRate(value);
            }
        }

        float _deadBand;
        public float DeadBand
        {
            get
            {
                return GetState().Deadband;
            }
            set
            {
                SetState(new OPCGROUPSTATE { Deadband = value });
            }
        }

        string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        OPCReader _plcReader;
        public IDriver Parent
        {
            get
            {
                return _plcReader;
            }
        }

        IDataServer _server;
        public IDataServer Server
        {
            get
            {
                return _server;
            }
        }

        Dictionary<short, ITag> mapping;
        List<ITag> _items;
        public IEnumerable<ITag> Items
        {
            get { return _items; }
        }

        object _grpObj;
        private IOPCItemMgt _itemMgt;
        private IOPCGroupStateMgt2 _grpState;
        private IOPCAsyncIO3 _async;
        private IOPCSyncIO2 _sync;

        private void SetState(OPCGROUPSTATE state)
        {
            _grpState.SetState(new int[] { state.UpdateRate },
                out state.UpdateRate, new bool[] { state.Active }, new int[] { state.TimeBias },
             new float[] { state.Deadband }, new int[] { state.LocaleId }, new int[] { state.ClientId });
        }

        private OPCGROUPSTATE GetState()
        {
            OPCGROUPSTATE state = new OPCGROUPSTATE();
            _grpState.GetState(out state.UpdateRate, out state.Active, out _name,
               out state.TimeBias, out state.Deadband, out state.LocaleId, out state.ClientId, out _Id);
            return state;
        }

        public int SetActive(bool active)
        {
            int updateRate;
            return _grpState.SetState(null, out updateRate, new bool[] { active }, null, null, null, null);
        }

        public int SetUpdateRate(int updateRate)
        {
            int revised;
            _grpState.SetState(new int[] { updateRate }, out revised, null, null, null, null, null);
            return revised;
        }

        public bool AddItems(IList<TagMetaData> items)
        {
            int count = items.Count;
            if (_items == null)
            {
                _items = new List<ITag>(count);
                mapping = new Dictionary<short, ITag>(count);
            }
            List<OPCITEMDEF> itemArray = new List<OPCITEMDEF>(count);
            for (int i = 0; i < count; i++)
            {
                if (items[i].GroupID == this._Id)
                {
                    itemArray.Add(new OPCITEMDEF { hClient = items[i].ID, szItemID = items[i].Address, bActive = true, wReserved = (short)i });
                }
            }
            IntPtr pAddResults;
            IntPtr pErrors;
            if (!HRESULTS.Succeeded(_itemMgt.AddItems(itemArray.Count, itemArray.ToArray(), out pAddResults, out pErrors)))
                return false;
            int iStructSize = Marshal.SizeOf(typeof(OPCITEMRESULT));
            lock (_server.SyncRoot)
            {
                for (int i = 0; i < itemArray.Count; i++)
                {
                    try
                    {
                        if (Marshal.ReadInt32(pErrors) == 0)
                        {
                            ITag dataItem = null;
                            var itemDef = itemArray[i];
                            //string addr = string.Concat(_serverId, ',', Marshal.ReadInt32(pAddResults));
                            DataType type = items[itemDef.wReserved].DataType;
                            switch (type)
                            {
                                case DataType.BOOL:
                                    dataItem = new BoolTag((short)itemDef.hClient, new DeviceAddress(-0x100, 0, 0, Marshal.ReadInt32(pAddResults), 1, 0, DataType.BOOL), this);
                                    break;
                                case DataType.BYTE:
                                    dataItem = new ByteTag((short)itemDef.hClient, new DeviceAddress(-0x100, 0, 0, Marshal.ReadInt32(pAddResults), 1, 0, DataType.BYTE), this);
                                    break;
                                case DataType.WORD:
                                    dataItem = new UShortTag((short)itemDef.hClient, new DeviceAddress(-0x100, 0, 0, Marshal.ReadInt32(pAddResults), 2, 0, DataType.WORD), this);
                                    break;
                                case DataType.SHORT:
                                    dataItem = new ShortTag((short)itemDef.hClient, new DeviceAddress(-0x100, 0, 0, Marshal.ReadInt32(pAddResults), 2, 0, DataType.SHORT), this);
                                    break;
                                case DataType.INT:
                                    dataItem = new IntTag((short)itemDef.hClient, new DeviceAddress(-0x100, 0, 0, Marshal.ReadInt32(pAddResults), 4, 0, DataType.INT), this);
                                    break;
                                case DataType.DWORD:
                                    dataItem = new UIntTag((short)itemDef.hClient, new DeviceAddress(-0x100, 0, 0, Marshal.ReadInt32(pAddResults), 4, 0, DataType.DWORD), this);
                                    break;
                                case DataType.FLOAT:
                                    dataItem = new FloatTag((short)itemDef.hClient, new DeviceAddress(-0x100, 0, 0, Marshal.ReadInt32(pAddResults), 4, 0, DataType.FLOAT), this);
                                    break;
                                case DataType.SYS:
                                case DataType.STR:
                                    dataItem = new StringTag((short)itemDef.hClient, new DeviceAddress(-0x100, 0, 0, Marshal.ReadInt32(pAddResults), 64, 0, DataType.STR), this);
                                    break;
                                default:
                                    break;
                                    //case VarEnum.VT_ARRAY:
                                    //    dataItem = new ArrayTag((short)itemDef.hClient, new DeviceAddress(-0x100, 0, 0, Marshal.ReadInt32(pAddResults), (byte)Marshal.ReadInt16(pAddResults + 18), 0, DataType.ARRAY), this);
                                    //    break;
                            }
                            if (dataItem != null)
                            {
                                _items.Add(dataItem);
                                mapping.Add((short)itemDef.hClient, dataItem);
                                _server.AddItemIndex(items[itemDef.wReserved].Name, dataItem);
                            }
                            pAddResults += iStructSize;
                            pErrors += 4;
                        }
                    }
                    catch(Exception err)
                    {
                        if (err.Message != null) { }
                    }
                }
                //Marshal.FreeCoTaskMem(pAddResults);
                //Marshal.FreeCoTaskMem(pErrors);
                _items.TrimExcess();
                return true;
            }
        }

        public bool AddTags(IEnumerable<ITag> tags)
        {
            if (_items == null)
            {
                _items = new List<ITag>();
            }
            foreach (ITag tag in tags)
            {
                if (tag != null)
                {
                    _items.Add(tag);
                    mapping.Add((short)tag.ID, tag);
                }
            }
            _items.TrimExcess();
            return true;
        }

        public bool RemoveItems(params ITag[] items)
        {
            int count = items.Length;
            int[] arrSvr = new int[count];
            for (int i = 0; i < count; i++)
            {
                ITag item = items[i];
                //if (item == null)
                //    return false;

                arrSvr[i] = item.Address.Start;
                _server.RemoveItemIndex(item.GetTagName());
                _items.Remove(item);
            }
            IntPtr ptrErr;
            int result = _itemMgt.RemoveItems(count, arrSvr, out ptrErr);
            Marshal.FreeCoTaskMem(ptrErr);
            return HRESULTS.Succeeded(result);
        }

        public bool SetActiveState(bool active, params short[] items)
        {
            int count = items.Length;
            int[] arrSvr = new int[count];
            for (int i = 0; i < count; i++)
            {
                ITag item = GetItemByID(items[i]);
                if (item != null)
                    arrSvr[i] = item.Address.Start;
            }
            IntPtr ptrErr;
            if (HRESULTS.Succeeded(_itemMgt.SetActiveState(count, arrSvr, active, out ptrErr)))
            {
                Marshal.FreeCoTaskMem(ptrErr);
                return true;
            }
            return false;
        }

        public ITag GetItemByID(short id)
        {
            ITag item = null;
            mapping.TryGetValue(id, out item);
            return item;
        }

        public ITag FindItemByAddress(DeviceAddress addr)
        {
            int start = addr.Start;
            for (int i = 0; i < _items.Count; i++)
            {
                if (start == _items[i].Address.Start)
                    return _items[i];
            }
            return null;
        }

        public HistoryData[] BatchRead(DataSource source, bool isSync, params ITag[] itemArray)
        {
            IntPtr pErrors;
            int len = itemArray.Length;
            int result = 0;
            int[] arrHSrv = Array.ConvertAll(itemArray, c => c.Address.Start);
            HistoryData[] values = new HistoryData[len];
            if (isSync)
            {
                IntPtr pItemValues;
                result = _sync.Read((OPCDATASOURCE)source, len, arrHSrv, out pItemValues, out pErrors);
                if (HRESULTS.Succeeded(result))
                {
                    for (int i = 0; i < len; i++)
                    {
                        var item = itemArray[i];
                        if (Marshal.ReadInt32(pErrors) == 0)
                        {
                            switch (item.Address.VarType)
                            {
                                case DataType.BOOL:
                                    values[i].Value.Boolean = Marshal.ReadByte(pItemValues + 16) > 0;
                                    break;
                                case DataType.BYTE:
                                    values[i].Value.Byte = Marshal.ReadByte(pItemValues + 16);
                                    break;
                                case DataType.WORD:
                                    values[i].Value.Word = (ushort)Marshal.ReadInt16(pItemValues + 16);
                                    break;
                                case DataType.SHORT:
                                    values[i].Value.Int16 = Marshal.ReadInt16(pItemValues + 16);
                                    break;
                                case DataType.DWORD:
                                    values[i].Value.DWord = (uint)Marshal.ReadInt32(pItemValues + 16);
                                    break;
                                case DataType.INT:
                                    values[i].Value.Int32 = Marshal.ReadInt32(pItemValues + 16);
                                    break;
                                case DataType.FLOAT:
                                    float[] x = new float[1];
                                    Marshal.Copy(pItemValues + 16, x, 0, 1);
                                    values[i].Value.Single = x[0];
                                    break;
                                case DataType.STR:
                                    string str = Marshal.PtrToStringUni(Marshal.ReadIntPtr(pItemValues + 16));
                                    StringTag tag = item as StringTag;
                                    if (tag != null)
                                        tag.String = str;
                                    break;
                            }
                            values[i].ID = item.ID;
                            values[i].Quality = (QUALITIES)Marshal.ReadInt16(pItemValues + 12);
                            values[i].TimeStamp = Marshal.ReadInt64(pItemValues + 4).ToDateTime();
                            item.Update(values[i].Value, values[i].TimeStamp, values[i].Quality);
                        }
                        pItemValues += 32;
                        pErrors += 4;
                    }
                    Marshal.FreeCoTaskMem(pItemValues);
                }
                else
                {
                    Marshal.FreeCoTaskMem(pItemValues);
                    return null;
                }
            }
            else
            {
                int cancelID = 0;
                result = _async.Read(len, arrHSrv, 1, out cancelID, out pErrors);
            }
            Marshal.FreeCoTaskMem(pErrors);
            return values;
        }

        public int BatchWrite(SortedDictionary<ITag, object> items, bool isSync = true)
        {
            IntPtr pErrors;
            int count = items.Count;
            int i = 0;
            int[] arrHSrv = new int[count];
            object[] arrVal = new object[count];
            foreach (var item in items)
            {
                ITag tag = item.Key;
                if (tag != null)
                {
                    //tag.Update(item.Value.ToStorage(), DateTime.Now, QUALITIES.QUALITY_GOOD);
                    arrHSrv[i] = tag.Address.Start;
                    arrVal[i] = item.Value;
                    i++;
                }
            }
            int result = isSync ? _sync.Write(count, arrHSrv, arrVal, out pErrors)
                : _async.Write(count, arrHSrv, arrVal, 1, out i, out pErrors);
            Marshal.FreeCoTaskMem(pErrors);
            return result;
        }

        public ItemData<int> ReadInt32(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            IntPtr pItemValues;
            IntPtr pErrors;
            ItemData<int> rt = new ItemData<int>();
            _sync.Read((OPCDATASOURCE)source, 1, new int[1] { address.Start }, out pItemValues, out pErrors);

            if (Marshal.ReadInt32(pErrors) == 0)
            {
                rt.TimeStamp = Marshal.ReadInt64(pItemValues + 4);
                rt.Quality = (QUALITIES)Marshal.ReadInt16(pItemValues + 12);
                VarEnum vt = (VarEnum)Marshal.ReadInt32(pItemValues + 16);
                if (vt == VarEnum.VT_I4 || vt == VarEnum.VT_UI4)
                {
                    rt.Value = Marshal.ReadInt32(pItemValues + 24);
                }
            }
            Marshal.FreeCoTaskMem(pItemValues);
            Marshal.FreeCoTaskMem(pErrors);
            return rt;
        }

        public ItemData<uint> ReadUInt32(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            var rt = ReadInt32(address, source);
            return new ItemData<uint>((uint)rt.Value, rt.TimeStamp, rt.Quality);
        }

        public ItemData<ushort> ReadUInt16(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            var rt = ReadInt16(address, source);
            return new ItemData<ushort>((ushort)rt.Value, rt.TimeStamp, rt.Quality);
        }

        public ItemData<short> ReadInt16(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            IntPtr pItemValues;
            IntPtr pErrors;
            ItemData<short> rt = new ItemData<short>();
            _sync.Read((OPCDATASOURCE)source, 1, new int[1] { address.Start }, out pItemValues, out pErrors);

            if (Marshal.ReadInt32(pErrors) == 0)
            {
                rt.TimeStamp = Marshal.ReadInt64(pItemValues + 4);
                rt.Quality = (QUALITIES)Marshal.ReadInt16(pItemValues + 12);
                VarEnum vt = (VarEnum)Marshal.ReadInt32(pItemValues + 16);
                if (vt == VarEnum.VT_I2 || vt == VarEnum.VT_UI2)
                    rt.Value = Marshal.ReadInt16(pItemValues + 24);
            }
            Marshal.FreeCoTaskMem(pItemValues);
            Marshal.FreeCoTaskMem(pErrors);
            return rt;
        }

        public ItemData<byte> ReadByte(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            IntPtr pItemValues;
            IntPtr pErrors;
            ItemData<byte> rt = new ItemData<byte>();
            _sync.Read((OPCDATASOURCE)source, 1, new int[1] { address.Start }, out pItemValues, out pErrors);

            if (Marshal.ReadInt32(pErrors) == 0)
            {
                rt.TimeStamp = Marshal.ReadInt64(pItemValues + 4);
                rt.Quality = (QUALITIES)Marshal.ReadInt16(pItemValues + 12);
                VarEnum vt = (VarEnum)Marshal.ReadInt32(pItemValues + 16);
                if (vt == VarEnum.VT_UI1)
                    rt.Value = Marshal.ReadByte(pItemValues + 24);
            }
            Marshal.FreeCoTaskMem(pItemValues);
            Marshal.FreeCoTaskMem(pErrors);
            return rt;
        }

        public ItemData<float> ReadFloat(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            IntPtr pItemValues;
            IntPtr pErrors;
            ItemData<float> rt = new ItemData<float>();
            _sync.Read((OPCDATASOURCE)source, 1, new int[1] { address.Start }, out pItemValues, out pErrors);

            if (Marshal.ReadInt32(pErrors) == 0)
            {
                rt.TimeStamp = Marshal.ReadInt64(pItemValues + 4);
                rt.Quality = (QUALITIES)Marshal.ReadInt16(pItemValues + 12);
                VarEnum vt = (VarEnum)Marshal.ReadInt32(pItemValues + 16);
                if (vt == VarEnum.VT_R4)
                {
                    float[] x = new float[1];
                    Marshal.Copy(pItemValues + 24, x, 0, 1);
                    rt.Value = x[0];
                }
            }
            Marshal.FreeCoTaskMem(pItemValues);
            Marshal.FreeCoTaskMem(pErrors);
            return rt;
        }

        public ItemData<bool> ReadBool(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            IntPtr pItemValues;
            IntPtr pErrors;
            ItemData<bool> rt = new ItemData<bool>();
            _sync.Read((OPCDATASOURCE)source, 1, new int[1] { address.Start }, out pItemValues, out pErrors);

            if (Marshal.ReadInt32(pErrors) == 0)
            {
                rt.TimeStamp = Marshal.ReadInt64(pItemValues + 4);
                rt.Quality = (QUALITIES)Marshal.ReadInt16(pItemValues + 12);
                VarEnum vt = (VarEnum)Marshal.ReadInt32(pItemValues + 16);
                if (vt == VarEnum.VT_BOOL)
                    rt.Value = Marshal.ReadByte(pItemValues + 24) > 0;
            }
            Marshal.FreeCoTaskMem(pItemValues);
            Marshal.FreeCoTaskMem(pErrors);
            return rt;
        }

        public ItemData<string> ReadString(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            IntPtr pItemValues;
            IntPtr pErrors;
            ItemData<string> rt = new ItemData<string>();
            _sync.Read((OPCDATASOURCE)source, 1, new int[1] { address.Start }, out pItemValues, out pErrors);

            if (Marshal.ReadInt32(pErrors) == 0)
            {
                rt.TimeStamp = Marshal.ReadInt64(pItemValues + 4);
                rt.Quality = (QUALITIES)Marshal.ReadInt16(pItemValues + 12);
                VarEnum vt = (VarEnum)Marshal.ReadInt32(pItemValues + 16);
                if (vt == VarEnum.VT_BSTR || vt == VarEnum.VT_LPWSTR)
                    rt.Value = Marshal.PtrToStringUni(Marshal.ReadIntPtr(pItemValues + 24));
            }
            Marshal.FreeCoTaskMem(pItemValues);
            Marshal.FreeCoTaskMem(pErrors);
            return rt;
        }

        public byte[] ReadBytes(DeviceAddress address, DataSource source = DataSource.Cache)
        {
            IntPtr pItemValues;
            IntPtr pErrors;
            byte[] rt = new byte[address.DataSize];
            _sync.Read((OPCDATASOURCE)source, 1, new int[1] { address.Start }, out pItemValues, out pErrors);

            if (Marshal.ReadInt32(pErrors) == 0)
            {
                VarEnum vt = (VarEnum)Marshal.ReadInt32(pItemValues + 16);
                if (vt == VarEnum.VT_BLOB || vt == VarEnum.VT_ARRAY)
                    Marshal.PtrToStructure(Marshal.ReadIntPtr(pItemValues + 24), rt);
            }
            Marshal.FreeCoTaskMem(pItemValues);
            Marshal.FreeCoTaskMem(pErrors);
            return rt;
        }

        public int WriteInt32(DeviceAddress address, int value)
        {
            IntPtr pErrors;
            int rt = _sync.Write(1, new int[1] { address.Start }, new object[] { value }, out pErrors);
            Marshal.FreeCoTaskMem(pErrors);
            return rt;
        }

        public int WriteUInt32(DeviceAddress address, uint value)
        {
            IntPtr pErrors;
            int rt = _sync.Write(1, new int[1] { address.Start }, new object[] { value }, out pErrors);
            Marshal.FreeCoTaskMem(pErrors);
            return rt;
        }

        public int WriteUInt16(DeviceAddress address, ushort value)
        {
            IntPtr pErrors;
            int rt = _sync.Write(1, new int[1] { address.Start }, new object[] { value }, out pErrors);
            Marshal.FreeCoTaskMem(pErrors);
            return rt;
        }

        public int WriteInt16(DeviceAddress address, short value)
        {
            IntPtr pErrors;
            int rt = _sync.Write(1, new int[1] { address.Start }, new object[] { value }, out pErrors);
            Marshal.FreeCoTaskMem(pErrors);
            return rt;
        }

        public int WriteFloat(DeviceAddress address, float value)
        {
            IntPtr pErrors;
            int rt = _sync.Write(1, new int[1] { address.Start }, new object[] { value }, out pErrors);
            Marshal.FreeCoTaskMem(pErrors);
            return rt;
        }

        public int WriteString(DeviceAddress address, string value)
        {
            IntPtr pErrors;
            int rt = _sync.Write(1, new int[1] { address.Start }, new object[] { value }, out pErrors);
            Marshal.FreeCoTaskMem(pErrors);
            return rt;
        }

        public int WriteBit(DeviceAddress address, bool value)
        {
            IntPtr pErrors;
            int rt = _sync.Write(1, new int[1] { address.Start }, new object[] { value }, out pErrors);
            Marshal.FreeCoTaskMem(pErrors);
            return rt;
        }

        public int WriteBits(DeviceAddress address, byte value)
        {
            IntPtr pErrors;
            int rt = _sync.Write(1, new int[1] { address.Start }, new object[] { value }, out pErrors);
            Marshal.FreeCoTaskMem(pErrors);
            return rt;
        }

        public int WriteBytes(DeviceAddress address, byte[] value)
        {
            IntPtr pErrors;
            int rt = _sync.Write(1, new int[1] { address.Start }, new object[] { value }, out pErrors);
            Marshal.FreeCoTaskMem(pErrors);
            return rt;
        }

        public void Dispose()
        {
            int count = _items.Count;
            int[] arrSvr = new int[count];
            for (int i = 0; i < count; i++)
            {
                arrSvr[i] = _items[i].Address.Start;
            }
            _items.Clear();
            IntPtr ptrErr;
            try
            {
                int result = _itemMgt.RemoveItems(count, arrSvr, out ptrErr);
                Marshal.FreeCoTaskMem(ptrErr);
                if (_grpObj != null)
                {
                    Marshal.ReleaseComObject(_grpObj);
                }
            }
            catch (Exception err)
            {

            }
            _grpObj = null;
            _itemMgt = null;
            _grpState = null;
            _async = null;
            _sync = null;
        }

        public OPCGroup(string name, short Id, int updateRate, float deadband, bool active, object grpObj, OPCReader reader)
        {
            _name = name;
            _Id = Id;
            _updateRate = updateRate;
            _deadBand = deadband;
            _isActive = active;
            _grpObj = grpObj;
            _plcReader = reader;
            _server = reader.Parent;
            _itemMgt = (IOPCItemMgt)_grpObj;
            //_grpState = (IOPCGroupStateMgt2)_grpObj;
            _async = (IOPCAsyncIO3)_grpObj;
            _sync = (IOPCSyncIO2)_grpObj;
        }

        public void OnDataChange(int dwTransid, int hGroup, int hrMasterquality, int hrMastererror, int dwCount,
            IntPtr phClientItems, IntPtr pvValues, IntPtr pwQualities, IntPtr pftTimeStamps, IntPtr ppErrors)
        {
            if (DataChange == null)
            {
                for (int i = 0; i < dwCount; i++)
                {
                    ITag item = GetItemByID(Marshal.ReadInt16(phClientItems));
                    if (item == null) continue;
                    if (Marshal.ReadInt32(ppErrors) == 0)
                    {
                        Storage value = Storage.Empty;
                        VarEnum vt = (VarEnum)Marshal.ReadInt32(pvValues);
                        switch (item.Address.VarType)
                        {
                            case DataType.BOOL:
                                value.Boolean = Marshal.ReadByte(pvValues + 8) > 0;
                                break;
                            case DataType.BYTE:
                                value.Byte = Marshal.ReadByte(pvValues + 8);
                                break;
                            case DataType.WORD:
                                value.Word = (ushort)Marshal.ReadInt16(pvValues + 8);
                                break;
                            case DataType.SHORT:
                                value.Int16 = Marshal.ReadInt16(pvValues + 8);
                                break;
                            case DataType.DWORD:
                                value.DWord = (uint)Marshal.ReadInt32(pvValues + 8);
                                break;
                            case DataType.INT:
                                value.Int32 = Marshal.ReadInt32(pvValues + 8);
                                break;
                            case DataType.FLOAT:
                                if (vt == VarEnum.VT_UI2)
                                {
                                    ushort us =(ushort) Marshal.ReadInt16(pvValues + 8);
                                    value.Single = Convert.ToSingle(us); ;
                                }
                                else
                                {
                                    float[] x = new float[1];
                                    Marshal.Copy(pvValues + 8, x, 0, 1);
                                    value.Single = x[0];
                                }
                                break;
                            case DataType.SYS:
                            case DataType.STR:
                                string str = Marshal.PtrToStringUni(Marshal.ReadIntPtr(pvValues + 8));
                                StringTag tag = item as StringTag;
                                if (tag != null)
                                    tag.String = str;
                                break;
                            default:
                                value.Boolean = Marshal.ReadByte(pvValues + 8) > 0;
                                break;
                        }
                        item.Update(value, DateTime.FromFileTime(Marshal.ReadInt64(pftTimeStamps)), (QUALITIES)Marshal.ReadInt16(pwQualities));
                    }
                    ppErrors += 4;
                    phClientItems += 4;
                    pvValues += 16;
                    pwQualities += 2;
                    pftTimeStamps += 8;
                }
            }
            else
                FireDataChange(dwTransid, hGroup, hrMasterquality, hrMastererror, dwCount,
                  phClientItems, pvValues, pwQualities, pftTimeStamps, ppErrors);
            //Marshal.FreeCoTaskMem(phClientItems);
            //Marshal.FreeCoTaskMem(pvValues);
            //Marshal.FreeCoTaskMem(pwQualities);
            //Marshal.FreeCoTaskMem(pftTimeStamps);
            //Marshal.FreeCoTaskMem(ppErrors);
        }

        private void FireDataChange(int dwTransid, int hGroup, int hrMasterquality, int hrMastererror, int dwCount,
            IntPtr phClientItems, IntPtr pvValues, IntPtr pwQualities, IntPtr pftTimeStamps, IntPtr ppErrors)
        {
            HistoryData[] clents = new HistoryData[dwCount];
            for (int i = 0; i < dwCount; i++)
            {
                ITag item = GetItemByID(Marshal.ReadInt16(phClientItems));
                if (item == null) continue;
                if (HRESULTS.Succeeded(Marshal.ReadInt32(ppErrors)))
                {
                    Storage value = Storage.Empty;
                    VarEnum vt = (VarEnum)Marshal.ReadInt32(pvValues);
                    switch (item.Address.VarType)
                    {
                        case DataType.BOOL:
                            value.Boolean = Marshal.ReadByte(pvValues + 8) > 0;
                            break;
                        case DataType.BYTE:
                            value.Byte = Marshal.ReadByte(pvValues + 8);
                            break;
                        case DataType.WORD:
                            value.Word = (ushort)Marshal.ReadInt16(pvValues + 8);
                            break;
                        case DataType.SHORT:
                            value.Int16 = Marshal.ReadInt16(pvValues + 8);
                            break;
                        case DataType.DWORD:
                            value.DWord = (uint)Marshal.ReadInt32(pvValues + 8);
                            break;
                        case DataType.INT:
                            value.Int32 = Marshal.ReadInt32(pvValues + 8);
                            break;
                        case DataType.FLOAT:
                            if (vt == VarEnum.VT_UI2)
                            {
                                ushort us = (ushort)Marshal.ReadInt16(pvValues + 8);
                                value.Single = Convert.ToSingle(us);
                            }
                            else
                            {
                                float[] x = new float[1];
                                Marshal.Copy(pvValues + 8, x, 0, 1);
                                value.Single = x[0];
                            }
                            break;
                        case DataType.SYS:
                        case DataType.STR:
                            string str = Marshal.PtrToStringUni(Marshal.ReadIntPtr(pvValues + 8));
                            StringTag tag = item as StringTag;
                            if (tag != null)
                                tag.String = str;
                            break;
                        default:
                            value.Boolean = Marshal.ReadByte(pvValues + 8) > 0;
                            break;
                    }
                    DateTime time = DateTime.FromFileTime(Marshal.ReadInt64(pftTimeStamps));
                    QUALITIES quality = (QUALITIES)Marshal.ReadInt16(pwQualities);
                    clents[i].ID = item.ID;
                    clents[i].Quality = quality;
                    clents[i].Value = value;
                    clents[i].TimeStamp = time;
                    item.Update(value, time, quality);
                }
                ppErrors += 4;
                phClientItems += 4;
                pvValues += 16;
                pwQualities += 2;
                pftTimeStamps += 8;
            }
            DataChange(this, new DataChangeEventArgs(1, clents));
        }

        public event DataChangeEventHandler DataChange;


        public void OnReadComplete(int dwTransid, int hGroup, int hrMasterquality, int hrMastererror, int dwCount, IntPtr phClientItems, IntPtr pvValues, IntPtr pwQualities, IntPtr pftTimeStamps, IntPtr ppErrors)
        {

        }

        public void OnWriteComplete(int dwTransid, int hGroup, int hrMastererr, int dwCount, IntPtr pClienthandles, IntPtr ppErrors)
        {

        }

        public void OnCancelComplete(int dwTransid, int hGroup)
        {

        }
    }

    [Description("OPC Client")]
    public class OPCReader : IOPCShutdown, IDriver
    {
        private string _clsidOPCserver = "{6E6170F0-FF2D-11D2-8087-00105AA8F840}", _serverIP;
        private object _opcServerObj;
        private IOPCServer _opcServer;
        private IOPCItemProperties _opcProp;
        private IOPCBrowseServerAddressSpace _opcBrowser;
        private IDataServer _dataServer;
        private IConnectionPoint _shutDownPoint;
        private int _shutDownCookie;

        public string TypeID
        {
            get { return _clsidOPCserver; }
            set { _clsidOPCserver = value; }
        }

        public OPCReader(IDataServer dataServer, short id, string name)
        {
            this._id = id;
            this._dataServer = dataServer;
            this._name = name;
        }

        public bool Connect()
        {
            if (_opcServerObj != null)
                Dispose();
            if (string.IsNullOrEmpty(_serverIP)) _serverIP = null;
            Guid cid;
            Type svrComponenttype = Guid.TryParse(_clsidOPCserver, out cid) ? Type.GetTypeFromCLSID(cid, _serverIP, false)
                : Type.GetTypeFromProgID(_clsidOPCserver, _serverIP, false);
            if (svrComponenttype == null)
                return false;
            try
            {
                _opcServerObj = Activator.CreateInstance(svrComponenttype);
            }
            catch (Exception err)
            {
                if (err.Message != null) { }
                return false;
            }
            _opcServer = (IOPCServer)_opcServerObj;
            _opcProp = (IOPCItemProperties)_opcServerObj;
            _opcBrowser = (IOPCBrowseServerAddressSpace)_opcServerObj;
            Guid sinkguid = typeof(IOPCShutdown).GUID;
            ((IConnectionPointContainer)_opcServerObj).FindConnectionPoint(ref sinkguid, out _shutDownPoint);
            if (_shutDownPoint == null)
                return false;
            try
            {
                _shutDownPoint.Advise(this, out _shutDownCookie);
            }
            catch (COMException err)
            {
                _opcServerObj = null;
                _opcServer = null;
                _shutDownPoint = null;
                return false;
            }
            if (_metaGroups.Count > 0 && _groups.Count == 0)
            {
                foreach (var metagrp in _metaGroups)
                {
                    var grp = AddGroup(metagrp.Name, metagrp.ID, metagrp.UpdateRate, metagrp.DeadBand, metagrp.Active);
                    if (grp != null)
                        grp.AddItems(_dataServer.MetaDataList);
                }
            }
            return true;
        }

        #region IOPCServer Members
        public List<string> Browse(OPCBROWSETYPE type = OPCBROWSETYPE.OPC_FLAT, string szFilter = "", short typeFilter = 0, OPCACCESSRIGHTS rightFilter = 0)
        {
            if (_opcBrowser == null)
                return null;
            List<string> list = new List<string>(14000);
            OPC.Data.Interface.IEnumString enumerator;
            _opcBrowser.BrowseOPCItemIDs(type, szFilter, typeFilter, rightFilter, out enumerator);
            if (enumerator == null)
                return list;

            int cft = 0;
            string[] strF = new string[1000];
            do
            {
                enumerator.RemoteNext(1000, strF, out cft);
                for (int i = 0; i < cft; i++)
                    list.Add(strF[i]);
            } while (cft > 0);
            Marshal.ReleaseComObject(enumerator);
            enumerator = null;
            list.TrimExcess();
            return list;
        }


        public object[] GetItemProperties(string itemID, params int[] propertyIDs)
        {
            int count = propertyIDs.Length;
            if (count < 1)
                return null;

            IntPtr ptrDat;
            IntPtr ptrErr;
            if (HRESULTS.Succeeded(_opcProp.GetItemProperties(itemID, count, propertyIDs, out ptrDat, out ptrErr)))
            {
                object[] propertiesData = new object[count];

                for (int i = 0; i < count; i++)
                {
                    if (Marshal.ReadInt32(ptrErr) == 0)
                    {
                        propertiesData[i] = Marshal.GetObjectForNativeVariant(ptrDat);
                    }
                    ptrErr += 4;
                    ptrDat += 16;
                }

                Marshal.FreeCoTaskMem(ptrDat);
                Marshal.FreeCoTaskMem(ptrErr);
                return propertiesData;
            }
            return null;
        }

        #endregion

        #region IOPCShutdown Members
        public event IOErrorEventHandler OnError;

        public void ShutdownRequest(string szReason)
        {
            this.Close();
            if (OnError != null)
                OnError(this, new IOErrorEventArgs(szReason));
        }

        #endregion


        public bool IsClosed
        {
            get { return _opcServerObj == null; }
        }

        short _id;
        public short ID
        {
            get
            {
                return _id;
            }
        }

        public int TimeOut
        {
            get { return 0; }
            set { }
        }

        string _name;
        public string Name
        {
            get
            {
                return _name;
            }
        }

        public string ServerName
        {
            get
            {
                return _serverIP;
            }
            set
            {
                _serverIP = value;
            }
        }

        public IDataServer Parent
        {
            get
            {
                return _dataServer;
            }
        }

        void Close()
        {
            foreach (var grp in _groups.Values)
            {
                (grp.Items as IList<ITag>).Clear();
            }
            _groups.Clear();
            _opcServerObj = null;
            _opcServer = null;
            _shutDownPoint = null;
            _shutDownCookie = 0;
        }

        public void Dispose()
        {
            try
            {
                if (_opcServer != null)
                {
                    foreach (var grp in _groups)
                    {
                        _opcServer.RemoveGroup(grp.Key, true);
                        grp.Value.Dispose();
                    }
                    _groups.Clear();
                }
                if (_shutDownPoint != null)
                {
                    if (_shutDownCookie != 0)
                    {
                        _shutDownPoint.Unadvise(_shutDownCookie);
                    }
                    Marshal.ReleaseComObject(_shutDownPoint);
                }
                if (_opcServerObj != null)
                {
                    Marshal.ReleaseComObject(_opcServerObj);
                }
            }
            catch (Exception e)
            {
                ShutdownRequest(e.Message);
            }
            _opcServerObj = null;
            _opcServer = null;
            _shutDownPoint = null;
            _shutDownCookie = 0;
        }

        Dictionary<int, OPCGroup> _groups = new Dictionary<int, OPCGroup>(30);
        public IEnumerable<IGroup> Groups
        {
            get { return _groups.Values; }
        }

        List<MetaGroup> _metaGroups = new List<MetaGroup>();

        public IGroup AddGroup(string name, short id, int updateRate, float deadBand, bool active)
        {
            if (IsClosed)
                Connect();
            if (!_metaGroups.Exists(x => x.ID == id))
                _metaGroups.Add(new MetaGroup { ID = id, Name = name, UpdateRate = updateRate, DeadBand = deadBand, Active = active }); 
            if (_opcServer == null) return null;
            GCHandle hDeadband, hTimeBias;
            hDeadband = GCHandle.Alloc(deadBand, GCHandleType.Pinned);
            hTimeBias = GCHandle.Alloc(0, GCHandleType.Pinned);
            Guid iidRequiredInterface = typeof(IOPCItemMgt).GUID;
            int serverId, svrUpdateRate; object grpObj;
            if (HRESULTS.Succeeded(_opcServer.AddGroup(name, active, updateRate, id,
                    hTimeBias.AddrOfPinnedObject(), hDeadband.AddrOfPinnedObject(), 0x0,
                    out serverId, out svrUpdateRate, ref  iidRequiredInterface, out grpObj)))
            {
                IConnectionPointContainer pIConnectionPointContainer = (IConnectionPointContainer)grpObj;
                Guid iid = typeof(IOPCDataCallback).GUID;
                IConnectionPoint pIConnectionPoint;
                pIConnectionPointContainer.FindConnectionPoint(ref iid, out pIConnectionPoint);
                int dwCookie;
                OPCGroup grp = new OPCGroup(name, id, svrUpdateRate, deadBand, active, grpObj, this);
                _groups.Add(serverId, grp);
                pIConnectionPoint.Advise(grp, out dwCookie);
                //OPCGroups.Add(serverId, grp);
                return grp;
            }
            else
                return null;
        }

        public bool RemoveGroup(IGroup grp)
        {
            if (_opcServer != null)
            {
                foreach (var group in _groups)
                {
                    if (group.Value.ID == grp.ID)
                    {
                        _opcServer.RemoveGroup(group.Key, true);
                        group.Value.Dispose();
                        return true;
                    }
                }
            }
            return false;
        }
    }

    internal class MetaGroup
    {
        public string Name { get; set; }
        public short ID { get; set; }
        public bool Active { get; set; }
        public int UpdateRate { get; set; }
        public float DeadBand { get; set; }
    }
}

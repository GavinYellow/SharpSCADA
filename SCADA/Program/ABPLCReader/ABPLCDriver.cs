using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using DataService;
using Tuxeip;

namespace ABPLCDriver
{
    [Description("EtherNet IP协议")]
    public unsafe class ABEtherNetReader : IPLCDriver
    {
        Eip_Connection* connection;
        Eip_Session* session;
        byte _rack = 1;
        byte _slot;
        string _ip;

        short _id;
        public short ID
        {
            get
            {
                return _id;
            }
        }

        public bool IsClosed
        {
            get
            {
                return connection == null || session == null;
            }
        }

        public int PDU
        {
            get
            {
                return 492;
            }
        }

        int _timeOut = 1000;
        public int TimeOut
        {
            get
            {
                return _timeOut;
            }
            set
            {
                _timeOut = value;
            }
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
                return _ip;
            }
            set
            {
                _ip = value;
            }
        }


        public byte Rack
        {
            get
            {
                return _rack;
            }
            set
            {
                _rack = value;
            }
        }

        public byte Slot
        {
            get
            {
                return _slot;
            }
            set
            {
                _slot = value;
            }
        }

        public ABEtherNetReader(IDataServer server, short id, string name)
        {
            _id = id;
            _server = server;
            _name = name;
        }

        public bool Connect()
        {
            try
            {
                if (session != null) Dispose();
                session = Tuxeip_Class.OpenSession(_ip);
                byte[] path = new byte[] { _rack, _slot };
                if (session != null)
                {
                    int res = Tuxeip_Class._RegisterSession(session);
                    connection = Tuxeip_Class.ConnectPLCOverCNET(session, Plc_Type.LGX, path);
                }
                return (session != null && connection != null);
            }
            catch (Exception error)
            {
                if (OnError != null)
                {
                    OnError(this, new IOErrorEventArgs(error.Message));
                }
                return false;
            }
        }

        List<IGroup> _groups = new List<IGroup>();
        public IEnumerable<IGroup> Groups
        {
            get { return _groups; }
        }

        IDataServer _server;
        public IDataServer Parent
        {
            get { return _server; }
        }

        public IGroup AddGroup(string name, short id, int updateRate, float deadBand, bool active)
        {
            ABPLCGroup grp = new ABPLCGroup(id, name, updateRate, active, this);
            _groups.Add(grp);
            return grp;
        }

        public bool RemoveGroup(IGroup grp)
        {
            grp.IsActive = false;
            return _groups.Remove(grp);
        }

        public event IOErrorEventHandler OnError;

        public void Dispose()
        {
            if (connection != null)
                Tuxeip_Class._Forward_Close(connection);
            if (session != null)
                Tuxeip_Class._UnRegisterSession(session);
            Tuxeip_Class.CloseSession(session);
            Marshal.FreeCoTaskMem((IntPtr)connection);
            Marshal.FreeCoTaskMem((IntPtr)session);
            connection = null;
            session = null;
        }

        public string GetAddress(DeviceAddress address)
        {
            return string.Concat(address.Area == ABAREA.N ? "N" : "F", address.DBNumber.ToString(), "[", address.Start.ToString(), "]",
                address.VarType == DataType.BOOL ? "." + address.Bit.ToString() : "");//考虑对localData I/O信号的解析
        }

        public DeviceAddress GetDeviceAddress(string address)
        {
            DeviceAddress addr = DeviceAddress.Empty;//考虑对localData I/O信号的解析
            if (!string.IsNullOrEmpty(address))
            {
                addr.Area = address[0] == 'N' ? ABAREA.N : ABAREA.F;
                int index = address.IndexOf('[');
                if (index > 0)
                {
                    ushort db;
                    ushort.TryParse(address.Substring(1, index - 1), out db);
                    addr.DBNumber = db;
                    int ind2 = address.IndexOf(']');
                    if (ind2 > 0)
                    {
                        int start;
                        int.TryParse(address.Substring(index + 1, ind2 - index - 1), out start);
                        addr.Start = start;
                    }
                    int dig = address.IndexOf('.');
                    if (dig > 0)
                    {
                        byte bit;
                        byte.TryParse(address.Substring(dig + 1), out bit);
                        addr.Bit = bit;
                        addr.VarType = DataType.BOOL;
                    }
                }
                else
                {
                    ushort db;
                    ushort.TryParse(address.Substring(1), out db);
                    addr.DBNumber = db;
                }
            }
            return addr;
        }


        public byte[] ReadBytes(DeviceAddress address, ushort size)
        {
            byte[] result = new byte[size];
            float[] buffer = ReadFloatArray(address, (ushort)(size / 4));
            if (buffer != null)
            {
                Buffer.BlockCopy(buffer, 0, result, 0, size);
                return result;
            }
            return null;
        }

        public float[] ReadFloatArray(DeviceAddress address, ushort size)
        {
            if (IsClosed)
                return null;
            else
            {
                float[] buffer = new float[size]; address.VarType = DataType.FLOAT; var addr = GetAddress(address);
                LGX_Read* data = Tuxeip_Class._ReadLgxData(session, connection, addr, size);
                if (data != null && data->Varcount > 0)
                {
                    for (int i = 0; i < data->Varcount; i++)
                    {
                        buffer[i] = Tuxeip_Class._GetLGXValueAsFloat(data, i);
                    }
                    data = null;
                    return buffer;
                }
                connection = null;
                return null;
            }
        }

        public ItemData<int> ReadInt32(DeviceAddress address)
        {
            if (IsClosed)
                return new ItemData<int>(0, 0, QUALITIES.QUALITY_NOT_CONNECTED);
            else
            {
                LGX_Read* data = Tuxeip_Class._ReadLgxData(session, connection, GetAddress(address), 1);
                return new ItemData<int>(Tuxeip_Class._GetLGXValueAsInteger(data, 0), 0, QUALITIES.QUALITY_GOOD);
            }
        }

        public ItemData<short> ReadInt16(DeviceAddress address)
        {
            if (IsClosed)
                return new ItemData<short>(0, 0, QUALITIES.QUALITY_NOT_CONNECTED);
            else
            {
                LGX_Read* data = Tuxeip_Class._ReadLgxData(session, connection, GetAddress(address), 1);
                return new ItemData<short>((short)Tuxeip_Class._GetLGXValueAsInteger(data, 0), 0, QUALITIES.QUALITY_GOOD);
            }
        }

        public ItemData<byte> ReadByte(DeviceAddress address)
        {
            if (IsClosed)
                return new ItemData<byte>(0, 0, QUALITIES.QUALITY_NOT_CONNECTED);
            else
            {
                LGX_Read* data = Tuxeip_Class._ReadLgxData(session, connection, GetAddress(address), 1);
                return new ItemData<byte>((byte)Tuxeip_Class._GetLGXValueAsInteger(data, 0), 0, QUALITIES.QUALITY_GOOD);
            }
        }

        public ItemData<string> ReadString(DeviceAddress address, ushort size)
        {
            if (IsClosed)
                return new ItemData<string>(string.Empty, 0, QUALITIES.QUALITY_NOT_CONNECTED);
            else
            {
                byte[] buffer = ReadBytes(address, size);
                if (buffer != null)
                {
                    return new ItemData<string>(Encoding.ASCII.GetString(buffer).Trim(), 0, QUALITIES.QUALITY_GOOD);
                }
                return new ItemData<string>(string.Empty, 0, QUALITIES.QUALITY_BAD);
            }
        }

        public ItemData<float> ReadFloat(DeviceAddress address)
        {
            if (IsClosed)
                return new ItemData<float>(0, 0, QUALITIES.QUALITY_NOT_CONNECTED);
            else
            {
                LGX_Read* data = Tuxeip_Class._ReadLgxData(session, connection, GetAddress(address), 1);
                return new ItemData<float>(Tuxeip_Class._GetLGXValueAsFloat(data, 0), 0, QUALITIES.QUALITY_GOOD);
            }
        }

        public ItemData<bool> ReadBit(DeviceAddress address)
        {
            if (IsClosed)
                return new ItemData<bool>(false, 0, QUALITIES.QUALITY_NOT_CONNECTED);
            else
            {
                LGX_Read* data = Tuxeip_Class._ReadLgxData(session, connection, GetAddress(address), 1);
                return new ItemData<bool>(Tuxeip_Class._GetLGXValueAsInteger(data, 0) > 0, 0, QUALITIES.QUALITY_GOOD);
            }
        }

        public ItemData<object> ReadValue(DeviceAddress address)
        {
            return this.ReadValueEx(address);
        }

        public unsafe int WriteBytes(DeviceAddress address, byte[] bit)
        {
            if (IsClosed) return -1;
            fixed (void* b = bit)
            {
                return Tuxeip_Class._WriteLgxData(session, connection, GetAddress(address), LGX_Data_Type.LGX_BITARRAY, b, 1);
            }
        }

        public int WriteBit(DeviceAddress address, bool bit)
        {
            if (IsClosed) return -1;
            return Tuxeip_Class._WriteLgxData(session, connection, GetAddress(address), LGX_Data_Type.LGX_BOOL, &bit, 1);
        }

        public int WriteBits(DeviceAddress address, byte bits)
        {
            if (IsClosed) return -1;
            return Tuxeip_Class._WriteLgxData(session, connection, GetAddress(address), LGX_Data_Type.LGX_SINT, &bits, 1);
        }

        public int WriteInt16(DeviceAddress address, short value)
        {
            if (IsClosed) return -1;
            return Tuxeip_Class._WriteLgxData(session, connection, GetAddress(address), LGX_Data_Type.LGX_INT, &value, 1);
        }

        public int WriteInt32(DeviceAddress address, int value)
        {
            if (IsClosed) return -1;
            return Tuxeip_Class._WriteLgxData(session, connection, GetAddress(address), LGX_Data_Type.LGX_DINT, &value, 1);
        }

        public int WriteFloat(DeviceAddress address, float value)
        {
            if (IsClosed) return -1;
            return Tuxeip_Class._WriteLgxData(session, connection, GetAddress(address), LGX_Data_Type.LGX_REAL, &value, 1);
        }

        public int WriteString(DeviceAddress address, string str)
        {
            byte[] b = Encoding.ASCII.GetBytes(str);
            return WriteBytes(address, b);
        }

        public int WriteValue(DeviceAddress address, object value)
        {
            return this.WriteValueEx(address, value);
        }

        public ItemData<uint> ReadUInt32(DeviceAddress address)
        {
            if (IsClosed)
                return new ItemData<uint>(0, 0, QUALITIES.QUALITY_NOT_CONNECTED);
            else
            {
                LGX_Read* data = Tuxeip_Class._ReadLgxData(session, connection, GetAddress(address), 1);
                return new ItemData<uint>((uint)Tuxeip_Class._GetLGXValueAsInteger(data, 0), 0, QUALITIES.QUALITY_GOOD);
            }
        }

        public ItemData<ushort> ReadUInt16(DeviceAddress address)
        {
            if (IsClosed)
                return new ItemData<ushort>(0, 0, QUALITIES.QUALITY_NOT_CONNECTED);
            else
            {
                LGX_Read* data = Tuxeip_Class._ReadLgxData(session, connection, GetAddress(address), 1);
                return new ItemData<ushort>((ushort)Tuxeip_Class._GetLGXValueAsInteger(data, 0), 0, QUALITIES.QUALITY_GOOD);
            }
        }

        public int WriteUInt16(DeviceAddress address, ushort value)
        {
            if (IsClosed) return -1;
            return Tuxeip_Class._WriteLgxData(session, connection, GetAddress(address), LGX_Data_Type.LGX_INT, &value, 1);
        }

        public int WriteUInt32(DeviceAddress address, uint value)
        {
            if (IsClosed) return -1;
            return Tuxeip_Class._WriteLgxData(session, connection, GetAddress(address), LGX_Data_Type.LGX_DINT, &value, 1);
        }

        public int Limit
        {
            get { return 122; }
        }
  
    }

    public sealed class ABPLCGroup : PLCGroup
    {
        public ABPLCGroup(short id, string name, int updateRate, bool active, ABEtherNetReader plcReader)
        {
            this._id = id;
            this._name = name;
            this._updateRate = updateRate;
            this._isActive = active;
            this._plcReader = plcReader;
            this._server = _plcReader.Parent;
            this._changedList = new List<int>();
            this._timer = new Timer();
            this._cacheReader = new FloatCacheReader();
        }

        protected override void Poll()
        {
            float[] cache = (float[])_cacheReader.Cache;
            int offset = 0;
            foreach (PDUArea area in _rangeList)
            {
                float[] prcv = ((ABEtherNetReader)_plcReader).ReadFloatArray(area.Start, (ushort)area.Len);//从PLC读取数据  
                if (prcv == null)
                {
                    return;
                }
                else
                {
                    int len = prcv.Length;
                    int index = area.StartIndex;//index指向_items中的Tag元数据
                    int count = index + area.Count;
                    while (index < count)
                    {
                        DeviceAddress addr = _items[index].Address;
                        int iInt = addr.CacheIndex;
                        int iInt1 = iInt - offset;
                        if (addr.VarType == DataType.BOOL)
                        {
                            int tmp = (int)prcv[iInt1] ^ (int)cache[iInt];
                            DeviceAddress next = addr;
                            if (tmp != 0)
                            {
                                while (addr.Start == next.Start)
                                {
                                    if ((tmp & (1 << next.Bit)) > 0) _changedList.Add(index);
                                    if (++index < count)
                                        next = _items[index].Address;
                                    else
                                        break;
                                }
                            }
                            else
                            {
                                while (addr.Start == next.Start && ++index < count)
                                {
                                    next = _items[index].Address;
                                }
                            }
                        }
                        else
                        {
                            if (addr.DataSize <= 4)
                            {
                                if (prcv[iInt1] != cache[iInt]) _changedList.Add(index);
                            }
                            else
                            {
                                int size = addr.DataSize / 4;
                                for (int i = 0; i < size; i++)
                                {
                                    if (prcv[iInt1 + i] != cache[iInt + i])
                                    {
                                        _changedList.Add(index);
                                        break;
                                    }
                                }
                            }
                            index++;
                        }
                    }
                    for (int j = 0; j < len; j++)
                    {
                        cache[j + offset] = prcv[j];
                    }//将PLC读取的数据写入到CacheReader中
                    offset += len;
                }
            }
        }
    }

    public class ABAREA
    {
        public const int N = 0;
        public const int F = 1;
        public const int I = 2;
        public const int O = 3;
    }
}

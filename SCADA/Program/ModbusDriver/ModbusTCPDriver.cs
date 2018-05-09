using DataService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ModbusDriver
{
    [Description("Modbus TCP协议")]
    public sealed class ModbusTCPReader : IPLCDriver, IMultiReadWrite                    //IPLCDriver : IDriver, IReaderWriter       IDriver : IDisposable
    {
        #region
        public int PDU
        {
            // get { return 252; }
            //get { return 256; }
            /* 更新人：yjz
              更新日期：20171125
              更新原因： 在 modbus——TCP中协议规定如下：
                       ADU=MBAP+功能码+数据    其中 ADU 256字节，MBAP 7字节，功能码1字节，数据为248字节 
                       PDU=功能码+数据        
                       所以PDU应为： 249字节
             */
            get { return 249; } //0xF9 十进制为249
        }

        public DeviceAddress GetDeviceAddress(string address)
        {
            DeviceAddress dv = DeviceAddress.Empty;
            if (string.IsNullOrEmpty(address))
                return dv;
            var sindex = address.IndexOf(':');
            if (sindex > 0)
            {
                int slaveId;
                if (int.TryParse(address.Substring(0, sindex), out slaveId))
                    dv.Area = slaveId;
                address = address.Substring(sindex + 1);
            }
            switch (address[0])
            {
                case '0':
                    {
                        dv.DBNumber = Modbus.fctReadCoil;
                        int st;
                        int.TryParse(address, out st);
                        //dv.Start = (st / 16) * 16;//???????????????????
                        dv.Bit = (byte)(st % 16);
                        st /= 16;
                        dv.Start = st;
                        dv.Bit--;
                    }
                    break;
                case '1':
                    {
                        dv.DBNumber = Modbus.fctReadDiscreteInputs;
                        int st;
                        int.TryParse(address.Substring(1), out st);
                        //dv.Start = (st / 16) * 16;//???????????????????
                        dv.Bit = (byte)(st % 16);
                        st /= 16;
                        dv.Start = st;
                        dv.Bit--;
                    }
                    break;
                case '4':
                    {
                        int index = address.IndexOf('.');
                        dv.DBNumber = Modbus.fctReadHoldingRegister;
                        if (index > 0)
                        {
                            dv.Start = int.Parse(address.Substring(1, index - 1));
                            dv.Bit = byte.Parse(address.Substring(index + 1));
                        }
                        else
                            dv.Start = int.Parse(address.Substring(1));
                        dv.Start--;
                        dv.Bit--;
                        dv.ByteOrder = ByteOrder.Network;
                    }
                    break;
                case '3':
                    {
                        int index = address.IndexOf('.');
                        dv.DBNumber = Modbus.fctReadInputRegister;
                        if (index > 0)
                        {
                            dv.Start = int.Parse(address.Substring(1, index - 1));
                            dv.Bit = byte.Parse(address.Substring(index + 1));
                        }
                        else
                            dv.Start = int.Parse(address.Substring(1));
                        dv.Start--;
                        dv.Bit--;
                        dv.ByteOrder = ByteOrder.Network;
                    }
                    break;
            }
            return dv;
        }

        public string GetAddress(DeviceAddress address)
        {
            return string.Empty;
        }

        #endregion
        private int _timeout = 1000;

        private Socket tcpSynCl;
        private byte[] tcpSynClBuffer = new byte[0xFF];

        short _id;
        public short ID
        {
            get
            {
                return _id;
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

        string _ip;
        public string ServerName
        {
            get { return _ip; }
            set { _ip = value; }
        }

        int _port = 502;
        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        public bool IsClosed
        {
            get
            {
                return tcpSynCl == null || tcpSynCl.Connected == false;
            }
        }

        public int TimeOut
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        List<IGroup> _grps = new List<IGroup>(20);
        public IEnumerable<IGroup> Groups
        {
            get { return _grps; }
        }

        IDataServer _server;
        public IDataServer Parent
        {
            get { return _server; }
        }

        public ModbusTCPReader(IDataServer server, short id, string name)
        {
            _id = id;
            _name = name;
            _server = server;
        }

        public bool Connect()
        {
            //int port = 502;
            try
            {
                if (tcpSynCl != null)
                    tcpSynCl.Close();
                //IPAddress ip = IPAddress.Parse(_ip);
                // ----------------------------------------------------------------
                // Connect synchronous client
                if (_timeout <= 0) _timeout = 1000;
                tcpSynCl = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                tcpSynCl.SendTimeout = _timeout;
                tcpSynCl.ReceiveTimeout = _timeout;
                tcpSynCl.NoDelay = true;
                tcpSynCl.Connect(_ip, _port);
                return true;
            }
            catch (SocketException error)
            {
                if (OnError != null)
                    OnError(this, new IOErrorEventArgs(error.Message));
                return false;
            }
        }

        private byte[] CreateReadHeader(int id, int startAddress, ushort length, byte function)
        {
            byte[] data = new byte[12];
            data[0] = 0;				// Slave id high byte
            data[1] = 0;				// Slave id low byte
            data[5] = 6;					// Message size
            data[6] = (byte)id;					// Slave address
            data[7] = function;				// Function code
            byte[] _adr = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)startAddress));
            data[8] = _adr[0];				// Start address
            data[9] = _adr[1];				// Start address
            byte[] _length = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)length));
            data[10] = _length[0];			// Number of data to read
            data[11] = _length[1];			// Number of data to read
            return data;
        }

        private byte[] CreateWriteHeader(int id, int startAddress, ushort numData, ushort numBytes, byte function)
        {
            byte[] data = new byte[numBytes + 11];
            data[0] = 0;				// Slave id high byte
            data[1] = 0;				// Slave id low byte+
            byte[] _size = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)(5 + numBytes)));
            data[4] = _size[0];				// Complete message size in bytes
            data[5] = _size[1];				// Complete message size in bytes
            data[6] = (byte)id;					// Slave address
            data[7] = function;				// Function code
            byte[] _adr = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)startAddress));
            data[8] = _adr[0];				// Start address
            data[9] = _adr[1];				// Start address
            if (function >= Modbus.fctWriteMultipleCoils)
            {
                byte[] _cnt = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)numData));
                data[10] = _cnt[0];			// Number of bytes
                data[11] = _cnt[1];			// Number of bytes
                data[12] = (byte)(numBytes - 2);
            }
            return data;
        }

        object _async = new object();
        private byte[] WriteSyncData(byte[] write_data)
        {
            short id = BitConverter.ToInt16(write_data, 0);
            if (IsClosed) CallException(id, write_data[7], Modbus.excExceptionConnectionLost);
            else
            {
                lock (_async)
                {
                    try
                    {
                        tcpSynCl.Send(write_data, 0, write_data.Length, SocketFlags.None);//是否存在lock的问题？
                        int result = tcpSynCl.Receive(tcpSynClBuffer, 0, 0xFF, SocketFlags.None);

                        byte function = tcpSynClBuffer[7];
                        byte[] data;

                        if (result == 0) CallException(id, write_data[7], Modbus.excExceptionConnectionLost);

                        // ------------------------------------------------------------
                        // Response data is slave ModbusModbus.exception
                        if (function > Modbus.excExceptionOffset)
                        {
                            function -= Modbus.excExceptionOffset;
                            CallException(id, function, tcpSynClBuffer[8]);
                            return null;
                        }
                        // ------------------------------------------------------------
                        // Write response data
                        else if ((function >= Modbus.fctWriteSingleCoil) && (function != Modbus.fctReadWriteMultipleRegister))
                        {
                            data = new byte[2];
                            Array.Copy(tcpSynClBuffer, 10, data, 0, 2);
                        }
                        // ------------------------------------------------------------
                        // Read response data
                        else
                        {
                            data = new byte[tcpSynClBuffer[8]];
                            Array.Copy(tcpSynClBuffer, 9, data, 0, tcpSynClBuffer[8]);
                        }
                        return data;
                    }
                    catch (SocketException)
                    {
                        CallException(id, write_data[7], Modbus.excExceptionConnectionLost);
                    }
                }
            }
            return null;
        }

        public byte[] WriteSingleCoils(int id, int startAddress, bool OnOff)
        {
            byte[] data;
            data = CreateWriteHeader(id, startAddress, 1, 1, Modbus.fctWriteSingleCoil);
            if (OnOff == true) data[10] = 255;
            else data[10] = 0;
            return WriteSyncData(data);
        }

        public byte[] WriteMultipleCoils(int id, int startAddress, ushort numBits, byte[] values)
        {
            byte numBytes = Convert.ToByte(values.Length);
            byte[] data;
            data = CreateWriteHeader(id, startAddress, numBits, (byte)(numBytes + 2), Modbus.fctWriteMultipleCoils);
            Array.Copy(values, 0, data, 13, numBytes);
            return WriteSyncData(data);
        }

        public byte[] WriteSingleRegister(int id, int startAddress, byte[] values)
        {
            byte[] data;
            data = CreateWriteHeader(id, startAddress, 1, 1, Modbus.fctWriteSingleRegister);
            data[10] = values[0];
            data[11] = values[1];
            return WriteSyncData(data);
        }

        public byte[] WriteMultipleRegister(int id, int startAddress, byte[] values)
        {
            ushort numBytes = Convert.ToUInt16(values.Length);
            if (numBytes % 2 > 0) numBytes++;
            byte[] data;

            data = CreateWriteHeader(id, startAddress, Convert.ToUInt16(numBytes / 2), Convert.ToUInt16(numBytes + 2), Modbus.fctWriteMultipleRegister);
            Array.Copy(values, 0, data, 13, values.Length);
            return WriteSyncData(data);
        }

        public IGroup AddGroup(string name, short id, int updateRate, float deadBand = 0f, bool active = false)
        {
            NetShortGroup grp = new NetShortGroup(id, name, updateRate, active, this);
            _grps.Add(grp);
            return grp;
        }

        public bool RemoveGroup(IGroup grp)
        {
            grp.IsActive = false;
            return _grps.Remove(grp);
        }

        public void Dispose()
        {
            if (tcpSynCl != null)
            {
                if (tcpSynCl.Connected)
                {
                    try { tcpSynCl.Shutdown(SocketShutdown.Both); }
                    catch { }
                    tcpSynCl.Close();
                }
                tcpSynCl = null;
            }
            foreach (IGroup grp in _grps)
            {
                grp.Dispose();
            }
            _grps.Clear();
        }



        internal void CallException(int id, byte function, byte exception)
        {
            if (tcpSynCl == null) return;
            if (OnError != null)
                OnError(this, new IOErrorEventArgs(Modbus.GetErrorString(exception)));
        }

        public byte[] ReadBytes(DeviceAddress address, ushort size)
        {
            int area = address.DBNumber;
            return area < 3 ? WriteSyncData(CreateReadHeader(address.Area, address.Start * 16, (ushort)(16 * size), (byte)area))
                : WriteSyncData(CreateReadHeader(address.Area, address.Start, size, (byte)area));
        }

        public ItemData<int> ReadInt32(DeviceAddress address)
        {
            byte[] data = WriteSyncData(CreateReadHeader(address.Area, address.Start, 2, (byte)address.DBNumber));
            if (data == null)
                return new ItemData<int>(0, 0, QUALITIES.QUALITY_BAD);
            else
                return new ItemData<int>(IPAddress.HostToNetworkOrder(BitConverter.ToInt32(data, 0)), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<uint> ReadUInt32(DeviceAddress address)
        {
            byte[] data = WriteSyncData(CreateReadHeader(address.Area, address.Start, 2, (byte)address.DBNumber));
            if (data == null)
                return new ItemData<uint>(0, 0, QUALITIES.QUALITY_BAD);
            else
                return new ItemData<uint>((uint)IPAddress.HostToNetworkOrder(BitConverter.ToInt32(data, 0)), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<ushort> ReadUInt16(DeviceAddress address)
        {
            byte[] data = WriteSyncData(CreateReadHeader(address.Area, address.Start, 1, (byte)address.DBNumber));
            if (data == null)
                return new ItemData<ushort>(0, 0, QUALITIES.QUALITY_BAD);
            else
                return new ItemData<ushort>((ushort)IPAddress.HostToNetworkOrder(BitConverter.ToInt16(data, 0)), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<short> ReadInt16(DeviceAddress address)
        {
            byte[] data = WriteSyncData(CreateReadHeader(address.Area, address.Start, 1, (byte)address.DBNumber));
            if (data == null)
                return new ItemData<short>(0, 0, QUALITIES.QUALITY_BAD);
            else
                return new ItemData<short>(IPAddress.HostToNetworkOrder(BitConverter.ToInt16(data, 0)), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<byte> ReadByte(DeviceAddress address)
        {
            byte[] data = WriteSyncData(CreateReadHeader(address.Area, address.Start, 1, (byte)address.DBNumber));
            if (data == null)
                return new ItemData<byte>(0, 0, QUALITIES.QUALITY_BAD);
            else
                return new ItemData<byte>(data[0], 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<string> ReadString(DeviceAddress address, ushort size)
        {
            byte[] data = WriteSyncData(CreateReadHeader(address.Area, address.Start, size, (byte)address.DBNumber));
            if (data == null)
                return new ItemData<string>(string.Empty, 0, QUALITIES.QUALITY_BAD);
            else
                return new ItemData<string>(Encoding.ASCII.GetString(data, 0, data.Length), 0, QUALITIES.QUALITY_GOOD);//是否考虑字节序问题？
        }

        public unsafe ItemData<float> ReadFloat(DeviceAddress address)
        {
            byte[] data = WriteSyncData(CreateReadHeader(address.Area, address.Start, 2, (byte)address.DBNumber));
            if (data == null)
                return new ItemData<float>(0.0f, 0, QUALITIES.QUALITY_BAD);
            else
            {
                return new ItemData<float>(IPAddress.HostToNetworkOrder(BitConverter.ToInt32(data, 0)), 0, QUALITIES.QUALITY_GOOD);
            }
        }

        public ItemData<bool> ReadBit(DeviceAddress address)
        {
            byte[] data = address.DBNumber > 2 ? WriteSyncData(CreateReadHeader(address.Area, address.Start, 1, (byte)address.DBNumber)) :
                   WriteSyncData(CreateReadHeader(address.Area, address.Start * 16 + address.Bit, 1, (byte)address.DBNumber));
            if (data == null)
                return new ItemData<bool>(false, 0, QUALITIES.QUALITY_BAD);
            if (data.Length == 1) return new ItemData<bool>(data[0] > 0, 0, QUALITIES.QUALITY_GOOD);
            unsafe
            {
                fixed (byte* p = data)
                {
                    short* p1 = (short*)p;
                    return new ItemData<bool>((*p1 & (1 << address.Bit.BitSwap()))
                        != 0, 0, QUALITIES.QUALITY_GOOD);
                }
            }
        }

        public ItemData<object> ReadValue(DeviceAddress address)
        {
            return this.ReadValueEx(address);
        }

        public int WriteBytes(DeviceAddress address, byte[] bit)
        {
            var data = address.DBNumber > 2 ? WriteMultipleRegister(address.Area, address.Start, bit)
                : WriteMultipleCoils(address.Area, address.Start, (ushort)(8 * bit.Length), bit);//应考虑到
            return data == null ? -1 : 0;
        }

        public int WriteBit(DeviceAddress address, bool bit)
        {
            if (address.DBNumber < 3)
            {
                var data = WriteSingleCoils(address.Area, address.Start + address.Bit, bit);
                return data == null ? -1 : 0;
            }
            return -1;
        }

        public int WriteBits(DeviceAddress address, byte bits)
        {
            if (address.DBNumber != 3) return -1;
            var data = WriteSingleRegister(address.Area, address.Start, new byte[] { bits });
            return data == null ? -1 : 0;
        }

        public int WriteInt16(DeviceAddress address, short value)
        {
            if (address.DBNumber != 3) return -1;
            var data = WriteSingleRegister(address.Area, address.Start, BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value)));
            return data == null ? -1 : 0;
        }

        public int WriteUInt16(DeviceAddress address, ushort value)
        {
            if (address.DBNumber != 3) return -1;
            var data = WriteSingleRegister(address.Area, address.Start, BitConverter.GetBytes((ushort)IPAddress.HostToNetworkOrder((short)value)));
            return data == null ? -1 : 0;
        }

        public int WriteUInt32(DeviceAddress address, uint value)
        {
            if (address.DBNumber != 3) return -1;
            var data = WriteMultipleRegister(address.Area, address.Start, BitConverter.GetBytes((uint)IPAddress.HostToNetworkOrder((int)value)));
            return data == null ? -1 : 0;
        }

        public int WriteInt32(DeviceAddress address, int value)
        {
            if (address.DBNumber != 3) return -1;
            var data = WriteMultipleRegister(address.Area, address.Start, BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value)));
            return data == null ? -1 : 0;
        }

        public int WriteFloat(DeviceAddress address, float value)
        {
            if (address.DBNumber != 3) return -1;
            var data = WriteMultipleRegister(address.Area, address.Start, BitConverter.GetBytes((int)value));
            return data == null ? -1 : 0;
        }

        public int WriteString(DeviceAddress address, string str)
        {
            if (address.DBNumber != 3) return -1;
            var data = WriteMultipleRegister(address.Area, address.Start, Encoding.ASCII.GetBytes(str));
            return data == null ? -1 : 0;
        }

        public int WriteValue(DeviceAddress address, object value)
        {
            return this.WriteValueEx(address, value);
        }

        public event IOErrorEventHandler OnError;

        public int Limit
        {
            get { return 60; }
        }

        public ItemData<Storage>[] ReadMultiple(DeviceAddress[] addrsArr)
        {
            return this.PLCReadMultiple(new NetShortCacheReader(), addrsArr);
        }

        public int WriteMultiple(DeviceAddress[] addrArr, object[] buffer)
        {
            return this.PLCWriteMultiple(new NetShortCacheReader(), addrArr, buffer, Limit);
        }
    }
}

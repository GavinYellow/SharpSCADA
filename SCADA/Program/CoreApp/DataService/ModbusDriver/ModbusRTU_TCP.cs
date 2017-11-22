using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Sockets;
using System.Text;
using DataService;

namespace ModbusDriver
{
    [Description("Modbus RTU_TCP协议")]
    public sealed class ModbusRTU_TCPReader : IPLCDriver, IMultiReadWrite
    {
        private int _timeout;

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

        int _slave = 1;
        string _ip;
        public string ServerName
        {
            get { return _ip; }
            set { _ip = value; }
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

        public ModbusRTU_TCPReader(IDataServer server, short id, string name, string ip, int timeOut = 500, string spare1 = "1", string spare2 = null)
        {
            _id = id;//id 
            _name = name;
            _server = server;
            _ip = ip;
            _timeout = timeOut;
            if (!string.IsNullOrEmpty(spare1))
                _slave = int.Parse(spare1);
        }

        public bool Connect()
        {
            int port = 7000;
            try
            {
                if (tcpSynCl != null)
                    tcpSynCl.Close();
                //IPAddress ip = IPAddress.Parse(_ip);
                // ----------------------------------------------------------------
                // Connect synchronous client
                tcpSynCl = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                tcpSynCl.SendTimeout = _timeout;
                tcpSynCl.ReceiveTimeout = _timeout;
                tcpSynCl.NoDelay = true;
                tcpSynCl.Connect(_ip, port);
                return true;
            }
            catch (SocketException error)
            {
                if (OnClose != null)
                    OnClose(this, new ShutdownRequestEventArgs(error.Message));
                return false;
            }
        }

        private byte[] CreateReadHeader(int startAddress, ushort length, byte function)
        {
            byte[] data = new byte[8];
            data[0] = (byte)_slave;				// Slave id high byte
            data[1] = function;					// Message size
            byte[] _adr = BitConverter.GetBytes((short)startAddress);
            data[2] = _adr[0];				// Start address
            data[3] = _adr[1];				// Start address
            byte[] _length = BitConverter.GetBytes((short)length);
            data[4] = _length[0];			// Number of data to read
            data[5] = _length[1];			// Number of data to read
            byte[] arr = Utility.CalculateCrc(data, 6);
            data[6] = arr[0];
            data[7] = arr[1];
            return data;
        }

        public byte[] WriteSingleCoils(int startAddress, bool OnOff)
        {
            byte[] data = new byte[8];
            data[0] = (byte)_slave;				// Slave id high byte
            data[1] = Modbus.fctWriteSingleCoil;				// Function code
            byte[] _adr = BitConverter.GetBytes((short)startAddress);
            data[2] = _adr[0];				// Start address
            data[3] = _adr[1];				// Start address
            if (OnOff) data[4] = 0xFF;
            byte[] arr = Utility.CalculateCrc(data, 6);
            data[6] = arr[0];
            data[7] = arr[1];
            return data;
        }

        public byte[] WriteMultipleCoils(int startAddress, ushort numBits, byte[] values)
        {
            int len = values.Length;
            byte[] data = new byte[len + 9];
            data[0] = (byte)_slave;				// Slave id high byte
            data[1] = Modbus.fctWriteMultipleCoils;				// Function code
            byte[] _adr = BitConverter.GetBytes((short)startAddress);
            data[2] = _adr[0];				// Start address
            data[3] = _adr[1];				// Start address
            byte[] _length = BitConverter.GetBytes((short)numBits);
            data[4] = _length[0];			// Number of data to read
            data[5] = _length[1];			// Number of data to read
            data[6] = (byte)len;
            Array.Copy(values, 0, data, 7, len);
            byte[] arr = Utility.CalculateCrc(data, len + 7);
            data[len + 7] = arr[0];
            data[len + 8] = arr[1];
            return data;
        }

        public byte[] WriteSingleRegister(int startAddress, byte[] values)
        {
            byte[] data = new byte[8];
            data[0] = (byte)_slave;				// Slave id high byte
            data[1] = Modbus.fctWriteSingleRegister;				// Function code
            byte[] _adr = BitConverter.GetBytes((short)startAddress);
            data[2] = _adr[0];				// Start address
            data[3] = _adr[1];				// Start address
            data[4] = values[0];
            data[5] = values[1];
            byte[] arr = Utility.CalculateCrc(data, 6);
            data[6] = arr[0];
            data[7] = arr[1];
            return data;
        }

        public byte[] WriteMultipleRegister(int startAddress, byte[] values)
        {
            int len = values.Length;
            if (len % 2 > 0) len++;
            byte[] data = new byte[len + 9];
            data[0] = (byte)_slave;				// Slave id high byte
            data[1] = Modbus.fctWriteMultipleRegister;				// Function code
            byte[] _adr = BitConverter.GetBytes((short)startAddress);
            data[2] = _adr[0];				// Start address
            data[3] = _adr[1];				// Start address
            byte[] _length = BitConverter.GetBytes((short)(len >> 1));
            data[4] = _length[0];			// Number of data to read
            data[5] = _length[1];			// Number of data to read
            data[6] = (byte)len;
            Array.Copy(values, 0, data, 7, len);
            byte[] arr = Utility.CalculateCrc(data, len + 7);
            data[len + 7] = arr[0];
            data[len + 8] = arr[1];
            return data;
        }

        public int PDU
        {
            get { return 252; }
            //get { return 256; }
        }

        public DeviceAddress GetDeviceAddress(string address)
        {
            DeviceAddress dv = DeviceAddress.Empty;
            if (string.IsNullOrEmpty(address))
                return dv;
            switch (address[0])
            {
                case '0':
                    {
                        dv.Area = Modbus.fctReadCoil;
                        int st;
                        int.TryParse(address, out st);
                        //dv.Start = (st / 16) * 16;//???????????????????
                        dv.Bit = (byte)(st % 16);
                        st /= 16;
                        dv.Start = st;
                    }
                    break;
                case '1':
                    {
                        dv.Area = Modbus.fctReadDiscreteInputs;
                        int st;
                        int.TryParse(address.Substring(1), out st);
                        //dv.Start = (st / 16) * 16;//???????????????????
                        dv.Bit = (byte)(st % 16);
                        st /= 16;
                        dv.Start = st;
                    }
                    break;
                case '4':
                    {
                        int index = address.IndexOf('.');
                        dv.Area = Modbus.fctReadHoldingRegister;
                        if (index > 0)
                        {
                            dv.Start = int.Parse(address.Substring(1, index - 1));
                            dv.Bit = byte.Parse(address.Substring(index + 1));
                        }
                        else
                            dv.Start = int.Parse(address.Substring(1));
                        dv.Start--;
                    }
                    break;
                case '3':
                    {
                        int index = address.IndexOf('.');
                        dv.Area = Modbus.fctReadInputRegister;
                        if (index > 0)
                        {
                            dv.Start = int.Parse(address.Substring(1, index - 1));
                            dv.Bit = byte.Parse(address.Substring(index + 1));
                        }
                        else
                            dv.Start = int.Parse(address.Substring(1));
                        dv.Start--;
                    }
                    break;
            }
            return dv;
        }

        public string GetAddress(DeviceAddress address)
        {
            return string.Empty;
        }


        public IGroup AddGroup(string name, short id, int updateRate, float deadBand = 0f, bool active = false)
        {
            ModbusTcpGroup grp = new ModbusTcpGroup(id, name, updateRate, active, this);
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

        public byte[] ReadBytes(DeviceAddress address, ushort size)
        {
            int area = address.Area;
            try
            {
                if (!tcpSynCl.Connected) return null;
                byte[] header = area == Modbus.fctReadCoil ? CreateReadHeader(address.Start * 16, (ushort)(16 * size), (byte)area) :
                 CreateReadHeader(address.Start, size, (byte)area);
                tcpSynCl.Send(header, 0, header.Length, SocketFlags.None);//是否存在lock的问题？
                byte[] frameBytes = new byte[size * 2 + 3];
                int result = tcpSynCl.Receive(frameBytes, 0, frameBytes.Length, SocketFlags.None);
                byte[] data = new byte[size * 2];
                if (frameBytes[0] == (byte)_slave)
                {
                    Array.Copy(frameBytes, 3, data, 0, data.Length);
                    return data;
                }
                else return new byte[0];
            }
            catch (Exception e)
            {
                if (OnClose != null)
                    OnClose(this, new ShutdownRequestEventArgs(e.Message));
                return null;
            }
        }

        public ItemData<int> ReadInt32(DeviceAddress address)
        {
            byte[] bit = ReadBytes(address, 2);
            return bit == null ? new ItemData<int>(0, 0, QUALITIES.QUALITY_BAD) :
                new ItemData<int>(BitConverter.ToInt32(bit, 0), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<short> ReadInt16(DeviceAddress address)
        {
            byte[] bit = ReadBytes(address, 1);
            return bit == null ? new ItemData<short>(0, 0, QUALITIES.QUALITY_BAD) :
                new ItemData<short>(BitConverter.ToInt16(bit, 0), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<byte> ReadByte(DeviceAddress address)
        {
            byte[] bit = ReadBytes(address, 1);
            return bit == null ? new ItemData<byte>(0, 0, QUALITIES.QUALITY_BAD) :
                 new ItemData<byte>(bit[0], 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<string> ReadString(DeviceAddress address, ushort size)
        {
            byte[] bit = ReadBytes(address, size);
            return bit == null ? new ItemData<string>(string.Empty, 0, QUALITIES.QUALITY_BAD) :
                new ItemData<string>(Encoding.ASCII.GetString(bit), 0, QUALITIES.QUALITY_GOOD);
        }

        public unsafe ItemData<float> ReadFloat(DeviceAddress address)
        {
            byte[] bit = ReadBytes(address, 2);
            return bit == null ? new ItemData<float>(0f, 0, QUALITIES.QUALITY_BAD) :
                new ItemData<float>(BitConverter.ToSingle(bit, 0), 0, QUALITIES.QUALITY_GOOD);
            //int value = BitConverter.ToInt32(bit, 0);
            //return new ItemData<float>(*(((float*)&value)), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<bool> ReadBit(DeviceAddress address)
        {
            byte[] bit = ReadBytes(address, 1);
            return bit == null ? new ItemData<bool>(false, 0, QUALITIES.QUALITY_BAD) :
                 new ItemData<bool>((bit[0] & (1 << (address.Bit))) > 0, 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<object> ReadValue(DeviceAddress address)
        {
            return this.ReadValueEx(address);
        }

        public int WriteBytes(DeviceAddress address, byte[] bit)
        {
            if (!tcpSynCl.Connected) return -1;
            var data = WriteMultipleRegister(address.Start, bit);
            tcpSynCl.Send(data, 0, data.Length, SocketFlags.None);//是否存在lock的问题？
            int result = tcpSynCl.Receive(tcpSynClBuffer, 0, 0xFF, SocketFlags.None);
            return (tcpSynClBuffer[1] & 0x80) > 0 ? -1 : 0;
        }

        public int WriteBit(DeviceAddress address, bool bit)
        {
            if (!tcpSynCl.Connected) return -1;
            var data = WriteSingleCoils(address.Start + address.Bit, bit);
            tcpSynCl.Send(data, 0, data.Length, SocketFlags.None);//是否存在lock的问题？
            int result = tcpSynCl.Receive(tcpSynClBuffer, 0, 0xFF, SocketFlags.None);
            return (tcpSynClBuffer[1] & 0x80) > 0 ? -1 : 0;
        }

        public int WriteBits(DeviceAddress address, byte bits)
        {
            if (!tcpSynCl.Connected) return -1;
            var data = WriteSingleRegister(address.Start, new byte[] { bits });
            tcpSynCl.Send(data, 0, data.Length, SocketFlags.None);//是否存在lock的问题？
            int result = tcpSynCl.Receive(tcpSynClBuffer, 0, 0xFF, SocketFlags.None);
            return (tcpSynClBuffer[1] & 0x80) > 0 ? -1 : 0;
        }

        public int WriteInt16(DeviceAddress address, short value)
        {
            if (!tcpSynCl.Connected) return -1;
            var data = WriteSingleRegister(address.Start, BitConverter.GetBytes(value));
            tcpSynCl.Send(data, 0, data.Length, SocketFlags.None);//是否存在lock的问题？
            int result = tcpSynCl.Receive(tcpSynClBuffer, 0, 0xFF, SocketFlags.None);
            return (tcpSynClBuffer[1] & 0x80) > 0 ? -1 : 0;
        }

        public int WriteInt32(DeviceAddress address, int value)
        {
            if (!tcpSynCl.Connected) return -1;
            var data = WriteMultipleRegister(address.Start, BitConverter.GetBytes(value));
            tcpSynCl.Send(data, 0, data.Length, SocketFlags.None);//是否存在lock的问题？
            int result = tcpSynCl.Receive(tcpSynClBuffer, 0, 0xFF, SocketFlags.None);
            return (tcpSynClBuffer[1] & 0x80) > 0 ? -1 : 0;
        }

        public int WriteFloat(DeviceAddress address, float value)
        {
            if (!tcpSynCl.Connected) return -1;
            var data = WriteMultipleRegister(address.Start, BitConverter.GetBytes(value));
            tcpSynCl.Send(data, 0, data.Length, SocketFlags.None);//是否存在lock的问题？
            int result = tcpSynCl.Receive(tcpSynClBuffer, 0, 0xFF, SocketFlags.None);
            return (tcpSynClBuffer[1] & 0x80) > 0 ? -1 : 0;
        }

        public int WriteString(DeviceAddress address, string str)
        {
            if (!tcpSynCl.Connected) return -1;
            var data = WriteMultipleRegister(address.Start, Encoding.ASCII.GetBytes(str));
            tcpSynCl.Send(data, 0, data.Length, SocketFlags.None);//是否存在lock的问题？
            int result = tcpSynCl.Receive(tcpSynClBuffer, 0, 0xFF, SocketFlags.None);
            return (tcpSynClBuffer[1] & 0x80) > 0 ? -1 : 0;
        }

        public int WriteValue(DeviceAddress address, object value)
        {
            return this.WriteValueEx(address, value);
        }

        public event ShutdownRequestEventHandler OnClose;

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


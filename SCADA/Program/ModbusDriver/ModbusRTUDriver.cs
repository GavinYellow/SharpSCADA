using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Timers;
using DataService;

namespace ModbusDriver
{
    [Description("Modbus RTU协议")]
    //ModbusRTUReader : IPLCDriver         IPLCDriver : IDriver, IReaderWriter              IDriver : IDisposable
    public sealed class ModbusRTUReader : IPLCDriver
    {
        //自定义构造函数
        public ModbusRTUReader(IDataServer server, short id, string name, string remote = null, int timeOut = 10000, string port = "COM1", string baudRate = "9600")
        {
            _id = id;
            _name = name;
            _server = server;
            _port = port;
            _serialPort = new SerialPort(port);
            _timeOut = timeOut;
            _serialPort.ReadTimeout = _timeOut;
            _serialPort.WriteTimeout = _timeOut;
            _serialPort.BaudRate = int.Parse(baudRate);
            _serialPort.DataBits = 8;
            _serialPort.Parity = Parity.Even;
            _serialPort.StopBits = StopBits.One;
        }
        private SerialPort _serialPort;


        /*
         Sbyte:代表有符号的8位整数，数值范围从-128 ～ 127
　      　Byte:代表无符号的8位整数，数值范围从0～255
　     　Short:代表有符号的16位整数，范围从-32768 ～ 32767
　     　ushort:代表有符号的16位整数，范围从0 到 65,535
        Int:代表有符号的32位整数，范围从-2147483648 ～ 2147483648
　　     uint:代表无符号的32位整数，范围从0 ～ 4294967295
　     　Long:代表有符号的64位整数，范围从-9223372036854775808 ～ 9223372036854775808
　     　Ulong:代表无符号的64位整数，范围从0 ～ 18446744073709551615。
        */
        private byte[] CreateReadHeader(int startAddress, ushort length, byte function)
        {
            byte[] data = new byte[8];
            data[0] = (byte)_id;				// Slave id high byte   从站地址
            data[1] = function;					// Message size
            byte[] _adr = BitConverter.GetBytes((short)startAddress);//以字节数组的形式返回指定的 16 位无符号整数值。
            data[2] = _adr[0];				// Start address high byte  起始地址的高八位
            data[3] = _adr[1];				// Start address low byte  起始地址的低八位
            byte[] _length = BitConverter.GetBytes((short)length);
            data[4] = _length[0];			// Number of data to read  high byte  寄存器数量的高八位
            data[5] = _length[1];			// Number of data to read  low byte   寄存器数量的低八位
            byte[] arr = Utility.CalculateCrc(data, 6);
            data[6] = arr[0];              //CRC校验的低八位
            data[7] = arr[1];              //CRC校验的高八位
            return data;
        }
        #region 写单个线圈或单个离散输出   功能码：0x05
        public byte[] WriteSingleCoils(int startAddress, bool OnOff)
        {
            byte[] data = new byte[8];
            data[0] = (byte)_id;				// Slave id high byte
            data[1] = Modbus.fctWriteSingleCoil;				// Function code
            byte[] _adr = BitConverter.GetBytes((short)startAddress);
            data[2] = _adr[0];				// Start address
            data[3] = _adr[1];				// Start address
            if (OnOff)
            {
                data[4] = 0xFF;
                data[5] = 0x00;
            }
            else
            {
                data[4] = 0x00;
                data[5] = 0x00;
            }
            byte[] arr = Utility.CalculateCrc(data, 6);
            data[6] = arr[0];
            data[7] = arr[1];
            return data;
        }
        #endregion

        #region 写多个线圈  功能码：0x0F   15
        public byte[] WriteMultipleCoils(int startAddress, ushort numBits, byte[] values)
        {
            int len = values.Length;
            byte[] data = new byte[len + 9];
            data[0] = (byte)_id;				// Slave id high byte  从站地址高八位
            data[1] = Modbus.fctWriteMultipleCoils;				// Function code  功能码
            byte[] _adr = BitConverter.GetBytes((short)startAddress);
            data[2] = _adr[0];				// Start address       开始地址高八位
            data[3] = _adr[1];				// Start address       开始地址低八位
            byte[] _length = BitConverter.GetBytes((short)numBits);
            data[4] = _length[0];			// Number of data to read  寄存器数量高八位
            data[5] = _length[1];           // Number of data to read  寄存器数量低八位


            data[6] = (byte)len;            //字节数量
            Array.Copy(values, 0, data, 7, len);  //在data中加入变更数据
            byte[] arr = Utility.CalculateCrc(data, len + 7);
            data[len + 7] = arr[0];  //CRC校验的低八位
            data[len + 8] = arr[1];  //CRC校验的高八位
            return data;
        }
        #endregion

        #region 写单个保持寄存器 功能码:0x06
        public byte[] WriteSingleRegister(int startAddress, byte[] values)
        {
            byte[] data = new byte[8];
            data[0] = (byte)_id;				// Slave id high byte 从站地址高八位
            data[1] = Modbus.fctWriteSingleRegister;				// Function code 功能码
            byte[] _adr = BitConverter.GetBytes((short)startAddress);
            data[2] = _adr[0];				// Start address    开始地址高八位
            data[3] = _adr[1];				// Start address    开始地址高八位
            data[4] = values[0];            //变更数据的高位
            data[5] = values[1];            //变更数据的低位
            byte[] arr = Utility.CalculateCrc(data, 6);
            data[6] = arr[0];               //CRC校验码低八位
            data[7] = arr[1];               //CRC校验码高八位
            return data;
        }
        #endregion

        #region 写多个保持寄存器 功能码：0x10    16
        public byte[] WriteMultipleRegister(int startAddress, byte[] values)
        {
            int len = values.Length;
            if (len % 2 > 0) len++;
            byte[] data = new byte[len + 9];
            data[0] = (byte)_id;				// Slave id high byte  从站地址
            data[1] = Modbus.fctWriteMultipleRegister;				// Function code  功能码
            byte[] _adr = BitConverter.GetBytes((short)startAddress);
            data[2] = _adr[0];				// Start address        开始地址高八位
            data[3] = _adr[1];				// Start address        开始地址低八位
            byte[] _length = BitConverter.GetBytes((short)(len >> 1));
            data[4] = _length[0];			// Number of data to read 寄存器数量高八位
            data[5] = _length[1];			// Number of data to read 寄存器数量低八位
            data[6] = (byte)len;            //字节数
            Array.Copy(values, 0, data, 7, len); //把变更数据加入data中
            byte[] arr = Utility.CalculateCrc(data, len + 7);
            data[len + 7] = arr[0];          //crc校验的低八位
            data[len + 8] = arr[1];          //CRC校验的高八位
            return data;
        }
        #endregion



        #region  :IPLCDriver
        public int PDU
        {
            get { return 0x100; }
        }

        public DeviceAddress GetDeviceAddress(string address)//PLC地址一般为5位 如40001，也有可能为40001.1      首位代表地址类型
        {
            DeviceAddress dv = DeviceAddress.Empty;
            if (string.IsNullOrEmpty(address))
                return dv;
            switch (address[0])
            {
                case '0':
                    {
                        dv.Area = Modbus.fctReadCoil;//功能码：01 读线圈或离散量输出状态    00001 - 09999：数字量输出（ 线圈）
                        int st;
                        int.TryParse(address, out st);
                        dv.Bit = (byte)(st % 16);
                        st /= 16;
                        dv.Start = st;
                    }
                    break;
                case '1':
                    {
                        dv.Area = Modbus.fctReadDiscreteInputs;//功能码：02 读离散量输入   10001 - 19999：数字量输入（触点） 
                        int st;
                        int.TryParse(address.Substring(1), out st);
                        dv.Bit = (byte)(st % 16);
                        st /= 16;
                        dv.Start = st;
                    }
                    break;
               
                case '4':
                    {
                        int index = address.IndexOf('.');
                        dv.Area = Modbus.fctReadHoldingRegister;//功能码：03 读取保持寄存器   40001 - 49999：数据保持寄存器
                        if (index > 0)
                        {
                            dv.Start = int.Parse(address.Substring(1, index - 1));
                            dv.Bit = byte.Parse(address.Substring(index + 1));
                        }
                        else
                            dv.Start = int.Parse(address.Substring(1));
                        dv.Start--;                //PLC的寄存器地址比modbus协议的通讯地址大1   如：40002 对应寻 址地址 0x0001
                    }
                    break;
                case '3':
                    {
                        int index = address.IndexOf('.');
                        dv.Area = Modbus.fctReadInputRegister;//功能码：04读输入寄存器   30001 - 39999：输入数据寄存器（通常为模拟量输入）
                        if (index > 0)
                        {
                            dv.Start = int.Parse(address.Substring(1, index - 1));
                            dv.Bit = byte.Parse(address.Substring(index + 1));
                        }
                        else
                            dv.Start = int.Parse(address.Substring(1));
                        dv.Start--;            //PLC的寄存器地址比modbus协议的通讯地址大1   如：40002 对应寻 址地址 0x0001
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

        #region :IDriver
        //从站地址
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

        string _port;
        public string ServerName
        {
            get { return _port; }
            set { _port = value; }
        }
        public bool IsClosed
        {
            get
            {
                return _serialPort.IsOpen == false;
            }
        }

        private int _timeOut;
        public int TimeOut
        {
            get { return _timeOut; }
            set { _timeOut = value; }
        }

        List<IGroup> _grps = new List<IGroup>();
        public IEnumerable<IGroup> Groups
        {
            get { return _grps; }
        }

        IDataServer _server;
        public IDataServer Parent
        {
            get { return _server; }
        }
        public bool Connect()
        {
            try
            {
                _serialPort.Open();
                return true;
            }
            catch (IOException error)
            {
                if (OnClose != null)
                {
                    OnClose(this, new ShutdownRequestEventArgs(error.Message));
                }
                return false;
            }
        }

        public IGroup AddGroup(string name, short id, int updateRate, float deadBand = 0f, bool active = false)
        {
            ModbusRtuGroup grp = new ModbusRtuGroup(id, name, updateRate, active, this);
            _grps.Add(grp);
            return grp;
        }

        public bool RemoveGroup(IGroup grp)
        {
            grp.IsActive = false;
            return _grps.Remove(grp);
        }
        public event ShutdownRequestEventHandler OnClose;
        #endregion

        #region : IDisposable

        public void Dispose()
        {
            foreach (IGroup grp in _grps)
            {
                grp.Dispose();
            }
            _grps.Clear();
            _serialPort.Close();
        }
        #endregion

        #region :IReaderWriter 
        public byte[] ReadBytes(DeviceAddress address, ushort size)
        {
            int area = address.Area;
            try
            {
                byte[] header = area == Modbus.fctReadCoil ? CreateReadHeader(address.Start * 16, (ushort)(16 * size), (byte)area) :
                 CreateReadHeader(address.Start, size, (byte)area);
                _serialPort.Write(header, 0, header.Length);
                byte[] frameBytes = new byte[size * 2 + 5];
                byte[] data = new byte[size * 2];
                int numBytesRead = 0;
                while (numBytesRead != size)
                    numBytesRead += _serialPort.Read(frameBytes, numBytesRead, size - numBytesRead);
                if (frameBytes[0] == (byte)_id && Utility.CheckSumCRC(frameBytes))
                {
                    Array.Copy(frameBytes, 3, data, 0, size);
                    return data;
                }
                return null;
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

        public ItemData<float> ReadFloat(DeviceAddress address)
        {
            byte[] bit = ReadBytes(address, 2);
            return bit == null ? new ItemData<float>(0f, 0, QUALITIES.QUALITY_BAD) :
                new ItemData<float>(BitConverter.ToSingle(bit, 0), 0, QUALITIES.QUALITY_GOOD);
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
            var data = WriteMultipleRegister(address.Start, bit);
            _serialPort.Write(data, 0, data.Length);
            _serialPort.ReadByte();
            var chr = _serialPort.ReadByte();
            return (chr & 0x80) > 0 ? -1 : 0;
        }

        public int WriteBit(DeviceAddress address, bool bit)
        {
            var data = WriteSingleCoils(address.Start + address.Bit, bit);
            _serialPort.Write(data, 0, data.Length);
            _serialPort.ReadByte();
            var chr = _serialPort.ReadByte();
            return (chr & 0x80) > 0 ? -1 : 0;
        }

        public int WriteBits(DeviceAddress address, byte bits)
        {
            var data = WriteSingleRegister(address.Start, new byte[] { bits });
            _serialPort.Write(data, 0, data.Length);
            _serialPort.ReadByte();
            var chr = _serialPort.ReadByte();
            return (chr & 0x80) > 0 ? -1 : 0;
        }

        public int WriteInt16(DeviceAddress address, short value)
        {
            var data = WriteSingleRegister(address.Start, BitConverter.GetBytes(value));
            _serialPort.Write(data, 0, data.Length);
            var chr = _serialPort.ReadByte();
            return (chr & 0x80) > 0 ? -1 : 0;
        }

        public int WriteInt32(DeviceAddress address, int value)
        {
            var data = WriteMultipleRegister(address.Start, BitConverter.GetBytes(value));
            _serialPort.Write(data, 0, data.Length);
            _serialPort.ReadByte();
            var chr = _serialPort.ReadByte();
            return (chr & 0x80) > 0 ? -1 : 0;
        }

        public int WriteFloat(DeviceAddress address, float value)
        {
            var data = WriteMultipleRegister(address.Start, BitConverter.GetBytes(value));
            _serialPort.Write(data, 0, data.Length);
            _serialPort.ReadByte();
            var chr = _serialPort.ReadByte();
            return (chr & 0x80) > 0 ? -1 : 0;
        }

        public int WriteString(DeviceAddress address, string str)
        {
            var data = WriteMultipleRegister(address.Start, Encoding.ASCII.GetBytes(str));
            _serialPort.Write(data, 0, data.Length);
            _serialPort.ReadByte();
            var chr = _serialPort.ReadByte();
            return chr == (byte)_id ? -1 : 0;
        }

        public int WriteValue(DeviceAddress address, object value)
        {
            return this.WriteValueEx(address, value);
        }

        #endregion





    }

    public sealed class ModbusRtuGroup : PLCGroup
    {
        public ModbusRtuGroup(short id, string name, int updateRate, bool active, ModbusRTUReader plcReader)
        {
            this._id = id;
            this._name = name;
            this._updateRate = updateRate;
            this._isActive = active;
            this._plcReader = plcReader;
            this._server = _plcReader.Parent;
            this._timer = new Timer();
            this._changedList = new List<int>();
            this._cacheReader = new ShortCacheReader();
        }

        protected override unsafe int Poll()
        {
            short[] cache = (short[])_cacheReader.Cache;
            int k = 0;
            foreach (PDUArea area in _rangeList)
            {
                byte[] rcvBytes = _plcReader.ReadBytes(area.Start, (ushort)area.Len);//从PLC读取数据  
                if (rcvBytes == null)
                {
                    _plcReader.Connect();
                    return -1;
                }
                else
                {
                    int len = rcvBytes.Length / 2;
                    fixed (byte* p1 = rcvBytes)
                    {
                        short* prcv = (short*)p1;
                        int index = area.StartIndex;//index指向_items中的Tag元数据
                        int count = index + area.Count;
                        while (index < count)
                        {
                            DeviceAddress addr = _items[index].Address;
                            int iShort = addr.CacheIndex;
                            int iShort1 = iShort - k;
                            if (addr.VarType == DataType.BOOL)
                            {
                                int tmp = prcv[iShort1] ^ cache[iShort];
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
                                if (addr.DataSize <= 2)
                                {
                                    if (prcv[iShort1] != cache[iShort]) _changedList.Add(index);
                                }
                                else
                                {
                                    int size = addr.DataSize / 2;
                                    for (int i = 0; i < size; i++)
                                    {
                                        if (prcv[iShort1 + i] != cache[iShort + i])
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
                            cache[j + k] = prcv[j];
                        }//将PLC读取的数据写入到CacheReader中
                    }
                    k += len;
                }
            }
            return 1;
        }

    }


    public sealed class Modbus
    {
        public const byte fctReadCoil = 1;
        public const byte fctReadDiscreteInputs = 2;
        public const byte fctReadHoldingRegister = 3;
        public const byte fctReadInputRegister = 4;
        public const byte fctWriteSingleCoil = 5;
        public const byte fctWriteSingleRegister = 6;
        public const byte fctWriteMultipleCoils = 15;
        public const byte fctWriteMultipleRegister = 16;
        public const byte fctReadWriteMultipleRegister = 23;

        /// <summary>Constant for exception illegal function.</summary>
        public const byte excIllegalFunction = 1;
        /// <summary>Constant for exception illegal data address.</summary>
        public const byte excIllegalDataAdr = 2;
        /// <summary>Constant for exception illegal data value.</summary>
        public const byte excIllegalDataVal = 3;
        /// <summary>Constant for exception slave device failure.</summary>
        public const byte excSlaveDeviceFailure = 4;
        /// <summary>Constant for exception acknowledge.</summary>
        public const byte excAck = 5;
        /// <summary>Constant for exception slave is busy/booting up.</summary>
        public const byte excSlaveIsBusy = 6;
        /// <summary>Constant for exception gate path unavailable.</summary>
        public const byte excGatePathUnavailable = 10;
        /// <summary>Constant for exception not connected.</summary>
        public const byte excExceptionNotConnected = 253;
        /// <summary>Constant for exception connection lost.</summary>
        public const byte excExceptionConnectionLost = 254;
        /// <summary>Constant for exception response timeout.</summary>
        public const byte excExceptionTimeout = 255;
        /// <summary>Constant for exception wrong offset.</summary>
        public const byte excExceptionOffset = 128;
        /// <summary>Constant for exception send failt.</summary>
        public const byte excSendFailt = 100;
    }

}

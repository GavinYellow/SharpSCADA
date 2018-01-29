using DataService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace OmronPlcDriver
{
    [Description("Omron(CS/CJ) UDP协议")]
    public sealed class OmronCsCjUDPReader : IPLCDriver, IMultiReadWrite                    //IPLCDriver : IDriver, IReaderWriter       IDriver : IDisposable
    {


        #region

        /****************************************/
        //更新人： zjf
        //更新内容：新增_pdu,可以根据实际情况进行采集
        //更新日期：20171205
        //更新原因：根据现场进行参数调整以提高采集响应速度或者减小网络压力
        /***************************************/
        /// <summary>
        /// PDU的值,包大小上限
        /// </summary>
        int _pdu;
        /// <summary>
        /// 获取PDU的值
        /// </summary>
        public int PDU
        {
            get { return _pdu; }
            set { _pdu = value; }
        }
        /// <summary>
        /// 获取设备地址
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public DeviceAddress GetDeviceAddress(string address)
        {
            DeviceAddress dv = DeviceAddress.Empty;
            if (string.IsNullOrEmpty(address))
                return dv;
            dv.Area = _plcNodeId;
            switch (address[0])
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    {
                        int index = address.IndexOf('.');
                        dv.DBNumber = OmronCSCJ.fctCIO;
                        if (index > 0)
                        {
                            dv.Start = int.Parse(address.Substring(0, index - 1));
                            dv.Bit = byte.Parse(address.Substring(index + 1));
                        }
                        else
                            dv.Start = int.Parse(address);
                    }
                    break;
                case 'D'://DM区
                    {
                        int index = address.IndexOf('.');
                        dv.DBNumber = OmronCSCJ.fctDM;
                        if (index > 0)
                        {
                            dv.Start = int.Parse(address.Substring(1, index - 1));
                            dv.Bit = byte.Parse(address.Substring(index + 1));
                        }
                        else
                            dv.Start = int.Parse(address.Substring(1));
                    }
                    break;
                case 'H'://HR区
                    {
                        int index = address.IndexOf('.');
                        dv.DBNumber = OmronCSCJ.fctHR;
                        if (index > 0)
                        {
                            dv.Start = int.Parse(address.Substring(1, index - 1));
                            dv.Bit = byte.Parse(address.Substring(index + 1));
                        }
                        else
                            dv.Start = int.Parse(address.Substring(1));
                    }
                    break;
                case 'A'://AR区
                    {
                        int index = address.IndexOf('.');
                        dv.DBNumber = OmronCSCJ.fctA;
                        if (index > 0)
                        {
                            dv.Start = int.Parse(address.Substring(1, index - 1));
                            dv.Bit = byte.Parse(address.Substring(index + 1));
                        }
                        else
                            dv.Start = int.Parse(address.Substring(1));
                    }
                    break;
            }
            dv.ByteOrder = ByteOrder.Network;
            return dv;
        }

        public string GetAddress(DeviceAddress address)
        {
            return string.Empty;
        }

        #endregion
        private int _timeout;//超时数据

        private Socket udpSynCl;
        //接受字符串
        private byte[] udpSynClBuffer = new byte[1024];

        short _id;//驱动id
        //驱动id
        public short ID
        {
            get
            {
                return _id;
            }
        }

        string _name;//驱动名称
        /// <summary>
        /// 驱动名称
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        string _ip;//服务ip
        int _port = 9600; //服务端口
        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        public string ServerName
        {
            get { return _ip; }
            set { _ip = value; }
        }
        /// <summary>
        /// 是否关闭
        /// </summary>
        public bool IsClosed
        {
            get
            {
                return udpSynCl == null || udpSynCl.Connected == false;
            }
        }
        /// <summary>
        /// 超时时间
        /// </summary>

        public int TimeOut
        {
            get { return _timeout; }
            set { _timeout = value; }
        }

        byte _plcNodeId;//plc节点号
        /// <summary>
        /// plc节点号，从para1参数读取
        /// </summary>
        public byte PlcNodeId
        {
            get { return _pcNodeId; }
            set { _pcNodeId = value; }
        }

        byte _pcNodeId;//电脑节点号
        /// <summary>
        /// 电脑节点号，从para2参数读取
        /// </summary>
        public byte PcNodeId
        {
            get { return _pcNodeId; }
            set { _pcNodeId = value; }
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

        public OmronCsCjUDPReader(IDataServer server, short id, string name)
        {
            _id = id;
            _name = name;
            _server = server;
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            try
            {
                Console.WriteLine("开始连接");

                if (udpSynCl != null)
                    udpSynCl.Close();
                //IPAddress ip = IPAddress.Parse(_ip);
                // ----------------------------------------------------------------
                // Connect synchronous client
                udpSynCl = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                udpSynCl.SendTimeout = _timeout;
                udpSynCl.ReceiveTimeout = _timeout;
                udpSynCl.Connect(_ip, _port);
                return true;
            }
            catch (SocketException error)
            {
                if (OnError != null)
                    OnError(this, new IOErrorEventArgs(error.Message));
                return false;
            }
        }
        /// <summary>
        /// 生成读命令头
        /// </summary>        
        /// <param name="pcnode">电脑节点号，设置和PLC节点不一致即可</param>
        /// <param name="startAddress">读取的起始地址</param>
        /// <param name="length">读取长度,2个字节为一个单位</param>
        /// <param name="function"></param>
        /// <param name="plcnode">PLC节点号，可为0</param>
        /// <returns></returns>
        private byte[] CreateReadHeader(byte pcnode, int startAddress, ushort length, byte function, byte plcnode = 0)
        {
            //80 00 00 14 00 00 00 FD 00 00 01 01 00 00 00 00 00 01 
            //80 00 02 00 41 00 00 0B 00 00 01 01 82 00 64 00 00 14
            byte[] data = new byte[18];
            data[0] = 0x80;
            data[1] = 0;
            data[2] = 0;            //80 00 02       固定帧头
            data[2] = 0;
            data[3] = plcnode;
            data[5] = 0;		    //设备的网络号，节点号，单元号			
            data[6] = 0;
            data[7] = pcnode;
            data[8] = 0;            //PC的网络号，节点号，单元号
            data[9] = 0;
            data[10] = 1;
            data[11] = 1;           //SID+MRC+SRC
            data[12] = function;    //数据区代码
            byte[] _adr = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)startAddress));
            data[13] = _adr[0];				// 首地址高字节
            data[14] = _adr[1];				// 首地址低字节
            data[15] = 0;                   // 固定0
            byte[] _length = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)length));
            data[16] = _length[0];			// 读取数量高字节
            data[17] = _length[1];			// 读取数量低字节
            return data;
        }
        /// <summary>
        /// 生成写命令头
        /// </summary>
        /// <param name="id"></param>
        /// <param name="startAddress"></param>
        /// <param name="numData"></param>
        /// <param name="length"></param>
        /// <param name="function"></param>
        /// <returns></returns>
        private byte[] CreateWriteHeader(byte pcnode, int startAddress, ushort numData, byte function, byte plcnode = 0)
        {
            byte[] data = new byte[numData + 18];
            data[0] = 0x80;
            data[1] = 0;
            data[2] = 0;            //80 00 02       固定帧头
            data[3] = 0;
            data[4] = plcnode;
            data[5] = 0;		    //设备的网络号，节点号，单元号			
            data[6] = 0;
            data[7] = pcnode;
            data[8] = 0;            //PC的网络号，节点号，单元号
            data[9] = 0;
            data[10] = 1;
            data[11] = 2;           //SID+MRC+SRC
            data[12] = function;    //数据区代码
            byte[] _adr = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)startAddress));
            data[13] = _adr[0];				// 首地址高字节
            data[14] = _adr[1];				// 首地址低字节
            data[15] = 0;                   // 固定0
            byte[] length = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)numData / 2));
            data[16] = length[0];			// 写取数量高字节
            data[17] = length[1];			// 写取数量低字节
            return data;
        }
        /// <summary>
        /// 同步写数据到udp
        /// </summary>
        /// <param name="write_data">写的字节数组</param>
        /// <returns></returns>
        private byte[] WriteSyncData(byte[] write_data)
        {
            short id = BitConverter.ToInt16(write_data, 4);
            if (IsClosed) CallException(id, write_data[12], OmronCSCJ.excExceptionConnectionLost);
            else
            {
                try
                {
                    udpSynCl.Send(write_data, 0, write_data.Length, SocketFlags.None);//是否存在lock的问题？
                    int result = udpSynCl.Receive(udpSynClBuffer, 0, 1024, SocketFlags.None);

                    byte function = udpSynClBuffer[11];//读写功能
                    byte[] data;

                    int err = udpSynClBuffer[12] * 256 + udpSynClBuffer[13];

                    if (result == 0) CallException(id, write_data[12], OmronCSCJ.excExceptionConnectionLost);

                    // ------------------------------------------------------------
                    // Response data is slave ModbusModbus.exception
                    if (err != 0)
                    {
                        CallException(id, function, 4);
                        return null;
                    }
                    // ------------------------------------------------------------
                    // Write response data
                    else if (function == 0x2)
                    {
                        data = new byte[2];
                        Array.Copy(udpSynClBuffer, 10, data, 0, 2);
                    }
                    // ------------------------------------------------------------
                    // Read response data
                    else if (function == 0x1)
                    {
                        data = new byte[(write_data[16] * 256 + write_data[17]) * 2];
                        Array.Copy(udpSynClBuffer, 14, data, 0, data.Length);
                    }
                    else
                    {
                        return null;
                    }
                    return data;
                }
                catch (SocketException)
                {
                    CallException(id, write_data[12], OmronCSCJ.excExceptionConnectionLost);
                }
            }
            return null;
        }

        /// <summary>
        /// 写单个寄存器
        /// </summary>
        /// <param name="pcnode">电脑节点号</param>
        /// <param name="startAddress">起始地址</param>
        /// <param name="function">内存区功能吗</param>
        /// <param name="values">写值数组</param>
        /// <param name="plcnode">plc节点号</param>
        /// <returns></returns>
        public byte[] WriteSingleRegister(byte pcnode, int startAddress, byte function, byte[] values, byte plcnode = 0)
        {
            byte[] data;
            data = CreateWriteHeader(pcnode, startAddress, 2, function, plcnode);
            data[19] = values[0];
            data[20] = values[1];
            return WriteSyncData(data);
        }
        /// <summary>
        /// 写多个寄存器
        /// </summary>
        /// <param name="pcnode">电脑节点号</param>
        /// <param name="startAddress">起始地址</param>
        /// <param name="function">内存区功能号</param>
        /// <param name="values">写值数组</param>
        /// <param name="plcnode">plc节点号</param>
        /// <returns></returns>
        public byte[] WriteMultipleRegister(byte pcnode, int startAddress, byte function, byte[] values, byte plcnode = 0)
        {
            ushort numBytes = Convert.ToUInt16(values.Length);
            if (numBytes % 2 > 0) numBytes++;
            byte[] data;

            data = CreateWriteHeader(pcnode, startAddress, numBytes, function, plcnode);
            Array.Copy(values, 0, data, 19, values.Length);
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
            if (udpSynCl != null)
            {
                try { udpSynCl.Shutdown(SocketShutdown.Both); }
                catch { }
                udpSynCl.Close();

                udpSynCl = null;
            }
            foreach (IGroup grp in _grps)
            {
                grp.Dispose();
            }
            _grps.Clear();
        }

        internal string GetErrorString(byte exception)
        {
            switch (exception)
            {
                case OmronCSCJ.excIllegalFunction:
                    return "Constant for OmronCSCJ.exception illegal function.";
                case OmronCSCJ.excIllegalDataAdr:
                    return "Constant for OmronCSCJ.exception illegal data address.";
                case OmronCSCJ.excIllegalDataVal:
                    return "Constant for OmronCSCJ.exception illegal data value.";
                case OmronCSCJ.excSlaveDeviceFailure:
                    return "Constant for OmronCSCJ.exception slave device failure.";
                case OmronCSCJ.excAck:
                    return "Constant for OmronCSCJ.exception acknowledge.";
                case OmronCSCJ.excSlaveIsBusy:
                    return "Constant for OmronCSCJ.exception slave is busy/booting up.";
                case OmronCSCJ.excGatePathUnavailable:
                    return "Constant for OmronCSCJ.exception gate path unavailable.";
                case OmronCSCJ.excExceptionNotConnected:
                    return "Constant for OmronCSCJ.exception not connected.";
                case OmronCSCJ.excExceptionConnectionLost:
                    return "Constant for OmronCSCJ.exception connection lost.";
                case OmronCSCJ.excExceptionTimeout:
                    return "Constant for OmronCSCJ.exception response timeout.";
                case OmronCSCJ.excExceptionOffset:
                    return "Constant for OmronCSCJ.exception wrong offset.";
                case OmronCSCJ.excSendFailt:
                    return "Constant for OmronCSCJ.exception send failt.";
            }
            return string.Empty;
        }

        internal void CallException(int id, byte function, byte exception)
        {
            if (udpSynCl == null) return;
            //Console.WriteLine("OmronReader错误->" + GetErrorString(exception));
            if (exception == OmronCSCJ.excExceptionConnectionLost && IsClosed == false)
            {
                if (OnError != null)
                    OnError(this, new IOErrorEventArgs(GetErrorString(exception)));
            }

        }

        /// <summary>
        /// 读取字节数组
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <param name="size">长度,</param>
        /// <returns></returns>
        public byte[] ReadBytes(DeviceAddress address, ushort size)
        {
            ushort len = size;
            if (len % 2 != 0)
            {
                len++;
            }
            return WriteSyncData(CreateReadHeader(PcNodeId, address.Start, (ushort)(len), (byte)address.DBNumber, (byte)address.Area));
        }
        /// <summary>
        /// 读取32位整数
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <returns></returns>
        public ItemData<int> ReadInt32(DeviceAddress address)
        {
            byte[] data = WriteSyncData(CreateReadHeader(PcNodeId, address.Start, 2, (byte)address.DBNumber, (byte)address.Area));
            if (data == null)
                return new ItemData<int>(0, 0, QUALITIES.QUALITY_BAD);
            else
                return new ItemData<int>(IPAddress.HostToNetworkOrder(BitConverter.ToInt32(data, 0)), 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<uint> ReadUInt32(DeviceAddress address)
        {
            byte[] data = WriteSyncData(CreateReadHeader(PcNodeId, address.Start, 2, (byte)address.DBNumber, (byte)address.Area));
            if (data == null)
                return new ItemData<uint>(0, 0, QUALITIES.QUALITY_BAD);
            else
                return new ItemData<uint>((uint)IPAddress.HostToNetworkOrder(BitConverter.ToInt32(data, 0)), 0, QUALITIES.QUALITY_GOOD);
        }
     
        public ItemData<ushort> ReadUInt16(DeviceAddress address)
        {
            byte[] data = WriteSyncData(CreateReadHeader(PcNodeId, address.Start, 1, (byte)address.DBNumber, (byte)address.Area));
            if (data == null)
                return new ItemData<ushort>(0, 0, QUALITIES.QUALITY_BAD);
            else
                return new ItemData<ushort>((ushort)IPAddress.HostToNetworkOrder(BitConverter.ToInt16(data, 0)), 0, QUALITIES.QUALITY_GOOD);
        }
        /// <summary>
        /// 读取16位整数
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <returns></returns>
        public ItemData<short> ReadInt16(DeviceAddress address)
        {
            byte[] data = WriteSyncData(CreateReadHeader(PcNodeId, address.Start, 1, (byte)address.DBNumber, (byte)address.Area));
            if (data == null)
                return new ItemData<short>(0, 0, QUALITIES.QUALITY_BAD);
            else
                return new ItemData<short>(IPAddress.HostToNetworkOrder(BitConverter.ToInt16(data, 0)), 0, QUALITIES.QUALITY_GOOD);
        }
        /// <summary>
        /// 读取1字节
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <returns></returns>
        public ItemData<byte> ReadByte(DeviceAddress address)
        {
            byte[] data = WriteSyncData(CreateReadHeader(PcNodeId, address.Start, 1, (byte)address.DBNumber, (byte)address.Area));
            if (data == null)
                return new ItemData<byte>(0, 0, QUALITIES.QUALITY_BAD);
            else
                return new ItemData<byte>(data[0], 0, QUALITIES.QUALITY_GOOD);
        }
        /// <summary>
        /// 读取字符串
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <param name="size">长度</param>
        /// <returns></returns>
        public ItemData<string> ReadString(DeviceAddress address, ushort size)
        {
            byte[] data = WriteSyncData(CreateReadHeader(PcNodeId, address.Start, size, (byte)address.DBNumber, (byte)address.Area));
            if (data == null)
                return new ItemData<string>(string.Empty, 0, QUALITIES.QUALITY_BAD);
            else
                return new ItemData<string>(Encoding.ASCII.GetString(data, 0, data.Length), 0, QUALITIES.QUALITY_GOOD);//是否考虑字节序问题？
        }
        /// <summary>
        /// 读取32位浮点数 
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <returns></returns>
        public unsafe ItemData<float> ReadFloat(DeviceAddress address)
        {
            byte[] data = WriteSyncData(CreateReadHeader(PcNodeId, address.Start, 2, (byte)address.DBNumber, (byte)address.Area));
            if (data == null)
                return new ItemData<float>(0.0f, 0, QUALITIES.QUALITY_BAD);
            else
            {
                int value = IPAddress.HostToNetworkOrder(BitConverter.ToInt32(data, 0));
                return new ItemData<float>(*(((float*)&value)), 0, QUALITIES.QUALITY_GOOD);
            }
        }
        /// <summary>
        /// 读取1位
        /// </summary>
        /// <param name="address">标签变量地址结构体</param>
        /// <returns></returns>
        public ItemData<bool> ReadBit(DeviceAddress address)
        {
            byte[] data = WriteSyncData(CreateReadHeader(PcNodeId, address.Start, 1, (byte)address.DBNumber, (byte)address.Area));
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
        /// <summary>
        /// 读object类型
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public ItemData<object> ReadValue(DeviceAddress address)
        {
            return this.ReadValueEx(address);
        }
        /// <summary>
        /// 写字节数组到设备
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <param name="bits">需写的字节数组</param>
        /// <returns></returns>
        public int WriteBytes(DeviceAddress address, byte[] bits)
        {
            var data = WriteMultipleRegister(PcNodeId, address.Start, (byte)address.DBNumber, bits, (byte)address.Area);
            return data == null ? -1 : 0;
        }

        public int WriteBit(DeviceAddress address, bool bit)
        {
            ItemData<short> item = ReadInt16(address);
            short value = (short)((bit ? 1 : 0) << (int)address.Bit);
            short result = (short)(item.Value | value);
            var data = WriteSingleRegister(PcNodeId, address.Start, (byte)address.DBNumber, BitConverter.GetBytes(result), (byte)address.Area);
            return data == null ? -1 : 0;
        }

        public int WriteBits(DeviceAddress address, byte bits)
        {
            var data = WriteSingleRegister(PcNodeId, address.Start, (byte)address.DBNumber, new byte[] { bits }, (byte)address.Area);
            return data == null ? -1 : 0;
        }

        public int WriteInt16(DeviceAddress address, short value)
        {
            var data = WriteSingleRegister(PcNodeId, address.Start, (byte)address.DBNumber, BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value)), (byte)address.Area);
            return data == null ? -1 : 0;
        }

        public int WriteUInt16(DeviceAddress address, ushort value)
        {
            var data = WriteSingleRegister(PcNodeId, address.Start, (byte)address.DBNumber, BitConverter.GetBytes((ushort)IPAddress.HostToNetworkOrder((short)value)), (byte)address.Area);
            return data == null ? -1 : 0;
        }

        public int WriteUInt32(DeviceAddress address, uint value)
        {
            var data = WriteMultipleRegister(PcNodeId, address.Start, (byte)address.DBNumber, BitConverter.GetBytes((uint)IPAddress.HostToNetworkOrder((int)value)), (byte)address.Area);
            return data == null ? -1 : 0;
        }

        public int WriteInt32(DeviceAddress address, int value)
        {
            var data = WriteMultipleRegister(PcNodeId, address.Start, (byte)address.DBNumber, BitConverter.GetBytes(IPAddress.HostToNetworkOrder(value)), (byte)address.Area);
            return data == null ? -1 : 0;
        }

        public int WriteFloat(DeviceAddress address, float value)
        {
            var data = WriteMultipleRegister(PcNodeId, address.Start, (byte)address.DBNumber, BitConverter.GetBytes(value), (byte)address.Area);
            return data == null ? -1 : 0;
        }

        public int WriteString(DeviceAddress address, string str)
        {
            var data = WriteMultipleRegister(PcNodeId, address.Start, (byte)address.DBNumber, Encoding.ASCII.GetBytes(str), (byte)address.Area);
            return data == null ? -1 : 0;
        }

        public int WriteValue(DeviceAddress address, object value)
        {
            return this.WriteValueEx(address, value);
        }

        public event IOErrorEventHandler OnError;

        public int Limit
        {
            get { return 960; }
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

    /// <summary>
    /// 欧姆龙Omron CS/CJ系列PLC数据区功能号和错误代码常数定义类
    /// </summary>
    public sealed class OmronCSCJ
    {
        /*****************************************************/
        //OMRON PLC CS/CJ系列数据区功能号
        /****************************************************/
        /// <summary>
        /// CIO 0000 to CIO 6143
        /// </summary>
        public const byte fctCIO = 0xB0;//CIO 0000 to CIO 6143
        /// <summary>
        /// H000 to H511
        /// </summary>
        public const byte fctHR = 0xB2;//H000 to H511
        /// <summary>
        /// A448 to A959
        /// </summary>
        public const byte fctA = 0xB3;//A448 to A959
        /// <summary>
        /// //D00000 to D32767
        /// </summary>
        public const byte fctDM = 0x82;//D00000 to D32767
        /// <summary>
        /// E0_E00000 to E0_E32765
        /// </summary>
        public const byte fctE0 = 0xA0;//E0_E00000 to E0_E32765
        /// <summary>
        /// E1_E00000 to E1_E32765
        /// </summary>
        public const byte fctE1 = 0xA1;//E1_E00000 to E1_E32765
        /// <summary>
        /// E2_E00000 to E2_E32765
        /// </summary>
        public const byte fctE2 = 0xA2;//E2_E00000 to E2_E32765
        /// <summary>
        /// E3_E00000 to E3_E32765
        /// </summary>
        public const byte fctE3 = 0xA3;
        /// <summary>
        /// E4_E00000 to E4_E32765
        /// </summary>
        public const byte fctE4 = 0xA4;//E4_E00000 to E4_E32765
        /// <summary>
        /// E5_E00000 to E5_E32765
        /// </summary>
        public const byte fctE5 = 0xA5;//E5_E00000 to E5_E32765
        /// <summary>
        /// E6_E00000 to E6_E32765
        /// </summary>
        public const byte fctE6 = 0xA6;//E6_E00000 to E6_E32765
        /// <summary>
        /// E7_E00000 to E7_E32765
        /// </summary>
        public const byte fctE7 = 0xA7;//E7_E00000 to E7_E32765
        /// <summary>
        /// E8_E00000 to E8_E32765
        /// </summary>
        public const byte fctE8 = 0xA8;//E8_E00000 to E8_E32765
        /// <summary>
        /// E9_E00000 to E9_E32765
        /// </summary>
        public const byte fctE9 = 0xA9;//E9_E00000 to E9_E32765
        /// <summary>
        /// EA_E00000 to EA_E32765
        /// </summary>
        public const byte fctEA = 0xAA;//EA_E00000 to EA_E32765
        /// <summary>
        /// EB_E00000 to EB_E32765
        /// </summary>
        public const byte fctEB = 0xAB;//EB_E00000 to EB_E32765
        /// <summary>
        /// EC_E00000 to EC_E32765
        /// </summary>
        public const byte fctEC = 0xAC;//EC_E00000 to EC_E32765
        /// <summary>
        /// E00000 to E32765
        /// </summary>
        public const byte fctCurrentbank = 98;//E00000 to E32765


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

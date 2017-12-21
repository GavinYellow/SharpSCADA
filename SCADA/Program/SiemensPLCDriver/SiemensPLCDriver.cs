﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Timers;
using DataService;

namespace SiemensPLCDriver
{
    [Description("S7 以太网协议")]
    public sealed class SiemensTCPReader : IPLCDriver, IMultiReadWrite
    {
        libnodave.daveOSserialType fds;
        libnodave.daveInterface di;
        internal libnodave.daveConnection dc;
        int _rack;
        int _slot;
        string _IP;
        object _async = new object();
        DateTime _closeTime = DateTime.Now;

        short _id;
        public short ID
        {
            get
            {
                return _id;
            }
        }

        bool _closed = true;
        public bool IsClosed
        {
            get
            {
                return _closed;
            }
        }

        public int PDU
        {
            get
            {
                return 220;//240
            }
        }

        int _timeOut;
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
                return _IP;
            }
            set
            {
                _IP = value;
            }
        }

        public SiemensTCPReader(IDataServer server, short id, string name, string IP, int timeOut = 10000, string rack = "0", string slot = "0")
        {
            _id = id;
            int.TryParse(rack, out _rack);
            int.TryParse(slot, out _slot);
            _IP = IP;
            _timeOut = timeOut;
            _server = server;
            _name = name;
        }
        #region 西门子驱动所有的变量组列表，通过组来划分循环时间不同的变量。
        List<IGroup> _groups = new List<IGroup>();
        public IEnumerable<IGroup> Groups
        {
            get { return _groups; }
        }

        IDataServer _server;//通讯服务器IDataServer
        public IDataServer Parent
        {
            get { return _server; }
        }

        #endregion

        #region 添加和移除组
        public IGroup AddGroup(string name, short id, int updateRate, float deadBand, bool active)//添加变量组
        {
            NetBytePLCGroup grp = new NetBytePLCGroup(id, name, updateRate, active, this);
            _groups.Add(grp);
            return grp;
        }
        public bool RemoveGroup(IGroup grp)//移除变量组
        {
            grp.IsActive = false;
            return _groups.Remove(grp);
        }
        #endregion

        #region 建立M,I，Q对应的地址区域的字典
        private static readonly Dictionary<char, int> dict = new Dictionary<char, int>(){ {'M',libnodave.daveFlags},
            {'I',libnodave.daveInputs},{'Q',libnodave.daveOutputs}};//
        #endregion
        #region 根据数据库里的设备地址(DeviceAddress)获取下位机PLC绝对地址（string)
        public string GetAddress(DeviceAddress address)//获取PLC地址
        {//如果PLC地址区域是Area,则字符串为“DB+DBNumer,D";如果是PLC区域是daveFlags,则字符串为M;如果是daveInputs，则字符串为I,否则为Q
            string addr = (address.Area == libnodave.daveDB ? string.Concat("DB", address.DBNumber, ",D") :
                    address.Area == libnodave.daveFlags ? "M" :
                    address.Area == libnodave.daveInputs ? "I" : "Q");
            switch (address.VarType)//根据变量类型Type来确定后面的字符。以DB1,D为例
            {
                case DataType.BOOL:
                    return string.Concat(addr, address.Start, ".", address.Bit);//如果是布尔型，则如DB1,D2.1
                case DataType.BYTE:
                    return string.Concat(addr, "BB", address.Start);//如果是字节型，则如DB1,DBB1
                case DataType.WORD:
                case DataType.SHORT:
                    return string.Concat(addr, "W", address.Start);//如果是短整型型，则如DB1,DW1
                case DataType.FLOAT:
                case DataType.INT:
                    return string.Concat(addr, "D", address.Start);//如果是浮点型或整型（32位），则如DB1,DD1
                default:
                    return string.Concat(addr, "BB", address.Start);//其他情况则如DB1,DBB1
            }
        }
        #endregion
        #region 根据下位机PLC绝对地址（string)获取数据库里的设备地址(DeviceAddress)
        public DeviceAddress GetDeviceAddress(string address)
        {
            DeviceAddress plcAddr = new DeviceAddress();
            if (string.IsNullOrEmpty(address) || address.Length < 2) return plcAddr;//如果地址为空或者字符长度小于2，则返回空的地址。
            #region 是DB存储器的处理
            if (address.Substring(0, 2) == "DB")//PLC绝对地址（string)前两位是DB时，DB1,D2.1
            {
                int index = 2;
                for (int i = index; i < address.Length; i++)//从第三位开始，如果不是十进制数，则返回索引号。
                {
                    if (!char.IsDigit(address[i]))
                    {
                        index = i; break;//index=3
                    }
                }
                plcAddr.Area = libnodave.daveDB;
                plcAddr.DBNumber = ushort.Parse(address.Substring(2, index - 2));//截取DB块的号，如DB1,D2.1,不是十进制的索引号是3，3-2=1，因此截取的长度为1
                string str = address.Substring(index + 1);//截取从索引4开始的字符串，包括索引4号位置D2.1
                if (!char.IsDigit(str[0]))//如果第一位不是数字的话，
                {
                    for (int i = 1; i < str.Length; i++)//则从第二位查找是数字的索引号，这里是1
                    {
                        if (char.IsDigit(str[i]))
                        {
                            index = i; break;//index=1
                        }
                    }
                    if (str[2] == 'W')//
                    {
                        int index1 = str.IndexOf('.');//index1=2
                        if (index1 > 0)
                        {
                            int start = int.Parse(str.Substring(3, index1 - 3));
                            byte bit = byte.Parse(str.RightFrom(index1));
                            plcAddr.Start = bit > 8 ? start : start + 1;
                            plcAddr.Bit = (byte)(bit > 7 ? bit - 8 : bit);
                            return plcAddr;
                        }
                    }
                    str = str.Substring(index);
                }
                index = str.IndexOf('.');
                if (index < 0)
                    plcAddr.Start = int.Parse(str);
                else
                {
                    plcAddr.Start = int.Parse(str.Substring(0, index));
                    plcAddr.Bit = byte.Parse(str.RightFrom(index));
                }
            }
            #endregion
            #region 不是DB存储区的处理
            else
            {
                plcAddr.DBNumber = 0;
                char chr = address[0];
                #region 是M I Q 地址区时的处理
                if (dict.ContainsKey(chr))
                {
                    plcAddr.Area = dict[chr];
                    int index = address.IndexOf('.');//MW1时，index=0;M1.0时，index=2
                    if (address[1] == 'W')//当是字地址时,如MW1
                    {
                        #region 不可能执行的
                        if (index > 0)//那index就不可能>0
                        {
                            int start = int.Parse(address.Substring(2, index - 2));
                            byte bit = byte.Parse(address.RightFrom(index));
                            plcAddr.Start = bit > 8 ? start : start + 1;
                            plcAddr.Bit = (byte)(bit > 7 ? bit - 8 : bit);

                        }
                        #endregion
                        else
                        {
                            plcAddr.Start = int.Parse(address.Substring(2));//只有Start位，没有Bit位
                        }
                    }
                    else//当不是字地址时,如M1.0
                    {
                        if (index > 0)
                        {
                            plcAddr.Start = int.Parse(address.Substring(1, index - 1));
                            plcAddr.Bit = byte.Parse(address.RightFrom(index));
                        }
                        else
                        {
                            plcAddr.Start = int.Parse(address.Substring(1));//只有Start位，没有Bit位
                        }
                    }
                }
                #endregion
            }
            #endregion
            return plcAddr;
        }
        #endregion
        #region 通过集成于PLC上的以太网建立通讯连接
        public bool Connect()
        {
            lock (_async)
            {
                if (!_closed) return true;
                double sec = (DateTime.Now - _closeTime).TotalMilliseconds;
                if (sec < 6000)
                    System.Threading.Thread.Sleep(6000 - (int)sec);
                fds.rfd = libnodave.openSocket(102, _IP);
                fds.wfd = fds.rfd;
                if (fds.rfd > 0)
                {
                    di = new libnodave.daveInterface(fds, "IF1", 0, libnodave.daveProtoISOTCP, libnodave.daveSpeed187k);
                    di.setTimeout(TimeOut);
                    //	    res=di.initAdapter();	// does nothing in ISO_TCP. But call it to keep your programs indpendent of protocols
                    //	    if(res==0) {
                    dc = new libnodave.daveConnection(di, 0, _rack, _slot);
                    if (0 == dc.connectPLC())
                    {
                        _closed = false;
                        return true;
                    }
                }
                if (dc != null) dc.disconnectPLC();
                libnodave.closeSocket(fds.rfd);
            }
            _closed = true;
            return false;
        }
        #endregion
        public void Dispose()
        {
            lock (_async)
            {
                if (dc != null) dc.disconnectPLC();
                libnodave.closeSocket(102);
                foreach (IGroup grp in _groups)
                {
                    grp.Dispose();
                }
                _closed = true;
            }
        }
        #region 报错处理
        string daveStrerror(int code)
        {
            switch (code)
            {
                case 0: return "ok";
                case 6: return "the CPU does not support reading a bit block of length<>1";
                case 10: return "the desired item is not available in the PLC";
                case 3: return "the desired item is not available in the PLC (200 family)";
                case 5: return "the desired address is beyond limit for this PLC";
                case -124: return "the PLC returned a packet with no result data";
                case -125: return "the PLC returned an error code not understood by this library";
                case -126: return "this result contains no data";
                case -127: return "cannot work with an undefined result set";
                case -123: return "cannot evaluate the received PDU";
                case 7: return "Write data size error";
                case 1: return "No data from I/O module";
                case -128: return "Unexpected function code in answer";
                case -129: return "PLC responds with an unknown data type";
                case -1024: return "Short packet from PLC";
                case -1025: return "Timeout when waiting for PLC response";
                case 0x8000: return "function already occupied.";
                case 0x8001: return "not allowed in current operating status.";
                case 0x8101: return "hardware fault.";
                case 0x8103: return "object access not allowed.";
                case 0x8104: return "context is not supported. Step7 says:Function not implemented or error in telgram.";
                case 0x8105: return "invalid address.";
                case 0x8106: return "data type not supported.";
                case 0x8107: return "data type not consistent.";
                case 0x810A: return "object does not exist.";
                case 0x8301: return "insufficient CPU memory ?";
                case 0x8402: return "CPU already in RUN or already in STOP ?";
                case 0x8404: return "severe error ?";
                case 0x8500: return "incorrect PDU size.";
                case 0x8702: return "address invalid."; ;
                case 0xd002: return "Step7:variant of command is illegal.";
                case 0xd004: return "Step7:status for this command is illegal.";
                case 0xd0A1: return "Step7:function is not allowed in the current protection level.";
                case 0xd201: return "block name syntax error.";
                case 0xd202: return "syntax error function parameter.";
                case 0xd203: return "syntax error block type.";
                case 0xd204: return "no linked block in storage medium.";
                case 0xd205: return "object already exists.";
                case 0xd206: return "object already exists.";
                case 0xd207: return "block exists in EPROM.";
                case 0xd209: return "block does not exist/could not be found.";
                case 0xd20e: return "no block present.";
                case 0xd210: return "block number too big.";
                //	case 0xd240: return "unfinished block transfer in progress?";  // my guess
                case 0xd240: return "Coordination rules were violated.";
                /*  Multiple functions tried to manipulate the same object.
                    Example: a block could not be copied,because it is already present in the target system
                    and
                */
                case 0xd241: return "Operation not permitted in current protection level.";
                /**/
                case 0xd242: return "protection violation while processing F-blocks. F-blocks can only be processed after password input.";
                case 0xd401: return "invalid SZL ID.";
                case 0xd402: return "invalid SZL index.";
                case 0xd406: return "diagnosis: info not available.";
                case 0xd409: return "diagnosis: DP error.";
                case 0xdc01: return "invalid BCD code or Invalid time format?";
                default: return "no message defined!";
            }
        }
        #endregion
        public byte[] ReadBytes(DeviceAddress address, ushort len)//从PLC中读取自己数组
        {
            if (dc != null)
            {
                byte[] buffer = new byte[len];
                int res = -1;
                lock (_async)
                {
                    res = dc == null ? -1 : dc.readBytes(address.Area, address.DBNumber, address.Start, len, buffer);
                }
                if (res == 0)
                    return buffer;
                _closed = true; dc = null; _closeTime = DateTime.Now;
                if (OnClose != null)
                {
                    OnClose(this, new ShutdownRequestEventArgs(daveStrerror(res))); _closeTime = DateTime.Now;
                }
            }
            return null;
        }

        public ItemData<string> ReadString(DeviceAddress address, ushort size = 0xFF)
        {
            if (dc != null)
            {
                byte[] buffer = new byte[size];
                int res = -1;
                lock (_async)
                {
                    res = dc.readBytes(address.Area, address.DBNumber, address.Start, size, buffer);
                }
                if (res == 0)
                    return new ItemData<string>(Utility.ConvertToString(buffer), 0, QUALITIES.QUALITY_GOOD);
                _closed = true; dc = null; _closeTime = DateTime.Now;
                if (OnClose != null)
                {
                    OnClose(this, new ShutdownRequestEventArgs(daveStrerror(res)));
                }
            }
            return new ItemData<string>(string.Empty, 0, QUALITIES.QUALITY_NOT_CONNECTED);
        }

        public ItemData<int> ReadInt32(DeviceAddress address)
        {
            if (dc != null)
            {
                int res = -1;
                lock (_async)
                {
                    res = dc.readBytes(address.Area, address.DBNumber, address.Start, 4, null);
                    if (res == 0) return new ItemData<int>(dc.getS32(), 0, QUALITIES.QUALITY_GOOD);
                }
                _closed = true; dc = null; _closeTime = DateTime.Now;
                if (OnClose != null)
                {
                    OnClose(this, new ShutdownRequestEventArgs(daveStrerror(res)));
                }
            }
            return new ItemData<int>(0, 0, QUALITIES.QUALITY_NOT_CONNECTED); ;
        }

        public ItemData<short> ReadInt16(DeviceAddress address)
        {
            if (dc != null)
            {
                int res = -1;
                lock (_async)
                {
                    res = dc.readBytes(address.Area, address.DBNumber, address.Start, 2, null);
                    if (res == 0) return new ItemData<short>((short)dc.getS16(), 0, QUALITIES.QUALITY_GOOD);
                }
                _closed = true; dc = null; _closeTime = DateTime.Now;
                if (OnClose != null)
                {
                    OnClose(this, new ShutdownRequestEventArgs(daveStrerror(res)));
                }
            }
            return new ItemData<short>(0, 0, QUALITIES.QUALITY_NOT_CONNECTED); ;
        }

        public ItemData<byte> ReadByte(DeviceAddress address)
        {
            if (dc != null)
            {
                int res = -1;
                lock (_async)
                {
                    res = dc.readBytes(address.Area, address.DBNumber, address.Start, 1, null);
                    if (res == 0)
                        return new ItemData<byte>((byte)dc.getS8(), 0, QUALITIES.QUALITY_GOOD);
                }
                _closed = true; dc = null; _closeTime = DateTime.Now;
                if (OnClose != null)
                {
                    OnClose(this, new ShutdownRequestEventArgs(daveStrerror(res)));
                }
            }
            return new ItemData<byte>(0, 0, QUALITIES.QUALITY_NOT_CONNECTED); ;
        }

        public ItemData<bool> ReadBit(DeviceAddress address)
        {
            int res = -1;
            if (dc != null)
            {
                lock (_async)
                {
                    res = dc.readBits(address.Area, address.DBNumber, address.Start * 8 + address.Bit, 1, null);//修改了原地址上的Bug
                    if (res == 0) return new ItemData<bool>(dc.getS8() != 0, 0, QUALITIES.QUALITY_GOOD);
                }
                _closed = true; dc = null; _closeTime = DateTime.Now;
                if (OnClose != null) { OnClose(this, new ShutdownRequestEventArgs(daveStrerror(res))); }
            }
            return new ItemData<bool>(false, 0, QUALITIES.QUALITY_NOT_CONNECTED);
        }

        public ItemData<float> ReadFloat(DeviceAddress address)
        {
            if (dc != null)
            {
                int res = -1;
                lock (_async)
                {
                    res = dc.readBytes(address.Area, address.DBNumber, address.Start, 4, null);
                    if (res == 0) return new ItemData<float>(dc.getFloat(), 0, QUALITIES.QUALITY_GOOD);
                }
                _closed = true; dc = null; _closeTime = DateTime.Now;
                if (OnClose != null)
                {
                    OnClose(this, new ShutdownRequestEventArgs(daveStrerror(res))); _closeTime = DateTime.Now;
                }
            }
            return new ItemData<float>(0, 0, QUALITIES.QUALITY_NOT_CONNECTED); ;
        }


        public ItemData<object> ReadValue(DeviceAddress address)
        {
            return this.ReadValueEx(address);
        }

        public int WriteBit(DeviceAddress address, bool bit)
        {
            lock (_async)
            {
                return dc == null ? -1 : dc.writeBits(address.Area, address.DBNumber, address.Start * 8 + address.Bit, 1, bit ? new byte[] { 0x1 } : new byte[] { 0x00 });
            }
        }

        public int WriteBits(DeviceAddress address, byte value)
        {
            lock (_async)
            {
                return dc == null ? -1 : dc.writeBits(address.Area, address.DBNumber, address.Start, 1, new byte[] { value });
            }
        }

        public int WriteInt16(DeviceAddress address, short value)
        {
            byte[] b = BitConverter.GetBytes(value); Array.Reverse(b);
            lock (_async)
            {
                return dc == null ? -1 : dc.writeBytes(address.Area, address.DBNumber, address.Start, 2, b);
            }
        }

        public int WriteInt32(DeviceAddress address, int value)
        {
            byte[] b = BitConverter.GetBytes(value); Array.Reverse(b);
            lock (_async)
            {
                return dc == null ? -1 : dc.writeBytes(address.Area, address.DBNumber, address.Start, 4, b);
            }
        }

        public int WriteFloat(DeviceAddress address, float value)
        {
            byte[] b = BitConverter.GetBytes(value); Array.Reverse(b);
            lock (_async)
            {
                return dc == null ? -1 : dc.writeBytes(address.Area, address.DBNumber, address.Start, 4, b);
            }
        }

        public int WriteString(DeviceAddress address, string str)
        {
            lock (_async)
            {
                if (str.Length > address.DataSize)
                    str.Remove(address.DataSize - 1);
                var textArr = Encoding.ASCII.GetBytes(str);
                var buffer = new byte[2 + textArr.Length];
                buffer[0] = (byte)address.DataSize;
                buffer[1] = (byte)textArr.Length;
                textArr.CopyTo(buffer, 2);
                return dc == null ? -1 : dc.writeManyBytes(address.Area, address.DBNumber, address.Start,
                    buffer.Length, buffer); //1200前置空格
            }
        }

        public int WriteValue(DeviceAddress address, object value)
        {
            return this.WriteValueEx(address, value);
        }

        public int WriteBytes(DeviceAddress address, byte[] bit)
        {
            lock (_async)
            {
                return dc == null ? -1 : dc.writeBytes(address.Area, address.DBNumber, address.Start, bit.Length, bit);
            }
        }
        public ItemData<Storage>[] ReadMultiple(DeviceAddress[] addrsArr)
        {
            int len = addrsArr.Length;
            if (len > Limit)
            {
                List<ItemData<Storage>> itemArr = new List<ItemData<Storage>>(len);
                int i = 0;
                while (i < len)
                {
                    int cnt = len - i > Limit ? Limit : len - i;
                    DeviceAddress[] addr = new DeviceAddress[cnt];
                    addrsArr.CopyTo(addr, i);
                    itemArr.AddRange(ReadMultipleInternal(addrsArr));
                    i += Limit;
                }
                return itemArr.ToArray();
            }
            else
                return ReadMultipleInternal(addrsArr);
        }

        ItemData<Storage>[] ReadMultipleInternal(DeviceAddress[] addrsArr)
        {
            int len = addrsArr.Length;
            if (len <= Limit)
            {
                lock (_async)
                {
                    if (dc == null) return null;
                    libnodave.PDU p = dc.prepareReadRequest();
                    for (int i = 0; i < len; i++)
                    {
                        DeviceAddress addr = addrsArr[i];
                        if (addr.VarType == DataType.BOOL)
                            p.addBitVarToReadRequest(addr.Area, addr.DBNumber, addr.Start, addr.DataSize);
                        else
                            p.addVarToReadRequest(addr.Area, addr.DBNumber, addr.Start, addr.DataSize);
                    }
                    libnodave.resultSet rs = new libnodave.resultSet();
                    int res = dc.execReadRequest(p, rs);
                    ItemData<Storage>[] itemArr = new ItemData<Storage>[len];
                    for (int i = 0; i < len; i++)
                    {
                        res = dc.useResult(rs, i);
                        if (res > 0)
                        {
                            itemArr[i].Quality = QUALITIES.QUALITY_GOOD;
                            switch (addrsArr[i].VarType)
                            {
                                case DataType.BOOL:
                                    itemArr[i].Value.Boolean = dc.getU8() > 0;//需测试
                                    break;
                                case DataType.BYTE:
                                    itemArr[i].Value.Byte = (byte)dc.getU8();
                                    break;
                                case DataType.WORD:
                                case DataType.SHORT:
                                    itemArr[i].Value.Int16 = (short)dc.getS16();
                                    break;
                                case DataType.INT:
                                    itemArr[i].Value.Int32 = dc.getS32();
                                    break;
                                case DataType.FLOAT:
                                    itemArr[i].Value.Single = dc.getFloat();
                                    break;
                            }
                        }
                    }
                    return itemArr;
                }
            }
            else
                return this.PLCReadMultiple(new NetByteCacheReader(), addrsArr);
        }

        public int WriteMultiple(DeviceAddress[] addrArr, object[] buffer)
        {
            int len = addrArr.Length;
            if (len > Limit)
            {
                int ret = 0;
                int i = 0;
                while (i < len)
                {
                    int cnt = len - i > Limit ? Limit : len - i;
                    DeviceAddress[] addr = new DeviceAddress[cnt];
                    Array.Copy(addrArr, i, addr, 0, cnt);
                    //addrArr.CopyTo(addr, i);
                    object[] values = new object[cnt];
                    Array.Copy(buffer, i, values, 0, cnt);
                    //buffer.CopyTo(values, i);
                    var res = WriteMultipleInternal(addr, values);
                    if (res < 0) ret = res;
                    i += Limit;
                }
                return ret;
            }
            return WriteMultipleInternal(addrArr, buffer);
        }

        int WriteMultipleInternal(DeviceAddress[] addrArr, object[] buffer)
        {
            lock (_async)
            {
                if (dc == null) return -1;
                int len = addrArr.Length;
                libnodave.PDU p2 = dc.prepareWriteRequest();
                for (int i = 0; i < len; i++)
                {
                    try
                    {
                        DeviceAddress addr = addrArr[i];
                        byte[] b;
                        switch (addr.VarType)
                        {
                            case DataType.BOOL:
                                bool bl = Convert.ToBoolean(buffer[i]);
                                b = new byte[] { (byte)(bl ? 1 : 0) };
                                p2.addBitVarToWriteRequest(addr.Area, addr.DBNumber, addr.Start * 8 + addr.Bit, 1, b);
                                break;
                            case DataType.BYTE:
                                b = BitConverter.GetBytes(Convert.ToByte(buffer[i]));
                                p2.addVarToWriteRequest(addr.Area, addr.DBNumber, addr.Start, 1, b);
                                break;
                            case DataType.WORD:
                                b = BitConverter.GetBytes(Convert.ToUInt16(buffer[i])); Array.Reverse(b);
                                p2.addVarToWriteRequest(addr.Area, addr.DBNumber, addr.Start, 2, b);
                                break;
                            case DataType.SHORT:
                                b = BitConverter.GetBytes(Convert.ToInt16(buffer[i])); Array.Reverse(b);
                                p2.addVarToWriteRequest(addr.Area, addr.DBNumber, addr.Start, 2, b);
                                break;
                            case DataType.INT:
                                b = BitConverter.GetBytes((int)Convert.ToDouble(buffer[i])); Array.Reverse(b);
                                p2.addVarToWriteRequest(addr.Area, addr.DBNumber, addr.Start, 4, b);
                                break;
                            case DataType.FLOAT:
                                b = BitConverter.GetBytes(Convert.ToSingle(buffer[i])); Array.Reverse(b);
                                p2.addVarToWriteRequest(addr.Area, addr.DBNumber, addr.Start, 4, b);
                                break;
                            case DataType.STR:
                                {
                                    b = Encoding.ASCII.GetBytes(buffer[i].ToString());
                                    var b1 = new byte[2 + b.Length];
                                    b1[0] = (byte)addrArr[i].DataSize;
                                    b1[1] = (byte)b.Length;
                                    b.CopyTo(b1, 2);
                                    p2.addVarToWriteRequest(addr.Area, addr.DBNumber, addr.Start, addr.DataSize, b1);
                                }
                                break;
                        }
                    }
                    catch (Exception err)
                    {
                        if (OnClose != null) OnClose(this, new ShutdownRequestEventArgs(err.Message));//可考虑把相应地址和数值加入
                    }
                }
                libnodave.resultSet rs = new libnodave.resultSet();
                return dc.execWriteRequest(p2, rs);
            }
        }

        public int Limit
        {
            get { return 10; }
        }

        public event ShutdownRequestEventHandler OnClose;
    }

}

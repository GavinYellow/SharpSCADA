using DataService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Text;

namespace PanasonicPLCriver
{
    [Description("松下协议串口通讯")]
    public class PanasonicSerialReader : IPLCDriver, IMultiReadWrite
    {
        private SerialPort _serialPort;
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
        #region 本程序不会用到的方法
        short _id;
        public short ID
        {
            get
            {
                return _id;
            }
        }
        public string GetAddress(DeviceAddress address)
        {
            return string.Empty;
        }
        #endregion
        public PanasonicSerialReader(IDataServer server, short id, string name)
        {
            _server = server;
            _id = id;
            _name = name;
            //spare1 = {COM3,9600,Odd,8,One}
        }

        public bool IsClosed
        {
            get
            {
                return _serialPort.IsOpen == false;
            }
        }

        public int Limit
        {
            get { return 60; }//应该是读取最大数的上限吧
        }

        string _name;
        public string Name
        {
            get
            {
                return _name;
            }
        }

        public int PDU
        {
            get
            {
                return 256;//每帧最大字节数
            }
        }
        string _com;
        public string ServerName
        {
            get { return _com; }
            set { _com = value; }
        }

        private int _timeOut;
        private byte _devId;

        public int TimeOut
        {
            get { return _timeOut; }
            set { _timeOut = value; }
        }

        public event IOErrorEventHandler OnError;

        public IGroup AddGroup(string name, short id, int updateRate, float deadBand = 0, bool active = false)
        {
            ShortGroup grp = new ShortGroup(id, name, updateRate, active, this);
            _grps.Add(grp);
            return grp;
        }

        public bool Connect()
        {
            try
            {
                if (_serialPort == null)
                    _serialPort = new SerialPort(_com, 57600, Parity.Odd, 8, StopBits.One);
                _serialPort.Open();
                _serialPort.NewLine = "\r";
                _serialPort.ReadTimeout = 10000;
                return true;
            }
            catch (IOException error)
            {
                if (OnError != null)
                {
                    OnError(this, new IOErrorEventArgs(error.Message));
                }
                return false;
            }
        }

        public void Dispose()
        {
            foreach (IGroup grp in _grps)
            {
                grp.Dispose();
            }
            _grps.Clear();
            _serialPort.Close();
        }
        private bool isBool(string area)
        {
            if (area == "X" || area == "Y" || area == "R" || area == "T" || area == "C")
                return true;
            else
                return false;
        }
        private bool isBool(int area)
        {
            if (area == Panasonic.Xarea || area == Panasonic.Yarea || area == Panasonic.Rarea || area == Panasonic.Tarea || area == Panasonic.Carea)
                return true;
            else
                return false;
        }
        public DeviceAddress GetDeviceAddress(string address)
        {
            DeviceAddress dv = DeviceAddress.Empty;
            if (string.IsNullOrEmpty(address))
                return dv;
            string area = address.Substring(0, 1);
            if (!isBool(area))//如果输入的不是单触点寄存器地址
            {
                int st;
                int.TryParse(address.Substring(2), out st);
                dv.Start = st;
                switch (address.Substring(0, 2))
                {
                    case "DT":
                        dv.Area = Panasonic.DTarea;
                        break;
                    case "SV":
                        dv.Area = Panasonic.SVarea;
                        break;
                    case "EV":
                        dv.Area = Panasonic.EVarea;
                        break;
                    case "IX":
                        dv.Area = Panasonic.IXarea;
                        break;
                    case "IY":
                        dv.Area = Panasonic.IYarea;
                        break;
                    case "WX":
                        dv.Area = Panasonic.WXarea;
                        break;
                    case "WY":
                        dv.Area = Panasonic.WYarea;
                        break;
                    case "WR":
                        dv.Area = Panasonic.WRarea;
                        break;
                }
            }
            else//输入的是布尔量  如R10.4   但在PLC那端定义位R104  R10.A  在PLC哪里就是..
            {
                int index = address.IndexOf('.');
                if (index > 0)// index = 3 
                {
                    string start = address.Substring(1, index - 1);
                    string bit = address.Substring(index + 1);
                    dv.Start = int.Parse(start);//10
                    dv.Bit = byte.Parse(bit, NumberStyles.HexNumber);//4
                }
                else //index = -1  用户输入了R104  就认为是R104.0
                    dv.Start = int.Parse(address.Substring(1));
                switch (address.Substring(0, 1))
                {
                    case "X":
                        dv.Area = Panasonic.Xarea;
                        break;
                    case "Y":
                        dv.Area = Panasonic.Yarea;
                        break;
                    case "R":
                        dv.Area = Panasonic.Rarea;
                        break;
                    case "T":
                        dv.Area = Panasonic.Tarea;
                        break;
                    case "C":
                        dv.Area = Panasonic.Carea;
                        break;
                }
            }
            return dv;
        }
        #region 实现了四个命令 其余没有做
        /// <summary>
        /// 按字单位读取触点状态 从0-999
        /// 如果是读取R100.4 和R101.5就是CreateRCCCmd(100, 2,Panasonic.Rarea,out respBeginStr)
        /// 然后从读来的16位数中获取需要的位
        /// </summary>
        /// <returns></returns>
        private string CreateRCCCmd(int start, ushort size, int areaType, out string respBeginStr)
        {
            string area = string.Empty;
            switch (areaType)
            {
                case Panasonic.Xarea:
                    area = "X";
                    break;
                case Panasonic.Yarea:
                    area = "Y";
                    break;
                case Panasonic.Rarea:
                    area = "R";
                    break;
                case Panasonic.Carea:
                    area = "C";
                    break;
            }
            respBeginStr = "<" + IntTo2Hex(_devId) + "$" + "RC" + area;
            string readCmd = "<" + IntTo2Hex(_devId) + "#" + Panasonic.RCCCmd + area + IntTo4Bcd(start) + IntTo4Bcd(start + size - 1);
            readCmd = readCmd + Utility.XorCheck(readCmd) + "\r";
            return readCmd;
        }
        /// <summary>
        /// 创建写入单触点状态命令
        /// </summary>
        /// <param name="start">开始地址</param>
        /// <param name="bit">位索引</param>
        /// <param name="areaType">触点类型：X，Y，R，C</param>
        /// <param name="value">写入的值</param>
        /// <param name="respBeginStr">响应的开始帧</param>
        private string CreateWCSCmd(int start, int bit, int areaType, bool value, out string respBeginStr)
        {
            string area = string.Empty;
            switch (areaType)
            {
                case Panasonic.Xarea:
                    area = "X";
                    break;
                case Panasonic.Yarea:
                    area = "Y";
                    break;
                case Panasonic.Rarea:
                    area = "R";
                    break;
                case Panasonic.Carea:
                    area = "C";
                    break;
            }
            string bcdStart = IntTo3Bcd(start);
            string hexBit = bit.ToString("X");
            string strData = area + bcdStart + hexBit + (value ? "1" : "0");
            respBeginStr = "<" + IntTo2Hex(_devId) + "$" + "WC";
            string readCmd = "<" + IntTo2Hex(_devId) + "#" + Panasonic.WCSCmd + strData;
            readCmd = readCmd + Utility.XorCheck(readCmd) + "\r";
            return readCmd;
        }
        /// <summary>
        /// 读取多个寄存器 只支持DT寄存器
        /// </summary>
        /// <param name="start">开始地址</param>
        /// <param name="size">寄存器个数</param>
        /// <param name="respBeginStr">响应的开始帧</param>
        /// <returns></returns>
        private string CreateRDCmd(int start, ushort size, out string respBeginStr)
        {
            respBeginStr = "<" + IntTo2Hex(_devId) + "$" + "RD";
            string readCmd = "<" + IntTo2Hex(_devId) + "#" + Panasonic.RDCmd + "D" + IntTo5Bcd(start) + IntTo5Bcd(start + size - 1);
            readCmd = readCmd + Utility.XorCheck(readCmd) + "\r";
            return readCmd;
        }
        /// <summary>
        /// 写入连续一块区域寄存器
        /// </summary>
        /// <param name="start">开始地址</param>
        /// <param name="size">寄存器个数(最小是1)</param>
        /// <param name="values">写入的值</param>
        /// <param name="respBeginStr">响应的开始帧</param>
        /// <returns></returns>
        public string CreateWDCmd(int start, short[] values, out string respBeginStr)
        {
            string dataStr = string.Empty;
            int size = values.Length;
            for (int i = 0; i < size; i++)
            {
                UInt16 value = Utility.ReverseInt16(values[i]);
                dataStr = dataStr + IntTo4Hex(value);
            }
            respBeginStr = "<" + IntTo2Hex(_devId) + "$" + Panasonic.WDCmd;
            string readCmd = "<" + IntTo2Hex(_devId) + "#" + Panasonic.WDCmd + "D" + IntTo5Bcd(start) + IntTo5Bcd(start + size - 1) + dataStr;
            readCmd = readCmd + Utility.XorCheck(readCmd) + "\r";
            return readCmd;
        }
        #endregion
        /// <summary>
        /// 读取多个字 松下只能读取16位数读取，不支持32位，浮点和字符串
        /// 如果是读取数据寄存器的值的话，读取的是4字符16位数
        /// 读来的数据比如是3124H   那么真实值应该是2431H   最大值为FFFFH
        /// </summary>
        /// <param name="startAddress">开始的地址</param>
        /// <param name="size">个数</param>
        /// <returns></returns>
        public byte[] ReadBytes(DeviceAddress startAddress, ushort size)
        {
            string respBeginStr;
            string cmd;
            try
            {
                if (!isBool(startAddress.Area))//读寄存器的情况 
                {
                    cmd = CreateRDCmd(startAddress.Start, size, out respBeginStr);
                }
                else //读触点的情况
                {
                    cmd = CreateRCCCmd(startAddress.Start, size, startAddress.Area, out respBeginStr);
                }
                return WriteSyncData(respBeginStr, cmd);
            }
            catch (Exception e)
            {
                if (OnError != null)
                    OnError(this, new IOErrorEventArgs(e.Message));
                return null;
            }
        }
        private static readonly object syncLock = new Object();
        private byte[] WriteSyncData(string respBeginStr, string cmd)
        {
            byte[] writeData = Encoding.Default.GetBytes(cmd);
            string recv = string.Empty;
            lock (syncLock)//这里加锁 防止一个刚写完还没全读来  另一就去写 
                           //但这就会造成锁死的情况  比如循环读的时候  触发去写 这时候必须要等读完才能去写
            {
                _serialPort.Write(writeData, 0, writeData.Length);
                try
                {
                    recv = _serialPort.ReadLine().Trim();
                }
                catch (Exception)
                {
                    if (OnError != null)
                        OnError(this, new IOErrorEventArgs("读取超时"));
                    return null;
                }
            }
            if (recv.Substring(3, 1) == "!")//返回为错误代码
            {
                string err = recv.Substring(4, 2);
                if (OnError != null)
                    OnError(this, new IOErrorEventArgs(daveStrerror(err)));
                return null;
            }
            string needXorStr = recv.Substring(0, recv.Length - 2);//需要进行xor校验的字符串
            string recvCheck = recv.Substring(recv.Length - 2, 2);//接收的xor字符串
            string checkStr = Utility.XorCheck(needXorStr);
            if (checkStr != recvCheck)
            {
                if (OnError != null)
                    OnError(this, new IOErrorEventArgs("校验失败"));
                return null;
            }
            else
            {
                if (recv.Substring(4, 1) == "W")//如果是写入命令
                {
                    return new byte[0];
                }
                string dataStr = Utility.Pinchstring(recv, respBeginStr, checkStr);
                return Utility.HexToBytes(dataStr);
            }
        }
        string daveStrerror(string code)
        {
            switch (code)
            {
                case "20": return "未定义";
                case "21": return "远程单元无法被正确识别，或者发生了数据错误.";
                case "22": return "用于远程单元的接收缓冲区已满.";
                case "23": return "远程单元编号(01 至16)设置与本地单元重复.";
                case "24": return "试图发送不符合传输格式的数据,或者某一帧数据溢出或发生了数据错误.";
                case "25": return "传输系统硬件停止操作.";
                case "26": return "远程单元的编号设置超出01 至63 的范围.";
                case "27": return "接收方数据帧溢出. 试图在不同的模块之间发送不同帧长度的数据.";
                case "28": return "远程单元不存在. (超时)";
                case "29": return "试图发送或接收处于关闭状态的缓冲区.";
                case "30": return "持续处于传输禁止状态.";
                case "40": return "在指令数据中发生传输错误.";
                case "41": return "所发送的指令信息不符合传输格式.";
                case "42": return "发送了一个未被支持的指令向未被支持的目标站发送了指令";
                case "43": return "在处于传输请求信息挂起时,发送了其他指令.";
                case "50": return "设置了实际不存在的链接编号..";
                case "51": return "当向其他单元发出指令时,本地单元的传输缓冲区已满..";
                case "52": return "无法向其他单元传输";
                case "53": return "在接收到指令时,正在处理其他指令.";
                case "60": return "在指令中包含有无法使用的代码,或者代码没有附带区域指定参数(X,Y,D,等以外.).";
                case "61": return "触点编号,区域编号,数据代码格式(BCD,hex,等)上溢出, 下溢出以及区域指定错误.";
                case "62": return "过多记录数据在未记录状态下的操作";
                case "63": return "当一条指令发出时，运行模式不能够对指令进行处理";
                case "65": return "在存储保护状态下执行写操作到程序区域或系统寄存器。";
                case "66": return "地址（程序地址、绝对地址等）数据编码形式（BCD、hex 等）、上溢、下溢或指定范围错误";
                case "67": return "要读的数据不存在。（读取没有写入注释寄存区的数据。）";
                default: return "no message defined!";
            }
        }
        public ItemData<bool> ReadBit(DeviceAddress address)
        {
            throw new NotImplementedException();
        }
        public ItemData<byte> ReadByte(DeviceAddress address)
        {
            throw new NotImplementedException();
        }
        public ItemData<float> ReadFloat(DeviceAddress address)
        {
            throw new NotImplementedException();
        }
        public ItemData<short> ReadInt16(DeviceAddress address)
        {
            throw new NotImplementedException();
        }
        public ItemData<ushort> ReadUInt16(DeviceAddress address)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 读取一个32位数  返回如果是6300  则认为是0063H 高低位要反 而且是16进账
        /// </summary>
        /// <param name="address"></param>
        public ItemData<int> ReadInt32(DeviceAddress address)
        {
            throw new NotImplementedException();
        }
        public ItemData<uint> ReadUInt32(DeviceAddress address)
        {
            throw new NotImplementedException();
        }

        public ItemData<Storage>[] ReadMultiple(DeviceAddress[] addrsArr)
        {
            throw new NotImplementedException();
        }

        public ItemData<string> ReadString(DeviceAddress address, ushort size)
        {
            throw new NotImplementedException();
        }

        public ItemData<object> ReadValue(DeviceAddress address)
        {
            throw new NotImplementedException();
        }

        public bool RemoveGroup(IGroup group)
        {
            throw new NotImplementedException();
        }

        public int WriteBit(DeviceAddress address, bool bit)
        {
            string respBeginStr = string.Empty;
            string cmd = CreateWCSCmd(address.Start, address.Bit, address.Area, bit, out respBeginStr);
            WriteSyncData(respBeginStr, cmd);
            return 0;
        }

        public int WriteBits(DeviceAddress address, byte bits)
        {
            throw new NotImplementedException();
        }

        public int WriteBytes(DeviceAddress address, byte[] bit)
        {
            throw new NotImplementedException();
        }

        public int WriteFloat(DeviceAddress address, float value)
        {
            throw new NotImplementedException();
        }

        public int WriteInt16(DeviceAddress address, short value)
        {
            string respBeginStr = string.Empty;
            string cmd = CreateWDCmd(address.Start, new short[1] { value }, out respBeginStr);
            WriteSyncData(respBeginStr, cmd);
            return 0;
        }

        public int WriteUInt16(DeviceAddress address, ushort value)
        {
            return WriteInt16(address, (short)value);
        }

        public int WriteUInt32(DeviceAddress address, uint value)
        {
            throw new NotImplementedException();
        }

        public int WriteInt32(DeviceAddress address, int value)
        {
            throw new NotImplementedException();
        }

        public int WriteMultiple(DeviceAddress[] addrArr, object[] buffer)
        {
            throw new NotImplementedException();
        }

        public int WriteString(DeviceAddress address, string str)
        {
            throw new NotImplementedException();
        }

        public int WriteValue(DeviceAddress address, object value)
        {
            throw new NotImplementedException();
        }

        #region 类中类中工具方法 
        /// <summary>
        /// 将byte类型转换成两位16进制字符串
        /// </summary>
        private static string IntTo2Hex(int num)
        {
            return num.ToString("X2").PadLeft(2, '0');//15 -> 0F
        }
        /// <summary>
        /// 将byte类型转换成4位16进制字符串
        /// </summary>
        private static string IntTo4Hex(int num)
        {
            return num.ToString("X2").PadLeft(4, '0');//15 -> 000F
        }
        /// <summary>
        /// 将Int类型转换成3位BCD字符串
        /// </summary>
        private string IntTo3Bcd(int num)
        {
            return num.ToString().PadLeft(3, '0');  //9->009
        }
        /// <summary>
        /// 将Int类型转换成4位BCD字符串
        /// </summary>
        private string IntTo4Bcd(int num)
        {
            return num.ToString().PadLeft(4, '0');  //9->0009
        }
        /// <summary>
        /// 将Int类型转换成5位BCD字符串
        /// </summary>
        private string IntTo5Bcd(int num)
        {
            return num.ToString().PadLeft(5, '0');  //9->00009
        }
        #endregion
    }

    public struct Panasonic
    {
        //用的是松下的协议 支持串口和以太网。
        /// <summary>
        ///1. 读取单触点状态 
        /// </summary>
        public const string RCSCmd = "RCS";
        /// <summary>
        /// 2. 写入单触点状态
        /// </summary>
        public const string WCSCmd = "WCS";
        /// <summary>
        ///3. 读取多触点状态
        /// </summary>
        public const string RCPCmd = "RCP";
        /// <summary>
        /// 4. 写入多触点状态
        /// </summary>
        public const string WCPCmd = "WCP";
        /// <summary>
        /// 5. 按字单位读取触点状态
        /// </summary>
        public const string RCCCmd = "RCC";
        /// <summary>
        /// 6. 按字单位写入触点状态
        /// </summary>
        public const string WCCCmd = "WCC";
        /// <summary>
        /// 7.读取数据寄存器值 不支持索引
        /// </summary>
        public const string RDCmd = "RD";
        /// <summary>
        /// 8. 写入数据寄存器值
        /// </summary>
        public const string WDCmd = "WD";

        public const byte Xarea = 0;//外部输入
        public const byte Yarea = 1;//外部输出
        public const byte Rarea = 2;//内部继电器
        public const byte Tarea = 3;//定时器
        public const byte Carea = 4;//计数器 不支持
        public const byte Larea = 5;//链接继电器 不支持

        public const byte DTarea = 6;//数据寄存器 DT
        public const byte LDarea = 7;//链接寄存器 LD 不支持
        public const byte FLarea = 8;//文件寄存器 FL 不支持
        public const byte SVarea = 9;//目标值 SV 
        public const byte EVarea = 10;//经过值 EV
        public const byte IXarea = 11;//索引寄存器 IX
        public const byte IYarea = 12;//索引寄存器 IY
        public const byte WXarea = 13;//字单位外部输入 WX
        public const byte WYarea = 14;//字单位外部输出 WY
        public const byte WRarea = 15;//字单位内部继电器 WR
        public const byte WLarea = 16;//字单位链接继电器 WL 不支持

    }
}

using System.Collections.Generic;

namespace DataService
{
    public abstract class IModbusReader: IPLCDevice
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

        public int PDU
        {
            get { return 0xFF; }
        }

        public int GetBlockSize(int area)
        {
            return area > 2 ? PDU / 2 : PDU;
        }

        public DeviceAddress GetDeviceAddress(string address)
        {
            DeviceAddress dv = DeviceAddress.Empty;
            if (address == null || address.Length < 3)
                return dv;
            else
            {
                int index = address.IndexOf('.');
                switch (address[0])
                {
                    case '0':
                        dv.Area = fctReadCoil;
                        if (index > 0)
                        {
                            dv.Start = 64 * int.Parse(address.Substring(1, index - 1));
                            dv.Bit = byte.Parse(address.Substring(index + 1));
                        }
                        break;
                    case '1':
                        dv.Area = fctReadDiscreteInputs;
                        if (index > 0)
                        {
                            dv.Start = 64 * int.Parse(address.Substring(1, index - 1));
                            dv.Bit = byte.Parse(address.Substring(index + 1));
                        }
                        break;
                    case '4':
                        dv.Area = fctReadHoldingRegister;
                        if (index > 0)
                        {
                            dv.Start = int.Parse(address.Substring(1, index - 1));
                            dv.Bit = byte.Parse(address.Substring(index + 1));
                        }
                        else
                            dv.Start = int.Parse(address.Mid(1));
                        break;
                    case '3':
                        dv.Area = fctReadInputRegister;
                        if (index > 0)
                        {
                            dv.Start = int.Parse(address.Substring(1, index - 1));
                            dv.Bit = byte.Parse(address.Substring(index + 1));
                        }
                        else
                            dv.Start = int.Parse(address.Mid(1));
                        break;
                }
            }
            return dv;
        }

        public string GetAddress(DeviceAddress address)
        {
            return string.Empty;
        }


        short _id;
        public short ID
        {
            get
            {
                return _id;
            }
        }

        string _server;
        public string ServerName
        {
            get { return _server; }
            set { _server = value; }
        }

        public abstract DeviceDriver Driver
        {
            get ;
        }

        public abstract bool IsClosed
        {
            get;
        }

        private int _timeout;
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

        IDataServer _parent;
        public IDataServer Parent
        {
            get { return _parent; }
        }

        public abstract bool Connect();

        public IGroup AddGroup(string name, short id, int updateRate, float deadBand = 0f, bool active = false)
        {
            ModbusGroup grp = new ModbusGroup(id, name, updateRate, active, this);
            _grps.Add(grp);
            return grp;
        }

        public bool RemoveGroup(IGroup grp)
        {
            grp.IsActive = false;
            return _grps.Remove(grp);
        }

        public event ShutdownRequestEventHandler OnClose;

        public abstract void Dispose();

        public abstract byte[] ReadBytes(DeviceAddress address, ushort size);

        public abstract ItemData<int> ReadInt32(DeviceAddress address);

        public abstract ItemData<short> ReadInt16(DeviceAddress address);

        public abstract ItemData<byte> ReadByte(DeviceAddress address);

        public abstract ItemData<string> ReadString(DeviceAddress address, ushort size);

        public abstract ItemData<float> ReadFloat(DeviceAddress address);

        public abstract ItemData<bool> ReadBit(DeviceAddress address);

        public abstract ItemData<object> ReadValue(DeviceAddress address);

        public abstract int WriteBytes(DeviceAddress address, byte[] bit);

        public abstract int WriteBit(DeviceAddress address, bool bit);

        public abstract int WriteBits(DeviceAddress address, byte bits);

        public abstract int WriteInt16(DeviceAddress address, short value);

        public abstract int WriteInt32(DeviceAddress address, int value);

        public abstract int WriteFloat(DeviceAddress address, float value);

        public abstract int WriteString(DeviceAddress address, string str);

        public abstract int WriteValue(DeviceAddress address, object value);
    }
}

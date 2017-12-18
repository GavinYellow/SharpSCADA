using System;

namespace DataService
{
    public abstract class ITag : IComparable<ITag>
    {
        protected short _id;
        public short ID
        {
            get
            {
                return _id;
            }
        }

        protected bool _active = true;
        public bool Active
        {
            get
            {
                return _active;
            }
            set
            {
                _group.SetActiveState(value, _id);
                _active = value;
            }
        }

        protected QUALITIES _quality;
        public QUALITIES Quality
        {
            get
            {
                return _quality;
            }
        }

        protected Storage _value;
        public Storage Value
        {
            get
            {
                return _value;
            }
        }

        protected DateTime _timeStamp = DateTime.MinValue;
        public DateTime TimeStamp
        {
            get
            {
                return _timeStamp;
            }
        }

        protected DeviceAddress _plcAddress;
        public DeviceAddress Address
        {
            get
            {
                return _plcAddress;
            }
            set
            {
                _plcAddress = value;
            }
        }

        protected IGroup _group;
        public IGroup Parent
        {
            get
            {
                return _group;
            }
        }

        protected ITag(short id, DeviceAddress address, IGroup group)
        {
            _id = id;
            _group = group;
            _plcAddress = address;
        }

        public void Update(Storage newvalue, DateTime timeStamp, QUALITIES quality)
        {
            if (_timeStamp > timeStamp) return;//如果时间戳更旧或值未改变
            if (ValueChanging != null)
            {
                ValueChanging(this, new ValueChangingEventArgs<Storage>(quality, _value, newvalue, _timeStamp, timeStamp));
            }
            _timeStamp = timeStamp;
            _quality = quality;
            if (quality == QUALITIES.QUALITY_GOOD)
            {
                _value = newvalue;
                if (ValueChanged != null)
                {
                    ValueChanged(this, new ValueChangedEventArgs(_value));
                }
            }
        }

        public abstract bool Refresh(DataSource source = DataSource.Device);

        public abstract Storage Read(DataSource source = DataSource.Cache);

        protected abstract int InnerWrite(Storage value);

        public abstract int Write(object value);

        public int Write(Storage value, bool bForce)
        {
            DateTime time = DateTime.Now;
            _timeStamp = time;
            if (bForce)
            {
                if (ValueChanging != null)
                {
                    ValueChanging(this, new ValueChangingEventArgs<Storage>(QUALITIES.QUALITY_GOOD, _value, value, _timeStamp, time));
                }
            }
            int result = InnerWrite(value);
            if (bForce || result != 0)
            {
                var data = Read(DataSource.Device);
                if (data != value)
                {
                    time = DateTime.Now;
                    if (ValueChanging != null)
                    {
                        ValueChanging(this, new ValueChangingEventArgs<Storage>(QUALITIES.QUALITY_GOOD, _value, data, _timeStamp, time));
                    }
                    _value = data;
                    _timeStamp = time;
                    return result;
                }
            }
            return 0;
        }

        public ValueChangingEventHandler<Storage> ValueChanging;

        public ValueChangedEventHandler ValueChanged;

        #region IComparable<PLCAddress> Members

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public int CompareTo(ITag other)
        {
            return _plcAddress.CompareTo(other._plcAddress);
        }

        #endregion
    }

    public sealed class BoolTag : ITag
    {
        public BoolTag(short id, DeviceAddress addr, IGroup group)
            : base(id, addr, group)
        {
        }

        #region IDevice Members

        public override bool Refresh(DataSource source = DataSource.Cache)
        {
            var _newvalue = _group.ReadBool(_plcAddress, source);
            if (_newvalue.Value != _value.Boolean)
            {
                Storage value = Storage.Empty;
                value.Boolean = _newvalue.Value;
                DateTime time = _newvalue.TimeStamp.ToDateTime();
                if (ValueChanging != null)
                {
                    ValueChanging(this, new ValueChangingEventArgs<Storage>(_newvalue.Quality, _value, value, _timeStamp, time));
                }
                _timeStamp = time;
                _quality = _newvalue.Quality;
                if (_quality == QUALITIES.QUALITY_GOOD)
                {
                    _value = value;
                    if (ValueChanged != null)
                    {
                        ValueChanged(this, new ValueChangedEventArgs(value));
                    }
                }
                return true;
            }
            return false;
        }

        public override Storage Read(DataSource source = DataSource.Cache)
        {
            Storage value = Storage.Empty;
            value.Boolean = _group.ReadBool(_plcAddress, source).Value;
            return value;
        }

        public override int Write(object value)
        {
            if (value == null) return -1;
            bool temp = _value.Boolean;
            var str = value as string;
            if (str == null)
                temp = Convert.ToBoolean(value);
            else if (!Boolean.TryParse(str, out temp))
                return -1;
            _timeStamp = DateTime.Now;
            return _group.WriteBit(_plcAddress, temp);
        }

        protected override int InnerWrite(Storage value)
        {
            return _group.WriteBit(_plcAddress, value.Boolean);
        }

        #endregion

        public override string ToString()
        {
            return _value.Boolean.ToString();
        }
    }

    public sealed class ByteTag : ITag
    {

        public ByteTag(short id, DeviceAddress addr, IGroup group)
            : base(id, addr, group)
        {
        }


        #region IDevice Members
        public override bool Refresh(DataSource source = DataSource.Device)
        {
            var _newvalue = _group.ReadByte(_plcAddress, source);
            if (_newvalue.Value != _value.Byte)
            {
                Storage value = Storage.Empty;
                value.Byte = _newvalue.Value;
                DateTime time = _newvalue.TimeStamp.ToDateTime();
                if (ValueChanging != null)
                {
                    ValueChanging(this, new ValueChangingEventArgs<Storage>(_newvalue.Quality, _value, value, _timeStamp, time));
                }
                _timeStamp = time;
                _quality = _newvalue.Quality;
                if (_quality == QUALITIES.QUALITY_GOOD)
                {
                    _value = value;
                    if (ValueChanged != null)
                    {
                        ValueChanged(this, new ValueChangedEventArgs(value));
                    }
                }
                return true;
            }
            return false;
        }

        public override Storage Read(DataSource source = DataSource.Cache)
        {
            Storage value = Storage.Empty;
            value.Byte = _group.ReadByte(_plcAddress, source).Value;
            return value;
        }

        public override int Write(object value)
        {
            byte temp = _value.Byte;
            var str = value as string;
            if (str == null)
                temp = Convert.ToByte(value);
            else if (!Byte.TryParse(str, out temp))
                return -1;
            _timeStamp = DateTime.Now;
            return _group.WriteBits(_plcAddress, temp);
        }

        protected override int InnerWrite(Storage value)
        {
            return _group.WriteBits(_plcAddress, value.Byte);
        }

        #endregion

        public override string ToString()
        {
            return _value.Byte.ToString();
        }
    }

    public sealed class ShortTag : ITag
    {

        public ShortTag(short id, DeviceAddress addr, IGroup group)
            : base(id, addr, group)
        {
        }


        #region IDevice Members
        public override bool Refresh(DataSource source = DataSource.Device)
        {
            var _newvalue = _group.ReadInt16(_plcAddress, source);
            if (_newvalue.Value != _value.Int16)
            {
                Storage value = Storage.Empty;
                value.Int16 = _newvalue.Value;
                DateTime time = _newvalue.TimeStamp.ToDateTime();
                if (ValueChanging != null)
                {
                    ValueChanging(this, new ValueChangingEventArgs<Storage>(_newvalue.Quality, _value, value, _timeStamp, time));
                }
                _timeStamp = time;
                _quality = _newvalue.Quality;
                if (_quality == QUALITIES.QUALITY_GOOD)
                {
                    _value = value;
                    if (ValueChanged != null)
                    {
                        ValueChanged(this, new ValueChangedEventArgs(value));
                    }
                }
                return true;
            }
            return false;
        }

        public override Storage Read(DataSource source = DataSource.Cache)
        {
            Storage value = Storage.Empty;
            value.Int16 = _group.ReadInt16(_plcAddress, source).Value;
            return value;
        }

        public override int Write(object value)
        {
            var temp = _value.Int16;
            var str = value as string;
            if (str == null)
                temp = Convert.ToInt16(value);
            else if (!short.TryParse(str, out temp))
                return -1;
            _timeStamp = DateTime.Now;
            return _group.WriteInt16(_plcAddress, temp);
        }

        protected override int InnerWrite(Storage value)
        {
            return _group.WriteInt16(_plcAddress, value.Int16);
        }

        #endregion

        public override string ToString()
        {
            return _value.Int16.ToString();
        }
    }

    public sealed class UShortTag : ITag
    {

        public UShortTag(short id, DeviceAddress addr, IGroup group)
            : base(id, addr, group)
        {
        }


        #region IDevice Members
        public override bool Refresh(DataSource source = DataSource.Device)
        {
            var _newvalue = _group.ReadUInt16(_plcAddress, source);
            if (_newvalue.Value != _value.Word)
            {
                Storage value = Storage.Empty;
                value.Word = _newvalue.Value;
                DateTime time = _newvalue.TimeStamp.ToDateTime();
                if (ValueChanging != null)
                {
                    ValueChanging(this, new ValueChangingEventArgs<Storage>(_newvalue.Quality, _value, value, _timeStamp, time));
                }
                _timeStamp = time;
                _quality = _newvalue.Quality;
                if (_quality == QUALITIES.QUALITY_GOOD)
                {
                    _value = value;
                    if (ValueChanged != null)
                    {
                        ValueChanged(this, new ValueChangedEventArgs(value));
                    }
                }
                return true;
            }
            return false;
        }

        public override Storage Read(DataSource source = DataSource.Cache)
        {
            Storage value = Storage.Empty;
            value.Word = _group.ReadUInt16(_plcAddress, source).Value;
            return value;
        }

        public override int Write(object value)
        {
            var temp = _value.Word;
            var str = value as string;
            if (str == null)
                temp = Convert.ToUInt16(value);
            else if (!ushort.TryParse(str, out temp))
                return -1;
            _timeStamp = DateTime.Now;
            return _group.WriteUInt16(_plcAddress, temp);
        }

        protected override int InnerWrite(Storage value)
        {
            return _group.WriteUInt16(_plcAddress, value.Word);
        }

        #endregion

        public override string ToString()
        {
            return _value.Word.ToString();
        }
    }

    public sealed class IntTag : ITag
    {

        public IntTag(short id, DeviceAddress addr, IGroup group)
            : base(id, addr, group)
        {
        }

        #region IDevice Members
        public override bool Refresh(DataSource source = DataSource.Device)
        {
            var _newvalue = _group.ReadInt32(_plcAddress, source);
            if (_newvalue.Value != _value.Int32)
            {
                Storage value = Storage.Empty;
                value.Int32 = _newvalue.Value;
                DateTime time = _newvalue.TimeStamp.ToDateTime();
                if (ValueChanging != null)
                {
                    ValueChanging(this, new ValueChangingEventArgs<Storage>(_newvalue.Quality, _value, value, _timeStamp, time));
                }
                _timeStamp = time;
                _quality = _newvalue.Quality;
                if (_quality == QUALITIES.QUALITY_GOOD)
                {
                    _value = value;
                    if (ValueChanged != null)
                    {
                        ValueChanged(this, new ValueChangedEventArgs(value));
                    }
                }
                return true;
            }
            return false;
        }

        public override Storage Read(DataSource source = DataSource.Cache)
        {
            Storage value = Storage.Empty;
            value.Int32 = _group.ReadInt32(_plcAddress, source).Value;
            return value;
        }

        public override int Write(object value)
        {
            var temp = _value.Int32;
            var str = value as string;
            if (str == null)
                temp = Convert.ToInt32(value);
            else if (!int.TryParse(str, out temp))
                return -1;
            _timeStamp = DateTime.Now;
            return _group.WriteInt32(_plcAddress, temp);
        }

        protected override int InnerWrite(Storage value)
        {
            return _group.WriteInt32(_plcAddress, value.Int32);
        }

        #endregion

        public override string ToString()
        {
            return _value.Int32.ToString();
        }
    }

    public sealed class UIntTag : ITag
    {

        public UIntTag(short id, DeviceAddress addr, IGroup group)
            : base(id, addr, group)
        {
        }

        #region IDevice Members
        public override bool Refresh(DataSource source = DataSource.Device)
        {
            var _newvalue = _group.ReadUInt32(_plcAddress, source);
            if (_newvalue.Value != _value.DWord)
            {
                Storage value = Storage.Empty;
                value.DWord = _newvalue.Value;
                DateTime time = _newvalue.TimeStamp.ToDateTime();
                if (ValueChanging != null)
                {
                    ValueChanging(this, new ValueChangingEventArgs<Storage>(_newvalue.Quality, _value, value, _timeStamp, time));
                }
                _timeStamp = time;
                _quality = _newvalue.Quality;
                if (_quality == QUALITIES.QUALITY_GOOD)
                {
                    _value = value;
                    if (ValueChanged != null)
                    {
                        ValueChanged(this, new ValueChangedEventArgs(value));
                    }
                }
                return true;
            }
            return false;
        }

        public override Storage Read(DataSource source = DataSource.Cache)
        {
            Storage value = Storage.Empty;
            value.DWord = _group.ReadUInt32(_plcAddress, source).Value;
            return value;
        }

        public override int Write(object value)
        {
            var temp = _value.DWord;
            var str = value as string;
            if (str == null)
                temp = Convert.ToUInt32(value);
            else if (!uint.TryParse(str, out temp))
                return -1;
            _timeStamp = DateTime.Now;
            return _group.WriteUInt32(_plcAddress, temp);
        }

        protected override int InnerWrite(Storage value)
        {
            return _group.WriteUInt32(_plcAddress, value.DWord);
        }

        #endregion

        public override string ToString()
        {
            return _value.DWord.ToString();
        }
    }

    public sealed class FloatTag : ITag
    {

        public FloatTag(short id, DeviceAddress addr, IGroup group)
            : base(id, addr, group)
        {
        }


        #region IDevice Members
        public override bool Refresh(DataSource source = DataSource.Device)
        {
            var _newvalue = _group.ReadFloat(_plcAddress, source);
            if (_newvalue.Value != _value.Single)
            {
                Storage value = Storage.Empty;
                value.Single = _newvalue.Value;
                DateTime time = _newvalue.TimeStamp.ToDateTime();
                if (ValueChanging != null)
                {
                    ValueChanging(this, new ValueChangingEventArgs<Storage>(_newvalue.Quality, _value, value, _timeStamp, time));
                }
                _timeStamp = time;
                _quality = _newvalue.Quality;
                if (_quality == QUALITIES.QUALITY_GOOD)
                {
                    _value = value;
                    if (ValueChanged != null)
                    {
                        ValueChanged(this, new ValueChangedEventArgs(value));
                    }
                }
                return true;
            }
            return false;
        }

        public override Storage Read(DataSource source = DataSource.Cache)
        {
            Storage value = Storage.Empty;
            value.Single = _group.ReadFloat(_plcAddress, source).Value;
            return value;
        }

        public override int Write(object value)
        {
            var temp = _value.Single;
            var str = value as string;
            if (str == null)
                temp = Convert.ToSingle(value);
            else if (!float.TryParse(str, out temp))
                return -1;
            _timeStamp = DateTime.Now;
            return _group.WriteFloat(_plcAddress, temp);
        }

        protected override int InnerWrite(Storage value)
        {
            return _group.WriteFloat(_plcAddress, value.Single);
        }

        #endregion

        public override string ToString()
        {
            return _value.Single.ToString();
        }
    }

    public sealed class StringTag : ITag
    {
        string _str;
        public string String
        {
            get
            {
                return _str;
            }
            set
            {
                _str = value;
            }
        }

        public StringTag(short id, DeviceAddress addr, IGroup group)
            : base(id, addr, group)
        {
        }


        #region IDevice Members
        public override bool Refresh(DataSource source = DataSource.Device)
        {
            var _newvalue = _group.ReadString(_plcAddress, source);
            if (_newvalue.Value != _str)
            {
                DateTime time = _newvalue.TimeStamp.ToDateTime();
                if (ValueChanging != null)
                {
                    ValueChanging(this, new ValueChangingEventArgs<Storage>(_newvalue.Quality, _value, _value, _timeStamp, time));
                }
                _timeStamp = time;
                _quality = _newvalue.Quality;
                if (_quality == QUALITIES.QUALITY_GOOD)
                {
                    _str = _newvalue.Value;
                    if (ValueChanged != null)
                    {
                        ValueChanged(this, new ValueChangedEventArgs(_value));
                    }
                }
                return true;
            }
            return false;
        }

        public override Storage Read(DataSource source = DataSource.Cache)
        {
            var value = _group.ReadString(_plcAddress, source);
            if (value.Quality == QUALITIES.QUALITY_GOOD)
                _str = value.Value;
            return Storage.Empty;
        }

        public override int Write(object value)
        {
            if (value == null) return -1;
            var str = (value is String) ? (String)value : value.ToString();
            _timeStamp = DateTime.Now;
            return _group.WriteString(_plcAddress, str);
        }

        protected override int InnerWrite(Storage value)
        {
            return _group.WriteString(_plcAddress, _str);
        }

        #endregion

        public override string ToString()
        {
            return _str ?? string.Empty;
        }
    }

    public delegate void ValueChangingEventHandler<T>(object sender, ValueChangingEventArgs<T> e);
    public delegate void ValueChangedEventHandler(object sender, ValueChangedEventArgs e);

    public class ValueChangingEventArgs<T> : EventArgs
    {
        public ValueChangingEventArgs(QUALITIES quality, T oldValue, T newValue, DateTime oldTime, DateTime newTime)
        {
            this.Quality = quality;
            this.OldValue = oldValue;
            this.NewValue = newValue;
            this.OldTimeStamp = oldTime;
            this.NewTimeStamp = newTime;
        }

        public QUALITIES Quality;
        public T OldValue;
        public T NewValue;
        public DateTime OldTimeStamp;
        public DateTime NewTimeStamp;
    }

    public class ValueChangedEventArgs : EventArgs
    {
        public ValueChangedEventArgs(Storage value)
        {
            this.Value = value;
        }

        public Storage Value;
    }

}

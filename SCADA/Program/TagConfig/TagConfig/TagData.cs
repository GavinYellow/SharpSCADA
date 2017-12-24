using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Reflection;
using System.ComponentModel;

namespace TagConfig
{
    public class TagData : IComparable<TagData>
    {
        bool _active, _alarm, _scale, _archive;
        byte _type;
        short _id, _groupId;
        ushort _size;
        float _max, _min;
        int _cycle;
        string _addr, _name, _desp;
        object _obj;

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

        public short ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        public short GroupID
        {
            get
            {
                return _groupId;
            }
            set
            {
                _groupId = value;
            }
        }

        public string Address
        {
            get
            {
                return _addr;
            }
            set
            {
                _addr = value;
            }
        }

        public byte DataType
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }

        public ushort Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
            }
        }

        public bool Active
        {
            get
            {
                return _active;
            }
            set
            {
                _active = value;
            }
        }

        public bool HasAlarm
        {
            get
            {
                return _alarm;
            }
            set
            {
                _alarm = value;
            }
        }

        public bool HasScale
        {
            get
            {
                return _scale;
            }
            set
            {
                _scale = value;
            }
        }

        public bool Archive
        {
            get
            {
                return _archive;
            }
            set
            {
                _archive = value;
            }
        }

        public float Maximum
        {
            get
            {
                return _max;
            }
            set
            {
                _max = value;
            }
        }

        public float Minimum
        {
            get
            {
                return _min;
            }
            set
            {
                _min = value;
            }
        }

        public int Cycle
        {
            get
            {
                return _cycle;
            }
            set
            {
                _cycle = value;
            }
        }

        public string Description
        {
            get
            {
                return _desp;
            }
            set
            {
                _desp = value;
            }
        }

        public object DefaultValue
        {
            get
            {
                return _obj;
            }
            set
            {
                _obj = value;
            }
        }

        public TagData(short id, short grpId, string name, string address, byte type, ushort size, bool active, bool alarm, bool scale, bool archive,
            object defaultV, string desp, float max, float min, int cycle)
        {
            _id = id;
            _groupId = grpId;
            _name = name;
            _addr = address;
            _desp = desp;
            _type = type;
            _size = size;
            _active = active;
            _alarm = alarm;
            _archive = archive;
            _max = max;
            _min = min;
            _scale = scale;
            _cycle = cycle;
            _obj = defaultV;
        }

        public TagData(short grpId, string name)
        {
            _groupId = grpId;
            _name = name;
        }

        public TagData()
        {
        }

        public int CompareTo(TagData other)
        {
            //return this._groupId.CompareTo(other._groupId);
            int cmp = this._groupId.CompareTo(other._groupId);
            return cmp == 0 ? this._name.CompareTo(other._name) : cmp;
        }
    }

    public class SortableBindingList<T> : BindingList<T>
    {
        private bool isSortedCore = true;
        private ListSortDirection sortDirectionCore = ListSortDirection.Ascending;
        private PropertyDescriptor sortPropertyCore = null;
        private string defaultSortItem;

        public SortableBindingList() : base() { }

        public SortableBindingList(IList<T> list) : base(list) { }

        protected override bool SupportsSortingCore
        {
            get { return true; }
        }

        protected override bool SupportsSearchingCore
        {
            get { return true; }
        }

        protected override bool IsSortedCore
        {
            get { return isSortedCore; }
        }

        protected override ListSortDirection SortDirectionCore
        {
            get { return sortDirectionCore; }
        }

        protected override PropertyDescriptor SortPropertyCore
        {
            get { return sortPropertyCore; }
        }

        protected override int FindCore(PropertyDescriptor prop, object key)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (Equals(prop.GetValue(this[i]), key)) return i;
            }
            return -1;
        }

        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            isSortedCore = true;
            sortPropertyCore = prop;
            sortDirectionCore = direction;
            Sort();
        }

        protected override void RemoveSortCore()
        {
            if (isSortedCore)
            {
                isSortedCore = false;
                sortPropertyCore = null;
                sortDirectionCore = ListSortDirection.Ascending;
                Sort();
            }
        }

        public string DefaultSortItem
        {
            get { return defaultSortItem; }
            set
            {
                if (defaultSortItem != value)
                {
                    defaultSortItem = value;
                    Sort();
                }
            }
        }

        private void Sort()
        {
            List<T> list = (this.Items as List<T>);
            list.Sort(CompareCore);
            ResetBindings();
        }

        private int CompareCore(T o1, T o2)
        {
            int ret = 0;
            if (SortPropertyCore != null)
            {
                ret = CompareValue(SortPropertyCore.GetValue(o1), SortPropertyCore.GetValue(o2), SortPropertyCore.PropertyType);
            }
            if (ret == 0 && DefaultSortItem != null)
            {
                PropertyInfo property = typeof(T).GetProperty(DefaultSortItem, BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.IgnoreCase, null, null, new Type[0], null);
                if (property != null)
                {
                    ret = CompareValue(property.GetValue(o1, null), property.GetValue(o2, null), property.PropertyType);
                }
            }
            if (SortDirectionCore == ListSortDirection.Descending) ret = -ret;
            return ret;
        }

        private static int CompareValue(object o1, object o2, Type type)
        {

            if (o1 == null) return o2 == null ? 0 : -1;
            else if (o2 == null) return 1;
            else if (type.IsPrimitive || type.IsEnum) return Convert.ToDouble(o1).CompareTo(Convert.ToDouble(o2));
            else if (type == typeof(DateTime)) return Convert.ToDateTime(o1).CompareTo(o2);
            else return String.Compare(o1.ToString().Trim(), o2.ToString().Trim());
        }
    }


    public class Driver
    {
        short _id;
        int _driver;
        string _name;
        object _target;

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

        public short ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        public int DeviceDriver
        {
            get
            {
                return _driver;
            }
            set
            {
                _driver = value;
            }
        }

        public object Target
        {
            get
            {
                return _target;
            }
            set
            {
                _target = value;
            }
        }

        public Driver(short id, int driver, string name)
        {
            _id = id;
            _driver = driver;
            _name = name;
        }

        public Driver()
        {

        }
    }

    public class Register
    {
        bool _enable;
        string _assembly, _className,_classFullName,_description;

        public bool Enable
        {
            get
            {
                return _enable;
            }
            set
            {
                _enable = value;
            }
        }

        public string AssemblyPath
        {
            get
            {
                return _assembly;
            }
            set
            {
                _assembly = value;
            }
        }

        public string ClassName
        {
            get
            {
                return _className;
            }
            set
            {
                _className = value;
            }
        }

        public string ClassFullName
        {
            get
            {
                return _classFullName;
            }
            set
            {
                _classFullName = value;
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
            }
        }

        public Register(string assembly, string className, string classFull, string description)
        {
            _assembly = assembly;
            _className = className;
            _classFullName = classFull;
            _description = description;
        }
    }

    public class Group
    {
        bool _active;
        short _id, _deviceId;
        int _updateRate;
        float _deadBand;
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

        public short ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        public int UpdateRate
        {
            get
            {
                return _updateRate;
            }
            set
            {
                _updateRate = value;
            }
        }

        public float DeadBand
        {
            get
            {
                return _deadBand;
            }
            set
            {
                _deadBand = value;
            }
        }

        public bool Active
        {
            get
            {
                return _active;
            }
            set
            {
                _active = value;
            }
        }

        public short DriverID
        {
            get
            {
                return _deviceId;
            }
            set
            {
                _deviceId = value;
            }
        }

        public Group(short id, short deviceId, string name, int updateRate, float deadBand, bool active)
        {
            _id = id;
            _deviceId = deviceId;
            _name = name;
            _updateRate = updateRate;
            _deadBand = deadBand;
            _active = active;
        }

        public Group()
        {
        }
    }


    public class TagDataReader : IDataReader
    {
        IEnumerator<TagData> _enumer;

        public TagDataReader(IEnumerable<TagData> list)
        {
            this._enumer = list.GetEnumerator();
        }

        #region IDataReader Members

        public void Close()
        {

        }

        public int Depth
        {
            get { return 0; }
        }

        public DataTable GetSchemaTable()
        {
            DataTable table = new DataTable("Meta_Tag");
            table.Columns.Add("TagID", typeof(short));
            table.Columns.Add("TagName", typeof(string));
            table.Columns.Add("DataType", typeof(byte));
            table.Columns.Add("DataSize", typeof(short));
            table.Columns.Add("Address", typeof(string));
            table.Columns.Add("GroupID", typeof(short));
            table.Columns.Add("IsActive", typeof(bool));
            table.Columns.Add("Archive", typeof(bool));
            table.Columns.Add("DefaultValue", typeof(object));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("Maximum", typeof(float));
            table.Columns.Add("Minimum", typeof(float));
            table.Columns.Add("Cycle", typeof(int));
            return table;
        }
        public bool IsClosed
        {
            get { return false; }
        }

        public bool NextResult()
        {
            return false;
        }

        public bool Read()
        {
            return _enumer.MoveNext();
        }

        public int RecordsAffected
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        #region IDataRecord Members

        public int FieldCount
        {
            get { return 13; }
        }

        public bool GetBoolean(int i)
        {
            return (bool)GetValue(i);
        }

        public byte GetByte(int i)
        {
            return (byte)GetValue(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            return (char)GetValue(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            return this;
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            return (DateTime)GetValue(i);
        }

        public decimal GetDecimal(int i)
        {
            return (decimal)GetValue(i);
        }

        public double GetDouble(int i)
        {
            return (double)GetValue(i);
        }

        public Type GetFieldType(int i)
        {
            switch (i)
            {
                case 0:
                    return typeof(short);
                case 1:
                    return typeof(string);
                case 2:
                    return typeof(byte);
                case 3:
                    return typeof(short);
                case 4:
                    return typeof(string);
                case 5:
                    return typeof(short);
                case 6:
                    return typeof(bool);
                case 7:
                    return typeof(bool);
                case 8:
                    return typeof(object);
                case 9:
                    return typeof(string);
                case 10:
                    return typeof(float);
                case 11:
                    return typeof(float);
                case 12:
                    return typeof(int);
                default:
                    return typeof(int);
            }
        }

        public float GetFloat(int i)
        {
            return Convert.ToSingle(GetValue(i));
        }

        public Guid GetGuid(int i)
        {
            return (Guid)GetValue(i);
        }

        public short GetInt16(int i)
        {
            return (short)GetValue(i);
        }
        public int GetInt32(int i)
        {
            return (int)GetValue(i);
        }

        public long GetInt64(int i)
        {
            return (long)GetValue(i);
        }

        public string GetName(int i)
        {
            switch (i)
            {
                case 0:
                    return "TagID";
                case 1:
                    return "TagName";
                case 2:
                    return "DataType";
                case 3:
                    return "DataSize";
                case 4:
                    return "Address";
                case 5:
                    return "GroupID";
                case 6:
                    return "IsActive";
                case 7:
                    return "Archive";
                case 8:
                    return "DefaultValue";
                case 9:
                    return "Description";
                case 10:
                    return "Maximum";
                case 11:
                    return "Minimum";
                case 12:
                    return "Cycle";
                default:
                    return string.Empty;
            }
        }

        public int GetOrdinal(string name)
        {
            switch (name)
            {
                case "TagID":
                    return 0;
                case "TagName":
                    return 1;
                case "DataType":
                    return 2;
                case "DataSize":
                    return 3;
                case "Address":
                    return 4;
                case "GroupID":
                    return 5;
                case "IsActive":
                    return 6;
                case "Archive":
                    return 7;
                case "DefaultValue":
                    return 8;
                case "Description":
                    return 9;
                case "Maximum":
                    return 10;
                case "Minimum":
                    return 11;
                case "Cycle":
                    return 12;
                default:
                    return -1;
            }
        }

        public string GetString(int i)
        {
            return (string)GetValue(i);
        }

        public object GetValue(int i)
        {
            switch (i)
            {
                case 0:
                    return _enumer.Current.ID;
                case 1:
                    return _enumer.Current.Name;
                case 2:
                    return _enumer.Current.DataType;
                case 3:
                    return _enumer.Current.Size;
                case 4:
                    return _enumer.Current.Address;
                case 5:
                    return _enumer.Current.GroupID;
                case 6:
                    return _enumer.Current.Active;
                case 7:
                    return _enumer.Current.Archive;
                case 8:
                    return _enumer.Current.DefaultValue;
                case 9:
                    return _enumer.Current.Description;
                case 10:
                    return _enumer.Current.Maximum;
                case 11:
                    return _enumer.Current.Minimum;
                case 12:
                    return _enumer.Current.Cycle;
                default:
                    return null;
            }
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            switch (i)
            {
                case 1:
                    return string.IsNullOrEmpty(_enumer.Current.Name);
                case 4:
                    return string.IsNullOrEmpty(_enumer.Current.Address);
                case 8:
                    return _enumer.Current.DefaultValue == null;
                case 9:
                    return string.IsNullOrEmpty(_enumer.Current.Description);
                default:
                    return false;
            }
        }

        public object this[string name]
        {
            get
            {
                return GetValue(GetOrdinal(name));
            }
        }

        public object this[int i]
        {
            get
            {
                return GetValue(i);
            }
        }

        #endregion
    }

    public class ConditionReader : IDataReader
    {
        IEnumerator<Condition> _enumer;

        public ConditionReader(IEnumerable<Condition> list)
        {
            this._enumer = list.GetEnumerator();
        }

        #region IDataReader Members

        public void Close()
        {

        }

        public int Depth
        {
            get { return 0; }
        }

        public DataTable GetSchemaTable()
        {
            DataTable table = new DataTable("Meta_Condition");
            table.Columns.Add("TypeID", typeof(int));
            table.Columns.Add("Source", typeof(string));
            table.Columns.Add("AlarmType", typeof(int));
            table.Columns.Add("EventType", typeof(byte));
            table.Columns.Add("ConditionType", typeof(byte));
            table.Columns.Add("Para", typeof(float));
            table.Columns.Add("IsEnabled", typeof(bool));
            table.Columns.Add("DeadBand", typeof(float));
            table.Columns.Add("Delay", typeof(int));
            table.Columns.Add("Comment", typeof(string));
            return table;
        }
        public bool IsClosed
        {
            get { return false; }
        }

        public bool NextResult()
        {
            return false;
        }

        public bool Read()
        {
            return _enumer.MoveNext();
        }

        public int RecordsAffected
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        #region IDataRecord Members

        public int FieldCount
        {
            get { return 10; }
        }

        public bool GetBoolean(int i)
        {
            return (bool)GetValue(i);
        }

        public byte GetByte(int i)
        {
            return (byte)GetValue(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            return (char)GetValue(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            return this;
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            return (DateTime)GetValue(i);
        }

        public decimal GetDecimal(int i)
        {
            return (decimal)GetValue(i);
        }

        public double GetDouble(int i)
        {
            return (double)GetValue(i);
        }

        public Type GetFieldType(int i)
        {
            switch (i)
            {
                case 0:
                    return typeof(int);
                case 1:
                    return typeof(string);
                case 2:
                    return typeof(int);
                case 3:
                    return typeof(byte);
                case 4:
                    return typeof(byte);
                case 5:
                    return typeof(float);
                case 6:
                    return typeof(bool);
                case 7:
                    return typeof(float);
                case 8:
                    return typeof(int);
                case 9:
                    return typeof(string);
                default:
                    return typeof(int);
            }
        }

        public float GetFloat(int i)
        {
            return Convert.ToSingle(GetValue(i));
        }

        public Guid GetGuid(int i)
        {
            return (Guid)GetValue(i);
        }

        public short GetInt16(int i)
        {
            return (short)GetValue(i);
        }
        public int GetInt32(int i)
        {
            return (int)GetValue(i);
        }

        public long GetInt64(int i)
        {
            return (long)GetValue(i);
        }

        public string GetName(int i)
        {
            switch (i)
            {
                case 0:
                    return "TypeID";
                case 1:
                    return "Source";
                case 2:
                    return "AlarmType";
                case 3:
                    return "EventType";
                case 4:
                    return "ConditionType";
                case 5:
                    return "Para";
                case 6:
                    return "IsEnabled";
                case 7:
                    return "DeadBand";
                case 8:
                    return "Delay";
                case 9:
                    return "Comment";
                default:
                    return string.Empty;
            }
        }

        public int GetOrdinal(string name)
        {
            switch (name)
            {
                case "TypeID":
                    return 0;
                case "Source":
                    return 1;
                case "AlarmType":
                    return 2;
                case "EventType":
                    return 3;
                case "ConditionType":
                    return 4;
                case "Para":
                    return 5;
                case "IsEnabled":
                    return 6;
                case "DeadBand":
                    return 7;
                case "Delay":
                    return 8;
                case "Comment":
                    return 9;
                default:
                    return -1;
            }
        }

        public string GetString(int i)
        {
            return (string)GetValue(i);
        }

        public object GetValue(int i)
        {
            switch (i)
            {
                case 0:
                    return _enumer.Current.TypeId;
                case 1:
                    return _enumer.Current.Source;
                case 2:
                    return _enumer.Current.AlarmType;
                case 3:
                    return _enumer.Current.EventType;
                case 4:
                    return _enumer.Current.ConditionType;
                case 5:
                    return _enumer.Current.Para;
                case 6:
                    return _enumer.Current.IsEnabled;
                case 7:
                    return _enumer.Current.DeadBand;
                case 8:
                    return _enumer.Current.Delay;
                case 9:
                    return _enumer.Current.Comment;
                default:
                    return null;
            }
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            switch (i)
            {
                case 1:
                    return string.IsNullOrEmpty(_enumer.Current.Source);
                case 9:
                    return string.IsNullOrEmpty(_enumer.Current.Comment);
                default:
                    return false;
            }
        }

        public object this[string name]
        {
            get
            {
                return GetValue(GetOrdinal(name));
            }
        }

        public object this[int i]
        {
            get
            {
                return GetValue(i);
            }
        }

        #endregion
    }

    public class SubConditionReader : IDataReader
    {
        IEnumerator<SubCondition> _enumer;

        public SubConditionReader(IEnumerable<SubCondition> list)
        {
            this._enumer = list.GetEnumerator();
        }

        #region IDataReader Members

        public void Close()
        {

        }

        public int Depth
        {
            get { return 0; }
        }

        public DataTable GetSchemaTable()
        {
            DataTable table = new DataTable("Meta_SubCondition");
            //table.Columns.Add("ID", typeof(int));
            table.Columns.Add("ConditionID", typeof(int));
            table.Columns.Add("SubAlarmType", typeof(int));
            table.Columns.Add("Threshold", typeof(float));
            table.Columns.Add("Severity", typeof(byte));
            table.Columns.Add("Message", typeof(string));
            table.Columns.Add("IsEnable", typeof(bool));
            return table;
        }
        public bool IsClosed
        {
            get { return false; }
        }

        public bool NextResult()
        {
            return false;
        }

        public bool Read()
        {
            return _enumer.MoveNext();
        }

        public int RecordsAffected
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        #region IDataRecord Members

        public int FieldCount
        {
            get { return 6; }
        }

        public bool GetBoolean(int i)
        {
            return (bool)GetValue(i);
        }

        public byte GetByte(int i)
        {
            return (byte)GetValue(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            return (char)GetValue(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            return this;
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            return (DateTime)GetValue(i);
        }

        public decimal GetDecimal(int i)
        {
            return (decimal)GetValue(i);
        }

        public double GetDouble(int i)
        {
            return (double)GetValue(i);
        }

        public Type GetFieldType(int i)
        {
            switch (i)
            {
                case 0:
                    return typeof(int);
                case 1:
                    return typeof(int);
                case 2:
                    return typeof(float);
                case 3:
                    return typeof(byte);
                case 4:
                    return typeof(string);
                case 5:
                    return typeof(bool);
                default:
                    return typeof(int);
            }
        }

        public float GetFloat(int i)
        {
            return Convert.ToSingle(GetValue(i));
        }

        public Guid GetGuid(int i)
        {
            return (Guid)GetValue(i);
        }

        public short GetInt16(int i)
        {
            return (short)GetValue(i);
        }
        public int GetInt32(int i)
        {
            return (int)GetValue(i);
        }

        public long GetInt64(int i)
        {
            return (long)GetValue(i);
        }

        public string GetName(int i)
        {
            switch (i)
            {
                case 0:
                    return "ConditionID";
                case 1:
                    return "SubAlarmType";
                case 2:
                    return "Threshold";
                case 3:
                    return "Severity";
                case 4:
                    return "Message";
                case 5:
                    return "IsEnable";
                default:
                    return string.Empty;
            }
        }

        public int GetOrdinal(string name)
        {
            switch (name)
            {
                case "ConditionID":
                    return 0;
                case "SubAlarmType":
                    return 1;
                case "Threshold":
                    return 2;
                case "Severity":
                    return 3;
                case "Message":
                    return 4;
                case "IsEnable":
                    return 5;
                default:
                    return -1;
            }
        }

        public string GetString(int i)
        {
            return (string)GetValue(i);
        }

        public object GetValue(int i)
        {
            switch (i)
            {
                case 0:
                    return _enumer.Current.ConditionId;
                case 1:
                    return _enumer.Current.SubAlarmType;
                case 2:
                    return _enumer.Current.Threshold;
                case 3:
                    return _enumer.Current.Severity;
                case 4:
                    return _enumer.Current.Message;
                case 5:
                    return _enumer.Current.IsEnabled;
                default:
                    return null;
            }
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            switch (i)
            {
                case 4:
                    return string.IsNullOrEmpty(_enumer.Current.Message);
                default:
                    return false;
            }
        }

        public object this[string name]
        {
            get
            {
                return GetValue(GetOrdinal(name));
            }
        }

        public object this[int i]
        {
            get
            {
                return GetValue(i);
            }
        }

        #endregion
    }

    public class ScaleReader : IDataReader
    {
        IEnumerator<Scaling> _enumer;

        public ScaleReader(IEnumerable<Scaling> list)
        {
            this._enumer = list.GetEnumerator();
        }

        #region IDataReader Members

        public void Close()
        {

        }

        public int Depth
        {
            get { return 0; }
        }

        public DataTable GetSchemaTable()
        {
            DataTable table = new DataTable("Meta_Scale");
            table.Columns.Add("ScaleID", typeof(short));
            table.Columns.Add("ScaleType", typeof(byte));
            table.Columns.Add("EUHI", typeof(float));
            table.Columns.Add("EULO", typeof(float));
            table.Columns.Add("RAWHI", typeof(float));
            table.Columns.Add("RAWLO", typeof(float));
            return table;
        }
        public bool IsClosed
        {
            get { return false; }
        }

        public bool NextResult()
        {
            return false;
        }

        public bool Read()
        {
            return _enumer.MoveNext();
        }

        public int RecordsAffected
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        #region IDataRecord Members

        public int FieldCount
        {
            get { return 6; }
        }

        public bool GetBoolean(int i)
        {
            return (bool)GetValue(i);
        }

        public byte GetByte(int i)
        {
            return (byte)GetValue(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            return (char)GetValue(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            return this;
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            return (DateTime)GetValue(i);
        }

        public decimal GetDecimal(int i)
        {
            return (decimal)GetValue(i);
        }

        public double GetDouble(int i)
        {
            return (double)GetValue(i);
        }

        public Type GetFieldType(int i)
        {
            switch (i)
            {
                case 0:
                    return typeof(short);
                case 1:
                    return typeof(byte);
                case 2:
                    return typeof(float);
                case 3:
                    return typeof(float);
                case 4:
                    return typeof(float);
                case 5:
                    return typeof(float);
                default:
                    return typeof(int);
            }
        }

        public float GetFloat(int i)
        {
            return Convert.ToSingle(GetValue(i));
        }

        public Guid GetGuid(int i)
        {
            return (Guid)GetValue(i);
        }

        public short GetInt16(int i)
        {
            return (short)GetValue(i);
        }
        public int GetInt32(int i)
        {
            return (int)GetValue(i);
        }

        public long GetInt64(int i)
        {
            return (long)GetValue(i);
        }

        public string GetName(int i)
        {
            switch (i)
            {
                case 0:
                    return "ScaleID";
                case 1:
                    return "ScaleType";
                case 2:
                    return "EUHI";
                case 3:
                    return "EULO";
                case 4:
                    return "RAWHI";
                case 5:
                    return "RAWLO";
                default:
                    return string.Empty;
            }
        }

        public int GetOrdinal(string name)
        {
            switch (name)
            {
                case "ScaleID":
                    return 0;
                case "ScaleType":
                    return 1;
                case "EUHI":
                    return 2;
                case "EULO":
                    return 3;
                case "RAWHI":
                    return 4;
                case "RAWLO":
                    return 5;
                default:
                    return -1;
            }
        }

        public string GetString(int i)
        {
            return (string)GetValue(i);
        }

        public object GetValue(int i)
        {
            switch (i)
            {
                case 0:
                    return _enumer.Current.ID;
                case 1:
                    return _enumer.Current.ScaleType;
                case 2:
                    return _enumer.Current.EUHi;
                case 3:
                    return _enumer.Current.EULo;
                case 4:
                    return _enumer.Current.RawHi;
                case 5:
                    return _enumer.Current.RawLo;
                default:
                    return null;
            }
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            return false;
        }

        public object this[string name]
        {
            get
            {
                return GetValue(GetOrdinal(name));
            }
        }

        public object this[int i]
        {
            get
            {
                return GetValue(i);
            }
        }

        #endregion
    }

    public class ArgumentReader : IDataReader
    {
        IEnumerator<DriverArgumet> _enumer;

        public ArgumentReader(IEnumerable<DriverArgumet> list)
        {
            this._enumer = list.GetEnumerator();
        }

        #region IDataReader Members

        public void Close()
        {

        }

        public int Depth
        {
            get { return 0; }
        }

        public DataTable GetSchemaTable()
        {
            DataTable table = new DataTable("Argument");
            table.Columns.Add("DriverID", typeof(short));
            table.Columns.Add("PropertyName", typeof(string));
            table.Columns.Add("PropertyValue", typeof(string));
            return table;
        }
        public bool IsClosed
        {
            get { return false; }
        }

        public bool NextResult()
        {
            return false;
        }

        public bool Read()
        {
            return _enumer.MoveNext();
        }

        public int RecordsAffected
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        #region IDataRecord Members

        public int FieldCount
        {
            get { return 3; }
        }

        public bool GetBoolean(int i)
        {
            return (bool)GetValue(i);
        }

        public byte GetByte(int i)
        {
            return (byte)GetValue(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            return (char)GetValue(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            return this;
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            return (DateTime)GetValue(i);
        }

        public decimal GetDecimal(int i)
        {
            return (decimal)GetValue(i);
        }

        public double GetDouble(int i)
        {
            return (double)GetValue(i);
        }

        public Type GetFieldType(int i)
        {
            switch (i)
            {
                case 0:
                    return typeof(short);
                case 1:
                    return typeof(string);
                case 2:
                    return typeof(string);
                default:
                    return typeof(string);
            }
        }

        public float GetFloat(int i)
        {
            return Convert.ToSingle(GetValue(i));
        }

        public Guid GetGuid(int i)
        {
            return (Guid)GetValue(i);
        }

        public short GetInt16(int i)
        {
            return (short)GetValue(i);
        }
        public int GetInt32(int i)
        {
            return (int)GetValue(i);
        }

        public long GetInt64(int i)
        {
            return (long)GetValue(i);
        }

        public string GetName(int i)
        {
            switch (i)
            {
                case 0:
                    return "DriverID";
                case 1:
                    return "PropertyName";
                case 2:
                    return "PropertyValue";
                default:
                    return string.Empty;
            }
        }

        public int GetOrdinal(string name)
        {
            switch (name)
            {
                case "DriverID":
                    return 0;
                case "PropertyName":
                    return 1;
                case "PropertyValue":
                    return 2;
                default:
                    return -1;
            }
        }

        public string GetString(int i)
        {
            return (string)GetValue(i);
        }

        public object GetValue(int i)
        {
            switch (i)
            {
                case 0:
                    return _enumer.Current.DriverID;
                case 1:
                    return _enumer.Current.PropertyName;
                case 2:
                    return _enumer.Current.PropertyValue;
                default:
                    return null;
            }
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            switch (i)
            {
                case 2:
                    return _enumer.Current.PropertyValue == null;
                default:
                    return false;
            }
        }

        public object this[string name]
        {
            get
            {
                return GetValue(GetOrdinal(name));
            }
        }

        public object this[int i]
        {
            get
            {
                return GetValue(i);
            }
        }

        #endregion
    }
}

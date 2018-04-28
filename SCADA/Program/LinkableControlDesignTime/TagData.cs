using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reflection;

namespace HMIControl.VisualStudio.Design
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

    public class DataTypeSource
    {
        byte _type;
        public byte DataType { get { return _type; } set { _type = value; } }

        string _name;
        public string Name { get { return _name; } set { _name = value; } }

        public DataTypeSource(byte type, string name)
        {
            _type = type;
            _name = name;
        }
    }

    public enum DataType : byte
    {
        NONE = 0,
        BOOL = 1,
        BYTE = 3,
        SHORT = 4,
        WORD = 5,
        DWORD = 6,
        INT = 7,
        FLOAT = 8,
        SYS = 9,
        STR = 11
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
                    return _enumer.Current.Address ?? "";
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
}

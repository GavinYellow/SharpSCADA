using DatabaseLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Windows.Forms;

namespace HMIControl.VisualStudio.Design
{
    public partial class TagList : Form
    {
        static List<string> list;
        static List<TagMetaData> metaList;

        public static readonly List<DataTypeSource> DataDict = new List<DataTypeSource>
        {
           new DataTypeSource (DataType.BOOL,"数字量"),new DataTypeSource (DataType.BYTE,"字节"), new DataTypeSource (DataType.SHORT,"短整型"),
           new DataTypeSource (DataType.WORD,"单字型"),new DataTypeSource (DataType.TIME,"时间型"),new DataTypeSource (DataType.INT,"双字型"),
           new DataTypeSource (DataType.FLOAT,"模拟量"),new DataTypeSource (DataType.SYS,"系统型"),new DataTypeSource (DataType.STR,"ASCII字符串"),
           new DataTypeSource (DataType.NONE,"UNICODE字符串")
        };

        public TagList(string txt = null)
        {
            InitializeComponent();
            GetTagNameList();
            bindingSource1.DataSource = new SortableBindingList<TagMetaData>(metaList);
            if (!string.IsNullOrEmpty(txt) && list != null)
            {
                tspText.Text = txt;
                int index = bindingSource1.Find("Name", txt);
                bindingSource1.Position = index;
            }
        }

        string currenText;
        public string CurrentText
        {
            get { return currenText; }
        }

        public static List<string> GetTagNameList()
        {
            if (list == null)
            {
                list = new List<string> { "@Time", "@Date", "@DateTime", "@User", "@AppName", "@LocName", "@Region", "@Path" };
                metaList = new List<TagMetaData>();
                using (var reader = DataHelper.Instance.ExecuteReader("SELECT ISNULL(TagName,''),ISNULL(ADDRESS,''),ISNULL(DESCRIPTION,''),DATATYPE,DATASIZE,TAGID,GROUPID,ISACTIVE,ARCHIVE,DEFAULTVALUE FROM Meta_Tag"))
                {
                    if (reader == null) return list;
                    while (reader.Read())
                    {
                        string name = reader.GetString(0);
                        list.Add(name);
                        metaList.Add(new TagMetaData(name.ToUpper(), reader.GetString(1), reader.GetString(2), (DataType)reader.GetByte(3), (ushort)reader.GetInt16(4),
                            reader.GetInt16(5), reader.GetInt16(6), reader.GetBoolean(7), reader.GetBoolean(8), reader.GetSqlValue(9)));
                    }
                }
                list.Sort();
            }
            //if (list != null)
            //    list.Sort();
            return list;
        }

        private void TagList_Load(object sender, EventArgs e)
        {
            cboType.DataSource = DataDict;
            cboType.DisplayMember = "Name";
            cboType.ValueMember = "DataType";
            txtName.DataBindings.Add("Text", bindingSource1, "Name", true);
            txtAddr.DataBindings.Add("Text", bindingSource1, "Address", true);
            txtDesp.DataBindings.Add("Text", bindingSource1, "Description", true);
            txtSize.DataBindings.Add("Text", bindingSource1, "Size", true);
            txtValue.DataBindings.Add("Text", bindingSource1, "DefaultValue", true);
            cboType.DataBindings.Add("SelectedValue", bindingSource1, "DataType", true);
            chkActive.DataBindings.Add("Checked", bindingSource1, "Active", true);
            chkAchive.DataBindings.Add("Checked", bindingSource1, "Archive", true);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            dataGridView1.CurrentCell = null;
            bindingSource1.EndEdit();
            DataHelper.Instance.BulkCopy(new TagDataReader(metaList), "Meta_Tag", "DELETE FROM Meta_Tag;", SqlBulkCopyOptions.KeepIdentity);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            int index = bindingSource1.Find("Name", tspText.Text);
            bindingSource1.Position = index;
        }

        private void dataGridView1_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            currenText = Convert.ToString(dataGridView1[0, e.RowIndex].Value);
            this.Close();
        }
    }

    public class TagMetaData
    {
        short _id, _groupId;
        ushort _size;
        bool _active, _archive;
        DataType _type;
        string _addr, _name, _desp;
        object _obj;

        public short ID
        {
            get
            {
                return _id;
            }
        }

        public short GroupID
        {
            get
            {
                return _groupId;
            }
        }

        public DataType DataType
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

        public TagMetaData(string name, string address, string desp,
            DataType type, ushort size, short id, short group, bool active, bool archive, object obj)
        {
            _name = name;
            _addr = address;
            _desp = desp;
            _type = type;
            _size = size;
            _id = id;
            _groupId = group;
            _active = active;
            _archive = archive;
            _obj = obj;
        }

    }

    public struct DataTypeSource
    {
        DataType _type;
        public DataType DataType { get { return _type; } set { _type = value; } }

        string _name;
        public string Name { get { return _name; } set { _name = value; } }

        public DataTypeSource(DataType type, string name)
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
        TIME = 6,
        INT = 7,
        FLOAT = 8,
        SYS = 9,
        STR = 11
    }

    public class TagDataReader : IDataReader
    {
        IEnumerator<TagMetaData> _enumer;

        public TagDataReader(IEnumerable<TagMetaData> list)
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
                case 0:
                    return _enumer.Current.ID == 0;
                case 1:
                    return string.IsNullOrEmpty(_enumer.Current.Name);
                case 2:
                    return _enumer.Current.DataType == 0;
                case 3:
                    return _enumer.Current.Size == 0;
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
}

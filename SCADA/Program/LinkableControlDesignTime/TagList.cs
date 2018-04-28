using DatabaseLib;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace HMIControl.VisualStudio.Design
{
    public partial class TagList : Form
    {
        static List<string> list;
        static List<TagData> metaList = new List<TagData>();
        AutoCompleteStringCollection scAutoComplete = new AutoCompleteStringCollection();

        public static readonly List<DataTypeSource> DataDict = new List<DataTypeSource>
        {
           new DataTypeSource (1,"开关型"),new DataTypeSource (3,"字节"), new DataTypeSource (4,"短整型"),
           new DataTypeSource (5,"单字型"),new DataTypeSource (6,"双字型"),new DataTypeSource (7,"长整型"),
           new DataTypeSource (8,"浮点型"),new DataTypeSource (9,"系统型"),new DataTypeSource (10,"ASCII字符串"),
           new DataTypeSource (11,"UNICODE字符串"),new DataTypeSource(0,"")
        };

        public TagList(string txt = null)
        {
            InitializeComponent();
            GetTagNameList();
            bindingSource1.DataSource = new SortableBindingList<TagData>(metaList);
            if (!string.IsNullOrEmpty(txt) && list != null)
            {
                tspText.Text = txt;
                int index = bindingSource1.Find("Name", txt);
                bindingSource1.Position = index;
            }
            scAutoComplete.AddRange(list.ToArray());
            tspText.AutoCompleteCustomSource = scAutoComplete;
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
                metaList.Clear();
                var sql = "SELECT TagID,GroupID,TagName,Address,DataType,DataSize,IsActive,"
      + "(SELECT COUNT(1) FROM Meta_Condition WHERE Source=t.TagName) HasAlarm," +
      "(SELECT COUNT(1) FROM Meta_Scale WHERE ScaleID=t.TagID) HasScale,"
      + "Archive,DefaultValue,Description,Maximum,Minimum,Cycle FROM Meta_Tag t WHERE DataType<12";
                using (var reader = DataHelper.Instance.ExecuteReader(sql))
                {
                    if (reader == null) return list;
                    while (reader.Read())
                    {
                        TagData tag = new TagData(reader.GetInt16(0), reader.GetInt16(1), reader.GetString(2), reader.GetString(3), reader.GetByte(4),
                            (ushort)reader.GetInt16(5), reader.GetBoolean(6), reader.GetInt32(7) > 0, reader.GetInt32(8) > 0, reader.GetBoolean(9),
                            reader.GetValue(10), reader.GetNullableString(11), reader.GetFloat(12), reader.GetFloat(13), reader.GetInt32(14));
                        list.Add(tag.Name);
                        metaList.Add(tag);
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
            if (DataHelper.Instance.BulkCopy(new TagDataReader(metaList), "Meta_Tag", "DELETE FROM Meta_Tag;", SqlBulkCopyOptions.KeepIdentity))
                MessageBox.Show("保存成功");
            else MessageBox.Show("保存失败");
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

}

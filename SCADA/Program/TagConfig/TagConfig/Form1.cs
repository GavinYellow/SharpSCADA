using DatabaseLib;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Excel = Microsoft.Office.Interop.Excel;

namespace TagConfig
{
    //可考虑支持EXCEL文件的导入导出
    public partial class Form1 : Form
    {
        const string FILENAME = "meta.xml";

        bool start = false;
        string file = null;
        TreeNode majorTop;
        bool isCut = false;
        short curgroupId = 0;
        List<Driver> devices = new List<Driver>();
        List<Group> groups = new List<Group>();
        List<TagData> list = new List<TagData>();
        //List<short> indexList = new List<short>();
        List<Scaling> scaleList = new List<Scaling>();
        List<Condition> conditions = new List<Condition>();
        List<SubCondition> subConds = new List<SubCondition>();
        List<TagData> selectedTags = new List<TagData>();
        List<DataTypeSource1> typeList = new List<DataTypeSource1>();
        List<DriverArgumet> arguments = new List<DriverArgumet>();

        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

        public static readonly List<DataTypeSource> DataDict = new List<DataTypeSource>
        {
           new DataTypeSource (1,"开关型"),new DataTypeSource (3,"字节"), new DataTypeSource (4,"短整型"),
           new DataTypeSource (5,"单字型"),new DataTypeSource (6,"双字型"),new DataTypeSource (7,"长整型"),
           new DataTypeSource (8,"浮点型"),new DataTypeSource (9,"系统型"),new DataTypeSource (10,"ASCII字符串"),
           new DataTypeSource(0,"")
        };


        public Form1()
        {
            InitializeComponent();
            DataGridViewComboBoxColumn col = dataGridView1.Columns["Column3"] as DataGridViewComboBoxColumn;
            col.DataSource = DataDict;
            col.DisplayMember = "Name";
            col.ValueMember = "DataType";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            majorTop = treeView1.Nodes.Add("", "服务器", 0, 0);
            LoadFromDatabase();
            treeView1.ExpandAll();
        }

        private void LoadFromDatabase()
        {
            list.Clear();
            //subConds.Clear();
            majorTop.Nodes.Clear();
            string sql = "SELECT DriverID,DriverType,DriverName FROM META_DRIVER;";
            using (var reader = DataHelper.Instance.ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    Driver device = new Driver(reader.GetInt16(0), reader.GetInt32(1), reader.GetString(2));
                    devices.Add(device);
                    majorTop.Nodes.Add(device.ID.ToString(), device.Name, 1, 1);
                }
            }
            foreach (TreeNode node in majorTop.Nodes)
            {
                sql = string.Format("SELECT GroupID,DriverID,GroupName,UpdateRate,DeadBand,IsActive FROM META_GROUP WHERE DriverID={0};", node.Name);
                using (var reader = DataHelper.Instance.ExecuteReader(sql))
                {
                    while (reader.Read())
                    {
                        Group group = new Group(reader.GetInt16(0), reader.GetInt16(1), reader.GetString(2), reader.GetInt32(3), reader.GetFloat(4), reader.GetBoolean(5));
                        groups.Add(group);
                        node.Nodes.Add(group.ID.ToString(), group.Name, 2, 2);
                    }
                }
            }
            sql = "SELECT TagID,GroupID,TagName,Address,DataType,DataSize,IsActive,"
                + "(SELECT COUNT(1) FROM Meta_Condition WHERE Source=t.TagName) HasAlarm," +
                "(SELECT COUNT(1) FROM Meta_Scale WHERE ScaleID=t.TagID) HasScale,"
                + "Archive,DefaultValue,Description,Maximum,Minimum,Cycle FROM Meta_Tag t WHERE DataType<12";
            using (var reader = DataHelper.Instance.ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    TagData tag = new TagData(reader.GetInt16(0), reader.GetInt16(1), reader.GetString(2), reader.GetString(3), reader.GetByte(4),
                        (ushort)reader.GetInt16(5), reader.GetBoolean(6), reader.GetInt32(7) > 0, reader.GetInt32(8) > 0, reader.GetBoolean(9),
                        reader.GetValue(10), reader.GetNullableString(11), reader.GetFloat(12), reader.GetFloat(13), reader.GetInt32(14));
                    list.Add(tag);
                }
            }
            sql = "SELECT TypeID,Source,AlarmType,EventType,ConditionType,Para,IsEnabled,Deadband,Delay,Comment FROM Meta_Condition";
            using (var reader = DataHelper.Instance.ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    Condition cond = new Condition(reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2), reader.GetByte(3), reader.GetByte(4), reader.GetFloat(5),
                       reader.GetBoolean(6), reader.GetFloat(7), reader.GetInt32(8), reader.GetNullableString(9));
                    conditions.Add(cond);
                }
            }
            sql = "SELECT IsEnable,Severity,ConditionID,SubAlarmType,Threshold,Message FROM Meta_SubCondition";
            using (var reader = DataHelper.Instance.ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    SubCondition sub = new SubCondition(reader.GetBoolean(0), reader.GetByte(1), reader.GetInt32(2), reader.GetInt32(3),
                        reader.GetFloat(4), reader.IsDBNull(5) ? null : reader.GetString(5));
                    subConds.Add(sub);
                }
            }
            sql = "SELECT ScaleID,ScaleType,EUHI,EULO,RAWHI,RAWLO FROM Meta_Scale";
            using (var reader = DataHelper.Instance.ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    Scaling scale = new Scaling(reader.GetInt16(0), reader.GetByte(1), reader.GetFloat(2), reader.GetFloat(3),
                        reader.GetFloat(4), reader.GetFloat(5));
                    scaleList.Add(scale);
                }
            }
            sql = "SELECT DRIVERID,ISNULL(Description,CLASSNAME),AssemblyName,ClassFullName FROM RegisterModule";
            using (var reader = DataHelper.Instance.ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    typeList.Add(new DataTypeSource1(reader.GetInt32(0), reader.GetString(1), reader.GetNullableString(2), reader.GetNullableString(3)));
                }
            }
            sql = "SELECT DriverID,PropertyName,PropertyValue FROM Argument";
            using (var reader = DataHelper.Instance.ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    arguments.Add(new DriverArgumet(reader.GetInt16(0), reader.GetString(1), reader.GetNullableString(2)));
                }
            }
            list.Sort();
            //conditions.Sort();
            subConds.Sort();
            scaleList.Sort();
            foreach (Condition condition in conditions)
            {
                if (condition.EventType != 4) continue;
                int condId = condition.TypeId;
                int index = subConds.BinarySearch(new SubCondition(condId));
                if (index < 0) break;
                SubCondition sub = subConds[index];
                int ind1 = index - 1;
                while (sub.ConditionId == condId)
                {
                    condition.SubConditions.Add(sub);
                    if (++index < subConds.Count)
                    {
                        sub = subConds[index];
                    }
                    else
                        break;
                }
                while (ind1 >= 0)
                {
                    sub = subConds[ind1--];
                    if (sub.ConditionId == condId)
                        condition.SubConditions.Add(sub);
                }
            }
            var obj = DataHelper.Instance.ExecuteScalar("SELECT MAX(TypeID) FROM Meta_Condition");
            if (obj != DBNull.Value) Program.MAXCONDITIONID = Convert.ToInt32(obj);
            start = true;
        }

        private bool Save()
        {
            //dataGridView1.CurrentCell = null;
            //bindingSource1.EndEdit();
            bool result = true;
            string sql = "DELETE FROM Meta_Driver;DELETE FROM Meta_Group;";
            foreach (Driver device in devices)
            {
                sql = string.Concat(sql, string.Format("INSERT INTO Meta_Driver(DriverID,DriverName,DriverType)"
                + " VALUES({0},'{1}',{2});",
                    device.ID, device.Name, device.DeviceDriver));
                if (device.Target != null)
                {
                    for (int i = arguments.Count - 1; i >= 0; i--)
                    {
                        if (arguments[i].DriverID == device.ID)
                            arguments.RemoveAt(i);
                    }
                    var type = device.Target.GetType();
                    foreach (var prop in type.GetProperties())
                    {
                        if (prop.CanWrite)
                        {
                            var value = prop.GetValue(device.Target, null);
                            var item = new DriverArgumet(device.ID, prop.Name, value == null ? null : value.ToString());
                            arguments.Add(item);
                        }
                    }
                }
            }
            foreach (Group grp in groups)
            {
                sql = string.Concat(sql, string.Format("INSERT INTO Meta_Group(GroupID,GroupName,DriverID,UpdateRate,DeadBand,IsActive) VALUES({0},'{1}',{2},{3},{4},'{5}');",
                    grp.ID, grp.Name, grp.DriverID, grp.UpdateRate, grp.DeadBand, grp.Active));
            }
            TagDataReader reader = new TagDataReader(list);
            ConditionReader condReader = new ConditionReader(conditions);
            SubConditionReader subReader = new SubConditionReader(subConds);
            ScaleReader scalereader = new ScaleReader(scaleList);
            ArgumentReader argumentreader = new ArgumentReader(arguments);
            result &= DataHelper.Instance.ExecuteNonQuery(sql) >= 0;
            result &= DataHelper.Instance.BulkCopy(reader, "Meta_Tag", "DELETE FROM Meta_Tag", SqlBulkCopyOptions.KeepIdentity);
            result &= DataHelper.Instance.BulkCopy(condReader, "Meta_Condition", "DELETE FROM Meta_Condition", SqlBulkCopyOptions.KeepIdentity);
            result &= DataHelper.Instance.BulkCopy(subReader, "Meta_SubCondition", "DELETE FROM Meta_SubCondition", SqlBulkCopyOptions.KeepIdentity);
            result &= DataHelper.Instance.BulkCopy(scalereader, "Meta_Scale", "DELETE FROM Meta_Scale", SqlBulkCopyOptions.KeepIdentity);
            result &= DataHelper.Instance.BulkCopy(argumentreader, "Argument", "DELETE FROM Argument");
            return result;
        }

        private void LoadFromXml(string file)
        {
            list.Clear();
            majorTop.Nodes.Clear();

            using (XmlReader reader = XmlReader.Create(file))
            {
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Attribute:
                            break;
                        case XmlNodeType.Element:
                            //if (reader.IsEmptyElement) continue;
                            switch (reader.Name)
                            {
                                case "Sever":

                                    break;
                                case "Device":
                                    {
                                        Driver device = new Driver();
                                        if (reader.MoveToAttribute("id"))
                                        {
                                            device.ID = short.Parse(reader.Value);
                                        }
                                        if (reader.MoveToAttribute("name"))
                                        {
                                            device.Name = reader.Value;
                                        }
                                        devices.Add(device);
                                        majorTop.Nodes.Add(device.ID.ToString(), device.Name, 1, 1);
                                    }
                                    break;
                                case "Group":
                                    {
                                        Group grp = new Group();
                                        if (reader.MoveToAttribute("id"))
                                        {
                                            grp.ID = short.Parse(reader.Value);
                                        }
                                        if (reader.MoveToAttribute("name"))
                                        {
                                            grp.Name = reader.Value;
                                        }
                                        if (reader.MoveToAttribute("deviceId"))
                                        {
                                            grp.DriverID = short.Parse(reader.Value);
                                        }
                                        if (reader.MoveToAttribute("updateRate"))
                                        {
                                            grp.UpdateRate = int.Parse(reader.Value);
                                        }
                                        if (reader.MoveToAttribute("deadBand"))
                                        {
                                            grp.DeadBand = float.Parse(reader.Value);
                                        }
                                        if (reader.MoveToAttribute("active"))
                                        {
                                            grp.Active = bool.Parse(reader.Value);
                                        }
                                        groups.Add(grp);
                                        TreeNode[] nodes = majorTop.Nodes.Find(grp.DriverID.ToString(), true);
                                        if (nodes != null && nodes.Length > 0)
                                        {
                                            nodes[0].Nodes.Add(grp.ID.ToString(), grp.Name, 2, 2);
                                        }
                                    }
                                    break;
                                case "Tag":
                                    {
                                        TagData tag = new TagData(0, string.Empty);
                                        if (reader.MoveToAttribute("id"))
                                        {
                                            tag.ID = short.Parse(reader.Value);
                                        }
                                        if (reader.MoveToAttribute("groupid"))
                                        {
                                            tag.GroupID = short.Parse(reader.Value);
                                        }
                                        if (reader.MoveToAttribute("name"))
                                        {
                                            tag.Name = reader.Value;
                                        }
                                        if (reader.MoveToAttribute("address"))
                                        {
                                            tag.Address = reader.Value;
                                        }
                                        if (reader.MoveToAttribute("datatype"))
                                        {
                                            tag.DataType = byte.Parse(reader.Value);
                                        }
                                        if (reader.MoveToAttribute("size"))
                                        {
                                            tag.Size = ushort.Parse(reader.Value);
                                        }
                                        if (reader.MoveToAttribute("active"))
                                        {
                                            tag.Active = bool.Parse(reader.Value);
                                        }
                                        if (reader.MoveToAttribute("value"))
                                        {
                                            tag.DefaultValue = reader.Value;
                                        }
                                        if (reader.MoveToAttribute("desp"))
                                        {
                                            tag.Description = reader.Value;
                                        }
                                        list.Add(tag);
                                    }
                                    break;
                            }
                            break;
                        case XmlNodeType.Text:
                            break;
                    }
                }
            }
            list.Sort();
            start = true;
        }


        private void SaveToXml(string file)
        {
            using (var writer = XmlWriter.Create(file))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Sever");
                foreach (Driver device in devices)
                {
                    writer.WriteStartElement("Device");
                    writer.WriteAttributeString("id", device.ID.ToString());
                    writer.WriteAttributeString("name", device.Name);
                    foreach (Group grp in groups)
                    {
                        if (grp.DriverID != device.ID)
                            continue;
                        writer.WriteStartElement("Group");
                        writer.WriteAttributeString("id", grp.ID.ToString());
                        writer.WriteAttributeString("name", grp.Name);
                        writer.WriteAttributeString("deviceId", grp.DriverID.ToString());
                        writer.WriteAttributeString("updateRate", grp.UpdateRate.ToString());
                        writer.WriteAttributeString("deadBand", grp.DeadBand.ToString());
                        writer.WriteAttributeString("active", grp.Active.ToString());
                        short grpId = grp.ID;
                        int index = list.BinarySearch(new TagData(grpId, null));
                        if (index < 0) index = ~index;
                        if (index < list.Count)
                        {
                            TagData tag = list[index];
                            while (tag.GroupID == grpId)
                            {
                                writer.WriteStartElement("Tag");
                                writer.WriteAttributeString("id", tag.ID.ToString());
                                writer.WriteAttributeString("groupid", tag.GroupID.ToString());
                                writer.WriteAttributeString("name", tag.Name);
                                writer.WriteAttributeString("address", tag.Address);
                                writer.WriteAttributeString("datatype", tag.DataType.ToString());
                                writer.WriteAttributeString("size", tag.Size.ToString());
                                writer.WriteAttributeString("active", tag.Active.ToString());
                                if (tag.DefaultValue != null)
                                    writer.WriteAttributeString("value", tag.DefaultValue.ToString());
                                if (!string.IsNullOrEmpty(tag.Description))
                                    writer.WriteAttributeString("desp", tag.Description);
                                writer.WriteEndElement();
                                if (++index < list.Count)
                                    tag = list[index];
                                else
                                    break;
                            }

                        }
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
        }

        private void LoadFromCsv()
        {
            if (Clipboard.ContainsText(TextDataFormat.Text))
            {
                string data = Clipboard.GetText(TextDataFormat.Text);
                if (string.IsNullOrEmpty(data)) return;
                list.Clear();
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
                {
                    using (var mysr = new StreamReader(stream))
                    {
                        string strline = "";
                        while ((strline = mysr.ReadLine()) != null)
                        {
                            string[] aryline = strline.Split('\t');
                            try
                            {
                                var id = Convert.ToInt16(aryline[0]);
                                var groupid = Convert.ToInt16(aryline[1]);
                                var name = aryline[2];
                                var address = aryline[3];
                                var type = Convert.ToByte(aryline[4]);
                                var size = Convert.ToUInt16(aryline[5]);
                                var active = Convert.ToBoolean(aryline[6]);
                                var desp = aryline[7];
                                TagData tag = new TagData(id, groupid, name, address, type, size, active, false, false, false, null, desp, 0, 0, 0);
                                list.Add(tag);
                            }
                            catch (Exception err)
                            {
                                continue;
                            }
                        }
                    }
                }
                list.Sort();
                start = true;
            }
        }

        private void LoadFromExcel(string file)
        {
            Excel.Application app = new Excel.Application();
            Workbook book = app.Workbooks.Open(file);
            Worksheet sheet = (Worksheet)book.Sheets[1];
            //list.Clear();
            Dictionary<string, byte> dic = new Dictionary<string, byte>() { { "Bool", 1 }, { "SInt", 3 }, { "Word", 4 }, { "DInt", 7 }, { "Int", 4 }, { "Real", 8 }, { "String", 11 }, };
            short maxid = list.Count == 0 ? (short)1 : list.Max(x => x.ID);
            for (int i = 2; i < sheet.Rows.Count; i++)
            {
                if (((Range)sheet.Cells[i, 2]).Value2 == null)
                    break;
                try
                {
                    TagData tag = new TagData(++maxid, curgroupId, ((Range)sheet.Cells[i, 1]).Value2.ToString(), ((Range)sheet.Cells[i, 5]).Value2.ToString().TrimStart('%'),
                        dic[((Range)sheet.Cells[i, 3]).Value2.ToString()], Convert.ToUInt16(((Range)sheet.Cells[i, 4]).Value2),
                         true, false, false, false, null, Convert.ToString(((Range)sheet.Cells[i, 6]).Value2), 0, 0, 0);
                    list.Add(tag);
                    //bindingSource1.Add(tag);
                }
                catch (Exception e)
                {
                    continue;
                    //Program.AddErrorLog(e);
                }
            }
            list.Sort();
            //indexList.Sort();
            start = true;
        }

        private void LoadFromKepserverCSV(string file)
        {
            Excel.Application app = new Excel.Application();
            Workbook book = app.Workbooks.Open(file);
            Worksheet sheet = (Worksheet)book.Sheets[1];
            //list.Clear();
            Dictionary<string, byte> dic = new Dictionary<string, byte>() { { "Boolean", 1 }, { "Byte", 3 }, { "Short", 4 }, { "Word", 5 }, { "DWord", 6 }, { "Long ", 7 }, { "Float", 8 }, { "String", 11 }, };
            short maxid = list.Count == 0 ? (short)1 : list.Max(x => x.ID);
            for (int i = 2; i < sheet.Rows.Count; i++)
            {
                var name = ((Range)sheet.Cells[i, 1]).Value2;
                if (name == null)
                    break;
                try
                {
                    var type = dic[((Range)sheet.Cells[i, 3]).Value2.ToString()];
                    TagData tag = new TagData(++maxid, curgroupId, name, ((Range)sheet.Cells[i, 2]).Value2.ToString().TrimStart('%'),
                        type, (ushort)(type < 4 ? 1 : type < 6 ? 2 : type < 11 ? 4 : 255),
                         true, false, false, false, null, Convert.ToString(((Range)sheet.Cells[i, 16]).Value2), 0, 0, 0);
                    list.Add(tag);
                    //bindingSource1.Add(tag);
                }
                catch (Exception e)
                {
                    continue;
                    //Program.AddErrorLog(e);
                }
            }
            list.Sort();
            //indexList.Sort();
            start = true;
        }

        public static readonly Dictionary<string, int> severitys = new Dictionary<string, int>
        {
             {"Infomations",0},{"Messages",1},{"Warnings",1},{"LO",2},{"MidLO",3},{"Mid",4},{"MidHI",5},{"HI",6},{"Errors",7}
        };

        private void LoadAlarmFromExcel(string file)
        {
            Excel.Application app = new Excel.Application();
            Workbook book = app.Workbooks.Open(file);
            Worksheet sheet = (Worksheet)book.Sheets[1];
            short maxid = list.Max(x => x.ID);
            for (int i = 2; i < sheet.Rows.Count; i++)
            {
                if (((Range)sheet.Cells[i, 1]).Value2 == null)
                    break;
                try
                {
                    string name = ((Range)sheet.Cells[i, 2]).Value2.Trim('"');
                    string digit = "." + ((Range)sheet.Cells[i, 7]).Value2.ToString();
                    string name1 = "";
                    int index = list.BinarySearch(new TagData(curgroupId, name));
                    if (index < 0)
                    {
                        for (int j = 0; j < list.Count; j++)
                        {
                            if (list[j].GroupID == curgroupId && list[j].Name.Contains(((Range)sheet.Cells[i, 6]).Value2.Trim('"')))
                            {
                                index = j; name1 = name;
                                break;
                            }
                        }
                    }
                    else name1 = name + digit;
                    TagData _ptag = list[index];
                    if (!string.IsNullOrEmpty(name))
                    {
                        //var _tag = list.Find((x) => x.Name == name);
                        TagData _tag = null;
                        index = list.BinarySearch(new TagData(curgroupId, name1));
                        if (index >= 0) _tag = list[index];
                        if (_tag == null)
                        {
                            _tag = new TagData(++maxid, curgroupId, name1, _ptag.Address.ToUpper().Replace("DBW", "DBX").Replace("DBD", "DBX") + digit, 1, 1, true, false, false, false, null, "", 0, 0, 0);
                            list.Add(_tag);
                        }
                        var condition = new Condition(++Program.MAXCONDITIONID, name1, 4, 4, 0, 0, true, 0, 0);
                        var sub = new SubCondition(true, severitys[((Range)sheet.Cells[i, 5]).Value2.ToString()], condition.TypeId, 64, 1, ((Range)sheet.Cells[i, 3]).Value2.ToString());
                        condition.SubConditions.Add(sub);
                        conditions.Add(condition);
                        subConds.Add(sub);
                        _tag.HasAlarm = true;
                    }
                }
                catch (Exception e)
                {
                    continue;
                }
            }
            list.Sort();
        }

        private void SaveToCsv(string file)
        {
            using (StreamWriter objWriter = new StreamWriter(file, false))
            {
                foreach (TagData tag in list)
                {
                    objWriter.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8}", tag.ID, tag.GroupID, tag.Name, tag.Address, tag.DataType, tag.Size, tag.Active, tag.DefaultValue, tag.Description);
                }
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (!start) return;
            List<TagData> data = new List<TagData>();
            switch (e.Node.Level)
            {
                case 0:
                    data = list;
                    break;
                case 1:
                    {
                        foreach (TreeNode node in e.Node.Nodes)
                        {
                            curgroupId = short.Parse(node.Name);
                            int index = list.BinarySearch(new TagData(curgroupId, null));
                            if (index < 0) index = ~index;
                            if (index < list.Count)
                            {
                                TagData tag = list[index];
                                while (tag.GroupID == curgroupId)
                                {
                                    data.Add(tag);
                                    if (++index < list.Count)
                                        tag = list[index];
                                    else
                                        break;
                                }
                            }
                        }
                    }
                    break;
                case 2:
                    {
                        curgroupId = short.Parse(e.Node.Name);
                        int index = list.BinarySearch(new TagData(curgroupId, null));
                        if (index < 0) index = ~index;
                        if (index < list.Count)
                        {
                            TagData tag = list[index];
                            while (tag.GroupID == curgroupId)
                            {
                                data.Add(tag);
                                if (++index < list.Count)
                                    tag = list[index];
                                else
                                    break;
                            }

                        }
                    }
                    break;
            }
            bindingSource1.DataSource = new SortableBindingList<TagData>(data);
            tspCount.Text = data.Count.ToString();
        }

        public void FindNode(string filter)
        {
            foreach (TreeNode tr in treeView1.Nodes)
            {
                TreeFind(tr, filter);
            }
        }

        private void TreeFind(TreeNode node, string filter)
        {
            if (node.Text == filter)
            {
                treeView1.SelectedNode = node;
                return;
            }
            foreach (TreeNode tn in node.Nodes)
            {
                TreeFind(tn, filter);
            }
        }

        public void AddNode()
        {
            TreeNode node = treeView1.SelectedNode;
            if (node != null)
            {
                short did = 0;// short.MinValue;
                if (node.Level == 0)
                {
                    for (int i = 0; i < devices.Count; i++)
                    {
                        short temp = devices[i].ID;
                        if (temp > did)
                            did = temp;
                    }
                    did++;
                    devices.Add(new Driver { ID = did });
                }
                else if (node.Level == 1)
                {
                    for (int i = 0; i < groups.Count; i++)
                    {
                        short temp = groups[i].ID;
                        if (temp > did)
                            did = temp;
                    }
                    did++;
                    groups.Add(new Group { ID = did, DriverID = short.Parse(node.Name) });
                }
                else if (node.Level == 2)
                {
                    AddTag();
                    return;
                }
                TreeNode nwNode = node.Nodes.Add(did.ToString(), "", node.Level + 1, node.Level + 1);
                treeView1.SelectedNode = nwNode;
                treeView1.LabelEdit = true;
                nwNode.BeginEdit();
                //bindingSource1.Clear();
            }
        }

        public void UpdateNode()
        {
            TreeNode node = treeView1.SelectedNode;
            if (node != null && node.Level != 0)
            {
                treeView1.LabelEdit = true;
                node.BeginEdit();
            }
            else
                treeView1.LabelEdit = false;
        }

        public void RemoveNode()
        {
            TreeNode node = treeView1.SelectedNode;
            if (node != null && ((node.Level == 2 && bindingSource1.Count == 0)
                || (node.Level == 1 && node.Nodes.Count == 0)))
            {
                if (node.Level == 1)
                {
                    foreach (Driver device in devices)
                    {
                        if (device.ID.ToString() == node.Name)
                        {
                            foreach (Group grp in groups)
                            {
                                if (grp.DriverID.ToString() == node.Name)
                                {
                                    groups.Remove(grp);
                                    node.Remove();
                                    return;
                                }
                            }
                            devices.Remove(device);
                            node.Remove();
                            return;
                        }
                    }
                }
                else
                {
                    foreach (Group grp in groups)
                    {
                        if (grp.ID.ToString() == node.Name)
                        {
                            groups.Remove(grp);
                            node.Remove();
                            return;
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("包含下级，禁止删除!");
            }
        }

        private void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (!start) return;
            if (string.IsNullOrEmpty(e.Label))
            {
                e.CancelEdit = false;
            }
            else
            {
                treeView1.LabelEdit = false;
                if (e.Node.Level == 1)
                {
                    foreach (Driver device in devices)
                    {
                        if (device.ID.ToString() == e.Node.Name)
                        {
                            device.Name = e.Label;
                            break;
                        }
                    }
                }
                else
                {
                    if (!groups.Exists(x => x.Name == e.Label))
                    {
                        foreach (Group grp in groups)
                        {
                            if (grp.ID.ToString() == e.Node.Name)
                            {
                                grp.Name = e.Label;
                                break;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("组名不能重复!");
                    }
                }
            }
        }

        IEnumerable<int> GetTagNames(string filter)
        {
            if (bindingSource1.Count == 0) yield break;
            int index = -1;
            foreach (TagData tag in bindingSource1.DataSource as IEnumerable<TagData>)
            {
                index++;
                if (string.IsNullOrEmpty(tag.Name))
                    continue;
                else if (tag.Name.ToUpper().Contains(filter))
                {
                    yield return index;
                }
            }
        }

        IEnumerable<int> GetTags(string filter)
        {
            if (bindingSource1.Count == 0) yield break;
            int index = -1;
            foreach (TagData tag in bindingSource1.DataSource as IEnumerable<TagData>)
            {
                index++;
                if (string.IsNullOrEmpty(tag.Description))
                    continue;
                else if (tag.Description.Contains(filter))
                {
                    yield return index;
                }
            }
        }

        private void AddTag()
        {
            TagData tag = new TagData((short)(list.Count == 0 ? 1 : list.Max(x => x.ID) + 1), short.Parse(treeView1.SelectedNode.Name), "", "", 1, 1, true, false, false, false, null, "", 0, 0, 0);
            bindingSource1.Add(tag);
            int index = list.BinarySearch(tag);
            if (index < 0) index = ~index;
            list.Insert(index, tag);
            dataGridView1.FirstDisplayedScrollingRowIndex = bindingSource1.Count - 1;
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Text)
            {
                case "增加":
                    if (treeView1.SelectedNode != null && treeView1.SelectedNode.Level == 2)
                    {
                        AddTag();
                    }
                    break;
                case "删除":
                    {
                        TagData tag = bindingSource1.Current as TagData;
                        bindingSource1.Remove(tag);
                        list.Remove(tag);
                    }
                    break;
                case "清除":
                    if (MessageBox.Show("将清除所有的标签，是否确定？", "警告", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        bindingSource1.Clear();
                        list.Clear();
                    }
                    break;
                case "保存":
                    if (Save())
                        MessageBox.Show("保存成功!");
                    break;

                case "注册":
                    {
                        RegisterSet frm = new RegisterSet();
                        frm.ShowDialog();
                    }
                    break;
                case "查找":
                    {
                        string filter = txtFind.Text.ToUpper();
                        for (int i = 0; i < dataGridView1.Rows.Count; i++)
                        {
                            if (dataGridView1[0, i].Value.ToString() == filter)
                            {
                                dataGridView1.Rows[i].Selected = true;
                                dataGridView1.FirstDisplayedScrollingRowIndex = i;
                                return;
                            }
                        }
                    }
                    break;
                case "名称过滤":
                    {
                        dataGridView1.ClearSelection();
                        string filter = txtFilter.Text.ToUpper();
                        var tags = GetTagNames(filter);
                        if (tags != null)
                        {
                            foreach (int index in tags)
                            {
                                dataGridView1.Rows[index].Selected = true;
                                dataGridView1.FirstDisplayedScrollingRowIndex = index;
                            }
                        }
                    }
                    break;
                case "描述过滤":
                    {
                        dataGridView1.ClearSelection();
                        string filter = txtFilter1.Text.ToUpper();
                        var tags = GetTags(filter);
                        if (tags != null)
                        {
                            foreach (int index in tags)
                            {
                                dataGridView1.Rows[index].Selected = true;
                                dataGridView1.FirstDisplayedScrollingRowIndex = index;
                            }
                        }
                    }
                    break;
                case "退出":
                    this.Close();
                    break;
            }
            tspCount.Text = bindingSource1.Count.ToString();
        }

        private void toolStrip2_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Text)
            {
                case "导入变量":
                    openFileDialog1.Filter = "xml文件 (*.xml)|*.xml|excel文件 (*.xlsx)|*.xlsx|kepserver文件 (*.csv)|*.csv|All files (*.*)|*.*";
                    openFileDialog1.DefaultExt = "xml";
                    if (openFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        string file = openFileDialog1.FileName;
                        switch (openFileDialog1.FilterIndex)
                        {
                            case 1:
                                LoadFromXml(file);
                                break;
                            case 2:
                                LoadFromExcel(file);
                                break;
                            case 3:
                                LoadFromKepserverCSV(file);
                                break;
                        }
                    }
                    break;
                case "导出变量":
                    saveFileDialog1.Filter = "xml文件 (*.xml)|*.xml|csv文件 (*.csv)|*.csv|All files (*.*)|*.*";
                    saveFileDialog1.DefaultExt = "xml";
                    if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        string file = saveFileDialog1.FileName;
                        switch (saveFileDialog1.FilterIndex)
                        {
                            case 1:
                                SaveToXml(file);
                                break;
                            case 2:
                                SaveToCsv(file);
                                break;
                        }
                    }
                    break;
                case "导入报警":
                    {
                        openFileDialog1.Filter = "excel文件 (*.xlsx)|*.xlsx|All files (*.*)|*.*";
                        openFileDialog1.DefaultExt = "xlsx";
                        if (openFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            string file = openFileDialog1.FileName;
                            LoadAlarmFromExcel(file);
                        }
                    }
                    break;
            }
        }

        private void toolStrip3_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Text)
            {
                case "前缀":
                    {
                        string front = txtFront.Text;
                        if (treeView1.SelectedNode.Level == 0)
                        {
                            foreach (var item in list)
                            {
                                item.Name = front + item.Name;
                            }
                        }
                        else if (treeView1.SelectedNode.Level == 1)
                        {
                            var rows = dataGridView1.Rows;
                            if (rows != null)
                            {
                                for (int i = 0; i < rows.Count; i++)
                                {
                                    TagData tag = rows[i].DataBoundItem as TagData;
                                    tag.Name = front + tag.Name;
                                }
                            }
                        }
                        else if (treeView1.SelectedNode.Level == 2)
                        {
                            foreach (var item in list.FindAll(x => x.GroupID == curgroupId))
                            {
                                item.Name = front + item.Name;
                            }
                        }
                    }
                    break;
                case "替换地址":
                    {
                        string filter = txtReplaceAddr1.Text;
                        string replace = txtReplaceAddr2.Text;
                        foreach (var item in list.FindAll(x => x.GroupID == curgroupId))
                        {
                            item.Address = item.Address.Replace(filter, replace);
                        }
                    }
                    break;
                case "偏移":
                    {
                        string offset = tspOffset.Text;
                        int baseaddr;
                        if (int.TryParse(offset, out baseaddr))
                        {
                            foreach (DataGridViewRow item in dataGridView1.SelectedRows)
                            {
                                var tag = item.DataBoundItem as TagData;
                                var index = tag.Address.LastIndexOf('W');
                                if (index < 0)
                                {
                                    index = tag.Address.LastIndexOf('D');
                                }
                                if (index >= 0)
                                {
                                    var ad = Convert.ToDecimal(tag.Address.Substring(index + 1));
                                    ad = ad + baseaddr;
                                    tag.Address = tag.Address.Substring(0, index + 1) + ad.ToString();
                                }
                            }
                        }
                    }
                    break;
                case "替换名称":
                    {
                        string filter = txtReplace1.Text;
                        string replace = txtReplace2.Text;
                        foreach (var item in list.FindAll(x => x.GroupID == curgroupId))
                        {
                            item.Name = item.Name.Replace(filter, replace);
                        }
                    }
                    break;
            }
        }

        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Text)
            {
                case "增加":
                    AddNode();
                    break;
                case "删除":
                    RemoveNode();
                    break;
                case "重命名":
                    UpdateNode();
                    break;
                case "参数设置":
                    {
                        TreeNode node = treeView1.SelectedNode;
                        if (node != null)
                        {
                            if (node.Level == 1)
                            {
                                short id = short.Parse(node.Name);
                                foreach (Driver device in devices)
                                {
                                    if (device.ID == id)
                                    {
                                        DriverSet frm = new DriverSet(device, typeList, arguments);
                                        frm.ShowDialog();
                                        node.Text = device.Name;
                                        return;
                                    }
                                }
                            }
                            if (node.Level == 2)
                            {
                                short id = short.Parse(node.Name);
                                foreach (Group grp in groups)
                                {
                                    if (grp.ID == id)
                                    {
                                        GroupParam frm = new GroupParam(grp);
                                        frm.ShowDialog();
                                        node.Text = grp.Name;
                                        return;
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
        }

        private void contextMenuStrip2_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Text)
            {
                case "变量编辑":
                    {
                        if (bindingSource1.Count > 0)
                        {
                            TagParam frm = new TagParam(bindingSource1);
                            frm.ShowDialog();
                        }
                    }
                    break;
                case "报警":
                    {
                        var tag = bindingSource1.Current as TagData;
                        if (tag != null)
                        {
                            AlarmParam frm = new AlarmParam(conditions, subConds, tag);
                            frm.ShowDialog();
                            //dataGridView1.CurrentCell = null;
                            bindingSource1.EndEdit();
                        }
                    }
                    break;
                case "量程":
                    {
                        var tag = bindingSource1.Current as TagData;
                        if (tag != null)
                        {
                            ScaleParam frm = new ScaleParam(scaleList, tag);
                            frm.ShowDialog();
                            bindingSource1.EndEdit();
                        }
                    }
                    break;
                case "复制":
                    {
                        selectedTags.Clear();
                        var rows = dataGridView1.SelectedRows;
                        if (rows != null)
                        {
                            short maxid = list.Max(x => x.ID);
                            for (int i = 0; i < rows.Count; i++)
                            {
                                TagData tag = rows[i].DataBoundItem as TagData;
                                TagData newtag = new TagData((short)++maxid, tag.GroupID, tag.Name, tag.Address, tag.DataType,
                                    tag.Size, tag.Active, false, false, tag.Archive, tag.DefaultValue, tag.Description, tag.Maximum, tag.Minimum, tag.Cycle);
                                selectedTags.Add(newtag);
                            }
                        }
                        isCut = false;
                    }
                    break;
                case "剪切":
                    {
                        selectedTags.Clear();
                        var rows = dataGridView1.SelectedRows;
                        if (rows != null)
                        {
                            for (int i = 0; i < rows.Count; i++)
                            {
                                TagData tag = rows[i].DataBoundItem as TagData;
                                selectedTags.Add(tag);
                            }
                        }
                        isCut = true;
                    }
                    break;
                case "粘贴CSV":
                    LoadFromCsv();
                    break;
                case "粘帖":
                    {
                        if (treeView1.SelectedNode == null || treeView1.SelectedNode.Level != 2)
                            return;
                        if (isCut)
                        {
                            foreach (var tag in selectedTags)
                            {
                                tag.GroupID = curgroupId;
                                bindingSource1.Add(tag);
                            }
                        }
                        else
                        {
                            foreach (var tag in selectedTags)
                            {
                                tag.GroupID = curgroupId;
                                bindingSource1.Add(tag);
                                list.Add(tag);
                            }
                        }
                        if (bindingSource1.Count > 0)
                        {
                            dataGridView1.FirstDisplayedScrollingRowIndex = bindingSource1.Count - 1;
                            list.Sort();
                        }
                        selectedTags.Clear();
                    }
                    break;
                case "批量删除":
                    {
                        var rows = dataGridView1.SelectedRows;
                        if (rows != null)
                        {
                            for (int i = 0; i < rows.Count; i++)
                            {
                                TagData tag = rows[i].DataBoundItem as TagData;
                                bindingSource1.Remove(tag);
                                list.Remove(tag);
                            }
                        }
                    }
                    break;
            }
            tspCount.Text = bindingSource1.Count.ToString();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            TreeFind(majorTop, textBox1.Text);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (MessageBox.Show("退出之前是否需要保存？", "警告", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                Save();
            }
        }

        public void SplitAddress(string ad, ref int offset, ref int bit)
        {
            if (string.IsNullOrEmpty(ad))
            {
                offset = bit = 0;
                return;
            }
            string[] ads = ad.Split('.');
            offset = int.Parse(ads[0]);
            if (ads.Length > 1)
                bit = int.Parse(ads[1]);
        }

        static string[] autolist;
        public static string[] AutoList
        {
            get
            {
                if (autolist == null)
                {
                    var templist = new List<string> { "@Time", "@Date", "@DateTime", "@User", "@AppName", "@LocName", "@Region", "@Path" };
                    using (var reader = DataHelper.Instance.ExecuteReader("SELECT ISNULL(TagName,'') FROM Meta_Tag ORDER BY TagName"))
                    {
                        while (reader.Read())
                        {
                            string name = reader.GetString(0).ToUpper();
                            templist.Add(name);
                        }
                    }
                    autolist = templist.ToArray();
                }
                return autolist;
            }
        }

        private void 事件归档ToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            var tag = bindingSource1.Current as TagData;
            if (tag != null)
            {
                string name = tag.Name.ToUpper();
                if (事件归档ToolStripMenuItem.Checked)
                {
                    foreach (var cond in conditions)
                    {
                        if (cond.Source == name && cond.ConditionType == 2)
                        {
                            return;
                        }
                    }
                    var condition = new Condition(++Program.MAXCONDITIONID, name, 0, 2, 0, 0, true, 0, 0);
                    conditions.Add(condition);
                    tag.HasAlarm = true;
                }
                else
                {
                    foreach (var cond in conditions)
                    {
                        if (cond.Source == name && cond.EventType == 2)
                        {
                            conditions.Remove(cond); return;
                        }
                    }
                }
            }
        }

        private void contextMenuStrip2_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var tag = bindingSource1.Current as TagData;
            if (tag != null)
            {
                string name = tag.Name.ToUpper();
                foreach (var cond in conditions)
                {
                    if (cond.Source.ToUpper() == name && cond.EventType == 2)
                    {
                        事件归档ToolStripMenuItem.Checked = true; return;
                    }
                }
            }
            事件归档ToolStripMenuItem.Checked = false;
        }

    }

    public struct StructInfo
    {
        public string BaseAddress;
        public string StructName;
        public string StructType;
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

    public class DataTypeSource1
    {
        int _type;
        public int DataType { get { return _type; } set { _type = value; } }

        string _name;
        public string Name { get { return _name; } set { _name = value; } }

        string _path;
        public string Path { get { return _path; } set { _path = value; } }

        string _className;
        public string ClassName { get { return _className; } set { _className = value; } }

        public DataTypeSource1(int type, string name, string path, string className)
        {
            _type = type;
            _name = name;
            _path = path;
            _className = className;
        }
    }

    public class DriverArgumet
    {
        public short DriverID;
        public string PropertyName;
        public string PropertyValue;

        public DriverArgumet(short id, string name, string value)
        {
            DriverID = id;
            PropertyName = name;
            PropertyValue = value;
        }
    }

}

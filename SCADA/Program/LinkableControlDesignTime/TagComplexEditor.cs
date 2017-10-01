using System.Collections.Generic;
using System.Windows.Forms;

namespace HMIControl.VisualStudio.Design
{
    public partial class TagComplexEditor : Form
    {
        ITagReader _tag;
        bool _isSaved;
        TreeNode _lastNode, _top;
        Dictionary<string, string> _dict;
        AutoCompleteStringCollection scAutoComplete = new AutoCompleteStringCollection();

        public string TagText
        {
            get
            {
                if (!_isSaved) return null;
                if (_lastNode != null && _lastNode.Nodes.Count == 0)
                {
                    string str = coolTextBox1.Text.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace(" ", string.Empty);
                    string fullPath = GetFullPath(_lastNode);
                    if (string.IsNullOrEmpty(str))
                    {
                        _dict.Remove(fullPath);
                    }
                    else
                    {
                        if (_dict.ContainsKey(fullPath))
                        {
                            _dict[fullPath] = str;
                        }
                        else
                            _dict.Add(fullPath, str);
                    }
                }
                string txt = string.Empty;
                foreach (var item in _dict)
                {
                    txt += item.Key + ':' + item.Value + '\\';
                }
                return txt;
            }
        }


        public TagComplexEditor(ITagReader tag)
        {
            InitializeComponent();
            _tag = tag;
            
        }

        void dataGridView1_RowValidating(object sender, DataGridViewCellCancelEventArgs e)
        {
            _dict[dataGridView1[0, e.RowIndex].Value.ToString()] = dataGridView1[1, e.RowIndex].Value.ToString();
        }

        private void AddTreeNode(ITagReader tag, TreeNode tn)
        {
            var parent = tag as ITagReader;
            if (parent != null)
            {
                var actions = parent.GetActions();
                if (actions != null)
                {
                    foreach (var item in actions)
                    {
                        TreeNode node = new TreeNode(item);
                        tn.Nodes.Add(node);
                        if (_dict.ContainsKey(GetFullPath(node)))
                            node.Checked = true;
                    }
                }
                if (parent.Children != null)
                {
                    foreach (ITagReader child in parent.Children)
                    {
                        TreeNode node = new TreeNode(child.Node);
                        tn.Nodes.Add(node);
                        string path = GetFullPath(node);
                        if (_dict.ContainsKey(path))
                            node.Checked = true;
                        //递归调用  
                        AddTreeNode(child, node);
                    }
                }
            }
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Text)
            {
                case "保存":
                    _isSaved = true;
                    //GetText();
                    this.Close();
                    break;
                case "+''":
                    string txt = coolTextBox1.Text;
                    if (!string.IsNullOrEmpty(txt) && txt[0] != '\'' && txt[txt.Length - 1] != '\'')
                        coolTextBox1.Text = "'" + txt + "'";
                    else
                        coolTextBox1.Text = txt.Trim('\'');
                    break;
                case "替换":
                    string str1 = told.Text;
                    string str2 = tnew.Text;
                    if (string.IsNullOrEmpty(str1) || str2 == null)
                        return;
                    _dict.Clear();
                    for (int i = 0; i < dataGridView1.RowCount; i++)
                    {
                        dataGridView1[1, i].Value = dataGridView1[1, i].Value.ToString().Replace(str1, str2);
                        _dict.Add(dataGridView1[0, i].Value.ToString(), dataGridView1[1, i].Value.ToString());
                    }
                    coolTextBox1.Text = coolTextBox1.Text.Replace(str1, str2);
                    break;
                case "列表":
                    TagList frm = new TagList(coolTextBox1.SelectedText);
                    frm.Owner = this;
                    frm.ShowDialog();
                    if (coolTextBox1.Enabled)
                        coolTextBox1.Text += frm.CurrentText;
                    break;
            }
        }

        private void treeView1_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Nodes.Count == 0)
            {
                coolTextBox1.Enabled = true;
                if (_lastNode != null)
                {
                    string txt = coolTextBox1.Text.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace(" ", string.Empty);
                    string fullPath = GetFullPath(_lastNode);
                    if (!string.IsNullOrEmpty(txt))
                    {
                        if (_dict.ContainsKey(fullPath))
                        {
                            _dict[fullPath] = txt;
                            for (int i = 0; i < dataGridView1.RowCount; i++)
                            {
                                if (dataGridView1[0, i].Value.ToString() == fullPath)
                                {
                                    dataGridView1[1, i].Value = txt;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            _dict.Add(fullPath, txt);
                            dataGridView1.Rows.Add(fullPath, txt);
                        }
                        _lastNode.Checked = true;
                    }
                    else
                    {
                        _dict.Remove(fullPath);//1
                        for (int i = 0; i < dataGridView1.RowCount; i++)
                        {
                            if (dataGridView1[0, i].Value.ToString() == fullPath)
                            {
                                dataGridView1.Rows.RemoveAt(i);
                                break;
                            }
                        }
                        _lastNode.Checked = false;
                    }
                }
                _lastNode = e.Node;
                {
                    string fullPath = GetFullPath(_lastNode);
                    coolTextBox1.Text = _dict.ContainsKey(fullPath) ? _dict[fullPath] : "";
                }
            }
            else
            {
                _lastNode = e.Node;
                coolTextBox1.Text = "";
                coolTextBox1.Enabled = false;
            }
            tspText.Text = GetWarnMessage(_lastNode);
        }

        string GetFullPath(TreeNode node)
        {
            if (node == null) return string.Empty;
            string path = node.Text;
            TreeNode parent = node.Parent;
            while (parent != null && parent != _top)
            {
                path = parent.Text + "." + path;
                parent = parent.Parent;
            }
            return path;
        }

        string GetWarnMessage(TreeNode node)
        {
            if (node == null) return string.Empty;
            else
            {
                if (node.Nodes.Count == 0)
                {
                    switch (node.Text)
                    {
                        case TagActions.ENABLE:
                        case TagActions.DISABLE:
                        case TagActions.ALARM:
                        case TagActions.RUN:
                        case TagActions.WARN:
                        case TagActions.VISIBLE:
                        case TagActions.ON:
                        case TagActions.ONALARM:
                        case TagActions.OFF:
                        case TagActions.OFFALARM:
                        case TagActions.LEFT:
                        case TagActions.RIGHT:
                        case TagActions.LEFTALARM:
                        case TagActions.RIGHTALARM:
                        case TagActions.STATE1:
                        case TagActions.STATE2:
                        case TagActions.STATE3:
                        case TagActions.STATE4:
                        case TagActions.STATE5:
                            return "返回一个数字信号";
                        case TagActions.DEVICENAME:
                        case TagActions.START:
                        case TagActions.STOP:
                        case TagActions.CAPTION:
                        case TagActions.TEXT:
                        case TagActions.RAWNAME:
                            return "返回一个字符串";
                        case TagActions.STATE:
                            return "返回一个整数";
                        case TagActions.PV:
                        case TagActions.AMPS:
                        case TagActions.SPEED:
                            return "返回一个浮点数";
                        default:
                            return "未知返回类型";
                    }
                }
                else
                {
                    return "节点";
                }
            }
        }

        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Text)
            {
                case "展开":
                    treeView1.ExpandAll();
                    return;
                case "缩进":
                    treeView1.CollapseAll();
                    return;
            }
        }

        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is TextBox)
            {
                TextBox _with1 = (TextBox)e.Control;
                _with1.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                _with1.AutoCompleteSource = AutoCompleteSource.CustomSource;
                _with1.AutoCompleteCustomSource = scAutoComplete;
            }
        }

        private void TagComplexEditor_Load(object sender, System.EventArgs e)
        {
            if (_tag == null) return;
            if (string.IsNullOrEmpty(_tag.TagReadText))
            {
                _dict = new Dictionary<string, string>();
            }
            else
            {
                _dict = _tag.TagReadText.GetListFromText();
                foreach (var item in _dict)
                {
                    dataGridView1.Rows.Add(item.Key, item.Value);
                }
            }
            var list = TagList.GetTagNameList();
            scAutoComplete.AddRange(list.ToArray());
            _top = new TreeNode(_tag.Node);
            treeView1.Nodes.Add(_top);
            AddTreeNode(_tag, _top);
            treeView1.ExpandAll();
            //coolTextBox1. = list;
            this.Validate();
            dataGridView1.RowValidating += new DataGridViewCellCancelEventHandler(dataGridView1_RowValidating);
        }

        private void toolStrip2_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (!coolTextBox1.Enabled) return;
            string sign = e.ClickedItem.Text;
            switch (sign)
            {
                case "''":
                    {
                        string txt = coolTextBox1.SelectedText;
                        if (!string.IsNullOrEmpty(txt) && txt[0] != '\'' && txt[txt.Length - 1] != '\'')
                            coolTextBox1.SelectedText = "'" + txt + "'";
                        else
                            coolTextBox1.SelectedText = txt.Trim('\'');
                    }
                    break;
                case "( )":
                    {
                        string txt = coolTextBox1.SelectedText;
                        if (!string.IsNullOrEmpty(txt) && txt[0] != '(' && txt[txt.Length - 1] != ')')
                            coolTextBox1.SelectedText = "(" + txt + ")";
                    }
                    break;
                case "&&":
                    coolTextBox1.Text = coolTextBox1.Text + "&";
                    break;
                case "T":
                    coolTextBox1.Text = coolTextBox1.Text + "True";
                    break;
                case "F":
                    coolTextBox1.Text = coolTextBox1.Text + "False";
                    break;
                default:
                    coolTextBox1.Text = coolTextBox1.Text + sign;
                    break;
            }
        }
    }
}
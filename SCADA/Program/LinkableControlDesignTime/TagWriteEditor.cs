using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace HMIControl.VisualStudio.Design
{
    public partial class TagWriteEditor : Form
    {
        bool _isSaved;
        ITagWriter _tag;
        List<TagValue> _list = new List<TagValue>();

        public TagWriteEditor(ITagWriter tag)
        {
            InitializeComponent();
            _tag = tag;
            if (!string.IsNullOrEmpty(_tag.TagWriteText))
            {
                foreach (var item in _tag.TagWriteText.GetListFromText())
                {
                    _list.Add(new TagValue { Tag = item.Key, Value = item.Value });
                }
            }
            bindingSource1.DataSource = new SortableBindingList<TagValue>(_list);
            var list = TagList.GetTagNameList();
            chkPulse.Checked = tag.IsPulse;
            foreach (var item in list)
            {
                combo1.Items.Add(item);
            }
        }

        public string TagText
        {
            get
            {
                if (!_isSaved) return null;
                string txt = string.Empty;
                foreach (var item in _list)
                {
                    txt += item.Tag + ':' + item.Value + '\\';
                }
                return txt;
            }
        }

        private void coolTextBox1_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Text)
            {
                case "保存":
                    {
                        string tagName = combo1.Text.Replace(" ", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);
                        string tagValue = coolTextBox1.Text.Replace(" ", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);
                        var item = _list.Find(x => x.Tag == tagName);
                        if (item == null)
                        {
                            _list.Add(new TagValue { Tag = tagName, Value = tagValue });
                        }
                        else
                        {
                            item.Value = tagValue;
                        }
                        //_list.Add(new TagValue { Tag = tagName, Value = tagValue });
                        _isSaved = true;
                        this.Close();
                    }
                    break;
                case "添加":
                    {
                        string tagName = combo1.Text.Replace(" ", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);
                        string tagValue = coolTextBox1.Text.Replace(" ", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);
                        if (string.IsNullOrEmpty(tagName)) return;
                        var item = _list.Find(x => x.Tag == tagName);
                        if (item == null)
                        {
                            bindingSource1.Add(new TagValue { Tag = tagName, Value = tagValue });
                        }
                        else
                        {
                            item.Value = tagValue;
                        }
                        //bindingSource1.Add(new TagValue { Tag = tagName, Value = tagValue });
                        bindingSource1.EndEdit();
                        dataGridView1.CurrentCell = dataGridView1.CurrentRow.Cells[1];
                        combo1.Text = string.Empty;
                        coolTextBox1.Text = string.Empty;
                    }
                    break;
                case "删除":
                    bindingSource1.RemoveCurrent();
                    combo1.Text = string.Empty;
                    coolTextBox1.Text = string.Empty;
                    break;

            }
        }

        public bool IsSaved
        {
            get { return _isSaved; }
        }

        public bool IsPulse
        {
            get { return chkPulse.Checked; }
        }

        private void toolStrip2_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            string txt = coolTextBox1.Text;
            string sign = e.ClickedItem.Text;
            switch (sign)
            {
                case "''":
                    if (!string.IsNullOrEmpty(txt) && txt[0] != '\'' && txt[txt.Length - 1] != '\'')
                        coolTextBox1.Text = "'" + txt + "'";
                    else
                        coolTextBox1.Text = txt.Trim('\'');
                    break;
                case "( )":
                    if (!string.IsNullOrEmpty(txt) && txt[0] != '(' && txt[txt.Length - 1] != ')')
                        coolTextBox1.Text = "(" + txt + ")";
                    break;
                case "+":
                case "-":
                case "*":
                case "/":
                case "|":
                case "~":
                case "^":
                case ">":
                case "<":
                case "=":
                case "%":
                case "?":
                case "#":
                    coolTextBox1.Text = txt + sign;
                    break;
                case "&&":
                    coolTextBox1.Text = txt + "&";
                    break;
                case "T":
                    coolTextBox1.Text = txt + "True";
                    break;
                case "F":
                    coolTextBox1.Text = txt + "False";
                    break;
            }
        }

        private void chkPulse_CheckedChanged(object sender, EventArgs e)
        {
            if (chkPulse.Checked)
            {
                coolTextBox1.Text = "";
                coolTextBox1.Enabled = false;
            }
            else
                coolTextBox1.Enabled = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                string tagName = combo1.Text.Replace(" ", string.Empty).Replace("\n", string.Empty).Replace("\r", string.Empty);
                if (!string.IsNullOrEmpty(tagName))
                {
                    coolTextBox1.Text = "~" + tagName;
                }
            }
            else
            {
                coolTextBox1.Text = string.Empty;
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            combo1.Text = ((TagValue)(bindingSource1.Current)).Tag;
            coolTextBox1.Text = ((TagValue)(bindingSource1.Current)).Value;
        }
    }

    public class TagValue
    {
        public string Tag { get; set; }
        public string Value { get; set; }
    }
}

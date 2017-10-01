using System;
using System.Windows.Forms;

namespace TagConfig
{
    public partial class TagParam : Form
    {
        BindingSource _source;
        bool _enabled = false;

        public TagParam(BindingSource source)
        {
            _source = source;
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            comboBox1.DataSource = Form1.DataDict;
            comboBox1.DisplayMember = "Name";
            comboBox1.ValueMember = "DataType";
            bindingNavigator1.BindingSource = _source;
            txtName.DataBindings.Add("Text", _source, "Name", true);
            txtLen.DataBindings.Add("Value", _source, "Size", true);
            txtAddress.DataBindings.Add("Text", _source, "Address", true);
            txtValue.DataBindings.Add("Text", _source, "DefaultValue", true);
            txtDesp.DataBindings.Add("Text", _source, "Description", true);
            txtmax.DataBindings.Add("Value", _source, "Maximum", true);
            txtmin.DataBindings.Add("Value", _source, "Minimum", true);
            txtcycle.DataBindings.Add("Value", _source, "Cycle", true);
            chkActive.DataBindings.Add("Checked", _source, "Active", true);
            chkarchive.DataBindings.Add("Checked", _source, "Archive", true);
            comboBox1.DataBindings.Add("SelectedValue", _source, "DataType", true);
            _enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //加入变量地址设置 帮助
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            if (_enabled)
            {
                switch (comboBox1.Text)
                {
                    case "开关型":
                    case "字节":
                        ((TagData)_source.Current).Size = 1;
                        break;
                    case "短整型":
                    case "单字型":
                        ((TagData)_source.Current).Size = 2;
                        break;
                    case "时间型":
                    case "双字型":
                    case "浮点型":
                        ((TagData)_source.Current).Size = 4;
                        break;
                    case "ASCII字符串":
                    case "UNICODE字符串":

                        break;
                }
            }
        }
    }
}

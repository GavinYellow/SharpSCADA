using System;
using System.Windows.Forms;
using System.Collections.Generic;
using DatabaseLib;

namespace TagConfig
{
    public partial class DriverSet : Form
    {
        Driver _device;

        public DriverSet(Driver device)
        {
            _device = device;
            InitializeComponent();
            List<DataTypeSource1> typeList = new List<DataTypeSource1>();
            using (var reader = DataHelper.Instance.ExecuteReader("SELECT DRIVERID,ISNULL(Description,CLASSNAME) FROM RegisterModule"))
            {
                while (reader.Read())
                {
                    typeList.Add(new DataTypeSource1(reader.GetInt32(0), reader.GetString(1)));
                }
            }
            col.DataSource = typeList;
            col.DisplayMember = "Name";
            col.ValueMember = "DataType";
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            if (_device != null)
            {
                txtName.Text = _device.Name;
                txtServer.Text = _device.ServerName;
                numTimout.Value = _device.TimeOut;
                col.SelectedValue = (int)_device.DeviceDriver;
                txtspare1.Text = _device.Spare1;
                txtspare2.Text = _device.Spare2;
                //col.SelectedValue = _device.Driver;
            }
        }

        private void Form3_FormClosed(object sender, FormClosedEventArgs e)
        {
            _device.Name = txtName.Text;
            _device.ServerName = txtServer.Text;
            _device.TimeOut = (int)numTimout.Value;
            _device.DeviceDriver = Convert.ToInt32(col.SelectedValue);
            _device.Spare1 = txtspare1.Text;
            _device.Spare2 = txtspare2.Text;
        }
    }
}

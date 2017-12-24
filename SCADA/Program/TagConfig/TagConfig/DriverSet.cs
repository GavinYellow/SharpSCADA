using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Windows.Forms;

namespace TagConfig
{
    public partial class DriverSet : Form
    {
        bool _started;
        Driver _device;
        List<DataTypeSource1> _typeList;
        List<DriverArgumet> _arguments;
        static Dictionary<string, Type> _classList = new Dictionary<string, Type>();

        public DriverSet(Driver device, List<DataTypeSource1> typeList, List<DriverArgumet> args)
        {
            _device = device;
            _typeList = typeList;
            _arguments = args;
            InitializeComponent();

            col.DataSource = typeList;
            col.DisplayMember = "Name";
            col.ValueMember = "DataType";
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            if (_device != null)
            {
                col.SelectedValue = _device.DeviceDriver;
                //col.SelectedValue = _device.Driver;
                if (_device.Target != null)
                {
                    propertyGrid1.SelectedObject = _device.Target;
                }
                else GetProperties(false);
                _started = true;
            }
        }

        private void Form3_FormClosed(object sender, FormClosedEventArgs e)
        {
            _device.DeviceDriver = Convert.ToInt32(col.SelectedValue);
        }

        private void GetProperties(bool isnew)
        {
            var item = _typeList.Find(x => x.DataType == _device.DeviceDriver);
            if (item != null)
            {
                try
                {
                    Type dvType;
                    if (!_classList.TryGetValue(item.ClassName, out dvType))
                    {
                        Assembly ass = Assembly.LoadFrom(item.Path);
                        dvType = ass.GetType(item.ClassName);
                        _classList[item.ClassName] = dvType;
                    }
                    if (dvType != null)
                    {
                        var dv = Activator.CreateInstance(dvType, new object[] { null, _device.ID, _device.Name });
                        if (dv != null)
                        {
                            if (!isnew)
                            {
                                foreach (var arg in _arguments)
                                {
                                    if (arg.DriverID == _device.ID)
                                    {
                                        var prop = dvType.GetProperty(arg.PropertyName);
                                        if (prop != null)
                                        {
                                            if (prop.PropertyType.IsEnum)
                                                prop.SetValue(dv, Enum.Parse(prop.PropertyType, arg.PropertyValue), null);
                                            else
                                                prop.SetValue(dv, Convert.ChangeType(arg.PropertyValue, prop.PropertyType, CultureInfo.CreateSpecificCulture("en-US")), null);
                                        }
                                    }
                                }
                            }
                            _device.Target = dv;
                            propertyGrid1.SelectedObject = dv;
                        }
                    }
                }
                catch (Exception err)
                { }
            }
        }

        private void col_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_started)
            {
                _device.DeviceDriver = Convert.ToInt32(col.SelectedValue);
                GetProperties(true);
            }
        }
    }
}

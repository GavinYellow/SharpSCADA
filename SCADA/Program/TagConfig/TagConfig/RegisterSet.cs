using DatabaseLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace TagConfig
{
    public partial class RegisterSet : Form
    {
        Type driverType;
        public RegisterSet()
        {
            InitializeComponent();
        }

        private void Register_Load(object sender, EventArgs e)
        {
            try
            {
                Assembly dsass = Assembly.LoadFrom(@"DataService.dll");
                driverType = dsass.GetType("DataService.IDriver");
            }
            catch (Exception ex)
            {
                Program.AddErrorLog(ex);
            }
        }

        private void txtPath_DoubleClick(object sender, EventArgs e)
        {
            string path = txtPath.Text;
            if (!string.IsNullOrEmpty(path))
            {
                int index = path.LastIndexOf('\\');
                if (index < 0)
                {
                    openFileDialog1.FileName = path;
                }
                else
                {
                    openFileDialog1.InitialDirectory = path.Substring(0, index);
                    openFileDialog1.FileName = path.Substring(index + 1, path.Length - index - 1);
                }
            }
            openFileDialog1.Filter = "dll文件 (*.dll)|*.dll|All files (*.*)|*.*";
            openFileDialog1.DefaultExt = "dll";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                List<Register> regList = new List<Register>();
                string file = openFileDialog1.FileName;
                txtPath.Text = file;
                try
                {
                    if (driverType != null)
                    {
                        Assembly ass = Assembly.LoadFrom(file);
                        foreach (Type type in ass.GetTypes())
                        {
                            if (driverType.IsAssignableFrom(type))
                            {
                                string attribute = null;
                                foreach (var attr in type.GetCustomAttributes(false))
                                {
                                    DescriptionAttribute desp = attr as DescriptionAttribute;
                                    if (desp != null)
                                    {
                                        attribute = desp.Description;
                                    }
                                }
                                regList.Add(new Register(file, type.Name, type.FullName, attribute));
                            }
                        }
                    }
                    bindingSource1.DataSource = new SortableBindingList<Register>(regList);
                }
                catch (Exception ex)
                {
                    Program.AddErrorLog(ex);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
          var source=  bindingSource1.DataSource as IEnumerable<Register>;
          if (source != null)
          {
              StringBuilder sb = new StringBuilder();
              foreach (var reg in source)
              {
                  if (reg.Enable)
                  {
                      sb.Append("DELETE FROM RegisterModule WHERE CLASSFULLNAME='").Append(reg.ClassFullName).Append("';");
                      sb.Append("INSERT INTO RegisterModule(AssemblyName,ClassName,ClassFullName,Description) VALUES('")
                          .Append(reg.AssemblyPath).Append("','").Append(reg.ClassName).Append("','").Append(reg.ClassFullName)
                         .Append("','").Append(reg.Description).Append("');");
                  }
              }
              if (DataHelper.Instance.ExecuteNonQuery(sb.ToString()) >= 0)
                  MessageBox.Show("注册成功！");
          }
        }
    }
}

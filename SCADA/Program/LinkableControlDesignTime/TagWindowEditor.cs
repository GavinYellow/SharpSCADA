using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Forms;

namespace HMIControl.VisualStudio.Design
{
    public partial class TagWindowEditor : Form
    {
        ITagWindow _window;
        bool _isSaved;
        public TagWindowEditor(ITagWindow window)
        {
            InitializeComponent();
            _window = window;
            tspModel.SelectedIndex = window.IsModel ? 0 : 1;
            checkBox1.Checked = window.IsUnique;
        }

        private void TagWindowEditor_Load(object sender, EventArgs e)
        {
            List<string> list = new List<string>();
            string[] strs = null;
            if (!string.IsNullOrEmpty(_window.TagWindowText))
            {
                strs = _window.TagWindowText.TrimEnd(';').Split(';');
            }
            var ass = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in ass)
            {
                if (assembly.EntryPoint != null)
                {
                    try
                    {
                        var types = assembly.GetTypes();
                        foreach (var type in types)
                        {
                            var name = type.FullName;
                            if (!list.Contains(name) && typeof(ContentControl).IsAssignableFrom(type))
                            {
                                list.Add(name);
                            }
                        }
                    }
                    catch(Exception er)
                    {
                        continue;
                    }
                }
            }
            //var types = System.Windows.Application.ResourceAssembly.GetTypes();
            //foreach (var type in types)
            //{
            //    var name = type.FullName;
            //    if (!list.Contains(name) && typeof(ContentControl).IsAssignableFrom(type))
            //    {
            //        list.Add(name);
            //    }
            //}
            list.Sort();
            foreach (var name in list)
            {
                int index = checkedListBox1.Items.Add(name);
                if (strs != null && strs.Contains(name))
                    checkedListBox1.SetItemChecked(index, true);
            }
        }

        public string GetText()
        {
            if (!_isSaved) return null;
            string str = "";
            foreach (var item in checkedListBox1.CheckedItems)
            {
                str += item + ";";
            }
            return str;
        }

        public bool IsModel
        {
            get
            {
                return tspModel.SelectedIndex == 0;
            }
        }

        public bool IsUnique
        {
            get { return checkBox1.Checked; }
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Text)
            {
                case "保存":
                    _isSaved = true;
                    this.Close();
                    break;
            }
        }
    }
}

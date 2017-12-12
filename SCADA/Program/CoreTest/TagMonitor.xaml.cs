using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using DataService;

namespace CoreTest
{
    /// <summary>
    /// Interaction logic for TagMonitor.xaml
    /// </summary>
    public partial class TagMonitor : Window
    {
        bool _isSim = false;
        ObservableCollection<TagItem> list = new ObservableCollection<TagItem>();
        public TagMonitor()
        {
            InitializeComponent();
            this.SetWindowState();
            this.CommandBindings.Add(new CommandBinding(MyCommands.Edit, (sender, e) =>
            {
                _isSim = e.Parameter.Equals("1");
                textBox1.Focus();
                childWindow1.Show();
            }, (sender, e) =>
            {
                e.CanExecute = list1.SelectedItem != null;
            }));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var metalist = App.Server.MetaDataList;
            for (int i = 0; i < metalist.Count; i++)
            {
                list.Add(new TagItem(metalist[i]));
            }
            list1.ItemsSource = list;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            foreach (var tag in list)
            {
                tag.Dispose();
            }
            this.SaveWindowState();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TagItem tag = list1.SelectedItem as TagItem;
            if (tag != null)
            {
                if (_isSim) tag.SimWrite(textBox1.Text); else tag.Write(textBox1.Text);
            }
            childWindow1.Close();
        }
    }

    public class TagItem : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        ITag _tag;

        string _tagname;
        public string TagName
        {
            get { return _tagname; }
            set { _tagname = value; }
        }

        string _addr;
        public string Address
        {
            get { return _addr; }
            set { _addr = value; }
        }

        string _tagValue;
        public string TagValue
        {
            get { return _tagValue; }
            set
            {
                if (_tagValue != value)
                {
                    _tagValue = value;
                    OnPropertyChanged("TagValue");
                }
            }
        }

        DateTime _timestamp;
        public DateTime TimeStamp
        {
            get { return _timestamp; }
            set
            {
                if (_timestamp != value)
                {
                    _timestamp = value;
                    OnPropertyChanged("TimeStamp");
                }
            }
        }

        public TagItem(TagMetaData metadata)
        {
            _tagname = metadata.Name;
            _addr = metadata.Address;
            _tag = App.Server[_tagname];
            if (_tag != null)
            {
                _tagValue = _tag.ToString();
                _timestamp = _tag.TimeStamp;
                _tag.ValueChanged += new ValueChangedEventHandler(TagValueChanged);
            }
        }

        private void TagValueChanged(object sender, ValueChangedEventArgs args)
        {
            TagValue = _tag.ToString();
            TimeStamp = _tag.TimeStamp;
        }

        public int Write(string value)
        {
            if (string.IsNullOrEmpty(value)) return -1;
            if (_tag.Address.VarType == DataType.BOOL)
            {
                if (value == "1") value = "true";
                if (value == "0") value = "false";
            }
            return _tag.Write(value);
        }

        public void SimWrite(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            Storage stor = Storage.Empty;
            try
            {
                if (_tag.Address.VarType == DataType.STR)
                {
                    ((StringTag)_tag).String = value;
                }
                else
                {
                    stor = _tag.ToStorage(value);
                }
                _tag.Update(stor, DateTime.Now, QUALITIES.QUALITY_GOOD);
            }
            catch { }
        }

        public void Dispose()
        {
            if (_tag != null)
            {
                _tag.ValueChanged -= new ValueChangedEventHandler(TagValueChanged);
            }
        }
    }
}

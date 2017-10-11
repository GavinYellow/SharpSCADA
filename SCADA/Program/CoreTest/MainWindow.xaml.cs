using DataService;
using HMIControl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace CoreTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        double x1 = SystemParameters.PrimaryScreenWidth;//得到屏幕整体宽度
        double y1 = SystemParameters.PrimaryScreenHeight;//得到屏幕整体高度

        public MainWindow()
        {
            InitializeComponent();
            this.Width = x1;//设置窗体宽度
            this.Height = y1;//设置窗体高度
        }

        List<TagNodeHandle> _valueChangedList;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Login.IsOpen)
            {
                Login frm1 = new Login();
                frm1.ShowDialog();
            }
            if (App.Principal != null)
            {
                txtuser.Text = string.Format("当前用户:{0}    权限{1}", App.Principal.Identity.Name, App.Principal.ToString());
                //确定中控员权限
            }
            #region  显示缺省界面
            if (Tag != null && !string.IsNullOrEmpty(Tag.ToString()))
            {
                var Wintypes = Tag.ToString().TrimEnd(';');
                ContentControl ctrl = Activator.CreateInstance(Type.GetType(Wintypes)) as ContentControl;
                if (ctrl != null)
                {
                    ScaleControl(ctrl);
                    ctrl.Loaded += new RoutedEventHandler(ctrl_Loaded);
                    ctrl.Unloaded += new RoutedEventHandler(ctrl_Unloaded);
                    dict[Wintypes] = ctrl;
                    cvs1.Child = ctrl;
                    this.Title = Wintypes;
                }
            }
            #endregion
            #region  显示状态栏时间、显示PLC连接状态、与PLC看门狗通讯
            DispatcherTimer ShowTimer = new DispatcherTimer();
            ShowTimer.Interval = new TimeSpan(0, 0, 1);
            ShowTimer.Tick += (s, e1) =>
            {
                txttime.InvokeAsynchronously(delegate { txttime.Text = DateTime.Now.ToString(); });
                p1_lamp1.Fill = App.Server.Drivers.Any(x => x.IsClosed) ? Brushes.Red : Brushes.Green;
            };
            ShowTimer.Start();
            #endregion
            #region  绑定到Server
            lock (this)
            {
                _valueChangedList = this.BindingToServer(App.Server);
            }
            BindingTagWindow(this);
            CommandBindings.AddRange(BindingCommandHandler());
             var condlist = App.Server.ActivedConditionList as ObservableCollection<ICondition>;
            if (condlist != null)
            {
                condlist.CollectionChanged += new NotifyCollectionChangedEventHandler(condlist_CollectionChanged);
            }
            var tag = App.Server["__CoreEvent"];
            if (tag != null)
            {
                tag.ValueChanged += (s, e1) =>
                {
                    if (tag != null)
                    {
                        App.Events.ReverseEnqueue(string.Format("{0}     {1}     {2}", tag.GetTagName(), DateTime.Now, tag.ToString()));
                        if (tag.ToString().Contains("错误:"))
                            MessageBox.Show(tag.ToString(), "错误！", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                };
            }

            #endregion
        }

        void condlist_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var condlist = sender as ObservableCollection<ICondition>;
            AlarmConverter convert = new AlarmConverter();
            txtAlarm.Inlines.Clear();
            for (int i = 0; i < condlist.Count; i++)
            {
                txtAlarm.Inlines.Add(new Run(string.Concat(i.ToString(), ":", condlist[i].Message, " ")) { Foreground = convert.Convert(condlist[i].Severity, null, null, null) as Brush });
            }
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (ICondition cond in e.NewItems)
                {
                    if (cond.Severity == Severity.Error)
                    {
                        var tag = App.Server["Sys_Alarm"];
                        tag.Write(true);
                        MessageBox.Show(cond.Message, "警告！", MessageBoxButton.OK, MessageBoxImage.Error);//发生致命错误，需要在系统服务内根据错误类型挂起相应Order;由客户端手动解挂。
                    }
                }
            }
        }

        Dictionary<string, ContentControl> dict = new Dictionary<string, ContentControl>();

        void BindingTagWindow(DependencyObject container)
        {
            if (container == null) return;
            foreach (var item in container.FindChildren<ITagWindow>())
            {
                if (!string.IsNullOrEmpty(item.TagWindowText))
                {
                    UIElement element = item as UIElement;
                    element.AddHandler(UIElement.MouseLeftButtonUpEvent,
                                       new MouseButtonEventHandler(item_MouseLeftButtonUp));
                }
            }
        }

        private void ShowContent(ITagWindow tagwindow)
        {
            if (tagwindow == null || string.IsNullOrEmpty(tagwindow.TagWindowText)) return;
            var windows = tagwindow.TagWindowText.TrimEnd(';').Split(';');
            foreach (string txt in windows)
            {
                if (dict.ContainsKey(txt))
                {
                    if (dict[txt].Tag.ToString() != "YES")
                    {
                        cvs1.Child = dict[txt];
                    }
                    continue;
                }
                if (tagwindow.IsUnique)
                {
                    foreach (var win in App.Current.Windows)
                    {
                        if (win.ToString() == txt)
                            goto lab1;
                    }
                }
                try
                {
                    ContentControl ctrl = Activator.CreateInstance(Type.GetType(txt)) as ContentControl;
                    if (ctrl != null)
                    {
                        var win = ctrl as Window;
                        if (win == null)
                            ScaleControl(ctrl);
                        ctrl.Loaded += new RoutedEventHandler(ctrl_Loaded);
                        ctrl.Unloaded += new RoutedEventHandler(ctrl_Unloaded);
                        if (win != null)
                        {
                            win.Owner = this;
                            win.ShowInTaskbar = false;
                            if (tagwindow.IsModel)
                                win.ShowDialog();
                            else
                                win.Show();
                        }
                        else
                        {
                            dict[txt] = ctrl;
                            cvs1.Child = ctrl;
                            this.Title = txt;
                        }
                    }
                }
                catch (Exception e)
                {
                    App.AddErrorLog(e);
                }
                lab1:
                continue;
            }
        }

        void ScaleControl(ContentControl ctrl)
        {
            var transform = ctrl.RenderTransform as MatrixTransform;
            if (transform != null && !double.IsNaN(ctrl.Width) && !double.IsNaN(ctrl.Height))
            {
                var matrix = transform.Matrix;
                matrix.Scale(x1 / ctrl.Width, y1 / ctrl.Height);
                ctrl.RenderTransform = new MatrixTransform(matrix);
                ctrl.Width = x1;
                ctrl.Height = y1;
                this.Background = ctrl.Background;
            }
        }

        void ctrl_Unloaded(object sender, RoutedEventArgs e)
        {
            ContentControl uie = sender as ContentControl;
            if (uie != null)
            {
                uie.Tag = "NO";
                var windows = uie.FindChildren<ITagWindow>();
                foreach (ITagWindow item in windows)
                {
                    if (!string.IsNullOrEmpty(item.TagWindowText))
                    {
                        ((UIElement)item).RemoveHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(item_MouseLeftButtonUp));
                    }
                }
            }
        }

        void ctrl_Loaded(object sender, RoutedEventArgs e)
        {
            ContentControl uie = sender as ContentControl;
            if (uie != null)
            {
                uie.Tag = "YES";
                BindingTagWindow(uie);
            }
        }

        void item_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ShowContent(sender as ITagWindow);
            e.Handled = true;
        }

        private void hMIButton7_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("确定退出系统？", "警告", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.Cancel) return;
            //SystemLog.AddLog(new SystemLog(EventType.Simple, DateTime.Now, App.LogSource, "退出"));
            App.Current.Shutdown();
            e.Handled = true;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (cvs3.Visibility == Visibility.Hidden)
                cvs3.Visibility = Visibility.Visible;
            else if (cvs3.Visibility == Visibility.Visible)
                cvs3.Visibility = Visibility.Hidden;
        }


        CommandBindingCollection BindingCommandHandler()
        {
            var srv = App.Server;
            CommandBindingCollection CommandBindings = new CommandBindingCollection();
         
            return CommandBindings;
        }
    }
}


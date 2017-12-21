using System;
using System.Diagnostics;
using System.Drawing;
using System.Security.Principal;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

namespace CoreTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        const string MYCOSLOGSOURCE = "MyWEM Application";
        const string MYCOSLOGNAME = "MyWEM";

        static EventLog Log = new EventLog(MYCOSLOGNAME);
        static readonly string machine = Environment.MachineName;
        public static readonly DAServer Server = new DAServer();

        static IPrincipal _princ;
        public static IPrincipal Principal
        {
            get
            {
                return _princ;
            }
            set
            {
                _princ = value;
            }
        }

        public static string LogSource
        {
            get
            {
                return machine + "\\" + (_princ == null ? null : _princ.Identity.Name);
            }
        }

        static ReverseObservableQueue<string> eventLog = new ReverseObservableQueue<string>(300);
        public static ReverseObservableQueue<string> Events
        {
            get { return eventLog; }
        }

        static App()
        {
            var curr = Process.GetCurrentProcess();
            foreach (var process in Process.GetProcessesByName(curr.ProcessName))
            {
                if (process.Id != curr.Id)
                {
                    System.Windows.MessageBox.Show("程序已启动!");
                    Environment.Exit(0);
                }
            }
            if (!EventLog.SourceExists(MYCOSLOGSOURCE))
                EventLog.CreateEventSource(MYCOSLOGSOURCE, MYCOSLOGNAME);
        }

        public static void AddErrorLog(Exception e)
        {
            string err = "";
            Exception exp = e;
            while (exp != null)
            {
                err += string.Format("\n {0}", exp.Message);
                exp = exp.InnerException;
            }
            err += string.Format("\n {0}", e.StackTrace);
            Log.Source = MYCOSLOGSOURCE;
            Log.WriteEntry(err, EventLogEntryType.Error);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            MainWindow w1 = null, w2 = null, w3 = null, w4 = null;
            Screen s1, s2, s3, s4;
            Rectangle r1, r2, r3, r4;

            Current.DispatcherUnhandledException += App_DispatcherUnhandledException;
            base.OnStartup(e);

            #region
            if (Screen.AllScreens.Length == 1)//Server.SD.ScreenCount
            {
                w1 = new MainWindow();
                w1.WindowStartupLocation = WindowStartupLocation.Manual;
                s1 = Screen.AllScreens[0];
                r1 = s1.WorkingArea;
                w1.Top = r1.Top;
                w1.Left = r1.Left;
                w1.Show();
            }
            else if (Screen.AllScreens.Length == 2)
            {
                w1 = new MainWindow();
                w1.WindowStartupLocation = WindowStartupLocation.Manual;
                s1 = Screen.AllScreens[0];
                r1 = s1.WorkingArea;
                w1.Top = r1.Top;
                w1.Left = r1.Left;
                w1.Show();

                w2 = new MainWindow();
                w2.WindowStartupLocation = WindowStartupLocation.Manual;
                s2 = Screen.AllScreens[1];
                r2 = s2.WorkingArea;
                w2.Top = r2.Top;
                w2.Left = r2.Left;
                w2.Show();
                w2.Owner = w1;
            }
            else if (Screen.AllScreens.Length == 3)
            {
                w1 = new MainWindow();
                w1.WindowStartupLocation = WindowStartupLocation.Manual;
                s1 = Screen.AllScreens[0];
                r1 = s1.WorkingArea;
                w1.Top = r1.Top;
                w1.Left = r1.Left;
                w1.Show();

                w2 = new MainWindow();
                w2.WindowStartupLocation = WindowStartupLocation.Manual;
                s2 = Screen.AllScreens[1];
                r2 = s2.WorkingArea;
                w2.Top = r2.Top;
                w2.Left = r2.Left;
                w2.Show();
                w2.Owner = w1;

                w3 = new MainWindow();
                w3.WindowStartupLocation = WindowStartupLocation.Manual;
                s3 = Screen.AllScreens[2];
                r3 = s3.WorkingArea;
                w3.Top = r3.Top;
                w3.Left = r3.Left;
                w3.Show();
                w3.Owner = w1;
            }
            else if (Screen.AllScreens.Length == 4)
            {
                w1 = new MainWindow();
                w1.WindowStartupLocation = WindowStartupLocation.Manual;
                s1 = Screen.AllScreens[0];
                r1 = s1.WorkingArea;
                w1.Top = r1.Top;
                w1.Left = r1.Left;
                w1.Show();

                w2 = new MainWindow();
                w2.WindowStartupLocation = WindowStartupLocation.Manual;
                s2 = Screen.AllScreens[1];
                r2 = s2.WorkingArea;
                w2.Top = r2.Top;
                w2.Left = r2.Left;
                w2.Show();
                w2.Owner = w1;

                w3 = new MainWindow();
                w3.WindowStartupLocation = WindowStartupLocation.Manual;
                s3 = Screen.AllScreens[2];
                r3 = s3.WorkingArea;
                w3.Top = r3.Top;
                w3.Left = r3.Left;
                w3.Show();
                w3.Owner = w1;

                w4 = new MainWindow();
                w4.WindowStartupLocation = WindowStartupLocation.Manual;
                s4 = Screen.AllScreens[3];
                r4 = s3.WorkingArea;
                w4.Top = r4.Top;
                w4.Left = r4.Left;
                w4.Show();
                w4.Owner = w1;
            }
            #endregion
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Server.Dispose();
            ConfigCache.SaveConfig();
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            AddErrorLog(e.Exception);
            e.Handled = true;
        }
    }
}

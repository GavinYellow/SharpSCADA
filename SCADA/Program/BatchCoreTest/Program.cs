using System;
using System.Runtime.InteropServices;
using System.ServiceModel;

namespace BatchCoreService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            BatchCoreTest srv = new BatchCoreTest();
            ConsoleUtil.RegisterCloseConsoleHandle();//注册控制台关闭事件,注意,只有执行该该操作,事件
            ConsoleUtil.ClosedConsole += (sender, e) => srv.Dispose();
            Console.ReadLine();
        }
    }

    public class BatchCoreTest : IDisposable
    {
        //DAService service;
        ServiceHost serviceHost = null;

        public BatchCoreTest()
        {
            //service = new DAService();
            if (serviceHost != null)
            {
                serviceHost.Close();
            }

            // Create a ServiceHost for the CalculatorService type and 
            // provide the base address.
            serviceHost = new ServiceHost(typeof(DAService));

            // Open the ServiceHostBase to create listeners and start 
            // listening for messages.
            serviceHost.Open();
            //Console.ReadLine();
        }

        public void Dispose()
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
                serviceHost = null;
            }
        }
    }

    public static class ConsoleUtil
    {
        #region 禁用控制台关闭按钮

        ///
        /// 禁用关闭按钮
        ///
        public static void DisableCloseButton()
        {
            DisableCloseButton(Console.Title);
        }

        ///
        /// 禁用关闭按钮
        ///
        /// 控制台名字
        public static void DisableCloseButton(string consoleName)
        {

            IntPtr windowHandle = FindWindow(null, consoleName);
            IntPtr closeMenu = GetSystemMenu(windowHandle, IntPtr.Zero);
            uint scClose = 0xF060;
            RemoveMenu(closeMenu, scClose, 0x0);
        }

        #region API
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "GetSystemMenu")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, IntPtr bRevert);

        [DllImport("user32.dll", EntryPoint = "RemoveMenu")]
        static extern IntPtr RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);

        #endregion

        #endregion

        #region 捕捉控制台关闭事件

        public delegate bool ConsoleCtrlDelegate(int ctrlType);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate handlerRoutine, bool add);



        ///
        /// 注册控制台关闭事件,通过事件进行捕捉.
        ///
        public static void RegisterCloseConsoleHandle()
        {
            SetConsoleCtrlHandler(OnClosedConsole, true);
        }

        ///
        /// 当控制台被关闭时,引发事件.
        ///
        public static event EventHandler ClosedConsole;

        private static bool OnClosedConsole(int ctrlType)
        {
            if (ClosedConsole != null)
            {
                var e = new CloseConsoleEventArgs((CloseConsoleCategory)ctrlType);
                ClosedConsole("Console", e);
                return e.IsCancel;
            }
            return false; //忽略处理，让系统进行默认操作
        }

        #endregion

    }

    ///
    /// 控制台关闭事件.
    ///
    public class CloseConsoleEventArgs : EventArgs
    {
        public CloseConsoleEventArgs()
        {

        }

        public CloseConsoleEventArgs(CloseConsoleCategory category)
        {
            Category = category;
        }

        public CloseConsoleCategory Category { get; set; }

        ///
        /// 是否取消操作.
        ///
        public bool IsCancel { get; set; }
    }

    ///
    /// 关闭控制台的类型.
    ///
    public enum CloseConsoleCategory
    {
        ///
        /// 当用户关闭Console
        ///
        CloseEvent = 2,
        ///
        /// Ctrl+C
        ///
        CtrlCEvent = 0,
        ///
        /// 用户退出（注销）
        ///
        LogoffEvent = 5,
        ///
        /// Ctrl+break
        ///
        CtrlBreakEvent = 1,
        ///
        /// 系统关闭
        ///
        ShutdownEvent = 6,
    }
}

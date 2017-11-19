using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

namespace TagConfig
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (!EventLog.SourceExists(TAGCONFIGSOURCE))
                EventLog.CreateEventSource(TAGCONFIGSOURCE, TAGCONFIGLOGNAME);
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            Application.Run(new Form1());
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            AddErrorLog(e.Exception);
        }

        const string TAGCONFIGSOURCE = "TagConfig Application";
        const string TAGCONFIGLOGNAME = "TagConfig";
        static readonly EventLog Log = new EventLog(TAGCONFIGLOGNAME);

        public static int MAXCONDITIONID = 0;

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
            Log.Source = TAGCONFIGSOURCE;
            Log.WriteEntry(err, EventLogEntryType.Error);
        }
    }
}

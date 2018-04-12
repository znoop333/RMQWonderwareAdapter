using NativeMethods;
using RMQWonderwareAdapter.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Win32Helper;

namespace RMQWonderwareAdapter
{
    static class Program
    {
        static string guid = ProgramInfo.GetMyGUID();
        static string MyUniqueName = guid + Settings.Default.PC_ID;
        static Mutex mutex = new Mutex(true, MyUniqueName);
        public static readonly uint WM_SHOWME_RESTORE_WINDOW = (uint)Win32Constants.RegisterWindowMessage(MyUniqueName);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.ThreadException += Application_ThreadException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            else
            {
                Win32Constants.PostMessage(
                (IntPtr)Win32Constants.HWND_BROADCAST,
                WM_SHOWME_RESTORE_WINDOW,
                IntPtr.Zero,
                IntPtr.Zero);
            }

        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string sCurDate = string.Format("{0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss   "));
            LogHelper.AppendToLogfile(Form1.FilenameForLog("ERROR-UnhandledException"), sCurDate + "Unhandled UI Exception: " + e.ToString() + ". \n" + (e.ExceptionObject as Exception).Message);
            LogHelper.FlushLogFiles();
            LogHelper.CloseLogFiles();

            EventLog.WriteEntry("RMQWonderwareAdapter", "Unhandled UI Exception: " + (e.ExceptionObject as Exception).Message, System.Diagnostics.EventLogEntryType.Error);

#if DEBUG
            MessageBox.Show((e.ExceptionObject as Exception).Message, "Unhandled UI Exception");
#endif
            Application.Exit();
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            string msg = String.Format("Unhandled Thread Exception - {0}: {1}", e.Exception.Message, e.Exception.StackTrace);
            if (e.Exception.InnerException != null)
            {
                msg += String.Format("\nInnner exception: {0}", e.Exception.InnerException.ToString());
                if (e.Exception.InnerException.StackTrace != null)
                {
                    msg += String.Format("\nInnner exception stack trace: {0}", e.Exception.InnerException.StackTrace.ToString());
                }
            }

            LogHelper.AppendToLogfile(Form1.FilenameForLog("ERROR-ThreadException"), msg);
            LogHelper.FlushLogFiles();
            LogHelper.CloseLogFiles();

            EventLog.WriteEntry("RMQWonderwareAdapter", msg, System.Diagnostics.EventLogEntryType.Error);

#if DEBUG
            MessageBox.Show(e.Exception.Message, "Unhandled Thread Exception");
#endif
            Application.Exit();
        }

    }
}

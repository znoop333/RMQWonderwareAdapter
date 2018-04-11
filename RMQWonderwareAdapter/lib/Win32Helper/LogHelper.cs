using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Win32Helper
{
    class LogHelper
    {
        private static Dictionary<string, StreamWriter> StreamWriters = new Dictionary<string, StreamWriter>();
        public static void AppendToLogfile(string filename, string msg, bool WantTimestamp = true)
        {
            string sCurDate = WantTimestamp ? string.Format("{0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss - ")) : "";

            if (StreamWriters.ContainsKey(filename))
            {
                StreamWriter w = StreamWriters[filename];
                w.WriteLine(sCurDate + msg);
                //w.Flush(); // flush using timer
                return;
            }

            try
            {
                StreamWriter y = File.AppendText(filename);
                y.AutoFlush = false;
                StreamWriters[filename] = y;
                AppendToLogfile(filename, msg);
            }
            catch (Exception )
            {
            }

        }

        public static void FlushLogFiles()
        {
            foreach (var w in StreamWriters)
            {
                try
                {
                    w.Value.Flush();
                }
                catch (Exception )
                {
                }
            }
        }

        public static void CloseLogFiles()
        {
            foreach (var w in StreamWriters)
            {
                try
                {
                    w.Value.Flush();
                    w.Value.Close();
                }
                catch (Exception )
                {
                }
            }

            StreamWriters.Clear();
        }

        private static Dictionary<string, StringBuilder> BufferedTextboxWriting = new Dictionary<string, StringBuilder>();
        private static Dictionary<string, DateTime> LastTextboxRedraw = new Dictionary<string, DateTime>();
        private static TimeSpan minRefreshSpan = TimeSpan.FromMilliseconds(1000.0);

        public static void FlushTextbox(TextBox tb)
        {
            if (BufferedTextboxWriting.ContainsKey(tb.Name) && BufferedTextboxWriting[tb.Name].Length > 0)
                AppendToTextbox(tb, null);
        }

        public static void AppendToTextbox(TextBox tb, string msg, int maxChars = 10000, bool WantTimestamp = true)
        {
            string sCurDate = WantTimestamp ? string.Format("{0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss - ")) : "";
            string newText = "";
            if(msg != null && msg != "")
                newText = sCurDate + msg + Environment.NewLine;

            if (!LastTextboxRedraw.ContainsKey(tb.Name))
            {
                BufferedTextboxWriting[tb.Name] = new StringBuilder();
                LastTextboxRedraw[tb.Name] = DateTime.UtcNow;

                // immediately redraw
            }
            else if (DateTime.UtcNow - LastTextboxRedraw[tb.Name] < minRefreshSpan)
            {
                // delay redraw 
                BufferedTextboxWriting[tb.Name].Append(newText);
                return;
            }
            else
            {
                // catch up with redrawing
                newText = BufferedTextboxWriting[tb.Name] + newText;
                BufferedTextboxWriting[tb.Name].Clear();
                LastTextboxRedraw[tb.Name] = DateTime.UtcNow;
            }


            int i = tb.TextLength;

            int j = newText.Length;
            
            if (i + j > maxChars)
            {
                // reduce .net GC pressure by forcing Win32 API to do all the string manipulations
                tb.SelectionStart = 0;
                tb.SelectionLength = j;
                tb.SelectedText = "";

                tb.AppendText(newText);
            }
            else
                tb.AppendText(newText);

            // move scrollbar to end
            NativeMethods.ScrollToBottom(tb);
        }

        public static string FormatRawBytes(byte[] b)
        {
            var s = new StringBuilder();
            if (b != null && b.Length > 0)
            {
                for (int i = 0; i < b.Length; i++)
                {
                    //if (i > 0 && i % 4 == 0)
                    //    s.Append(" ");

                    s.Append( b[i].ToString("X2") + " " );
                }
            }
            return s.ToString();
        }

    }
}

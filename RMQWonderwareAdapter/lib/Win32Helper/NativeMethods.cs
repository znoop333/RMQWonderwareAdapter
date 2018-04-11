using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Text;
using static NativeMethods.Win32Constants;
using System.Drawing;

namespace Win32Helper
{
    class NativeMethods
    {
        
        /// <summary>
        /// Scrolls the vertical scroll bar of a multi-line text box to the bottom.
        /// </summary>
        /// <param name="tb">The text box to scroll</param>
        public static void ScrollToBottom(TextBox tb)
        {
            SendMessage(tb.Handle, WM_VSCROLL, (IntPtr)SB_BOTTOM, IntPtr.Zero);
        }


        public static void AppendToTextbox(TextBox tb, string msg, int maxChars = 1000)
        {
            tb.AppendText(msg + Environment.NewLine);
            string s = tb.Text;

            if (s.Length > maxChars)
            {
                s = s.Substring(s.Length - maxChars, maxChars);
                tb.Text = s;
            }

            // move scrollbar to end
            NativeMethods.ScrollToBottom(tb);
        }

        public static void SendKeys(IntPtr hwndTarget, string barcode)
        {

            if (!hwndTarget.Equals(IntPtr.Zero) && SetForegroundWindow(hwndTarget))
            {
                System.Windows.Forms.SendKeys.Send(barcode + "{ENTER}");

                //IntPtr result3 = SendMessage(WindowToFind, WM_KEYDOWN, ((IntPtr)k), (IntPtr)0);
                //result3 = SendMessage(WindowToFind, WM_CHAR, ((IntPtr)k), (IntPtr)0);
                //result3 = SendMessage(WindowToFind, WM_KEYUP, ((IntPtr)k), (IntPtr)0);

                //IntPtr edithWnd = FindWindowEx(WindowToFind, IntPtr.Zero, "Edit", null);

                //if (!edithWnd.Equals(IntPtr.Zero))
                //    // send WM_SETTEXT message with "Hello World!"
                //    SendMessage(edithWnd, WM_CHAR, ((IntPtr)0x32), IntPtr.Zero);

                //SendMessage(edithWnd, WM_SETTEXT, IntPtr.Zero, k);
            }
        }

        public static bool SendScannerInput(IntPtr hwndTarget, string barcode)
        {
            //SendKeys(hwndTarget, barcode);

            //return true;

            if (IntPtr.Zero.Equals(hwndTarget))
                return false;

            SendMessage(hwndTarget, WM_SETTEXT, IntPtr.Zero, barcode);
            SendMessage(hwndTarget, WM_KEYDOWN, ((IntPtr)VK_RETURN), IntPtr.Zero);
            SendMessage(hwndTarget, WM_CHAR, ((IntPtr)VK_RETURN), IntPtr.Zero);
            SendMessage(hwndTarget, WM_KEYUP, ((IntPtr)VK_RETURN), IntPtr.Zero);

            return true;
        }

        public static bool GetControlText(IntPtr hwndTarget, out string data)
        {
            if (IntPtr.Zero.Equals(hwndTarget))
            {
                data = "";
                return false;
            }

            var len = SendMessage(hwndTarget, WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero).ToInt32();
            if(len == 0)
            {
                data = "";
                return true;
            }

            StringBuilder text = new StringBuilder();
            text = new StringBuilder(len + 1);

            SendMessage(hwndTarget, (IntPtr)WM_GETTEXT, (IntPtr)text.Capacity, text);
            data = text.ToString();

            return true;
        }

        public static bool ClickButton(IntPtr hwndTarget)
        {

            if (IntPtr.Zero.Equals(hwndTarget))
                return false;

            SendMessage(hwndTarget, WM_LBUTTONDOWN, IntPtr.Zero, IntPtr.Zero);
            // Thread.Sleep() ???
            SendMessage(hwndTarget, WM_LBUTTONUP, IntPtr.Zero, IntPtr.Zero);

            return true;
        }

        public static bool Check_Checkbox(IntPtr hwndTarget, bool newCheckedState = true)
        {
            if (IntPtr.Zero.Equals(hwndTarget))
                return false;

            //SendMessage(hwndTarget, BM_SETCHECK, newCheckedState ? ((IntPtr)BST_CHECKED) : ((IntPtr)BST_UNCHECKED), IntPtr.Zero);
            
            ClickOnPointTool.ClickOnPoint(hwndTarget, new Point(6,15));

            return true;
        }

    }
}

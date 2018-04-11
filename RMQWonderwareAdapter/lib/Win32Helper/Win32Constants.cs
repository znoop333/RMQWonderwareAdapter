using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace NativeMethods
{
    class Win32Constants
    {
        public const int WM_VSCROLL = 0x115;
        public const int SB_BOTTOM = 7;

        public const int HWND_BROADCAST = 0xffff;

        public const int WM_KEYDOWN = 0x100;
        public const int WM_KEYUP = 0x101;
        public const int WM_CHAR = 0x102;
        public const int WM_SYSCOMMAND = 0x018;
        public const int SC_CLOSE = 0x053;
        public const int WM_SETTEXT = 0x000C;

        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;


        public const int BM_SETCHECK = 0x00f1;
        public const int BST_UNCHECKED = 0x0000;
        public const int BST_CHECKED = 0x0001;
        public const int BST_INDETERMINATE = 0x0002;


        public const int VK_RETURN = 0x0D;
        

        public static readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOWME_84888FF4");
        public static readonly uint WM_SHOWME2 = (uint)RegisterWindowMessage("WM_SHOWME_123456");

        public delegate bool EnumWindowProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr window, EnumWindowProc callback, IntPtr lParam);

        public const int WM_GETTEXT = 0x000D;
        public const int WM_GETTEXTLENGTH = 0x000E;

        [DllImport("User32.Dll")]
        public static extern void GetClassName(IntPtr hWnd, StringBuilder s, int nMaxCount);

        [DllImport("User32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("User32.dll")]
        public static extern Int32 SendMessage(IntPtr hWnd, IntPtr Msg, IntPtr wParam, StringBuilder lParam);

        // another overload so that manually marshalling the string is not required.
        // see "Automatic casting for string DllImport arguments vs Marshal.StringToCoTaskMemUni" http://stackoverflow.com/a/19616497/4051006
        [DllImport("User32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint uMsg, IntPtr wParam, string lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint wMsg, IntPtr wParam, IntPtr lParam);



        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd, uint msg, IntPtr wparam, IntPtr lparam);
        [DllImport("user32")]
        public static extern int RegisterWindowMessage(string message);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("User32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindows);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetParent(IntPtr hWnd);


    }
}

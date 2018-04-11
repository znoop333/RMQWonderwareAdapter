using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using static NativeMethods.Win32Constants;

namespace NativeMethods
{
    /// <summary>
    /// GetAllChildHandles() returns a List<IntPtr> of all descendent windows (via EnumChildWindows), which can be used with SendMessage et al.
    /// Extended from "How can I get the child windows of a window given its HWND?" http://stackoverflow.com/q/1363167/4051006
    /// Overwrite FilterWindow() and ContinueEnumerating() to change the behavior of this helper class.
    /// </summary>
    class WindowHandleInfo
    {
        internal IntPtr _MainHandle;
        internal List<IntPtr> childHandles = new List<IntPtr>();

        public WindowHandleInfo()
        {
        }

        public List<IntPtr> GetAllChildHandles()
        {
            this.childHandles = new List<IntPtr>();

            GCHandle gcChildhandlesList = GCHandle.Alloc(this.childHandles);
            IntPtr pointerChildHandlesList = GCHandle.ToIntPtr(gcChildhandlesList);

            try
            {
                EnumWindowProc childProc = new EnumWindowProc(EnumWindow);
                EnumChildWindows(this._MainHandle, childProc, pointerChildHandlesList);
            }
            finally
            {
                gcChildhandlesList.Free();
            }

            return childHandles;
        }

        internal virtual bool EnumWindow(IntPtr hWnd, IntPtr lParam)
        {
            GCHandle gcChildhandlesList = GCHandle.FromIntPtr(lParam);

            if (gcChildhandlesList == null || gcChildhandlesList.Target == null)
            {
                return false;
            }

            List<IntPtr> childHandles_ = gcChildhandlesList.Target as List<IntPtr>;

            StringBuilder sbClass = new StringBuilder(256);
            GetClassName(hWnd, sbClass, sbClass.Capacity);
            var className = sbClass.ToString();

            // Get the Window Title text.
            int txtLength = SendMessage(hWnd, (IntPtr)WM_GETTEXTLENGTH, IntPtr.Zero, null);
            StringBuilder sbText = new StringBuilder(txtLength + 1);
            SendMessage(hWnd, (IntPtr)WM_GETTEXT, (IntPtr)sbText.Capacity, sbText);
            string windowTitleText = sbText.ToString();

            // get the parent window (possibly NULL!)
            IntPtr hWndParent = GetParent(hWnd);

            // get the parent window's class and title (if available)
            string parentClassName = "", parentTitleText = "";
            if (!IntPtr.Zero.Equals(hWndParent))
            {
                sbClass = new StringBuilder(256);
                GetClassName(hWndParent, sbClass, sbClass.Capacity);
                parentClassName = sbClass.ToString();

                txtLength = SendMessage(hWnd, (IntPtr)WM_GETTEXTLENGTH, IntPtr.Zero, null);
                sbText = new StringBuilder(txtLength + 1);
                SendMessage(hWnd, (IntPtr)WM_GETTEXT, (IntPtr)sbText.Capacity, sbText);
                parentTitleText = sbText.ToString();
            }

            if (FilterWindow(hWnd, className, windowTitleText, hWndParent, parentClassName, parentTitleText))
                childHandles_.Add(hWnd);

            return ContinueEnumerating();
        }

        /// <summary>
        /// Overwrite this function to limit which child windows are added to the final list.
        /// </summary>
        /// <param name="hWndChild"></param>
        /// <returns></returns>
        public virtual bool FilterWindow(IntPtr hWndChild, string className, string windowTitleText, IntPtr hWndParent, string parentClassName, string parentTitleText)
        {
            return true;
        }

        /// <summary>
        /// Overwrite to provide a criterion for stopping the enumeration after you've found the child window(s) you were looking for.
        /// </summary>
        /// <returns></returns>
        public virtual bool ContinueEnumerating()
        {
            return true;
        }

    }
}

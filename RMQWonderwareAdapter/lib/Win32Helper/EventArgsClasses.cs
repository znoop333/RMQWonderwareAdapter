using System;
using System.Collections.Generic;
using System.Text;

namespace Win32Helper
{
    public class PrintingStatusEventArgs : EventArgs
    {
        public string Status { get; set; }
        public string Message { get; set; }
    }

    public class EventArgsString : EventArgs
    {
        public string Message { get; set; }
    }

    public class EventArgsBool : EventArgs
    {
        public bool b { get; set; }
    }

}

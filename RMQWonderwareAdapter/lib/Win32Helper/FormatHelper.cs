using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Win32Helper
{
    class FormatHelper
    {
        public static string FormatHexByteArray(byte[] b1, string separator = "")
        {
            return string.Join(separator, (from z in b1 select z.ToString("X2")).ToArray());
        }

        public static string FormatLocalTimestamp(DateTime timestamp)
        {
            return timestamp.ToString("yyyy-MM-dd HH:mm:ss");
        }

    }
}

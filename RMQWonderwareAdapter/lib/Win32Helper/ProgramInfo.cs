using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using CSharp411;
using System.Reflection;

namespace Win32Helper
{
    class ProgramInfo
    {
        public static DateTime Boottime
        {
            get
            {
                PerformanceCounter systemUpTime = new PerformanceCounter("System", "System Up Time");

                TimeSpan upTimeSpan = TimeSpan.FromSeconds(systemUpTime.NextValue());

                return DateTime.Now.Subtract(upTimeSpan);
            }
        }

        public static DateTime Loadtime
        {
            get
            {
                PerformanceCounter systemUpTime = new PerformanceCounter("System", "System Up Time");

                TimeSpan upTimeSpan = TimeSpan.FromSeconds(systemUpTime.NextValue());

                return DateTime.Now.Subtract(upTimeSpan);
            }
        }

        // get a friendly product string like "Windows 8 Pro", not "Microsoft Windows NT 6.2.9200.0" from Environment.OSVersion
        public static string WinInfo
        {
            get {
                return OSInfo.Name + " " + OSInfo.Edition + " " + OSInfo.Bits.ToString() +"bit " + OSInfo.ServicePack;
            }
            //get
            //{
            //    var name = (from x in new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem").Get().OfType<ManagementObject>()
            //                select x.GetPropertyValue("Caption")).FirstOrDefault();
            //    return name != null ? name.ToString() : "Unknown";
            //}
        }

        private System.Reflection.Assembly assembly;
        private FileVersionInfo fvi; 
        public ProgramInfo()
        {
            assembly = System.Reflection.Assembly.GetExecutingAssembly();
            fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
        }

        public string Filename
        {
            get
            {
                return assembly.Location;
            }
        }

        public string FileDescription
        {
            get
            {
                return fvi.FileDescription;
            }
        }

        public string FileVersion
        {
            get
            {
                return fvi.FileVersion;
            }
        }

        public string FileComments
        {
            get
            {
                return fvi.Comments;
            }
        }

        public string IP
        {
            get
            {
                IPHostEntry host;
                host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
                return "";
            }
        }

        public static T GetAssemblyAttribute<T>(Assembly assembly) where T : Attribute
        {

            if (assembly == null)
                return null;

            object[] attributes = assembly.GetCustomAttributes(typeof(T), true);

            if (attributes == null)
                return null;

            if (attributes.Length == 0)
                return null;

            return (T)attributes[0];

        }

        public static string GetMyGUID()
        {
            var attr = GetAssemblyAttribute<System.Runtime.InteropServices.GuidAttribute>(System.Reflection.Assembly.GetExecutingAssembly());
            return attr.Value;
        }





    }
}

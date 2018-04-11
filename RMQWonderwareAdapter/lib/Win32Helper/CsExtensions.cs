using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace Win32Helper
{
    public static class CsExtensions
    {
        /// <summary>e.g., List<int> ints = new List<int>( new[] {1,5,7}); bool isIn = 5.In(ints);</summary> 
        public static bool In<T>(this T o, params T[] values)
        {
            if (values == null) 
                return false;
            return values.Contains(o);
        }

        /// e.g., bool isIn = 5.In(1,2,3,4,5);
        public static bool In<T>(this T o, IEnumerable<T> values)
        {
            if (values == null) return false;

            return values.Contains(o);
        }        

        /// e.g., "123".Left(2) returns "12"
        public static string Left(this string str, int length)
        {
            str = (str ?? string.Empty);
            return str.Substring(0, Math.Min(length, str.Length));
        }

        public static string Right(this string str, int length)
        {
            str = (str ?? string.Empty);
            return (str.Length >= length)
                ? str.Substring(str.Length - length, length)
                : str;
        }

        /// "5,3,5,63,0".Split(",")
        public static string[] Split(this string value, string regexPattern, RegexOptions options)
        {
            return Regex.Split(value, regexPattern, options);
        }
    public static string[] Split(this string value, string regexPattern)
        {
            return Regex.Split(value, regexPattern, RegexOptions.None);
        }


        // see https://stackoverflow.com/questions/2367718/automating-the-invokerequired-code-pattern

        public delegate void InvokeIfRequiredDelegate<T>(T obj)
            where T : ISynchronizeInvoke;

        public static void InvokeIfRequired<T>(this T obj, InvokeIfRequiredDelegate<T> action)
            where T : ISynchronizeInvoke
        {
            if (obj.InvokeRequired)
            {
                obj.Invoke(action, new object[] { obj });
            }
            else
            {
                action(obj);
            }
        }


        public static Task Delay(double milliseconds)
        {
            var tcs = new TaskCompletionSource<bool>();
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += (obj, args) =>
            {
                tcs.TrySetResult(true);
            };
            timer.Interval = milliseconds;
            timer.AutoReset = false;
            timer.Start();
            return tcs.Task;
        }

        

    }
}

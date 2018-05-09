using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Farm.Core.Common
{
    internal static class Extensions
    {
        private static readonly Dictionary<object, Type> CacheTypes = new Dictionary<object, Type>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Type GetTypeOptimized<T>(this T Object)
        {
            return Object.GetType();
            //var str = Object.ToString();
            //if (!CacheTypes.ContainsKey(str))
            //    CacheTypes[str] = Object.GetType();
            //return CacheTypes[str];
        }

        public static Dictionary<string, string> ParseParameter(this string data)
        {
            var regex = new Regex(@"(\?|\&)([^=]+)\=([^&]+)", RegexOptions.Compiled);
            var result = new Dictionary<string, string>();
            foreach (Match groups in regex.Matches(data))
                result[groups.Groups[2].Value] = groups.Groups[3].Value;
            return result;
        }

        public static byte[] ToByte(this string data, Encoding encoding = null) => (encoding ?? Encoding.UTF8).GetBytes(data);
    }
}

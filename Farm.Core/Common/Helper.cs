using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management;
using System.Threading;

namespace Farm.Core.Common
{
    internal static class Helper
    {
        public static IEnumerable<(string Card, Guid guid, string Path)> GetCards()
        {
            var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController");
            var listCards = new List<object>();
            foreach (var queryObj in searcher.Get())
            {
                yield return ($"{queryObj["name"]}", Guid.Empty, "" );
            }
        }

        public static void AwaitDebugger()
        {
            while(!Debugger.IsAttached)
                Thread.Sleep(0);
        }
    }
}

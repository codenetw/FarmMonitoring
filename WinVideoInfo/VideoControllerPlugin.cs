using System;
using System.Collections.Generic;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PluginManager;

namespace WinVideoInfo
{
    public sealed class VideoControllerPlugin : IFarmPlugin
    {
        public PluginType Type => PluginType.CardInformation;
        public string Name => nameof(VideoControllerPlugin);
        public async Task<object> Execute(CancellationToken ctx, params string[] parameters)
        {
            ctx.ThrowIfCancellationRequested();
            var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_VideoController");
            var listCards = new List<object>();
            foreach (var queryObj in searcher.Get())
            {
                listCards.Add(new
                {
                    Name = queryObj["name"],
                    Status = queryObj["status"],
                    DriverVersion = queryObj["driverversion"],
                    DeviceId = queryObj["pnpdeviceid"],
                    IsActive = (queryObj["CurrentBitsPerPixel"] != null && queryObj["CurrentHorizontalResolution"] != null)
                });
            }
            return await Task.FromResult(listCards);
        }
        
        public void Dispose()
        {
        }
    }
}

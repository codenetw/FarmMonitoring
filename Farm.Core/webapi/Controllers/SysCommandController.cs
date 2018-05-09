using System;
using System.Reflection;
using Farm.Core.Common;
using Farm.Core.Common.DisableDevice;
using log4net;
using Nancy;

namespace Farm.Core.webapi.Controllers
{
    public sealed class SysCommandController : NancyModule
    {
        private readonly Configuration _configuration;

        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public SysCommandController(Configuration configuration)
        {
            _configuration =
                configuration ?? throw new ArgumentNullException($"{nameof(configuration)} must be define");
        
            Get("/resetcards",x => ResetCards());
            Get("/reboot", x => Reboot());
        }
        
        private object Reboot()
        {
            try
            {
                NativePowerMethods.Reboot();
                return "OK REBOOT";
            }
            catch
            {
                return "ERROR REBOOT";
            }
        }

        private object ResetCards()
        {
            try
            {
                if (_configuration.Cards.Length == 0)
                {
                    _logger.Info($"ResetCards : CARD LIST EMPTY");
                    return "CARD LIST EMPTY";
                }

                foreach (var card in _configuration.Cards)
                {
                    _logger.Info($"DISABLE CARD ID: {card.GUID}");
                    DeviceHelper.SetDeviceEnabled(new Guid(card.GUID), card.Path, false);
                }

                foreach (var card in _configuration.Cards)
                {
                    _logger.Info($"ENABLE CARD ID: {card.GUID}");
                    DeviceHelper.SetDeviceEnabled(new Guid(card.GUID), card.Path, true);
                }

                return "OK RESET";
            }
            catch
            {
                return "ERROR RESET";
            }
        }
    }
}

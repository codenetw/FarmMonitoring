using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using FarmMonitoring.backend.Common;
using FarmMonitoring.backend.Common.DisableDevice;
using log4net;
using Nancy;

namespace FarmMonitoring.webapi.Controllers
{
    public sealed class CmdController : NancyModule
    {
        private readonly Configuration _configuration;

        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public CmdController(Configuration configuration)
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

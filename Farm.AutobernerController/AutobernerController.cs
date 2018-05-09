using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Farm.AutobernerController.Model;
using Farm.BaseController.CommunicationMessage;
using Farm.BaseController.CommunicationMessageModel;
using Farm.MessageBus;
using Farm.MessageBus.MessageBus;
using log4net;
using MSI.Afterburner;

namespace Farm.AutobernerController
{
    public sealed class AutobernerController : ICommunicationBase, IAutobernerController, IDisposable
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static volatile ManualResetEvent _lockEvent = new ManualResetEvent(true);
        private static CancellationTokenSource _token => new CancellationTokenSource();
        private readonly MessageCommunicationFactory  _messageBusFactory;
        private readonly AutobernerConfigure _config;

        public string Name => "autoberner";

        public AutobernerController(AutobernerConfigure configuration, MessageCommunicationFactory messageCommunicationFactory)
        {
            _messageBusFactory = messageCommunicationFactory;
            _config = configuration;
        }

        public void Start()
        {
            var messageBus = _messageBusFactory.Create();
            _logger.Info("AutobernerController started");

            var result = messageBus.Execute<ProcessStatus, CheckProcess>(new CheckProcess { Name = _config.DependencyProcess.Name }, "process");
            if (!result.Ready)
                messageBus.Execute(new StartProcess { Process = _config.DependencyProcess });

            CardsMonitoring();
        }
        public void Stop()
        {
            _token.Cancel();
            _logger.Info("AutobernerController stopped");

        }
        public void Dispose()
        {
            _token.Cancel();
        }

        #region listener message

        public void Handle(ProcessStatus procesStatus)
        {
            var requiredProcName = _config.DependencyProcess.Name;
            if (!procesStatus.IsName(requiredProcName))
                return;

            if (procesStatus.Status == MessageStatus.Info  && !procesStatus.Ready)
                _lockEvent.Reset(); //pause
            

            if (procesStatus.Status == MessageStatus.Info && procesStatus.Ready)
                _lockEvent.Set(); //restore
        }

        public void Handle(AutobernerResetCardMessage resetCardMessage)
        {
            ResetCard(resetCardMessage.CardsPramsModels);
        }

        #endregion

        #region monitoring 

        private void CardsMonitoring()
        {
            var messageBus = _messageBusFactory.Create();
            Task.Factory.StartNew(async () =>
                {
                    do
                    {
                        _lockEvent.WaitOne();
                        try
                        {
                            var ctrlGpu = new ControlMemory();
                            var voltageMonitor = new HardwareMonitor();
                            var voltage = new List<VoltageInfo>();
                            var temp = new List<TempInfo>();

                            foreach (var vol in voltageMonitor.Entries.Where(x => x.SrcId == 96))
                                voltage.Add(new VoltageInfo {Current = vol.Data, Max = vol.MaxLimit, CardId = vol.GPU});
                            messageBus.Handle(new AutobernerVoltageCardsInfoMessage { VoltageInfoModel = voltage.ToArray() });

                            foreach (var vol in voltageMonitor.Entries.Where(x => x.SrcId == 0))
                                temp.Add(new TempInfo { Current = vol.Data, CardId = vol.GPU });
                            messageBus.Handle(new AutobernerTempCardsInfoMessage { TempInfoModel = temp.ToArray() });

                            ctrlGpu.Connect();
                            ctrlGpu.ReloadAll();
                            ctrlGpu.Reinitialize();

                            var actualList = FindCards(ctrlGpu);

                            messageBus.Handle(new AutobernerInformationMessage {CurrentInfoCards = actualList.ToList()});
                            ctrlGpu.Disconnect();
                        }
                        catch (Exception ex)
                        {
                           _logger.Error($"CardsMonitoring error: {ex.Message}");
                        }
                        await Task.Delay(_config.MonitoringInterval, _token.Token);
                    } while (!_token.IsCancellationRequested);
                }, _token.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default)
                .ContinueWith((x) =>
                {
                    messageBus.Handle<AutobernerStopWatchingMessage>();
                },
                TaskContinuationOptions.OnlyOnFaulted);
        }

        private static IEnumerable<CardParam> FindCards(ControlMemory ctlm)
        {
            const int divider = 1000;
            var cardCount = ctlm.GpuEntries.Length;
            var cards = new CardParam[cardCount];

            for (uint i = 0; i < cardCount; i++)
            {
                var v1 = ctlm.GpuEntries[i];

                var flags = v1.Flags;
                var voltageBoost = flags.HasFlag(MACM_SHARED_MEMORY_GPU_ENTRY_FLAG.CORE_VOLTAGE_BOOST);
                var voltage = new CardMinMaxCurrent
                {
                    Current = voltageBoost ? v1.CoreVoltageBoostCur : (int) v1.CoreVoltageCur,
                    Max = voltageBoost ? v1.CoreVoltageBoostMax : (int) v1.CoreVoltageMax,
                    Min = voltageBoost ? v1.CoreVoltageBoostMin : (int) v1.CoreVoltageMin
                };
                var templimit = new CardMinMaxCurrent
                {
                    Current = v1.ThermalLimitCur,
                    Max = v1.ThermalLimitMax,
                    Min = v1.ThermalLimitMin
                };
                var voltagelimit = new CardMinMaxCurrent
                {
                    Current = v1.PowerLimitCur,
                    Max = v1.PowerLimitMax,
                    Min = v1.PowerLimitMin
                };
                var coreclockBoost = flags.HasFlag(MACM_SHARED_MEMORY_GPU_ENTRY_FLAG.CORE_CLOCK_BOOST);
                var coreclock = new CardMinMaxCurrent
                {
                    Current = coreclockBoost ? v1.CoreClockBoostCur / divider : (int) v1.CoreClockCur / divider,
                    Max = coreclockBoost ? v1.CoreClockBoostMax / divider : (int) v1.CoreClockMax / divider,
                    Min = coreclockBoost ? v1.CoreClockBoostMin / divider : (int) v1.CoreClockMin / divider
                };
                var memoryclockBoost = flags.HasFlag(MACM_SHARED_MEMORY_GPU_ENTRY_FLAG.MEMORY_CLOCK_BOOST);
                var memoryclock = new CardMinMaxCurrent
                {
                    Current = memoryclockBoost ? v1.MemoryClockBoostCur / divider : (int) v1.MemoryClockCur / divider,
                    Max = memoryclockBoost ? v1.MemoryClockBoostMax / divider : (int) v1.MemoryClockMax / divider,
                    Min = memoryclockBoost ? v1.MemoryClockBoostMin / divider : (int) v1.MemoryClockMin / divider
                };

                var fancontrol =
                    new CardMinMaxCurrent {Current = v1.FanSpeedCur, Max = v1.FanSpeedMax, Min = v1.FanSpeedMin};
                var ismaster = v1.IsMaster;
                cards[i] = new CardParam(v1.Index, ismaster, voltage,
                    templimit, voltagelimit, coreclock, memoryclock, fancontrol);

            }

            return cards;
        }

        #endregion
      
        private void ResetCard(IEnumerable<CardPramsMessageModel> actualListCards)
        {
            try
            {
                if (_config.ReadOnlyCards)
                {
                    _logger.Debug($"[WD] autobernerController cards readonly");
                    return;
                }

                foreach (var card in actualListCards)
                    RestoreCard(card);
            }
            catch (Exception ex)
            {
                _logger.Error("[WD] autobernerController", ex);
            }
        }

        private void RestoreCard(CardPramsMessageModel card)
        {
            const int divider = 1000;
            var messageBus = _messageBusFactory.Create();
            try
            {
                var lmacm = new ControlMemory();
                lmacm.Connect();
                
                var flags = lmacm.GpuEntries[card.Id].Flags;

                var voltageBoost = flags.HasFlag(MACM_SHARED_MEMORY_GPU_ENTRY_FLAG.CORE_VOLTAGE_BOOST);
                var coreclockBoost = flags.HasFlag(MACM_SHARED_MEMORY_GPU_ENTRY_FLAG.CORE_CLOCK_BOOST);
                var memoryclockBoost = flags.HasFlag(MACM_SHARED_MEMORY_GPU_ENTRY_FLAG.MEMORY_CLOCK_BOOST);


                lmacm.GpuEntries[card.Id].ThermalLimitCur = (int)card.TempLimit;
                lmacm.GpuEntries[card.Id].PowerLimitCur = (int)card.PowerLimit;
                lmacm.GpuEntries[card.Id].FanSpeedCur = (uint)card.FanSpeed;

                if (voltageBoost)
                    lmacm.GpuEntries[card.Id].CoreVoltageBoostCur = (int)card.Voltage;
                else
                    lmacm.GpuEntries[card.Id].CoreVoltageCur = (uint)card.Voltage;

                if (coreclockBoost)
                    lmacm.GpuEntries[card.Id].CoreClockBoostCur = (int)card.CoreClock * divider;
                else
                    lmacm.GpuEntries[card.Id].CoreClockCur = (uint)card.CoreClock * divider;

                if (memoryclockBoost)
                    lmacm.GpuEntries[card.Id].MemoryClockBoostCur = (int)card.MemoryClock * divider;
                else
                    lmacm.GpuEntries[card.Id].MemoryClockCur = (uint)card.MemoryClock * divider;


                lmacm.CommitChanges();
                lmacm.Disconnect();

                if (_logger.IsWarnEnabled)
                    _logger.Warn($"Restore card > {card}");
                messageBus.Handle(new AutobernerWatchdogResetMessage { Status = MessageStatus.Ok});
            }
            catch (Exception ex)
            {
                _logger.Error($"inner exception: {ex.InnerException?.Message ?? string.Empty} > exception: {ex.Message}", ex);
                messageBus.Handle(new AutobernerWatchdogResetMessage { Status = MessageStatus.Error });
            }
        }
    }
}
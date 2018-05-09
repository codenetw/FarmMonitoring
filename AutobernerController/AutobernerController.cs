using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AutobernerController.Model;
using BaseController;
using BaseController.CommunicationMessage;
using log4net;
using MessageBus;
using MessageBus.MessageBus;
using MSI.Afterburner;

namespace AutobernerController
{
    public sealed class AutobernerController : ICommunicationBase, IAutobernerController, IDisposable
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static CancellationTokenSource _token => new CancellationTokenSource();
        private static volatile ManualResetEvent _lockEvent = new ManualResetEvent(true);
        private readonly MessageCommunicationFactory  _messageBusFactory;
        private readonly AutobernerConfigure _config;


        public AutobernerController(AutobernerConfigure configuration, ILog logger, MessageCommunicationFactory messageCommunicationFactory)
        {
            _messageBusFactory = messageCommunicationFactory;
            _config = configuration;
        }


        #region listener message

        public void Handle(PrcessStatus procesStatus)
        {
            var requiredProcName = _config.DependencyProcess.Name;
            if (!procesStatus.PrcessName(requiredProcName))
                return;

            if (procesStatus.Status == MessageStatus.Info  && !procesStatus.Ready)
                _lockEvent.Reset(); //pause
            

            if (procesStatus.Status == MessageStatus.Info && procesStatus.Ready)
                _lockEvent.Set(); //restore
        }

        #endregion

        #region monitoring 

        private void CardsMonitoring(CardParam[] configCards)
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
                            ctrlGpu.Connect();
                            ctrlGpu.ReloadAll();
                            ctrlGpu.Reinitialize();

                            var actualList = FindCards(ctrlGpu);
                       
                            ctrlGpu.Disconnect();

                            if (_config.WatchDog)
                                WatchDog(configCards, actualList);
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
                    messageBus.Publish<AutobernerStopWatchingMessage>();
                },
                TaskContinuationOptions.OnlyOnFaulted);
        }

        private static CardParam[] FindCards(ControlMemory ctlm)
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
        

        private static CardParam[] NotValidCards(IEnumerable<CardParam> cardsInConfig, IEnumerable<CardParam> cardsInWork) => cardsInConfig.Except(cardsInWork).ToArray();

        private void WatchDog(CardParam[] configCards, CardParam[] actualListCards)
        {
            try
            {
                var invalidCards = NotValidCards(configCards, actualListCards);

                _logger.Info($"[WD] autobernerController check cards - {(invalidCards.Length > 0 ? "restore" : "OK")}");
                foreach (var invalidCard in invalidCards)
                    RestoreCard(configCards, invalidCard);
            }
            catch (Exception ex)
            {
                _logger.Error("[WD] autobernerController", ex);
            }
        }

        private void RestoreCard(CardParam[] configCards, CardParam card)
        {
            const int divider = 1000;
            var messageBus = _messageBusFactory.Create();
            try
            {
                if (card == null)
                {
                    _logger.Error("card for restore is null!, ret <");
                    return;
                }

                var lmacm = new ControlMemory();
                lmacm.Connect();

                var configCardParam = configCards.FirstOrDefault(x => x.Id == card.Id);

                if (configCardParam == null)
                {
                    if (_logger.IsDebugEnabled)
                        _logger.Debug($"Try restore card > {card} - error > not found in config");
                    return;
                }

                var flags = lmacm.GpuEntries[card.Id].Flags;

                var voltageBoost = flags.HasFlag(MACM_SHARED_MEMORY_GPU_ENTRY_FLAG.CORE_VOLTAGE_BOOST);
                var coreclockBoost = flags.HasFlag(MACM_SHARED_MEMORY_GPU_ENTRY_FLAG.CORE_CLOCK_BOOST);
                var memoryclockBoost = flags.HasFlag(MACM_SHARED_MEMORY_GPU_ENTRY_FLAG.MEMORY_CLOCK_BOOST);


                lmacm.GpuEntries[card.Id].ThermalLimitCur = (int)configCardParam.TempLimit.Current;
                lmacm.GpuEntries[card.Id].PowerLimitCur = (int)configCardParam.PowerLimit.Current;
                lmacm.GpuEntries[card.Id].FanSpeedCur = (uint)configCardParam.FanSpeed.Current;

                if (voltageBoost)
                    lmacm.GpuEntries[card.Id].CoreVoltageBoostCur = (int)configCardParam.Voltage.Current;
                else
                    lmacm.GpuEntries[card.Id].CoreVoltageCur = (uint)configCardParam.Voltage.Current;

                if (coreclockBoost)
                    lmacm.GpuEntries[card.Id].CoreClockBoostCur = (int)configCardParam.CoreClock.Current * divider;
                else
                    lmacm.GpuEntries[card.Id].CoreClockCur = (uint)configCardParam.CoreClock.Current * divider;

                if (memoryclockBoost)
                    lmacm.GpuEntries[card.Id].MemoryClockBoostCur = (int)configCardParam.MemoryClock.Current * divider;
                else
                    lmacm.GpuEntries[card.Id].MemoryClockCur = (uint)configCardParam.MemoryClock.Current * divider;


                lmacm.CommitChanges();
                lmacm.Disconnect();

                if (_logger.IsWarnEnabled)
                    _logger.Warn($"Restore card > {card}");
                messageBus.Publish(new AutobernerWatchdogResetMessage { Status = MessageStatus.Ok});
            }
            catch (Exception ex)
            {
                _logger.Error($"inner exception: {ex.InnerException.Message} > exception: {ex.Message}", ex);
                messageBus.Publish(new AutobernerWatchdogResetMessage { Status = MessageStatus.Error });
            }
        }
   
        /// <summary>
        /// Модуль зависит от работы AutobernerController
        /// отправляем запрос на состояние процесса 
        /// и запускаем если он не работает
        /// </summary>
        public void Start()
        {
            var messageBus = _messageBusFactory.Create();
            var requiredProcName = _config.DependencyProcess.Name;
            _logger.Info("AutobernerController started");
            
            var result = messageBus.Execute<PrcessStatus, CheckProcess,ICommunicationBase>(new CheckProcess { Name = _config.DependencyProcess.Name } );
            if(!result.Ready)
                messageBus.Execute<StartProcess, ICommunicationBase> (new StartProcess { Process = _config.DependencyProcess });
            CardsMonitoring(_config.ActualConfigCards.ToArray());
            
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
    }
}
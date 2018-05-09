using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Farm.BaseController.CommunicationMessage;
using Farm.MessageBus;
using Farm.MessageBus.MessageBus;
using log4net;

namespace Farm.ProcessController
{
    /// <summary>
    /// Модуль работает по запросу от других воркеров
    /// создает, завершает, отслеживает процессы
    /// </summary>
    public class ProcessController : ICommunicationBase, IProcessController
    {
        private readonly MessageCommunicationFactory _messageCommunicationFactory;
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static CancellationTokenSource _token => new CancellationTokenSource();

        public string Name => "process";

        private readonly ProcessConfigure _config;
        private const int waitTimeOut = 5000;
        private readonly TimeSpan _watchDogTimeout = TimeSpan.FromSeconds(10);
        private static readonly ConcurrentDictionary<string, bool> _watchDogFlags = new ConcurrentDictionary<string, bool>();
        public ProcessController(ProcessConfigure configuration, MessageCommunicationFactory messageCommunicationFactory)
        {
            _messageCommunicationFactory = messageCommunicationFactory;
            _config = configuration;
        }

        #region listener message
        
        public void Execute(StartProcess startProcess)
        {
            Start(startProcess.Process);
        }

        public List<string> Query(ProcessList startProcess)
        {
            return Process.GetProcesses().Select(x => x.ProcessName).ToList();
        }

        public void Handle(CheckProcess checkProcess)
        {
            var result = CheckPrcessStatus(checkProcess);
            var messanger = _messageCommunicationFactory.Create();
            result.InitModule = checkProcess.IsInitModule;
            messanger.Handle(result);
        }

        public ProcessStatus Execute(CheckProcess checkProcess)
        {
            return CheckPrcessStatus(checkProcess);
        }

        public ProcessStatus Execute(RestartProcess rebootProcess)
        {
            Kill(rebootProcess.Process.Name);
            Start(rebootProcess.Process);
            return CheckPrcessStatus(new CheckProcess{ Name = rebootProcess.Process.Name });
        }

        #endregion


        public void Start()
        {
            _logger.Info("ProcessController started > await commands");
        }

        public void Stop()
        {
            _logger.Info("ProcessController stoped > send cancel signal");
            _token.Cancel();
        }


        private ProcessStatus CheckPrcessStatus(CheckProcess checkProcess)
        {
            
            var processInfoResult = new ProcessStatus();
            var proc = Process.GetProcesses().FirstOrDefault(x => string.Compare(checkProcess.Name, x.ProcessName, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase) == 0);
            processInfoResult.Name = checkProcess.Name;

            if ((!_watchDogFlags.ContainsKey(checkProcess.Name) || _watchDogFlags[checkProcess.Name] == false) && proc != null)
                _logger.Warn($"processController {checkProcess.Name} without WD, need restart app");

            processInfoResult.StatusProcess =
                proc != null ? ProcessStatus.ProcStatus.Running : ProcessStatus.ProcStatus.NotRunning;
            return processInfoResult;
        }

    
        public Task Start(ProcessInfo procInfo)
        {
            try
            {
                SetEnvVariable(procInfo.EnvVariable);

                var info = new ProcessStartInfo(procInfo.Exec, procInfo.Parameters)
                {
                    CreateNoWindow = false,
                    UseShellExecute = true
                };

                var myProcess = System.Diagnostics.Process.Start(info);

                if (_logger.IsInfoEnabled)
                    _logger.Warn($"processController: {procInfo.Exec} started");

                WatchDog(myProcess, procInfo);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            finally
            {
                if (_logger.IsInfoEnabled)
                    _logger.Info($"try start : {procInfo.Exec} ");
            }
            return Task.CompletedTask;
        }

        private void WatchDog(System.Diagnostics.Process proc, ProcessInfo procInfo = null)
        {
            if (procInfo == null || procInfo.WatchDog)
            {
                if (_logger.IsInfoEnabled)
                {
                    _logger.Warn(procInfo == null
                        ? $"attach [WD] on processController :{proc.ProcessName}"
                        : $"start [WD] on :{procInfo.Exec}");
                }

                var proLocInf = procInfo;
                var bus = _messageCommunicationFactory.Create();
                Task.Factory.StartNew(async () =>
                {
                    var mylocInfo = proc;
                    _watchDogFlags[proc.ProcessName] = true;
                    do
                    {
                        do
                        {
                            if (mylocInfo.HasExited)
                            {
                                _logger.Warn($"[WD] Process: {proc.ProcessName} >  exit time {proc.ExitTime} ");
                                bus.Handle(new ProcessStatus { Name = proLocInf.Name ,StatusProcess = ProcessStatus.ProcStatus.NotRunning, Status = MessageStatus.Info});
                                break;
                            }

                            if (!mylocInfo.Responding)
                            {
                                try
                                {
                                    mylocInfo.WaitForExit(waitTimeOut);
                                    bus.Handle(new ProcessStatus { Name = proLocInf.Name, StatusProcess = ProcessStatus.ProcStatus.NotResponding , Status = MessageStatus.Info });
                                    _watchDogFlags[mylocInfo.ProcessName] = false;
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error($"[WD] Try kill processController: {proc.ProcessName} > error {ex.Message} ");
                                }
                                break;
                            }
                            await Task.Delay(_watchDogTimeout, _token.Token);
                        } while (!_token.IsCancellationRequested);
                        await Task.Delay(10000, _token.Token);
                        SetEnvVariable(proLocInf.EnvVariable);
                        var info = new ProcessStartInfo(proLocInf.Exec, proLocInf.Parameters)
                        {
                            CreateNoWindow = false,
                            UseShellExecute = true
                        };
                        mylocInfo = System.Diagnostics.Process.Start(info);
                        _logger.Info($"{procInfo.Exec} is started");
                        _watchDogFlags[mylocInfo.ProcessName] = true;
                        bus.Handle(new ProcessStatus { Name = proLocInf.Name, StatusProcess = ProcessStatus.ProcStatus.Running, Status = MessageStatus.Info });
                    } while (!_token.IsCancellationRequested);
                }, _token.Token);
            }
        }

        private void SetEnvVariable(Dictionary<string, string> envProc)
        {
            foreach (var env in envProc)
                Environment.SetEnvironmentVariable(env.Key, env.Value);
        }

        public void Kill(params string[] procStartList)
        {
            var task = Task.Run(async () =>
            {
                foreach (var process in System.Diagnostics.Process.GetProcesses()
                    .Where(x => procStartList.Contains(Path.GetFileName(x.ProcessName))))
                {
                    _logger.Warn($"Process: {process.ProcessName} send > KILL SIGNAL");
                    try
                    {
                        foreach (ProcessThread thread in process.Threads)
                        {
                            thread.Dispose();
                        }
                        process.Kill();
                        
                        while (System.Diagnostics.Process.GetProcesses().Select(x => x.ProcessName).Contains(process.ProcessName))
                            await Task.Delay(TimeSpan.FromSeconds(2));

                        _logger.Warn($"Process: {process.ProcessName} has died");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                    }

                }
            });
            task.Wait(TimeSpan.FromSeconds(30));
        }
    }
}
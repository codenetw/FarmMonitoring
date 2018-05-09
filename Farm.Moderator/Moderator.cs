using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Farm.BaseController.CommunicationMessage;
using Farm.MessageBus.MessageBus;
using Farm.Moderator.ConditionModels;
using Farm.Moderator.Helper;
using log4net;

namespace Farm.Moderator
{
    public sealed class Moderator : IModerator
    {
        private readonly IFarmVisitor<Result> _visitor;
        private readonly ModeratorConfigure _configure;
        private readonly MessageCommunicationFactory _messageCommunicationFactory;
        private readonly CancellationTokenSource _ctx = new CancellationTokenSource();
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static AggregatorState _currentState = null;

        public Moderator(IFarmVisitor<Result> visitor
            , ModeratorConfigure configure
            , MessageCommunicationFactory messageCommunicationFactory)
        {
            _visitor = visitor;
            _configure = configure;
            _messageCommunicationFactory = messageCommunicationFactory;
        }

        public void Start() => Task.Factory.StartNew(async (obj) =>
        {
            var bus = _messageCommunicationFactory.Create();
            do
            {
                if (_currentState != null)
                {
                    foreach (var condition in _configure.Condtitions.Where(x => !x.IsLock()).AsParallel())
                    {
                        condition.Lock(bus, TimeSpan.FromMinutes(1));

                        var listResult = new List<string>(0);
                        var resultAggregate = true;
                        foreach (var model in condition.Model)
                        {
                            var resut = _visitor.Validate(model, _currentState);
                            listResult.AddRange(resut.AdditionInfo);
                            resultAggregate &= resut.IsValid;
                        }

                        if (resultAggregate)
                        {
                            foreach (var exec in condition.PositiveExec)
                            {
                                bus.Execute(exec);   
                            }
                        }
                        else
                        {
                            foreach (var exec in condition.NegativeExec)
                            {
                                bus.Execute(exec);
                            }
                        }
                        
                        condition.UnLock();

                        if (listResult.Any())
                            listResult.ForEach(_logger.Warn);
                    }
                }
                await Task.Delay(_configure.CheckInterval);
            } while (_ctx.IsCancellationRequested);
        }, _ctx, TaskCreationOptions.LongRunning);

        public void Stop() => _ctx.Cancel();

        public void UpdateState(AggregatorState currentState)
        {
            _currentState = currentState;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Farm.BaseController;
using Farm.BaseController.CommunicationMessage;
using Farm.MessageBus;
using Farm.MessageBus.MessageBus;

namespace Farm.MinerController
{
    public class ModeratorFlowCommands : ICommunicationBase
    {
        private readonly MessageCommunicationFactory _messageCommunicationFactory;
        private readonly MinerConfigure _configure;
        public string Name => "Miner moderator";

        #region listener message

        public ModeratorFlowCommands(MessageCommunicationFactory messageCommunicationFactory
                                , MinerConfigure configure)
        {
            _messageCommunicationFactory = messageCommunicationFactory;
            _configure = configure;
        }

        public ModeratorResultStatus Execute(ModeratorChangeCardActive resetAdapter)
        {
            var tryRestartCount = 5;
            var tryExecuteMessage = 5;

            var message = _messageCommunicationFactory.Create();
            ModeratorResultStatus result;
            do
            {
                result = message.Execute<ModeratorResultStatus, CardChangeStatus>(new CardChangeStatus
                {
                    IdCard = resetAdapter.IdCard,
                    State = resetAdapter.Enable
                        ? CardChangeStatus.StateGpu.MainOnly
                        : CardChangeStatus.StateGpu.Disable
                }, "miner");

                if (ModeratorResultStatus.IsOk(result))
                    return result;
                do
                {
                    var process = message.Execute<ProcessStatus, RestartProcess>(new RestartProcess
                    {
                        Process = _configure.DependencyProcess
                    });
                    if (process.Ready)
                        break;
                    tryRestartCount--;
                } while (tryRestartCount != 0);
                
                tryExecuteMessage--;
            } while (!ModeratorResultStatus.IsOk(result) || tryExecuteMessage != 0);

            return result;
        }

        public ModeratorResultStatus Execute(ModeratorRestartMiner resetAdapter)
        {
            var tryRestartCount = 5;

            var message = _messageCommunicationFactory.Create();
            ModeratorResultStatus result;
            do
            {
                result = message.Execute<ModeratorResultStatus, RestartApiMiner>();
            } while (!ModeratorResultStatus.IsOk(result) || tryRestartCount != 0);
            return result;
        }

        #endregion
    }
}

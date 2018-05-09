using System;
using AutobernerController.Model;
using BaseController;
using BaseController.CommunicationMessage;
using FarmMonitoring.backend.Common;
using MessageBus;
using MessageBus.MessageBus;

namespace AutobernerController.webapi
{
    internal sealed class AutobernerHub : WebSocketHub<AutobernerHub>, ICommunicationBase
    {
        public AutobernerHub(AutobernerConfigure configuration) { }

        #region listener message

        /// <summary>
        /// Получает сообщения от AutobernerWatching и ретранслируем в веб сокет
        /// </summary>
        /// <param name="cardsInfo"></param>
        public void Handle(AutobernerInformationMessage cardsInfo)
        {
            if (cardsInfo.Status == MessageStatus.Ok)
                BroadcastMessage(cardsInfo);
        }

        /// <summary>
        ///  Получает сообщения от AutobernerWatching об остановке слежения за картами
        /// </summary>
        /// <param name="stopWatch"></param>
        public void Handle(AutobernerStopWatchingMessage stopWatch)
        {
            _ctxCancellationToken.Cancel();
            _logger.Warn($"{nameof(AutobernerHub)} stopped, reason -> signal AutobernerStopWatchingMessage");
        }

        #endregion

    }
}

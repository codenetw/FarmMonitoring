using Farm.BaseController.CustomError;
using Farm.MessageBus.MessageBus;

namespace Farm.BaseController
{
    public class ModeratorRestartMiner: IMessageBase { }
    public class ModeratorRestartAutoberner : IMessageBase { }
    public class ModeratorResetLan : IMessageBase { }
    public class ModeratorResetPciExpress : IMessageBase { }

    public class ModeratorResultStatus : ResultMessageBase
    {
        public ErrorCode ErrorCode { get; set; }
        public static ModeratorResultStatus Ok =>  new ModeratorResultStatus { Status = MessageStatus.Ok };
        public static ModeratorResultStatus Fail => new ModeratorResultStatus { Status = MessageStatus.Error };
        public static bool IsOk(ModeratorResultStatus obj) => obj.Status == MessageStatus.Ok ;
        public static ModeratorResultStatus FromException(FarmException ex)
        {
            return new ModeratorResultStatus
            {
                Status = MessageStatus.Error,
                ErrorCode = ex.Code
            };
        }
    }

    public class ModeratorChangeCardActive : IMessageBase
    {
        public int IdCard { get; set; }
        public bool Enable { get; set; }
    }
   
    public class ModeratorKillProces : IMessageBase
    {
        public string Name { get; set; }
    }
    public class ModeratorRestartWindows : IMessageBase { }
    public class ModeratorTimeoutOperation : IMessageBase { }
}

using MessageBus.MessageBus;

namespace MinerController
{
    public class MinerStatistics : MessageBase
    {
        public string Version { get; set; }
        public long RuntimeMinutes { get; set; }
        public decimal TotalMainShare { get; set; }
        public decimal NumberMainShare { get; set; }
        public decimal NumberRejectedMainShare { get; set; }
        public string[] HashMainRateCards { get; set; }
        public decimal TotalSecondShare { get; set; }
        public decimal NumberSecondShare { get; set; }
        public decimal NumberRejectedSecondShare { get; set; }
        public string[] HashSecondRateCards { get; set; }
        public TempAndFan[] FanSpeedAndTemp { get; set; }
    }

    public class TempAndFan
    {
        public int Temperature { get; set; }
        public int Fanspeed { get; set; }
    }

    public enum GPUCommand
    {
        Disable = 0 ,
        MainOnly = 1,
        DualMode = 2
    }

    public class MinerRequest
    {
        public int id { get; set; }
        public string jsonrpc { get; set; }
        public string method { get; set; }
        public string psw { get; set; }
        public int[] @params { get; set; }

        public static MinerRequest GetStatistics(string password)
        {
            return new MinerRequest{id = 0,jsonrpc = "2.0", method = "miner_getstat1" , psw = password};
        }
        public static MinerRequest RebootMiner(string password)
        {
            return new MinerRequest { id = 0, jsonrpc = "2.0", method = "miner_restart",psw = password };
        }
        public static MinerRequest ControlCard(string password, GPUCommand command, int gpunum = -1)
        {
            return new MinerRequest { id = 0, jsonrpc = "2.0", method = "control_gpu", psw = password , @params = new[]{gpunum, (int)command} };
        }
    }
}

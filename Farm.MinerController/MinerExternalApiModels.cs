using Farm.MessageBus.MessageBus;

namespace Farm.MinerController
{
   
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

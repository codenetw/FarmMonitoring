namespace BaseController.CommunicationMessage
{
    public struct Card
    {
        /// <summary>
        /// Номер карты
        /// </summary>
        public int Id { get; }
      
        /// <summary>
        /// Параметры карты
        /// </summary>
        public CardParam Param { get; }

        public Card(CardParam param, int id)
        {
            Param = param;
            Id = id;
        }
    
        public bool Equals(Card other)
        {
            return Id == other.Id && Equals(Param, other.Param);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Card) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id * 397) ^ (Param != null ? Param.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return $"Card Id[{Id}] " +
                   $"IsMaster[{Param.IsMaster}] " +
                   $"CoreVoltage:[{Param.Voltage}] " +
                   $"TempLimit:[{Param.TempLimit}]" +
                   $"PowerLimit:[{Param.PowerLimit}]" +
                   $"CoreClock:[{Param.CoreClock}]" +
                   $"MemoryClock:[{Param.MemoryClock}]"+
                   $"FanSpeed:[{Param.FanSpeed}]";
        }
       
    }
}
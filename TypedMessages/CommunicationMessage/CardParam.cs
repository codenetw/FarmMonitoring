namespace BaseController.CommunicationMessage
{
    /// <summary>
    /// Настройка параметра карты с мин. и макс. границами
    /// </summary>
    public class CardMinMaxCurrent
    {
        /// <summary>
        /// Минимальный порог значения
        /// </summary>
        public float Min { get; set; }

        /// <summary>
        /// Максимальный порог значения
        /// </summary>
        public float Max { get; set; }

        /// <summary>
        /// Текущее значения
        /// </summary>
        public float Current { get; set; }

        public static implicit operator CardMinMaxCurrent(float baseVal)
        {
            return new CardMinMaxCurrent { Current = baseVal};
        }

        protected bool Equals(CardMinMaxCurrent other)
        {
            return Min.Equals(other.Min) && Max.Equals(other.Max) && Current.Equals(other.Current);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            var cur = (CardMinMaxCurrent) obj;
            return cur.Current == Current;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Current.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{Current}";
        }
    }

    /// <summary>
    /// Список настроек карт
    /// </summary>
    public class CardParam
    {
        public CardParam(int id, bool isMaster, CardMinMaxCurrent voltage, CardMinMaxCurrent tempLimit, CardMinMaxCurrent powerLimit,
            CardMinMaxCurrent coreClock, CardMinMaxCurrent memoryClock, CardMinMaxCurrent fanSpeed)
        {
            Voltage = voltage;
            IsMaster = isMaster;
            TempLimit = tempLimit;
            PowerLimit = powerLimit;
            CoreClock = coreClock;
            MemoryClock = memoryClock;
            FanSpeed = fanSpeed;
            Id = id;
        }

        /// <summary>
        /// Номер карты
        /// </summary>
        public int Id { get; }
        
        /// <summary>
        /// Карта является главной
        /// </summary>
        public bool IsMaster { get; }
        
        /// <summary>
        /// Вольтаж
        /// </summary>
        public CardMinMaxCurrent Voltage { get; }

        /// <summary>
        /// Температурный лимит
        /// </summary>
        public CardMinMaxCurrent TempLimit { get; }

        /// <summary>
        /// Лимит питания
        /// </summary>
        public CardMinMaxCurrent PowerLimit { get; }

        /// <summary>
        /// Частота ядра
        /// </summary>
        public CardMinMaxCurrent CoreClock { get; }
        
        /// <summary>
        /// Частота памяти
        /// </summary>
        public CardMinMaxCurrent MemoryClock { get; }

        /// <summary>
        /// Скороть куллера
        /// </summary>
        public CardMinMaxCurrent FanSpeed { get; }

        public bool Equals(CardParam other)
        {
            return  Equals(Voltage, other.Voltage) 
                                  && Equals(TempLimit, other.TempLimit)
                                  && Equals(PowerLimit, other.PowerLimit) 
                                  && Equals(CoreClock, other.CoreClock) 
                                  && Equals(MemoryClock, other.MemoryClock) 
                                  && Equals(FanSpeed, other.FanSpeed);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CardParam) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 1;
                hashCode = (hashCode * 397) ^ (Voltage != null ? Voltage.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TempLimit != null ? TempLimit.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (PowerLimit != null ? PowerLimit.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CoreClock != null ? CoreClock.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (MemoryClock != null ? MemoryClock.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (FanSpeed != null ? FanSpeed.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
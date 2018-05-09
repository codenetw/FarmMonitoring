using System;
using System.Collections.Generic;
using Farm.BaseController.CommunicationMessageModel;
using Farm.MessageBus.MessageBus;

namespace Farm.BaseController.CommunicationMessage
{
    /// <summary>
    /// Сообщение отправляется с актуальный на данный момент информацией о картах
    /// </summary>
    public class AutobernerInformationMessage : MessageBase
    {
        public List<CardParam> CurrentInfoCards { get; set; }
    }

    /// <summary>
    /// Сообщение отправляется когда WatchDog сбросил настройки автобернера
    /// </summary>
    public class AutobernerWatchdogResetMessage : MessageBase { }

    /// <summary>
    /// Сообщение отправляется когда WatchDog перестал работать
    /// </summary>
    public class AutobernerWatchdogStopedMessage : MessageBase { }

    /// <summary>
    /// Сообщение отправляется когда опрос информации о картах оставновлен
    /// </summary>
    public class AutobernerStopWatchingMessage : MessageBase { }


    /// <summary>
    /// Сообщение отправляется когда опрос информации о картах оставновлен
    /// </summary>
    public class AutobernerResetCardMessage : MessageBase
    {
        public CardPramsMessageModel[] CardsPramsModels { get; set; }
    }

    /// <summary>
    /// Сообщение отправляется с информацие о потреблении электроэнергии на картах
    /// </summary>
    public class AutobernerVoltageCardsInfoMessage : MessageBase
    {
        public VoltageInfo[] VoltageInfoModel { get; set; }
    }   

    /// <summary>
    /// Сообщение отправляется с информацие о температуре на картах
    /// </summary>
    public class AutobernerTempCardsInfoMessage : MessageBase
    {
        public TempInfo[] TempInfoModel { get; set; }
    }

    /// <summary>
    /// Сообщение отправляется с информацие о температуре на картах
    /// </summary>
    public class AutobernerPathCardsInfoMessage : MessageBase
    {
        public CardPath[] CardPath { get; set; }
    }
}

namespace Farm.BaseController.CommunicationMessageModel
{
    public class CardPath
    {
        public string name { get; set; }
        public string path { get; set; }
    }
    public class VoltageInfo
    {
        public uint CardId { get; set; }
        public float Max { get; set; }
        public float Current { get; set; }
    }
    public class TempInfo
    {
        public uint CardId { get; set; }
        public float Current { get; set; }
    }
    public struct CardPramsMessageModel
    {
        public int Id { get; }
        public float Voltage { get; }
        public float TempLimit { get; }
        public float PowerLimit { get; }
        public float CoreClock { get; }
        public float MemoryClock { get; }
        public float FanSpeed { get; }

    }

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
                   $"MemoryClock:[{Param.MemoryClock}]" +
                   $"FanSpeed:[{Param.FanSpeed}]";
        }

    }

    /// <summary>
    /// Настройка параметра карты с мин. и макс. границами
    /// </summary>
    [Serializable]
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
            return new CardMinMaxCurrent {Current = baseVal};
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
    [Serializable]
    public class CardParam
    {
        public CardParam(int id, bool isMaster, CardMinMaxCurrent voltage, CardMinMaxCurrent tempLimit,
            CardMinMaxCurrent powerLimit,
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
            return Equals(Voltage, other.Voltage)
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
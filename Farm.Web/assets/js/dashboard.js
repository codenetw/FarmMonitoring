var dashboard = {
    autoberner: function (cards) {  
        var result = cards.CurrentInfoCards;
            $.each(result, function (key, v) {
         
                Templates.blockCardInfo($("#cardsAb"), $(".card[data-value='vd" + key + "']").length !== 0, {
                    key: key,
                    IsMaster: v.IsMaster,
                    VoltageMin: v.Voltage.Min,
                    VoltageMax: v.Voltage.Max,
                    VoltageCurrent: v.Voltage.Current,
                    PowerLimitMin: v.PowerLimit.Min,
                    PowerLimitMax: v.PowerLimit.Max,
                    PowerLimitCurrent: v.PowerLimit.Current,
                    TempLimitMin: v.TempLimit.Min,
                    TempLimitMax: v.TempLimit.Max,
                    TempLimitCurrent: v.TempLimit.Current,
                    CoreClockMin: v.CoreClock.Min,
                    CoreClockMax: v.CoreClock.Max,
                    CoreClockCurrent: v.CoreClock.Current,
                    MemoryClockMin: v.MemoryClock.Min,
                    MemoryClockMax: v.MemoryClock.Max,
                    MemoryClockCurrent: v.MemoryClock.Current,
                    FanSpeedMin: v.FanSpeed.Min,
                    FanSpeedMax: v.FanSpeed.Max,
                    FanSpeedCurrent: v.FanSpeed.Current
                });

            });      
    },
    cardPath: function (cards) {
        cards.map((x, c, v) => {
            document.cardPath.append("<tr><td>" + x.name +"</td><td>"+x.path+"</td></tr>");
        });
        
    }

}
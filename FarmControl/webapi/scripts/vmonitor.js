var monitorfx = function() {
    var host = "WS_HOST_TMPL/info_cards";
    var hostAb = "WS_HOST_TMPL/autoberner_cards";
    var ws = new WebSocket(host);
    var wsAb = new WebSocket(hostAb);
    var _templates = new Templates();

    wsAb.onopen = function (socket) {
        console.log("connected!");
        wsAb.send("");
    };
    ws.onopen = function(socket) {
        console.log("connected!");
        ws.send("?type=detail");
    };

    ws.onclose = function() {
        console.log("disconnected!");
    };

    ws.onmessage = function(evt) {
        //var result = JSON.parse(evt.data);
        //result["0"].Result.forEach(function (item) {
        //    var devid = item.DeviceId.replace(/[\\\]/]/gi, '');
        //    if (!$(".card[data-value='" + devid+"']"))
        //        $("#cards").add(Templates.blockCardInfo.format(devid, item.DriverVersion, item.Status, item.Name));
        //});
    };
   
    wsAb.onmessage = function (evt) {
        var result = JSON.parse(evt.data);
        $.each(result, function (key, val) {    
        var v = val.Value.Param;
                _templates.blockCardInfo($("#cards"), $(".card[data-value='vd" + key + "']").exists(),{
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
    };
   
}();
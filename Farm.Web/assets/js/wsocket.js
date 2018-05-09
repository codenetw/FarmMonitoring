var socket;
var uri = "ws://192.168.1.65:2012/aggregator";

ws = {
    connect: function() {
        socket = new WebSocket(uri);

        socket.onopen = function() {
            demo.showNotification("Соединение установлено.", 'success');
        };

        socket.onclose = function(event) {
            demo.showNotification("Соединение потеряно, попытка подключения.", 'danger');
            setTimeout(function() { ws.connect(); }, 5000);
        };

        socket.onmessage = function(event) {
            var result = JSON.parse(event.data);
            if (result.MessageType === "AutobernerInformationMessage")
                dashboard.autoberner(result.Data);

            if (result.MessageType === "AutobernerVoltageCardsInfoMessage") {
                dashboardCharts.update("#containerVoltage",
                    {
                        xAxis: { categories: result.Data.VoltageInfoModel.map((c, s, x) => "GPU " + c.CardId) },
                        series: [
                            {
                                data: result.Data.VoltageInfoModel.map((c, s, x) => c.Current)
                            }
                        ]
                    });
            }
            if (result.MessageType === "AutobernerTempCardsInfoMessage") {
                var resArray = result.Data.TempInfoModel.map((c, s, x) => {
                    var obj = {
                        name: "GPU " + c.CardId,
                        Id: c.CardId,
                        data: c.Current
                    };
                    return obj;
                });
                dashboardCharts.update("#containerTemp", resArray);
            };

            if (result.MessageType === "AutobernerCardsPathInfoMessage") {
                return result.Data.CardPath;
                }
            }
           socket.onerror = function (error) {
            };
        
    },

   
}

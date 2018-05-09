var socket;
var uri = "ws://192.168.1.64:2012/aggregator";

ws = {
connect: function(){
	socket = new WebSocket(uri);
	
	socket.onopen = function() {
		demo.showNotification("Соединение установлено.", 'success')  
	};

	socket.onclose = function(event) {
		demo.showNotification("Соединение потеряно, попытка подключения.", 'danger')
		setTimeout(function() { ws.connect(); }, 5000);
	};

	socket.onmessage = function(event) {
		alert(event.data);
	};

	socket.onerror = function(error) {    
		setTimeout(function() { ws.connect(); }, 5000);
	};
}
}
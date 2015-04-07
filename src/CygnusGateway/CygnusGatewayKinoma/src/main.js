//@program

var THEME = require('themes/sample/theme');
var CONTROL = require('mobile/control');
var SCROLLER = require('mobile/scroller');

var blueSkin = new Skin( { fill:"blue" } );
var labelStyle = new Style( { font: "bold 40px", color: "white" } );

var whiteSkin = new Skin( { fill: "blue" } );
var defaultStyle = new Style( { align: "left" } );

var logContainer = Container.template(function($) { return { 
	left: 0, right: 0, top: 0, bottom: 0, skin: whiteSkin, active: true,
	contents: [
		SCROLLER.VerticalScroller($, {
			left: 4, right: 4, top: 0, bottom: 0, active: true, clip: true, contents: [
					new Column({ left:0, right: 0, top: 0 })
				]
			})],
	behavior: Object.create(Container.prototype, {
		log: { value: function(container, text, color) {
					if (!color) color = '#000';
			
					var block = new Text({left: 4, right: 4, top: 0});
					block.string = text;
					block.style = new Style("18px Arial", color);
			
					var column = container.first.first;
					if (column.first) {
						column.insert(block, column.first);
					} else {
						column.add(block);
					}
				}}
			})
		}});

var mainContainer = new Container ({ left: 0, right: 0, top: 0, bottom: 0, skin: blueSkin, active: true });

var logger = new logContainer();
var log = function(text, color) {
	logger.delegate('log', text, color);
}

application.add(mainContainer);
mainContainer.add(logger);

log(application.uuid);

var serverPort = 9300;
var serverName = "CygnusGateway" + serverPort;

var server = new WebSocketServer(serverPort);

server.onlaunch = function() {
	log("server is ready to accept a new connection");
};

server.onconnect = function(conn, options) {
	log("-CONNECT");
	
	var send = function(conn, type, data) {
		log("-SEND:");
		conn.send(JSON.stringify({ type: type, data: data }));
	};
	
	conn.onopen = function() {
		log("-OPEN");
		send(conn, "TestType", { message: "TestMessage", text: "TestText" });
	};
	
	conn.onmessage = function(e) {
		log("-MESSAGE");
	};
	
	conn.onclose = function(e) { 
		log("-CLOSE");
	};
	
	conn.onerror = function(e) {
		log("-ERROR", '#f00');
		conn.close();
	};

};

log("server is launching on port " + serverPort);

// Register this device with the web service
var uri = "https://localhost:44300/api/gateways/";
var body = {
	"Id": application.uuid,
	"Name": serverName};
	
var message = new Message(uri);
message.method = "POST";
message.requestText = JSON.stringify(body);
message.setRequestHeader("Content-Length", message.requestText.length);
message.setRequestHeader("Content-Type", "Application/Json");
application.invoke(message);

log("api call made");


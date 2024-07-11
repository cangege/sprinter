var mqtt = require('mqtt');
mqttClient = mqtt.connect('mqtt://localhost:1883',{clientId:"iamserver"}); //连接到服务端
const Koa = require('koa');
const app = new Koa();
let router = require("./router")
const bodyparser = require('koa-bodyparser');

app.use(bodyparser());
var cors = require('koa2-cors');
app.use(cors()); //跨域


//注册路由到app上
app.use(router());

let port = 9010
app.listen(port,async function(){
	console.log("koa项目已启动，监听端口:" + port)
	
	
	
});





process.on('uncaughtException', function(err) {
	console.log(err.message);

}) //监听未捕获的异常

process.on('unhandledRejection', function(err, promise) {
	console.log(err)
	
}) //监听Promise没有被捕获的失败函数




console.log("*************MQTT服务************************************")
/*************MQTT服务************************************/
var mosca = require('mosca');
const { async } = require('q');
//构建自带服务器
MqttClients={}
var MqttServer = new mosca.Server({
	port: 1883
	
});
//对服务器端口进行配置， 在此端口进行监听
MqttServer.on('clientConnected', function(client) {
	//监听连接
	MqttClients[client.id]=1
});

MqttServer.on('clientDisconnected', function(client) {
	//监听连接
	MqttClients[client.id]=0
});
/**
 * 监听MQTT主题消息
 **/
MqttServer.on('published', function(packet, client) {
	//当客户端有连接发布主题消息
	// var topic = packet.topic;
	// if (topic.indexOf("unsubscribes") > 0) {
	// 	console.log("unsubscribes")
	// 	console.log(topic)
	// 	if (packet.payload.indexOf("{") >= 0) {
	// 		let topicstr = JSON.parse(packet.payload)

	// 	}

	// } else if (topic.indexOf("subscribes") > 0) {
	// 	console.log("subscribes")
	// 	console.log(topic)

	// 	if (packet.payload.indexOf("{") >= 0) {
	// 		let topicstr = JSON.parse(packet.payload)

	// 	}
	// }else{
	// 	console.log(topic)
	// 	console.log((packet.payload.toString()))
	// }

});

MqttServer.on('ready', function() {
	//当服务开启时
	console.log('mqtt is running...', MqttServer.opts.port);
});
/*************MQTT服务************************************/





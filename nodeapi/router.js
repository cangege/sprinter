const router = require('koa-router')({
	prefix: '/api/'
}) //加接口前缀，所有接口都以 /api/开头
const fs = require('fs')
const path = require('path');
const multer = require('koa-multer')
const storage = multer.diskStorage({
	destination: function(req, file, cb) {
		cb(null, 'temp') // 确保这个文件夹已经存在
	},
	filename: function(req, file, cb) {
		cb(null, file.fieldname + '-' + Date.now())
	}
})
const upload = multer({
	storage: storage
})

const OSSTemp = require('./osstemp.js');
const { async } = require('q');





/**
 * 文件遍历方法
 * @param filePath 需要遍历的文件路径
 */

function fileDisplay(filePath) {
	//根据文件路径读取文件，返回文件列表
	let files = fs.readdirSync(filePath)
	let filesList = [];

	for (let filename of files) {
		var filedir = path.join(filePath, filename);
		let stats = fs.statSync(filedir)
		var isFile = stats.isFile(); //是文件
		var isDir = stats.isDirectory(); //是文件夹
		if (isFile && filedir.endsWith(".js")) {
			filesList.push(filedir)

		} else if (isDir) {
			filesList = filesList.concat(fileDisplay(filedir)); //递归，如果是文件夹，就继续遍历该文件夹下面的文件
		}
	}

	return filesList
}






router.post('printFile', upload.single('file'), async (ctx, next) => {
	const file = ctx.req.file; // 获取上传的文件
	if (ctx.query.printerName && ctx.query.clientID) {
		let clientID = ctx.query.clientID
		let printerName = decodeURIComponent(ctx.query.printerName)
		//检查客户端是否在线
		if (MqttClients[clientID]) {


			if (!file) {
				if (ctx.body.url) {
					//有url，直接发给客户端
					mqttClient.publish(clientID + "_order", `${ctx.body.url};${printerName}`, {
						qos: 0,
						retain: false
					});
					ctx.body = {
						code: 0,
						msg: "send success"
					}
				} else {
					ctx.body = {
						code: 500,
						msg: '请上传文件或传文件url'
					}

				}
			} else {
				let fileType = file.originalname.substring(file.originalname.lastIndexOf('.') + 1);
				fileType = fileType.toLowerCase();
				let absPath = path.join(__dirname, file.path)

				let nFile = await OSSTemp.createOSSFile(absPath, file.filename + "." + fileType)

				if (nFile.url) {
					mqttClient.publish(clientID + "_order", `${nFile.url};${printerName}`, {
						qos: 0,
						retain: false
					});
				}
				ctx.body = {
					code: 0,
					msg: "send success"
				}
			}
		} else {
			ctx.body = {
				code: 500,
				msg: '客户端不在线~'
			};
		}
	} else {
		ctx.body = {
			code: 500,
			msg: '没传printerName和clientID'
		};
	}

	if (file) {
		let absPath = path.join(__dirname, file.path)
		try {
			fs.unlinkSync(absPath);
		} catch (error) {

		}

	}

});





router.post('printBase64Image', async (ctx, next) => {

	let imageData = ctx.request.body.imageData

	console.log("printBase64Image");
	console.log(ctx.query);
	if (ctx.query.printerName && ctx.query.clientID) {
		let clientID = ctx.query.clientID
		let printerName = decodeURIComponent(ctx.query.printerName)
		//检查客户端是否在线
		if (MqttClients[clientID]) {

			if (imageData) {



				// 设置文件路径和名称
				let filename = new Date().getTime() + "" + Math.ceil(Math.random() * 100000) + ".png"
				let absPath = path.join(__dirname, "temp", filename)

				console.log(absPath);
	
				// 将 Base64 数据写入文件
				fs.writeFile(absPath, imageData, 'base64', async (err) => {
					if (err) {
						console.error('写入文件时出错:', err);
						ctx.body = {
							code: 500,
							msg: "图片格式错误"
						}
					} else {
						let nFile = await OSSTemp.createOSSFile(absPath, filename)

						if (nFile.url) {
							mqttClient.publish(clientID + "_order", `${nFile.url};${printerName}`, {
								qos: 0,
								retain: false
							});
						}
						console.log("发送ok",nFile.url);
						ctx.body = {
							code: 0,
							msg: "send success"
						}
					}
				});
				
			} else {
				ctx.body = {
					code: 500,
					msg: 'imageData没传'
				};
			}


		} else {
			ctx.body = {
				code: 500,
				msg: '客户端不在线~'
			};
		}
	} else {
		ctx.body = {
			code: 500,
			msg: '没传printerName和clientID'
		};
	}

});


router.get("test", (ctx) => {
	ctx.response.body = MqttClients

})

//将设置好的router输出
module.exports = () => {
	return router.routes()
}
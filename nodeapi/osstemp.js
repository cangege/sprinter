let Q = require('q');
//存储临时文件使用的oss，临时文件存储1天
async function put(osspath, localpath) {
	let p = Q.defer();
	try {
		let OSS = require('ali-oss')
		let client = new OSS({
			region: 'xxx',
			accessKeyId: 'xx',
			accessKeySecret: 'xx',
			bucket: 'x'
		});
	
		
		let result = await client.put(osspath, localpath);
		p.resolve(result)
	} catch (e) {
		console.log(e);
	}
	
	return p.promise
}
let createOSSFile = (localUrl, filename) => {
	let p = Q.defer();
	var fs = require('fs');
	let ossconfig = {
		url: filename,
		outurl: "https://xxxx.oss-cn-hangzhou.aliyuncs.com/",
		path: process.cwd(), //发布后的目录所在文件夹名字
		osspath: "" //oss文件所在目录
	}
	//process.cwd()是首页入口js文件地址 ,此处替换

	put(ossconfig.osspath + ossconfig.url, localUrl).then(res=>{

		fs.unlink(localUrl, () => {});
		p.resolve({
			url: ossconfig.outurl + filename
		})
	});
	

	return p.promise
}


module.exports = {
	createOSSFile
};

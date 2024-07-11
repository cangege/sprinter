# 这是一个共享打印机的软件：sprinter

#### 使用说明

1. winform需要vs2013以上版本，使用.netframework4.7.2

2. nodejs 需要16以上版本，需要搭配阿里云oss使用，oss记得设置生命周期为1天，并且为公共读

3. nodejs 启动时候mosca会报错，到nodemodules里把错误修正即可，参考

   Expected `schema` to be an object or boolean

   https://blog.csdn.net/LIZHUOLONG1/article/details/125978781

4. 不想编译直接开箱即用，下载  云打印机.zip 解压执行即可，由于开机启动代码涉及敏感权限，可能会报毒，自己斟酌是否运行，或者自己编译！



#### 接口文档

​
软件提供接口：
软件启动后，后显示连接字符串，打印服务基于http协议，启动后，可以访问url测试服务是否正常启动

直接访问：http://你的IP:7086/



获取打印机列表

请求地址：http://你的IP:7086/getPrinters

请求方式：get



局域网打印接口

请先确认局域网服务已启动并设置固定内网ip，否则ip会变化，这里以 http://你的IP:7086/ 为例

打印word
请求地址：http://你的IP:7086/printWord?printName=（encodeURIComponent（你的打印机名称））

请求方式：post multipart/form-data

提交字段：file 需要打印的word文件，支持doc，docx

打印pdf
请求地址：http://你的IP:7086/printPdf?printName=（encodeURIComponent（你的打印机名称））

请求方式：post multipart/form-data

提交字段：file 需要打印的PDF文件

打印图片文件
请求地址：http://你的IP:7086/printImage?printName=（encodeURIComponent（你的打印机名称））

请求方式：post multipart/form-data

提交字段：file 需要打印的图片文件（默认为宽度100%竖向打印，请自行调整打印图片尺寸）

打印base64图片(前端小票打印机用这个，可以使用html2canvas转成base64图片，如果打印不清晰，记得将font-size设置成bold）
请求地址：http://你的IP:7086/imageBase64?printName=（encodeURIComponent（你的打印机名称））

请求方式：post json格式

提交字段：{"ImageData":"base64imagedata，不要data:image前缀"}

例如：{ImageData:"iVBORw0KGgoAAAANSUhEUgAAAj..."}

云打印-需要软件上的客户端ID

接口地址 提供一个公网地址：https://zzapi.mmteck.cn/blwy/

1、上传文件打印pdf,word，图片文件

请求地址：https://zzapi.mmteck.cn/blwy/api/printFile?printerName=（encodeURIComponent（你的打印机名称））&clientID=（你的客户端ID）

请求方式：post multipart/form-data

提交字段：file 需要打印的word文件，支持doc，docx

2、直接传文件url打印

请求地址：https://zzapi.mmteck.cn/blwy/api/printFile?printerName=（encodeURIComponent（你的打印机名称））&clientID=（你的客户端ID）

请求方式：post json格式

提交字段：url， 可以直接在浏览器访问的文件url，需要带后缀！！！

3、打印base64图片

请求地址：https://zzapi.mmteck.cn/blwy/api/printBase64Image?printerName=（encodeURIComponent（你的打印机名称））&clientID=（你的客户端ID）

请求方式：post json格式

提交字段：{"imageData":"base64imagedata，不要data:image前缀"}

例如：{imageData:"iVBORw0KGgoAAAANSUhEUgAAAj..."}




​
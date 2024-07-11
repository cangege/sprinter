
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;



namespace SPrinterServer
{

    public class webServer
    {
        private HttpListener httpListener;
        FilePrinter printer;
        LocalPrinter lcPrinters;
        public webServer()
        {
            this.httpListener = new HttpListener();
            printer = new FilePrinter();
            lcPrinters = new LocalPrinter();

        }
       
        public string start()
        {

            // 获取本地主机的相关信息
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            string IP = "";
            // 遍历所有IP地址
            foreach (IPAddress ipAddress in host.AddressList)
            {
                // 过滤掉IPv6地址和非本地链接地址
                if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !IPAddress.IsLoopback(ipAddress))
                {
                    IP = ipAddress.ToString();
                    break;
                }
            }
            if (IP == "")
            {
                IP = "127.0.0.1";
            }
            IP = "http://" + IP + ":7086/";
            try
            {
                httpListener.Prefixes.Add(IP);
                httpListener.Start();
                System.Threading.Tasks.Task.Run(Listen);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
          
            return IP;
        }

        string TempPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\temp";



        private async System.Threading.Tasks.Task Listen()
        {
            while (true)
            {
                HttpListenerContext context = await httpListener.GetContextAsync();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                response.Headers.Add("Access-Control-Allow-Origin", "*"); // 允许任意来源访问，生产环境中应根据实际情况设置具体来源

                // 如果是预检请求（OPTIONS 请求），处理并回复
                if (request.HttpMethod == "OPTIONS")
                {
                    response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS"); // 允许的请求方法
                    response.Headers.Add("Access-Control-Allow-Headers", "Content-Type"); // 允许的请求头
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.Close();
                    continue; // 跳过当前请求，等待下一个请求
                }
                string responseString = "{\"code\":0,\"msg\":\"服务启动成功\"}";
                string url = request.Url.AbsolutePath;
                string query = request.Url.Query;

                query = query.Replace("?", "");

                string[] queryArr = query.Split('&');
                string printName = "";
                foreach(string item in queryArr) {
                    if (item.Contains("printName")) {
                        printName = item.Split('=')[1];
                        printName = Uri.UnescapeDataString(printName);
                    }
                }

                // 解析 Query 参数成键值对



                if (url == "/printPdf" && request.HttpMethod == "POST")
                {
                    try
                    {
                
                        string fileNewName = new Random().Next(10000, 99999) + ".pdf";
                        string filePath = Path.Combine(TempPath, fileNewName); // 指定文件保存路径
                       
                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            request.InputStream.CopyTo(fs); // 将请求的文件流保存到本地文件
                        }

                        printer.printPdf(filePath, printName);

                    }
                    catch (Exception e)
                    {

                        responseString = "{\"code\":400,\"msg\":\"" + e.Message + "\"}";
                    }




                }
                else if (url == "/printWord" && request.HttpMethod == "POST")
                {
                    try
                    {

                        List<FormKeyValue> FormData = ParseFormData(request);
                        string filetype = getParam("filetype", FormData);
                        string fileNewName = new Random().Next(10000, 99999) + "."+ filetype;
                        string filePath = Path.Combine(TempPath, fileNewName); // 指定文件保存路径
                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            request.InputStream.CopyTo(fs); // 将请求的文件流保存到本地文件
                        }

                        printer.printWord(filePath, printName);
                    }
                    catch (Exception e)
                    {

                        responseString = "{\"code\":400,\"msg\":\"" + e.Message + "\"}";
                    }

                }
                else if (url == "/printImage" && request.HttpMethod == "POST")
                {
                    try
                    {
                        List<FormKeyValue> FormData = ParseFormData(request);

                        string filetype = getParam("filetype", FormData);


                        string fileNewName = new Random().Next(10000, 99999) + "."+ filetype;
                        string filePath = Path.Combine(TempPath, fileNewName); // 指定文件保存路径

                        using (FileStream fs = new FileStream(filePath, FileMode.Create))
                        {
                            request.InputStream.CopyTo(fs); // 将请求的文件流保存到本地文件
                        }

                        printer.printImage(filePath, printName);
                    }
                    catch (Exception e)
                    {

                        responseString = "{\"code\":400,\"msg\":\"" + e.Message + "\"}";
                    }

                }
                
                else if (url == "/imageBase64" && request.HttpMethod == "POST")
                {
                    
                    try
                    {
                        List<FormKeyValue> FormData = ParseJsonData(request);
                        string ImageData = getParam("ImageData", FormData);
                        byte[] imageBytes = Convert.FromBase64String(ImageData);

                        string fileNewName = new Random().Next(10000, 99999) + ".png";
                        string filePath = Path.Combine(TempPath, fileNewName); // 指定文件保存路径
                        // 将字节数组保存为图片文件
                        File.WriteAllBytes(filePath, imageBytes);

                        printer.printImage(filePath, printName);


                    }
                    catch (Exception e)
                    {
                        responseString = "{\"code\":400,\"msg\":\"" + e.Message + "\"}";
                    }
                }
                else if (url == "/getPrinters")
                {

                    try
                    {
                        responseString = "";
                        lcPrinters.InitLocalPrinters();
                      
                        foreach (PrinterModel p in Printers.MyPrinters) {
                            responseString += "{\"DeviceName\":\"" + p.DeviceName + "\",\"StatusCode\":\"" + p.StatusCode + "\",\"StatusName\":\"" + p.StatusName+"\"},";
                        }
                        if (responseString != "")
                        {
                            responseString = responseString.Substring(0, responseString.Length - 1);
                            responseString = "[" + responseString + "]";
                        }
                        else {
                            responseString = "[" + responseString + "]";
                        }
                       

                    }
                    catch (Exception e)
                    {
                        responseString = "{\"code\":400,\"msg\":\"" + e.Message + "\"}";
                    }
                }




                byte[] buffer = Encoding.UTF8.GetBytes(responseString);

                response.ContentLength64 = buffer.Length;
                response.ContentType = "application/json";
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                response.StatusCode = 200; // 设置响应状态码为200，表示成功
                response.Close();
            }


        }

        private string getParam(string key, List<FormKeyValue> fromData) {

            string value = "";

            foreach (FormKeyValue kv in fromData)
            {
                if (kv.key == key) {

                    value = kv.value;
                    break;

                }
            }

            return value;
        
        }

        // 解析 form-data 字段数据
        private List<FormKeyValue> ParseFormData(HttpListenerRequest request)
        {
            List<FormKeyValue> list = new List<FormKeyValue>();
            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {



                string requestBody = reader.ReadToEnd();

                string[] lines = requestBody.Split(new string[] { "---" }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < lines.Length; i++)
                {

                    try
                    {
                        if (lines[i].Contains("form-data; "))
                        {

                            string[] values = lines[i].Split(new string[] { "form-data; " }, StringSplitOptions.RemoveEmptyEntries);
                            if (values.Length > 1)
                            {

                                //单独取出filename

                                if (values[1].Contains("name=\"file\"; filename="))
                                {
                                    string[] filenames = values[1].Split(new string[] { "name=\"file\"; filename=" }, StringSplitOptions.RemoveEmptyEntries);

                                    string[] kvArr = filenames[0].Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                                    foreach (string item in kvArr)
                                    {
                                        if (item.Contains("."))
                                        {
                                            string filename = item.Replace("\"", "");
                                            FormKeyValue formitem = new FormKeyValue();
                                            formitem.key = "filename";
                                            formitem.value = filename;
                                            list.Add(formitem);

                                            FormKeyValue filetype = new FormKeyValue();
                                            filetype.key = "filetype";
                                            filetype.value = filename.Split('.')[1].ToLower();
                                            list.Add(filetype);

                                            break;
                                        }

                                    }




                                }
                                else if (values[1].Contains("name="))
                                {
                                    string kyStr = values[1].Replace("name=", "");
                                    kyStr = kyStr.Replace("\"", "");
                                    string[] kvArr = kyStr.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                                    if (kvArr.Length > 1)
                                    {
                                        FormKeyValue formitem = new FormKeyValue();
                                        formitem.key = kvArr[0];
                                        formitem.value = kvArr[1];
                                        list.Add(formitem);
                                    }

                                }



                            }


                        }
                    }
                    catch (Exception)
                    {

                    }
                }


            }

            return list;
        }

        private List<FormKeyValue> ParseJsonData(HttpListenerRequest request)
        {
            List<FormKeyValue> list = new List<FormKeyValue>();
            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {



                string requestBody = reader.ReadToEnd();

                requestBody = requestBody.Replace("{\"ImageData\":\"", "").Replace("\"}","");
                FormKeyValue formitem = new FormKeyValue();
                formitem.key = "ImageData";
                formitem.value = requestBody;
                list.Add(formitem);

            }

            return list;
        }
    }
    public class FormKeyValue
    {
        public string key = "";
        public string value = "";


    }
}

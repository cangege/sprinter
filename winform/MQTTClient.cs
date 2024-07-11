
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Word;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Subscribing;
using MQTTnet.Packets;
using MQTTnet.Protocol;

namespace SPrinterServer
{
    public class MQTTClient
    {
        string version = "20220727";
        private string tempPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\temp\\";
        public MqttClient mqttClient;
        public string ShopCode = "";
        public System.Windows.Forms.Label statusLabel = null;
        private FilePrinter filePrinter = new FilePrinter();
        public async void StartClient()
        {


            var options = new MqttClientOptions();

            ShopCode = getShopCode();
            options.ClientId = ShopCode;

            //设置服务器地址与端口
            options.ChannelOptions = new MqttClientTcpOptions()
            {

                Server = "frp.aiwx.org.cn",
                //Server = "localhost",
                Port = 1883
            };
            //设置账号与密码
            options.Credentials = new MqttClientCredentials()
            {
                Username = "blwy",
                Password = Encoding.Default.GetBytes("bilinweiyin")
            };
            options.CleanSession = true;

            //保持期
            options.KeepAlivePeriod = TimeSpan.FromSeconds(100.5);

            //构建客户端对象
            mqttClient = new MqttFactory().CreateMqttClient() as MqttClient;

            try
            {
                //绑定消息接收方法
                mqttClient.UseApplicationMessageReceivedHandler(ApplicationMessageReceivedHandler);

                //绑定连接成功状态接收方法
                mqttClient.UseConnectedHandler(ConnectedHandler);

                //绑定连接断开状态接收方法
                mqttClient.UseDisconnectedHandler(DisconnectedHandler);

                //启动连接
                await mqttClient.ConnectAsync(options);
                //MYLog.write("开启订阅：" + ShopCode + "_print");
                //订阅消息
                await mqttClient.SubscribeAsync(
                    new MqttTopicFilter[] { //订阅消息集合
                      
                        new MqttTopicFilter() //订阅消息对象
                        {
                            Topic = ShopCode + "_order",  //订单变动
                            QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce  //消息类型
                        }
                    });
            }
            catch
            {
                //MYLog.write($"连接失败");
            }

        }
        private System.Threading.Tasks.Task DisconnectedHandler(MqttClientDisconnectedEventArgs arg)
        {


            if (statusLabel.InvokeRequired)
            {
                statusLabel.Invoke(new Action(() => statusLabel.Text = "离线"));
            }
            else
            {
                statusLabel.Text = "离线";
            }


            return System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                //MYLog.write($"客户端已断开，重新连接");
                StartClient();

            });
        }

        private System.Threading.Tasks.Task ConnectedHandler(MqttClientConnectedEventArgs arg)
        {
            return System.Threading.Tasks.Task.Factory.StartNew(() =>
            {

                if (statusLabel.InvokeRequired)
                {
                    statusLabel.Invoke(new Action(() => statusLabel.Text = "在线"));
                }
                else
                {
                    statusLabel.Text = "在线";
                }
                //MYLog.write($"客户端已连接");

                //var Result = mqttClient.PublishAsync("123", "123", MqttQualityOfServiceLevel.AtMostOnce, false).Result;
                //if (Result.ReasonCode == MQTTnet.Client.Publishing.MqttClientPublishReasonCode.Success)
                //{
                //    Console.WriteLine("发送成功");
                //}
                //else
                //{
                //    Console.WriteLine("发送失败");
                //}

            });
        }


        private System.Threading.Tasks. Task ApplicationMessageReceivedHandler(MqttApplicationMessageReceivedEventArgs args)
        {
            return System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                Console.WriteLine($"客户端号:{args.ClientId}" +
                $"\n┕ 主题:{args.ApplicationMessage.Topic}" +
                $"\n┕ 装载:{Encoding.UTF8.GetString(args.ApplicationMessage.Payload)}" +
                $"\n┕ QoS:{args.ApplicationMessage.QualityOfServiceLevel} " +
                $"\n┕ 保持:{args.ApplicationMessage.Retain}");
                //MYLog.write(
                //格式   url;打印机名称
                if (args.ApplicationMessage.Topic == ShopCode + "_order")
                {
                    string order = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
                    if (order.Length > 500)
                    {
                        string url = order.Split(';')[0];
                        string printerName = order.Split(';')[1];
                        byte[] imageBytes = Convert.FromBase64String(url);

                        string fileNewName = new Random().Next(10000, 99999) + ".png";
                        string filePath = Path.Combine(tempPath, fileNewName); // 指定文件保存路径
                        // 将字节数组保存为图片文件
                        File.WriteAllBytes(filePath, imageBytes);
                        filePrinter.printImage(filePath, printerName);
                       
                    }
                    else
                    {
                        string url = order.Split(';')[0];
                        string printerName = order.Split(';')[1];
                        string path = downFile(url);
                        Uri uri = new Uri(url);
                        string fileName = uri.Segments[uri.Segments.Length - 1];
                        string houzhui = fileName.Split('.')[1];
                        houzhui = houzhui.ToLower();
                        if (houzhui == "doc" || houzhui == "docx")
                        {
                            filePrinter.printWord(path, printerName);
                        }
                        else if (houzhui == "pdf")
                        {
                            filePrinter.printPdf(path, printerName);
                        }
                        else if (houzhui == "png" || houzhui == "jpg" || houzhui == "jpeg")
                        {
                            filePrinter.printImage(path, printerName);
                        }
                    }

                }

            });
        }



        public string getShopCode()
        {
            string shopcode = "";
            try
            {

                string filepath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\shopid.txt";

                if (File.Exists(filepath))
                {
                    // 创建一个 StreamReader 的实例来读取文件 
                    // using 语句也能关闭 StreamReader
                    using (StreamReader sr = new StreamReader(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\shopid.txt"))
                    {
                        shopcode = sr.ReadToEnd();
                        if (shopcode == "")
                        {
                            shopcode = GenerateRandomNumber(10);
                        }
                    }
                }
                else
                {
                    FileStream fs = new FileStream(filepath, FileMode.Append, FileAccess.Write);
                    StreamWriter sr = new StreamWriter(fs);
                    shopcode = this.GenerateRandomNumber(10);
                    sr.Write(shopcode);//开始写入值
                    sr.Close();
                    fs.Close();
                }

            }
            catch (Exception e)
            {

            }

            return shopcode;

        }


        private char[] constant =
     {
        '0','1','2','3','4','5','6','7','8','9',
        'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
        'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'
      };
        private string GenerateRandomNumber(int Length)
        {
            System.Text.StringBuilder newRandom = new System.Text.StringBuilder(62);
            Random rd = new Random();
            for (int i = 0; i < Length; i++)
            {
                newRandom.Append(constant[rd.Next(62)]);
            }
            return newRandom.ToString().ToUpper();
        }


        public void checkUpgrade()
        {
            string rootURl = "http://bilinweiyin.oss-cn-hangzhou.aliyuncs.com";
            var cli = new System.Net.WebClient();
            cli.Encoding = System.Text.Encoding.UTF8;//定义对象语言
            string versionStr = cli.DownloadString(rootURl + "/version.js");
            string[] files = versionStr.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (files.Length > 0)
            {
                string versionNum = files[0];
                if (version != versionNum)
                {
                    //需要升级
                    System.Diagnostics.Process.Start(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\blwyUpdate.exe");
                    System.Windows.Forms.Application.Exit();
                }
            }

        }


        public string downFile(string url)
        {

            var cli = new System.Net.WebClient();
            try
            {
                Uri uri = new Uri(url);
                string fileName = uri.Segments[uri.Segments.Length - 1];
                string houzhui = fileName.Split('.')[1];
                string nFilename = GenerateRandomNumber(8);
                string npath = tempPath + nFilename + "." + houzhui;
                cli.DownloadFile(url, npath);

                return npath;
            }
            catch (Exception)
            {
                return "";
            }



        }


        /// <summary>
        /// 清理缓存文件
        /// </summary>
        public void clearTempFiles()
        {


            string temppath = tempPath;
        


                string[] files = Directory.GetFiles(temppath);
                foreach (string fpath in files)
                {
                    FileInfo file = new FileInfo(fpath);
                    int second = DateDiff(file.CreationTime, DateTime.Now);
                    if (second >= 1800)
                    {//超过10分钟的文件删掉
                        try
                        {
                            File.Delete(fpath);
                        }
                        catch (Exception)
                        {
                        }


                    }
                }
               
            
        }


        //得到时间差
        public int DateDiff(DateTime datetime1, DateTime datetime2)
        {
            int seconds = 0;
            try
            {
                TimeSpan ts1 = new TimeSpan(datetime1.Ticks);
                TimeSpan ts2 = new TimeSpan(datetime2.Ticks);

                seconds = Math.Abs((int)ts1.Subtract(ts2).TotalSeconds);

            }
            catch
            {

            }
            return seconds;
        }


    }
}

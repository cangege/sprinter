using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace autoupdate
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {


            //使用
            SetTimeout(2000, delegate
            {
                this.Invoke(new Action(() =>
                {
                    try
                    {
                        string rootURl = "http://bilinweiyin.oss-cn-hangzhou.aliyuncs.com";
                        var cli = new WebClient();
                        cli.Encoding = System.Text.Encoding.UTF8;//定义对象语言
                        string version = cli.DownloadString(rootURl + "/version.js");
                        string[] files = version.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 1; i < files.Length; i++)
                        {
                            DownFile(rootURl + files[i]);
                        }
                        System.Diagnostics.Process.Start(System.IO.Directory.GetCurrentDirectory() + "\\SPrinterServer.exe");
                        Application.Exit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        Application.Exit();

                    }
                }));
            });

        }

        void SetTimeout(double interval, Action action)
        {
            System.Timers.Timer timer = new System.Timers.Timer(interval);
            timer.Elapsed += delegate (object sender, System.Timers.ElapsedEventArgs e)
            {
                timer.Enabled = false;
                action();
            };
            timer.Enabled = true;
        }




        void DownFile(string xls)
        {
            if (xls != "")
            {
                try
                {
                    string URLAddress = xls;


                    string receivePath = System.IO.Directory.GetCurrentDirectory() + @"\";

                    WebClient client = new WebClient();
                    client.DownloadFile(URLAddress, receivePath + System.IO.Path.GetFileName(URLAddress));
                }
                catch (Exception ex)
                {

                }

            }


        }
    }
}

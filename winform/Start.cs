using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Windows.Forms;

namespace SPrinterServer
{
    public partial class Start : Form
    {
        MQTTClient mqtt;
        string shopCode = "";
        LocalPrinter printer;
        public Start()
        {
            InitializeComponent();

            mqtt = new MQTTClient();
            printer = new LocalPrinter();

            mqtt.checkUpgrade();
            mqtt.statusLabel = this.labelStatus;
            mqtt.StartClient();
            refreshPrinterList();



        }


        private void refreshPrinterList() {
            printer.InitLocalPrinters();
            panel1.Controls.Clear();



            for (int j = 0; j < Printers.MyPrinters.Count; j++)
            {

                System.Windows.Forms.Label lb = new System.Windows.Forms.Label();
                lb.Text = Printers.MyPrinters[j].DeviceName + ";[" + Printers.MyPrinters[j].StatusName + "]【点击复制】";
                lb.Font = new System.Drawing.Font("", 12);
                lb.Click += Lb_Click;
                lb.Width = 500;
                lb.Top = j * 30;
                panel1.Controls.Add(lb);
            }

          

        }
        private void Lb_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Label clickedLabel = sender as System.Windows.Forms.Label;
            string deviceId = clickedLabel.Text.Split(';')[0];

            Clipboard.SetText(deviceId);

            this.labelLog.Text = deviceId + "[已复制]";


        }



        private void Form1_Load(object sender, EventArgs e)
        {
            webServer web = new webServer();
            this.labelServer.Text = web.start();
            shopCode = mqtt.getShopCode();
            this.labelClient.Text = "客户端ID："+ shopCode;
            
        }

        private void linkLabel_copyClient_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Clipboard.SetText(mqtt.getShopCode());
            this.labelLog.Text = "已复制["+ shopCode + "]";
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Clipboard.SetText(this.labelServer.Text);
            this.labelLog.Text = "已复制[" + this.labelServer.Text + "]";
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
        }

        private void Start_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
                this.Hide();
                this.ShowInTaskbar = false;
                return;
            }
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        int second = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            second++;
            if (second == 20) {//60秒刷新一次打印机
                second = 0;
                
                refreshPrinterList();
                this.labelLog.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "--刷新了打印机列表";

                mqtt.clearTempFiles();
            }
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Console.WriteLine("设置开机自启动，需要修改注册表", "提示");
            string SoftWare = "SPrinterServer";
            string path = Application.ExecutablePath;
            RegistryKey rk = Registry.CurrentUser; //
                                                   // 添加到 当前登陆用户的 注册表启动项     
            try
            {
                //  
                //SetValue:存储值的名称   
                RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");

                // 检测是否之前有设置自启动了，如果设置了，就看值是否一样
                string old_path = (string)rk2.GetValue(SoftWare);
                Console.WriteLine("\r\n注册表值: {0}", old_path);

                if (old_path == null || !path.Equals(old_path))
                {
                    rk2.SetValue(SoftWare, path);

                    MessageBox.Show("添加开启启动成功");
                }
                else
                {
                    rk2.DeleteValue(SoftWare, false);
                    MessageBox.Show("已取消开机启动");
                }

                rk2.Close();
                rk.Close();

            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);

            }
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://www.yuque.com/cancan-wvvto/kawdmw/kwm5yqrbk4wp4uyq") { UseShellExecute = true });
        
        }
    }
}

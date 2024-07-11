using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Management;

namespace SPrinterServer
{
    public static class Printers {
        public static List<PrinterModel> MyPrinters = new List<PrinterModel>();
    }
    public class LocalPrinter
    {
           
        public void InitLocalPrinters()
        {
            string query = string.Format("SELECT * from Win32_Printer ");
            var searcher = new ManagementObjectSearcher(query);
            var printers = searcher.Get();
            Printers.MyPrinters.Clear();


            foreach (var printer in printers)
            {
                string printname = printer.Properties["DeviceID"].Value.ToString();
                //|| printname.Contains("Fax") || printname.Contains("OneNote") || printname.Contains("XPS") || printname.Contains("PDF")
                if (false)
                {

                }
                else
                {
                    string status = printer.Properties["PrinterStatus"].Value.ToString();
                    string status_name = GetPrinterStatus(status);
                    bool WorkOffline = (bool)printer["WorkOffline"];
                    if (WorkOffline)
                    {//已离线
                        status = "7";
                        status_name = GetPrinterStatus(status);
                    }
                    PrinterModel p = new PrinterModel();
                    p.DeviceName = printname;
                    
                    p.StatusCode = int.Parse(status);
                    p.StatusName = status_name;

                    Printers.MyPrinters.Add(p);
                }

            }

        }


        public string GetPrinterStatus(object PrinterStatus)
        {
            int intValue = int.Parse(PrinterStatus.ToString());
            string strRet = string.Empty;
            switch (intValue)
            {
                case 1:
                    strRet = "休眠中/打印机故障";
                    break;
                case 2:
                    strRet = "打印机错误";
                    break;
                case 3:
                    strRet = "空闲中";
                    break;
                case 4:
                    strRet = "正在打印";
                    break;
                case 5:
                    strRet = "预热";
                    break;
                case 6:
                    strRet = "停止打印";
                    break;
                case 7:
                    strRet = "离线";
                    break;
                default:
                    strRet = intValue.ToString();
                    break;
            }
            return strRet;
        }

    }

    public class PrinterModel {
        public string StatusName;
        public string DeviceName;
        public int StatusCode;
    }
}
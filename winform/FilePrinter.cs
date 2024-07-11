using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using Spire.Pdf;
using Microsoft.Office.Interop.Word;
using System.Windows.Forms;
using System.Xml.Linq;
using System.IO;
using System.Reflection;


namespace SPrinterServer
{
    public class FilePrinter
    {
        private string tempPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\temp\\";

        public FilePrinter()
        {
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }
        }

        public bool printWord(string filePath, string printerName)
        {

            try
            {
                Microsoft.Office.Interop.Word.Application wordApp = new Microsoft.Office.Interop.Word.Application();
                // 打开 Word 文档
                Document doc = wordApp.Documents.Open(filePath);
                // 指定打印机名称
              
                // 设置打印选项
                object missing = System.Reflection.Missing.Value;
                PrintDialog dialog = new PrintDialog();
                dialog.PrinterSettings.PrinterName = printerName;
                // 直接打印文档
                doc.PrintOut(missing, missing, missing, missing, missing, missing, missing, missing, dialog.PrinterSettings.PrinterName);
                // 关闭并释放资源
                doc.Close();
                wordApp.Quit();
             
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            

            return true;
        }


        public bool printPdf(string filePath,string printerName)
        {
            try
            {
                PdfDocument document = new PdfDocument();
                document.LoadFromFile(filePath);

                document.PrintSettings.PrinterName = printerName;
                document.PrintSettings.PrintController = new StandardPrintController();
                document.Print();
             
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return true;
        }
        public bool printImage(string filePath, string printerName)
        {
            try
            {
                PrintDocument printDocument = new PrintDocument();
                printDocument.PrinterSettings.PrinterName = printerName;
                printDocument.PrintController = new StandardPrintController();
                printDocument.PrintPage += (sender, e) =>
                {
                    Image image = Image.FromFile(filePath);
                    float width = e.PageBounds.Width;

                    float height = width * image.Height / image.Width;
                    RectangleF rect = new RectangleF(0, 0, width, height);
                    System.Drawing.Point lo = new System.Drawing.Point();
                    lo.X = 0;
                    lo.Y = 0;
                    rect.Offset(lo);
                    e.Graphics.DrawImage(image, rect);
                };
                printDocument.Print();
              
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return true;
        }



       

    }
}

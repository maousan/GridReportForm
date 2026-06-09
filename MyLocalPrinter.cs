using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GridReportForm
{
    public class MyLocalPrinter
    {

        [DllImport("winspool.drv")]
        public static extern bool SetDefaultPrinter(string name);

        private static PrintDocument fpd = new PrintDocument();
        //获取本机默认打印机名称 
        public static string DefaultPrinter()
        {
            return fpd.PrinterSettings.PrinterName;
        }
        public static List<string> GetLocalPrinters()
        {
            List<String> Printers = new List<String>();
            //默认打印机始终出现在列表的第一项
            Printers.Add(DefaultPrinter());
            foreach (String printerName in PrinterSettings.InstalledPrinters)
            {
                if (!Printers.Contains(printerName))
                {
                    Printers.Add(printerName);
                }
            }
            return Printers;
        }
    }
}

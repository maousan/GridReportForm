using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config")]

namespace GridReportForm
{

    internal static class Program
    {

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            var command = string.Join(" ", args);
            AntdUI.Localization.DefaultLanguage = "zh-CN";
            //AntdUI.Style.Set(AntdUI.Colour.Primary, Color.FromArgb(0, 69, 120));
            var lang = AntdUI.Localization.CurrentLanguage;
            AntdUI.Config.Mode = AntdUI.TMode.Light;
            AntdUI.Config.TextRenderingHighQuality = true;
            AntdUI.Config.Animation = false;
            AntdUI.Config.ShadowEnabled = false;
            AntdUI.Config.SetCorrectionTextRendering("Microsoft YaHei UI", "微软雅黑", "宋体"); //需要修正的字体列表
            AntdUI.Config.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            AntdUI.Config.Font = new Font("微软雅黑", 10);
            //处理未捕获的异常   
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            //处理UI线程异常
            Application.ThreadException += Application_ThreadException;
            //处理多线程异常
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandleException;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        /// <summary>
        /// UI线程异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            //可以记录日志并转向错误bug窗口友好提示用户
            MessageBox.Show(e.Exception.Message);
        }
        /// <summary>
        /// 多线程异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void CurrentDomain_UnhandleException(object sender, UnhandledExceptionEventArgs e)
        {
            //可以记录日志并转向错误bug窗口友好提示用户
            Exception ex = e.ExceptionObject as Exception;
            MessageBox.Show(ex.Message);
        }
    }
}

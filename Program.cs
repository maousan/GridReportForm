using System;
using System.Drawing;
using System.Windows.Forms;

namespace GridReportForm
{

    internal static class Program
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                logger.Info("Application starting. BaseDirectory={BaseDirectory}, Args={Args}", AppDomain.CurrentDomain.BaseDirectory, string.Join(" ", args ?? new string[0]));
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
                logger.Info("Application exited normally.");
            }
            catch (Exception exception)
            {
                logger.Fatal(exception, "Application startup failed.");
                MessageBox.Show(exception.Message);
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        /// <summary>
        /// UI线程异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            //可以记录日志并转向错误bug窗口友好提示用户
            logger.Error(e.Exception, "Unhandled UI thread exception.");
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
            logger.Fatal(ex, "Unhandled non-UI exception. IsTerminating={IsTerminating}", e.IsTerminating);
            MessageBox.Show(ex?.Message ?? e.ExceptionObject?.ToString());
        }
    }
}

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using gregn6Lib;
using System.IO;
using Newtonsoft.Json.Linq;

namespace GridReportForm
{
	/// <summary>
	/// DesignForm 的摘要说明。
	/// </summary>
	public class PrintForm : System.Windows.Forms.Form
	{
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private System.ComponentModel.IContainer components;
        private string template;
        private string source;
        private JObject extInfo;
        private string tmpFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tmp");
        private GridppReport Report = new GridppReport();

        public PrintForm()
        {
            //
            // Windows 窗体设计器支持所必需的
            //
            InitializeComponent();

            //
            // TODO: 在 InitializeComponent 调用后添加任何构造函数代码
            //
        }

        public PrintForm(string template, string source, JObject extInfo = null)
        {
            InitializeComponent();
            this.template = template;
            this.source = source;
            this.extInfo = extInfo;
            if (string.IsNullOrEmpty(template))
            {
                MessageBox.Show("打印失败，打印格式不存在", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (template.StartsWith("http"))
                {
                    Uri uri = new Uri(template);
                    string fileName = System.IO.Path.GetFileName(uri.LocalPath);
                    HttpClientUtils.DownloadFile(template, fileName);
                    template = fileName;
                }
                Report.LoadFromFile(template);
                //设置与数据源的连接串，因为在设计时指定的数据库路径是绝对路径。
                if (!string.IsNullOrEmpty(source))
                {
                    Report.ConnectionString = "XML";
                    if (source.StartsWith("http"))
                    {
                        Report.QuerySQL = "";
                        Report.LoadDataFromURL(source);
                    }
                    else
                    {
                        //生成临时的json文件
                        Report.QuerySQL = CreateDataFile(source);
                    }
                }
                Report.Printer.PrinterName = extInfo.Value<string>("printerName");
                bool showPrintDialog = extInfo.Value<bool>("showPrintDialog");
                Report.Print(showPrintDialog);
            }
            this.Close();
        }

        private void CreateFolder()
        {
            if (!Directory.Exists(tmpFolderPath))
            {
                Directory.CreateDirectory(tmpFolderPath);
            }
        }

        private string CreateTemplateFile()
        {
            string id = extInfo.Value<string>("id");
            string fileId = string.IsNullOrEmpty(id) ? $"template-{Guid.NewGuid().ToString()}" : $"template-{id}";
            string filePath = Path.Combine(tmpFolderPath, fileId + ".grf");
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Dispose();
            }
            return filePath;
        }

        private string CreateDataFile(string data)
        {
            string id = extInfo.Value<string>("id");
            string fileId = string.IsNullOrEmpty(id) ? $"template-data-{Guid.NewGuid().ToString()}" : $"template-data-{id}";
            string filePath = Path.Combine(tmpFolderPath, fileId + ".json");
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Dispose();
            }
            // 创建一个文件流
            using (FileStream fs = new FileStream(filePath, FileMode.Truncate))
            {
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(data);
                // 写入数据到临时文件
                fs.Write(buffer, 0, buffer.Length);
            }
            return filePath;
        }


        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
            //删除数据文件
			base.Dispose( disposing );
            if (File.Exists(source))
            {
                try
                {
                    File.Delete(source);
                }
                catch (Exception e)
                {

                }
            }
        }

		#region Windows 窗体设计器生成的代码
		/// <summary>
		/// 设计器支持所需的方法 - 不要使用代码编辑器修改
		/// 此方法的内容。
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PrintForm));
            this.SuspendLayout();
            // 
            // PrintForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(640, 446);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PrintForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PrintForm";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ResumeLayout(false);

		}
        #endregion

        protected override void SetVisibleCore(bool value)
        {
            try
            {
                if (!this.IsHandleCreated)
                {
                    this.CreateHandle();
                    value = false;
                }
                base.SetVisibleCore(value);
            }
            catch (Exception err)
            {
                logger.Error(err);
            }

        }
    }
}

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
	/// PreviewForm 的摘要说明。
	/// </summary>
	public class PreviewForm : System.Windows.Forms.Form, IGridForm
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private Axgregn6Lib.AxGRPrintViewer axGRPrintViewer;
		/// <summary>
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.Container components = null;
        private string template;
        private string source;
        private JObject extInfo;
        private string tmpFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tmp");
        private GridppReport Report = new GridppReport();

        public PreviewForm()
		{
			//
			// Windows 窗体设计器支持所必需的
			//
			InitializeComponent();

			//
			// TODO: 在 InitializeComponent 调用后添加任何构造函数代码
			//
		}

        public PreviewForm(string template, string source = "", JObject extInfo = null)
        {
            //
            // Windows 窗体设计器支持所必需的
            //
            InitializeComponent();
            logger.Info("Creating preview form. Template={Template}, HasSource={HasSource}", template, !string.IsNullOrWhiteSpace(source));
            if (string.IsNullOrEmpty(template))
            {
                logger.Warn("Preview failed because template is empty.");
                MessageBox.Show("预览失败，打印格式不存在", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            } 
            else
            {
                this.template = template;
                this.source = source;
                this.extInfo = extInfo;
            }
            CreateFolder();
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
			base.Dispose( disposing );
            if (File.Exists(source))
            {
                try
                {
                    File.Delete(source);
                }
                catch (Exception e)
                {
                    logger.Warn(e, "Deleting preview source temp file failed. Source={Source}", source);
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PreviewForm));
            this.axGRPrintViewer = new Axgregn6Lib.AxGRPrintViewer();
            ((System.ComponentModel.ISupportInitialize)(this.axGRPrintViewer)).BeginInit();
            this.SuspendLayout();
            // 
            // axGRPrintViewer1
            // 
            this.axGRPrintViewer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axGRPrintViewer.Enabled = true;
            this.axGRPrintViewer.Location = new System.Drawing.Point(0, 0);
            this.axGRPrintViewer.Name = "axGRPrintViewer1";
            this.axGRPrintViewer.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axGRPrintViewer1.OcxState")));
            this.axGRPrintViewer.Size = new System.Drawing.Size(488, 398);
            this.axGRPrintViewer.TabIndex = 0;
            // 
            // PreviewForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(488, 398);
            this.Controls.Add(this.axGRPrintViewer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PreviewForm";
            this.Text = "PreviewForm";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Closed += new System.EventHandler(this.PreviewForm_Closed);
            this.Load += new System.EventHandler(this.PreviewForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.axGRPrintViewer)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

		public void AttachReport(GridppReport Report)
		{
			//设定查询显示器关联的报表
			axGRPrintViewer.Report = Report;
		}

        private void PreviewForm_Load(object sender, System.EventArgs e)
		{
            logger.Info("Preview form loaded. Template={Template}, HasSource={HasSource}", template, !string.IsNullOrWhiteSpace(source));
            if (!string.IsNullOrEmpty(template))
            {
                if (template.StartsWith("http"))
                {
                    Uri uri = new Uri(template);
                    string fileName = System.IO.Path.GetFileName(uri.LocalPath);
                    logger.Info("Downloading preview template. Url={Url}, FileName={FileName}", template, fileName);
                    HttpClientUtils.DownloadFile(template, fileName);
                    template = fileName;
                }
                logger.Debug("Loading preview template. Template={Template}", template);
                Report.LoadFromFile(template);
                if (!string.IsNullOrEmpty(source))
                {
                    Report.ConnectionString = "XML";
                    if (source.StartsWith("http"))
                    {
                        Report.QuerySQL = "";
                        logger.Debug("Loading preview data from url. DataUrl={DataUrl}", source);
                        Report.LoadDataFromURL(source);
                    }
                    else
                    {
                        //生成临时的json文件
                        logger.Debug("Creating preview data temp file.");
                        Report.QuerySQL = CreateDataFile(source);
                    }
                }
                AttachReport(Report);
            }
            logger.Debug("Starting preview viewer.");
            axGRPrintViewer.Start();
		}

		private void PreviewForm_Closed(object sender, System.EventArgs e)
		{
            logger.Debug("Preview form closed. Stopping viewer.");
			axGRPrintViewer.Stop();
		}
	}
}

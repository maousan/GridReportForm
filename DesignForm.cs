using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using gregn6Lib;
using System.Diagnostics.Eventing.Reader;
using System.Security.Policy;
using System.IO;
using Newtonsoft.Json.Linq;

namespace GridReportForm
{
    /// <summary>
    /// DesignForm 的摘要说明。
    /// </summary>
    public class DesignForm : System.Windows.Forms.Form
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private System.Windows.Forms.ToolTip toolTip1;
        private System.ComponentModel.IContainer components;
        private string template;
        private string source;
        private JObject extInfo;
        private AntdUI.FlowPanel flowPanel1;
        private Axgrdes6Lib.AxGRDesigner axGRDesigner;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private string tmpFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tmp");
        private AntdUI.Button saveButton;
        private AntdUI.Button openButton;
        private AntdUI.Button saveAsButton;

        //定义Grid++Report报表主对象
        private GridppReport Report = new GridppReport();

        public DesignForm(
            string template = "", 
            string source = "",
            JObject extInfo = null)
        {
            InitializeComponent();
            this.template = template;
            this.source = source;
            this.extInfo = extInfo;
            CreateFolder();
            openFileDialog.InitialDirectory = tmpFolderPath;
        }

        private void DesignForm_Load(object sender, EventArgs e)
        {
            logger.Info("Design form loaded. Template={Template}, HasSource={HasSource}", template, !string.IsNullOrWhiteSpace(source));
            if (!string.IsNullOrEmpty(template))
            {
                if (template.StartsWith("http"))
                {
                    Uri uri = new Uri(template);
                    //string fileName = System.IO.Path.GetFileName(uri.LocalPath);
                    string fileName = CreateTemplateFile();
                    logger.Info("Downloading design template. Url={Url}, FileName={FileName}", template, fileName);
                    HttpClientUtils.DownloadFile(template, fileName);
                    template = fileName;
                }
                openFileDialog.FileName = template;
                logger.Debug("Loading design template. Template={Template}", template);
                Report.LoadFromFile(template);
            }
            if (!string.IsNullOrEmpty(source))
            {
                Report.ConnectionString = "XML";
                if (source.StartsWith("http"))
                {
                    Report.QuerySQL = "";
                    logger.Debug("Loading design data from url. DataUrl={DataUrl}", source);
                    Report.LoadDataFromURL(source);
                }
                else
                {
                    //生成临时的json文件
                    logger.Debug("Creating design data temp file.");
                    Report.QuerySQL = CreateDataFile(source);
                }
            }
            AttachReport(Report);
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
                catch(Exception e)
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DesignForm));
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.flowPanel1 = new AntdUI.FlowPanel();
            this.saveAsButton = new AntdUI.Button();
            this.saveButton = new AntdUI.Button();
            this.openButton = new AntdUI.Button();
            this.axGRDesigner = new Axgrdes6Lib.AxGRDesigner();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.flowPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.axGRDesigner)).BeginInit();
            this.SuspendLayout();
            // 
            // flowPanel1
            // 
            this.flowPanel1.Controls.Add(this.saveAsButton);
            this.flowPanel1.Controls.Add(this.saveButton);
            this.flowPanel1.Controls.Add(this.openButton);
            this.flowPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowPanel1.Location = new System.Drawing.Point(0, 0);
            this.flowPanel1.Name = "flowPanel1";
            this.flowPanel1.Size = new System.Drawing.Size(640, 30);
            this.flowPanel1.TabIndex = 1;
            this.flowPanel1.Text = "flowPanel1";
            this.flowPanel1.Visible = false;
            // 
            // saveAsButton
            // 
            this.saveAsButton.BadgeBack = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(90)))), ((int)(((byte)(158)))));
            this.saveAsButton.BorderWidth = 1F;
            this.saveAsButton.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.saveAsButton.Location = new System.Drawing.Point(175, 3);
            this.saveAsButton.Name = "saveAsButton";
            this.saveAsButton.Size = new System.Drawing.Size(80, 27);
            this.saveAsButton.TabIndex = 8;
            this.saveAsButton.Text = "另存为";
            this.saveAsButton.Type = AntdUI.TTypeMini.Primary;
            this.saveAsButton.WaveSize = 0;
            this.saveAsButton.Click += new System.EventHandler(this.saveAsButton_Click);
            // 
            // saveButton
            // 
            this.saveButton.BadgeBack = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(90)))), ((int)(((byte)(158)))));
            this.saveButton.BorderWidth = 1F;
            this.saveButton.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.saveButton.Location = new System.Drawing.Point(89, 3);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(80, 27);
            this.saveButton.TabIndex = 7;
            this.saveButton.Text = "保存";
            this.saveButton.Type = AntdUI.TTypeMini.Primary;
            this.saveButton.WaveSize = 0;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // openButton
            // 
            this.openButton.BadgeBack = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(90)))), ((int)(((byte)(158)))));
            this.openButton.BorderWidth = 1F;
            this.openButton.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.openButton.Location = new System.Drawing.Point(3, 3);
            this.openButton.Name = "openButton";
            this.openButton.Size = new System.Drawing.Size(80, 27);
            this.openButton.TabIndex = 6;
            this.openButton.Text = "打开";
            this.openButton.Type = AntdUI.TTypeMini.Primary;
            this.openButton.WaveSize = 0;
            this.openButton.Click += new System.EventHandler(this.openButton_Click);
            // 
            // axGRDesigner
            // 
            this.axGRDesigner.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axGRDesigner.Enabled = true;
            this.axGRDesigner.Location = new System.Drawing.Point(0, 30);
            this.axGRDesigner.Name = "axGRDesigner";
            this.axGRDesigner.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axGRDesigner.OcxState")));
            this.axGRDesigner.Size = new System.Drawing.Size(640, 416);
            this.axGRDesigner.TabIndex = 2;
            this.axGRDesigner.OpenReport += new System.EventHandler(this.axGRDesigner1_OpenReport);
            this.axGRDesigner.SaveReport += new System.EventHandler(this.axGRDesigner1_SaveReport);
            // 
            // DesignForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(640, 446);
            this.Controls.Add(this.axGRDesigner);
            this.Controls.Add(this.flowPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DesignForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DesignForm";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Closed += new System.EventHandler(this.DesignForm_Closed);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DesignForm_Closed);
            this.Load += new System.EventHandler(this.DesignForm_Load);
            this.flowPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.axGRDesigner)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

		public void AttachReport(GridppReport Report)
		{
			//设定查询显示器关联的报表
			axGRDesigner.Report = Report;
        }

		private void DesignForm_Closed(object sender, System.EventArgs e)
		{
			if (axGRDesigner.Dirty)
				axGRDesigner.Post();		
		}

        private void saveButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog.FileName != "")
            {
                axGRDesigner.Post();
                Report.SaveToFile(openFileDialog.FileName);
                RemoteSave(openFileDialog.FileName);
            }
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Report.LoadFromFile(openFileDialog.FileName);
                axGRDesigner.Reload();
            }
        }

        private void saveAsButton_Click(object sender, EventArgs e)
        {
            saveFileDialog.FileName = openFileDialog.FileName;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                axGRDesigner.Post();
                Report.SaveToFile(saveFileDialog.FileName);
            }
        }

        private void axGRDesigner1_OpenReport(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Report.LoadFromFile(openFileDialog.FileName);
                axGRDesigner.Reload();
            }

            //将 DefaultAction 属性为假, 忽略掉设计器控件本身的打开行为
            axGRDesigner.DefaultAction = false;
        }

        private void axGRDesigner1_SaveReport(object sender, EventArgs e)
        {
            bool ToSave = true;
            saveFileDialog.FileName = openFileDialog.FileName;
            if (saveFileDialog.FileName == "")
                ToSave = saveFileDialog.ShowDialog() == DialogResult.OK;

            if (ToSave)
            {
                axGRDesigner.Post();
                Report.SaveToFile(saveFileDialog.FileName);
                RemoteSave(saveFileDialog.FileName);
            }

            //将 DefaultAction 属性为假, 忽略掉设计器控件本身的保存行为
            axGRDesigner.DefaultAction = false;
        }

        private void RemoteSave(string fileName)
        {
        }
    }
}

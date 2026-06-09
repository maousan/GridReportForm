using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using gregn6Lib;

namespace GridReportForm
{
	/// <summary>
	/// MethodBaseForm 的摘要说明。
	/// </summary>
	public class DisplayForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button btnPrintPreview;
		internal System.Windows.Forms.Button btnSearchAgain;
		internal System.Windows.Forms.Button btnSerchDlg;
		internal System.Windows.Forms.Button btnSearchDirect;
		internal System.Windows.Forms.TextBox tbSearchText;
		internal System.Windows.Forms.CheckBox cbPreviewLine;
		private System.Windows.Forms.ComboBox cmbLanguage;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label lbPageStatus;
		private System.Windows.Forms.Button btnApplyRowsPerPage;
		private System.Windows.Forms.TextBox txtRowsPerPage;
		private System.Windows.Forms.RadioButton rbFixRowMultiPage;
		private System.Windows.Forms.RadioButton rbAutoRowMultiPage;
        private System.Windows.Forms.RadioButton rbSinglePage;
        private Axgregn6Lib.AxGRDisplayViewer axGRDisplayViewer1;
		/// <summary>
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.Container components = null;

		public DisplayForm()
		{
			//
			// Windows 窗体设计器支持所必需的
			//
			InitializeComponent();

			//
			//在 InitializeComponent 调用后添加任何构造函数代码

			cmbLanguage.Items.Add( "简体中文" );
			cmbLanguage.Items.Add( "繁体中文" );
			cmbLanguage.Items.Add( "英文" );
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
		}

		#region Windows 窗体设计器生成的代码
		/// <summary>
		/// 设计器支持所需的方法 - 不要使用代码编辑器修改
		/// 此方法的内容。
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DisplayForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.cmbLanguage = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnSearchAgain = new System.Windows.Forms.Button();
            this.btnSerchDlg = new System.Windows.Forms.Button();
            this.btnSearchDirect = new System.Windows.Forms.Button();
            this.tbSearchText = new System.Windows.Forms.TextBox();
            this.cbPreviewLine = new System.Windows.Forms.CheckBox();
            this.btnPrintPreview = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lbPageStatus = new System.Windows.Forms.Label();
            this.btnApplyRowsPerPage = new System.Windows.Forms.Button();
            this.txtRowsPerPage = new System.Windows.Forms.TextBox();
            this.rbFixRowMultiPage = new System.Windows.Forms.RadioButton();
            this.rbAutoRowMultiPage = new System.Windows.Forms.RadioButton();
            this.rbSinglePage = new System.Windows.Forms.RadioButton();
            this.axGRDisplayViewer1 = new Axgregn6Lib.AxGRDisplayViewer();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.axGRDisplayViewer1)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cmbLanguage);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.btnSearchAgain);
            this.panel1.Controls.Add(this.btnSerchDlg);
            this.panel1.Controls.Add(this.btnSearchDirect);
            this.panel1.Controls.Add(this.tbSearchText);
            this.panel1.Controls.Add(this.cbPreviewLine);
            this.panel1.Controls.Add(this.btnPrintPreview);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(720, 40);
            this.panel1.TabIndex = 8;
            // 
            // cmbLanguage
            // 
            this.cmbLanguage.Location = new System.Drawing.Point(600, 8);
            this.cmbLanguage.Name = "cmbLanguage";
            this.cmbLanguage.Size = new System.Drawing.Size(112, 20);
            this.cmbLanguage.TabIndex = 22;
            this.cmbLanguage.SelectedIndexChanged += new System.EventHandler(this.cmbLanguage_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(528, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 16);
            this.label2.TabIndex = 21;
            this.label2.Text = "界面语言:";
            // 
            // btnSearchAgain
            // 
            this.btnSearchAgain.Location = new System.Drawing.Point(448, 8);
            this.btnSearchAgain.Name = "btnSearchAgain";
            this.btnSearchAgain.Size = new System.Drawing.Size(75, 23);
            this.btnSearchAgain.TabIndex = 17;
            this.btnSearchAgain.Text = "继续查找(F3)";
            this.btnSearchAgain.Click += new System.EventHandler(this.btnSearchAgain_Click);
            // 
            // btnSerchDlg
            // 
            this.btnSerchDlg.Location = new System.Drawing.Point(371, 8);
            this.btnSerchDlg.Name = "btnSerchDlg";
            this.btnSerchDlg.Size = new System.Drawing.Size(69, 23);
            this.btnSerchDlg.TabIndex = 16;
            this.btnSerchDlg.Text = "查找...";
            this.btnSerchDlg.Click += new System.EventHandler(this.btnSerchDlg_Click);
            // 
            // btnSearchDirect
            // 
            this.btnSearchDirect.Enabled = false;
            this.btnSearchDirect.Location = new System.Drawing.Point(299, 8);
            this.btnSearchDirect.Name = "btnSearchDirect";
            this.btnSearchDirect.Size = new System.Drawing.Size(64, 23);
            this.btnSearchDirect.TabIndex = 15;
            this.btnSearchDirect.Text = "直接查找";
            this.btnSearchDirect.Click += new System.EventHandler(this.btnSearchDirect_Click);
            // 
            // tbSearchText
            // 
            this.tbSearchText.Location = new System.Drawing.Point(227, 8);
            this.tbSearchText.Name = "tbSearchText";
            this.tbSearchText.Size = new System.Drawing.Size(64, 21);
            this.tbSearchText.TabIndex = 14;
            this.tbSearchText.TextChanged += new System.EventHandler(this.tbSearchText_TextChanged);
            // 
            // cbPreviewLine
            // 
            this.cbPreviewLine.Location = new System.Drawing.Point(104, 8);
            this.cbPreviewLine.Name = "cbPreviewLine";
            this.cbPreviewLine.Size = new System.Drawing.Size(112, 24);
            this.cbPreviewLine.TabIndex = 13;
            this.cbPreviewLine.Text = "显示垂直分页线";
            this.cbPreviewLine.CheckedChanged += new System.EventHandler(this.cbPreviewLine_CheckedChanged);
            // 
            // btnPrintPreview
            // 
            this.btnPrintPreview.Location = new System.Drawing.Point(8, 8);
            this.btnPrintPreview.Name = "btnPrintPreview";
            this.btnPrintPreview.Size = new System.Drawing.Size(75, 23);
            this.btnPrintPreview.TabIndex = 7;
            this.btnPrintPreview.Text = "打印预览";
            this.btnPrintPreview.Click += new System.EventHandler(this.btnPrintPreview_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.groupBox1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 454);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(720, 48);
            this.panel2.TabIndex = 11;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lbPageStatus);
            this.groupBox1.Controls.Add(this.btnApplyRowsPerPage);
            this.groupBox1.Controls.Add(this.txtRowsPerPage);
            this.groupBox1.Controls.Add(this.rbFixRowMultiPage);
            this.groupBox1.Controls.Add(this.rbAutoRowMultiPage);
            this.groupBox1.Controls.Add(this.rbSinglePage);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(720, 48);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "分页设置：设定 RowsPerPage 属性控制分页";
            // 
            // lbPageStatus
            // 
            this.lbPageStatus.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbPageStatus.ForeColor = System.Drawing.Color.Red;
            this.lbPageStatus.Location = new System.Drawing.Point(532, 18);
            this.lbPageStatus.Name = "lbPageStatus";
            this.lbPageStatus.Size = new System.Drawing.Size(100, 16);
            this.lbPageStatus.TabIndex = 5;
            this.lbPageStatus.Text = "label1";
            // 
            // btnApplyRowsPerPage
            // 
            this.btnApplyRowsPerPage.Location = new System.Drawing.Point(460, 18);
            this.btnApplyRowsPerPage.Name = "btnApplyRowsPerPage";
            this.btnApplyRowsPerPage.Size = new System.Drawing.Size(56, 24);
            this.btnApplyRowsPerPage.TabIndex = 4;
            this.btnApplyRowsPerPage.Text = "应用";
            this.btnApplyRowsPerPage.Click += new System.EventHandler(this.btnApplyRowsPerPage_Click);
            // 
            // txtRowsPerPage
            // 
            this.txtRowsPerPage.Location = new System.Drawing.Point(404, 18);
            this.txtRowsPerPage.MaxLength = 4;
            this.txtRowsPerPage.Name = "txtRowsPerPage";
            this.txtRowsPerPage.Size = new System.Drawing.Size(48, 21);
            this.txtRowsPerPage.TabIndex = 3;
            this.txtRowsPerPage.Text = "20";
            // 
            // rbFixRowMultiPage
            // 
            this.rbFixRowMultiPage.Location = new System.Drawing.Point(252, 18);
            this.rbFixRowMultiPage.Name = "rbFixRowMultiPage";
            this.rbFixRowMultiPage.Size = new System.Drawing.Size(128, 23);
            this.rbFixRowMultiPage.TabIndex = 2;
            this.rbFixRowMultiPage.Text = "指定每页行数分页";
            this.rbFixRowMultiPage.Click += new System.EventHandler(this.rbFixRowMultiPage_CheckedChanged);
            // 
            // rbAutoRowMultiPage
            // 
            this.rbAutoRowMultiPage.Location = new System.Drawing.Point(92, 18);
            this.rbAutoRowMultiPage.Name = "rbAutoRowMultiPage";
            this.rbAutoRowMultiPage.Size = new System.Drawing.Size(152, 23);
            this.rbAutoRowMultiPage.TabIndex = 1;
            this.rbAutoRowMultiPage.Text = "每页行数自动适应分页";
            this.rbAutoRowMultiPage.Click += new System.EventHandler(this.rbAutoRowMultiPage_CheckedChanged);
            // 
            // rbSinglePage
            // 
            this.rbSinglePage.Location = new System.Drawing.Point(12, 18);
            this.rbSinglePage.Name = "rbSinglePage";
            this.rbSinglePage.Size = new System.Drawing.Size(72, 23);
            this.rbSinglePage.TabIndex = 0;
            this.rbSinglePage.Text = "不分页";
            this.rbSinglePage.CheckedChanged += new System.EventHandler(this.rbSinglePage_CheckedChanged);
            this.rbSinglePage.Click += new System.EventHandler(this.rbSinglePage_CheckedChanged);
            // 
            // axGRDisplayViewer1
            // 
            this.axGRDisplayViewer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axGRDisplayViewer1.Enabled = true;
            this.axGRDisplayViewer1.Location = new System.Drawing.Point(0, 40);
            this.axGRDisplayViewer1.Name = "axGRDisplayViewer1";
            this.axGRDisplayViewer1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axGRDisplayViewer1.OcxState")));
            this.axGRDisplayViewer1.Size = new System.Drawing.Size(720, 414);
            this.axGRDisplayViewer1.TabIndex = 12;
            // 
            // DisplayForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(720, 502);
            this.Controls.Add(this.axGRDisplayViewer1);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "DisplayForm";
            this.Text = "MethodBaseForm";
            this.Closed += new System.EventHandler(this.DisplayForm_Closed);
            this.Load += new System.EventHandler(this.DisplayForm_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.axGRDisplayViewer1)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

		public void AttachReport(GridppReport Report)
		{
			//设定查询显示器关联的报表
			axGRDisplayViewer1.Report = Report;
		}

		private void btnPrintPreview_Click(object sender, System.EventArgs e)
		{
			//在进行打印之前，先将列的当前显示布局（包括宽度与顺序）提交到报表对象
			axGRDisplayViewer1.PostColumnLayout();
			axGRDisplayViewer1.Report.PrintPreview(true);
		}

		private void DisplayForm_Load(object sender, System.EventArgs e)
		{
			axGRDisplayViewer1.Start();
		}

		private void DisplayForm_Closed(object sender, System.EventArgs e)
		{
			axGRDisplayViewer1.Stop();
		}

		private void cbPreviewLine_CheckedChanged(object sender, System.EventArgs e)
		{
			axGRDisplayViewer1.ShowPreviewLine = cbPreviewLine.Checked;
		}

		private void tbSearchText_TextChanged(object sender, System.EventArgs e)
		{
			btnSearchDirect.Enabled = (tbSearchText.Text.Length > 0);
		}

		private void btnSearchDirect_Click(object sender, System.EventArgs e)
		{
			axGRDisplayViewer1.Search(tbSearchText.Text, false, false, false, true, false, true);
			axGRDisplayViewer1.Focus();
		}

		private void btnSerchDlg_Click(object sender, System.EventArgs e)
		{
			axGRDisplayViewer1.Search("", false, false, false, true, true, true);
			axGRDisplayViewer1.Focus();
		}

		private void btnSearchAgain_Click(object sender, System.EventArgs e)
		{
			axGRDisplayViewer1.SearchAgain(true);
			axGRDisplayViewer1.Focus();
		}

		private void cmbLanguage_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			int Language;
			if (cmbLanguage.SelectedIndex == 0)
				Language = 0x0804;
			else if (cmbLanguage.SelectedIndex == 1)
				Language = 0x0404;
			else
				Language = 0x0409;
			axGRDisplayViewer1.Report.Language = Language;
			axGRDisplayViewer1.UpdateLanguage();
		}

		private void rbSinglePage_CheckedChanged(object sender, System.EventArgs e)
		{
			axGRDisplayViewer1.RowsPerPage = 0;
			txtRowsPerPage.Enabled = false;
			btnApplyRowsPerPage.Enabled = false;
		}

		private void rbAutoRowMultiPage_CheckedChanged(object sender, System.EventArgs e)
		{
			axGRDisplayViewer1.RowsPerPage = -1;
			txtRowsPerPage.Enabled = false;
			btnApplyRowsPerPage.Enabled = false;
		}

		private void rbFixRowMultiPage_CheckedChanged(object sender, System.EventArgs e)
		{
			btnApplyRowsPerPage_Click(sender, e);

			//axGRDisplayViewer1.RowsPerPage = -1;
			txtRowsPerPage.Enabled = true;
			btnApplyRowsPerPage.Enabled = true;
		
		}

		private void btnApplyRowsPerPage_Click(object sender, System.EventArgs e)
		{
			short RowsPerPage = Convert.ToInt16( txtRowsPerPage.Text );
			axGRDisplayViewer1.RowsPerPage = RowsPerPage;
		}

		private void axGRDisplayViewer1_StatusChange(object sender, System.EventArgs e)
		{
			if (axGRDisplayViewer1.RowsPerPage == 0)
			{
				lbPageStatus.Text = "不分页";
			}
			else
			{
				
				string Text = String.Format("第{0}页 共{1}页",axGRDisplayViewer1.CurPageNo, axGRDisplayViewer1.PageCount);
				lbPageStatus.Text = Text;
			}
		}
	}
}

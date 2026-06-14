using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using gregn6Lib;
using ReactiveUI;
using Microsoft.Win32;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace GridReportForm
{
	/// <summary>
	/// MainForm 的摘要说明。
	/// </summary>
	public class MainForm : Form, IViewFor<MainViewModel>
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly RegistryKey registryKey;
        private readonly ReportTaskExecutor taskExecutor;
        private readonly CloudWebSocketTransport cloudTransport;
        private readonly ApplicationUpdateService updateService = new ApplicationUpdateService();
        private bool updateCheckRunning;


        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (MainViewModel)value;
        }

        public MainViewModel ViewModel { get; set; } = new MainViewModel();


        //定义Grid++Report报表主对象
        protected GridppReport Report = new GridppReport();
		private System.Windows.Forms.ImageList imageList1;
        private NotifyIcon mainNotifyIcon;
        private ContextMenuStrip mainContextMenuStrip;
        private ToolStripMenuItem autoStartToolStripMenuItem;
        private ToolStripMenuItem checkUpdateToolStripMenuItem;
        private ToolStripMenuItem appDirectoryToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private System.ComponentModel.IContainer components;
        static object lockDesignForm = new object();
        private AntdUI.Select defaultPrinterSelect;
        private AntdUI.Label label5;
        private AntdUI.Label label1;
        private AntdUI.Input cloudServerUrlInput;
        private AntdUI.Label deviceTokenLabel;
        private AntdUI.Input cloudDeviceTokenInput;
        private AntdUI.Label activationCodeLabel;
        private AntdUI.Input activationCodeInput;
        private AntdUI.Label deviceIdLabel;
        private AntdUI.Input cloudDeviceIdInput;
        private AntdUI.Label deviceNameLabel;
        private AntdUI.Input cloudDeviceNameInput;
        private AntdUI.Checkbox cloudEnabledCheckbox;
        private AntdUI.Checkbox allowPreviewCheckbox;
        private AntdUI.Button toggleButton;
        private AntdUI.Button activateButton;
        private AntdUI.Button refreshPrintersButton;
        private AntdUI.Button settingsButton;
        private AntdUI.Label statusLabel;
        private AntdUI.Label deviceSummaryLabel;
        private AntdUI.Label activationSectionLabel;
        private AntdUI.Label connectionSectionLabel;
        private AntdUI.Label deviceSectionLabel;
        private AntdUI.Label printerSectionLabel;
        private string lastAlertedConnectionError;

        public MainForm()
		{
            //
            // Windows 窗体设计器支持所必需的
            //
            this.Font = new Font("微软雅黑", 10);
            InitializeComponent();

            this.registryKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
            this.taskExecutor = new ReportTaskExecutor(this, () => ViewModel.AllowPreview);
            this.cloudTransport = new CloudWebSocketTransport(CreateCloudOptions, taskExecutor.Execute);
            this.cloudTransport.StatusChanged += CloudTransport_StatusChanged;
            Init();
            if (ViewModel.CloudEnabled)
            {
                logger.Info("Cloud mode enabled on startup. DeviceId={DeviceId}, DeviceName={DeviceName}, HasToken={HasToken}, ServerUrl={ServerUrl}", ViewModel.CloudDeviceId, ViewModel.CloudDeviceName, !string.IsNullOrWhiteSpace(ViewModel.CloudDeviceToken), ViewModel.CloudServerUrl);
                cloudTransport.Start();
            }
            else
            {
                logger.Info("Cloud mode disabled on startup.");
            }
        }

        private void Init()
        {
            this.WhenActivated(d =>
            {
                defaultPrinterSelect.Items.AddRange(MyLocalPrinter.GetLocalPrinters().ToArray<object>());
                defaultPrinterSelect.SelectedValue = MyLocalPrinter.DefaultPrinter();
            });
            cloudServerUrlInput.Text = ViewModel.CloudServerUrl;
            Observable.FromEventPattern(cloudServerUrlInput, nameof(Control.TextChanged)).Subscribe(x =>
            {
                ViewModel.CloudServerUrl = cloudServerUrlInput.Text.Trim();
            });
            cloudDeviceTokenInput.Text = ViewModel.CloudDeviceToken;
            Observable.FromEventPattern(cloudDeviceTokenInput, nameof(Control.TextChanged)).Subscribe(x =>
            {
                ViewModel.CloudDeviceToken = cloudDeviceTokenInput.Text.Trim();
            });
            cloudDeviceIdInput.Text = ViewModel.CloudDeviceId;
            Observable.FromEventPattern(cloudDeviceIdInput, nameof(Control.TextChanged)).Subscribe(x =>
            {
                ViewModel.CloudDeviceId = cloudDeviceIdInput.Text.Trim();
            });
            cloudDeviceNameInput.Text = ViewModel.CloudDeviceName;
            Observable.FromEventPattern(cloudDeviceNameInput, nameof(Control.TextChanged)).Subscribe(x =>
            {
                ViewModel.CloudDeviceName = cloudDeviceNameInput.Text.Trim();
            });
            cloudEnabledCheckbox.Checked = ViewModel.CloudEnabled;
            Observable.FromEventPattern(cloudEnabledCheckbox, nameof(AntdUI.Checkbox.CheckedChanged)).Subscribe(x =>
            {
                ViewModel.CloudEnabled = cloudEnabledCheckbox.Checked;
                logger.Info("Cloud enabled changed. Enabled={Enabled}", ViewModel.CloudEnabled);
                if (ViewModel.CloudEnabled)
                {
                    cloudTransport.Start();
                }
                else
                {
                    cloudTransport.Stop();
                }
            });
            allowPreviewCheckbox.Checked = ViewModel.AllowPreview;
            Observable.FromEventPattern(allowPreviewCheckbox, nameof(AntdUI.Checkbox.CheckedChanged)).Subscribe(x =>
            {
                ViewModel.AllowPreview = allowPreviewCheckbox.Checked;
                logger.Info("Allow preview changed. AllowPreview={AllowPreview}", ViewModel.AllowPreview);
            });
            Observable.FromEventPattern(defaultPrinterSelect, nameof(AntdUI.Select.SelectedValueChanged)).Subscribe(x =>
            {
                var args = x as EventPattern<object>;
                var e = args.EventArgs as AntdUI.ObjectNEventArgs;
                logger.Info("Default printer changed from main form. PrinterName={PrinterName}", e.Value);
                MyLocalPrinter.SetDefaultPrinter(e.Value.ToString());
            });
            autoStartToolStripMenuItem.Checked = ViewModel.AutoStartUp;
            RefreshCloudStatus();
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
            cloudTransport?.Dispose();
		}

		#region Windows 窗体设计器生成的代码
		/// <summary>
		/// 设计器支持所需的方法 - 不要使用代码编辑器修改
		/// 此方法的内容。
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.mainNotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.mainContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.autoStartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.checkUpdateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.appDirectoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new AntdUI.Label();
            this.cloudServerUrlInput = new AntdUI.Input();
            this.deviceTokenLabel = new AntdUI.Label();
            this.cloudDeviceTokenInput = new AntdUI.Input();
            this.activationCodeLabel = new AntdUI.Label();
            this.activationCodeInput = new AntdUI.Input();
            this.deviceIdLabel = new AntdUI.Label();
            this.cloudDeviceIdInput = new AntdUI.Input();
            this.deviceNameLabel = new AntdUI.Label();
            this.cloudDeviceNameInput = new AntdUI.Input();
            this.toggleButton = new AntdUI.Button();
            this.activateButton = new AntdUI.Button();
            this.cloudEnabledCheckbox = new AntdUI.Checkbox();
            this.allowPreviewCheckbox = new AntdUI.Checkbox();
            this.refreshPrintersButton = new AntdUI.Button();
            this.settingsButton = new AntdUI.Button();
            this.statusLabel = new AntdUI.Label();
            this.deviceSummaryLabel = new AntdUI.Label();
            this.activationSectionLabel = new AntdUI.Label();
            this.connectionSectionLabel = new AntdUI.Label();
            this.deviceSectionLabel = new AntdUI.Label();
            this.printerSectionLabel = new AntdUI.Label();
            this.label5 = new AntdUI.Label();
            this.defaultPrinterSelect = new AntdUI.Select();
            this.mainContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "");
            this.imageList1.Images.SetKeyName(1, "");
            this.imageList1.Images.SetKeyName(2, "");
            // 
            // mainNotifyIcon
            // 
            this.mainNotifyIcon.ContextMenuStrip = this.mainContextMenuStrip;
            this.mainNotifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("mainNotifyIcon.Icon")));
            this.mainNotifyIcon.Text = "报表助手";
            this.mainNotifyIcon.Visible = true;
            this.mainNotifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.mainNotifyIcon_MouseDoubleClick);
            // 
            // mainContextMenuStrip
            // 
            this.mainContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.autoStartToolStripMenuItem,
            this.checkUpdateToolStripMenuItem,
            this.appDirectoryToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.mainContextMenuStrip.Name = "mainContextMenuStrip";
            this.mainContextMenuStrip.Size = new System.Drawing.Size(125, 92);
            // 
            // autoStartToolStripMenuItem
            // 
            this.autoStartToolStripMenuItem.CheckOnClick = true;
            this.autoStartToolStripMenuItem.Name = "autoStartToolStripMenuItem";
            this.autoStartToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.autoStartToolStripMenuItem.Text = "开机启动";
            this.autoStartToolStripMenuItem.Click += new System.EventHandler(this.autoStartToolStripMenuItem_Click);
            // 
            // checkUpdateToolStripMenuItem
            // 
            this.checkUpdateToolStripMenuItem.Name = "checkUpdateToolStripMenuItem";
            this.checkUpdateToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.checkUpdateToolStripMenuItem.Text = "检查更新";
            this.checkUpdateToolStripMenuItem.Click += new System.EventHandler(this.checkUpdateToolStripMenuItem_Click);
            // 
            // appDirectoryToolStripMenuItem
            // 
            this.appDirectoryToolStripMenuItem.Name = "appDirectoryToolStripMenuItem";
            this.appDirectoryToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.appDirectoryToolStripMenuItem.Text = "程序目录";
            this.appDirectoryToolStripMenuItem.Click += new System.EventHandler(this.appDirectoryToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.exitToolStripMenuItem.Text = "退出";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // statusLabel
            // 
            this.statusLabel.Location = new System.Drawing.Point(9, 12);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(232, 24);
            this.statusLabel.TabIndex = 121;
            this.statusLabel.Text = "云连接：未连接";
            // 
            // activationSectionLabel
            // 
            this.activationSectionLabel.Location = new System.Drawing.Point(9, 88);
            this.activationSectionLabel.Name = "activationSectionLabel";
            this.activationSectionLabel.Size = new System.Drawing.Size(110, 24);
            this.activationSectionLabel.TabIndex = 126;
            this.activationSectionLabel.Text = "设备激活";
            // 
            // activationCodeLabel
            // 
            this.activationCodeLabel.Location = new System.Drawing.Point(9, 117);
            this.activationCodeLabel.Name = "activationCodeLabel";
            this.activationCodeLabel.Size = new System.Drawing.Size(110, 23);
            this.activationCodeLabel.TabIndex = 123;
            this.activationCodeLabel.Text = "激活码";
            // 
            // activationCodeInput
            // 
            this.activationCodeInput.Location = new System.Drawing.Point(9, 140);
            this.activationCodeInput.Name = "activationCodeInput";
            this.activationCodeInput.PlaceholderText = "6 位激活码";
            this.activationCodeInput.Radius = 4;
            this.activationCodeInput.Size = new System.Drawing.Size(112, 34);
            this.activationCodeInput.TabIndex = 124;
            // 
            // activateButton
            // 
            this.activateButton.BadgeBack = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(90)))), ((int)(((byte)(158)))));
            this.activateButton.BorderWidth = 1F;
            this.activateButton.Location = new System.Drawing.Point(129, 140);
            this.activateButton.Name = "activateButton";
            this.activateButton.Radius = 4;
            this.activateButton.Size = new System.Drawing.Size(112, 34);
            this.activateButton.TabIndex = 125;
            this.activateButton.Text = "激活设备";
            this.activateButton.Type = AntdUI.TTypeMini.Primary;
            this.activateButton.WaveSize = 0;
            this.activateButton.Click += new System.EventHandler(this.activateButton_Click);
            // 
            // connectionSectionLabel
            // 
            this.connectionSectionLabel.Location = new System.Drawing.Point(9, 184);
            this.connectionSectionLabel.Name = "connectionSectionLabel";
            this.connectionSectionLabel.Size = new System.Drawing.Size(110, 24);
            this.connectionSectionLabel.TabIndex = 127;
            this.connectionSectionLabel.Text = "云连接";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(9, 209);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(110, 24);
            this.label1.TabIndex = 111;
            this.label1.Text = "云服务器地址";
            // 
            // cloudServerUrlInput
            // 
            this.cloudServerUrlInput.Location = new System.Drawing.Point(9, 232);
            this.cloudServerUrlInput.Name = "cloudServerUrlInput";
            this.cloudServerUrlInput.PlaceholderText = "wss://localhost:8087/platform/gridReport/cloud/ws";
            this.cloudServerUrlInput.Radius = 4;
            this.cloudServerUrlInput.Size = new System.Drawing.Size(232, 34);
            this.cloudServerUrlInput.TabIndex = 110;
            // 
            // deviceTokenLabel
            // 
            this.deviceTokenLabel.Location = new System.Drawing.Point(9, 271);
            this.deviceTokenLabel.Name = "deviceTokenLabel";
            this.deviceTokenLabel.Size = new System.Drawing.Size(110, 24);
            this.deviceTokenLabel.TabIndex = 112;
            this.deviceTokenLabel.Text = "设备 Token";
            // 
            // cloudDeviceTokenInput
            // 
            this.cloudDeviceTokenInput.Location = new System.Drawing.Point(9, 294);
            this.cloudDeviceTokenInput.Name = "cloudDeviceTokenInput";
            this.cloudDeviceTokenInput.PlaceholderText = "device token";
            this.cloudDeviceTokenInput.Radius = 4;
            this.cloudDeviceTokenInput.Size = new System.Drawing.Size(232, 34);
            this.cloudDeviceTokenInput.TabIndex = 113;
            // 
            // cloudEnabledCheckbox
            // 
            this.cloudEnabledCheckbox.Location = new System.Drawing.Point(9, 331);
            this.cloudEnabledCheckbox.Name = "cloudEnabledCheckbox";
            this.cloudEnabledCheckbox.Size = new System.Drawing.Size(135, 24);
            this.cloudEnabledCheckbox.TabIndex = 114;
            this.cloudEnabledCheckbox.Text = "启用云模式";
            // 
            // deviceSectionLabel
            // 
            this.deviceSectionLabel.Location = new System.Drawing.Point(9, 366);
            this.deviceSectionLabel.Name = "deviceSectionLabel";
            this.deviceSectionLabel.Size = new System.Drawing.Size(110, 24);
            this.deviceSectionLabel.TabIndex = 128;
            this.deviceSectionLabel.Text = "设备信息";
            // 
            // deviceIdLabel
            // 
            this.deviceIdLabel.Location = new System.Drawing.Point(9, 391);
            this.deviceIdLabel.Name = "deviceIdLabel";
            this.deviceIdLabel.Size = new System.Drawing.Size(110, 23);
            this.deviceIdLabel.TabIndex = 117;
            this.deviceIdLabel.Text = "设备 ID";
            // 
            // cloudDeviceIdInput
            // 
            this.cloudDeviceIdInput.Location = new System.Drawing.Point(9, 414);
            this.cloudDeviceIdInput.Name = "cloudDeviceIdInput";
            this.cloudDeviceIdInput.PlaceholderText = "STORE-A-PC-01";
            this.cloudDeviceIdInput.Radius = 4;
            this.cloudDeviceIdInput.Size = new System.Drawing.Size(232, 34);
            this.cloudDeviceIdInput.TabIndex = 118;
            // 
            // deviceNameLabel
            // 
            this.deviceNameLabel.Location = new System.Drawing.Point(9, 453);
            this.deviceNameLabel.Name = "deviceNameLabel";
            this.deviceNameLabel.Size = new System.Drawing.Size(110, 23);
            this.deviceNameLabel.TabIndex = 119;
            this.deviceNameLabel.Text = "设备名称";
            // 
            // cloudDeviceNameInput
            // 
            this.cloudDeviceNameInput.Location = new System.Drawing.Point(9, 476);
            this.cloudDeviceNameInput.Name = "cloudDeviceNameInput";
            this.cloudDeviceNameInput.PlaceholderText = "前台收银机01";
            this.cloudDeviceNameInput.Radius = 4;
            this.cloudDeviceNameInput.Size = new System.Drawing.Size(232, 34);
            this.cloudDeviceNameInput.TabIndex = 120;
            // 
            // allowPreviewCheckbox
            // 
            this.allowPreviewCheckbox.Location = new System.Drawing.Point(9, 513);
            this.allowPreviewCheckbox.Name = "allowPreviewCheckbox";
            this.allowPreviewCheckbox.Size = new System.Drawing.Size(135, 24);
            this.allowPreviewCheckbox.TabIndex = 115;
            this.allowPreviewCheckbox.Text = "允许云端预览";
            // 
            // printerSectionLabel
            // 
            this.printerSectionLabel.Location = new System.Drawing.Point(9, 548);
            this.printerSectionLabel.Name = "printerSectionLabel";
            this.printerSectionLabel.Size = new System.Drawing.Size(110, 24);
            this.printerSectionLabel.TabIndex = 129;
            this.printerSectionLabel.Text = "打印机";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(9, 573);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(110, 23);
            this.label5.TabIndex = 1;
            this.label5.Text = "默认打印机";
            // 
            // defaultPrinterSelect
            // 
            this.defaultPrinterSelect.Location = new System.Drawing.Point(9, 596);
            this.defaultPrinterSelect.Name = "defaultPrinterSelect";
            this.defaultPrinterSelect.Size = new System.Drawing.Size(232, 38);
            this.defaultPrinterSelect.TabIndex = 0;
            // 
            // refreshPrintersButton
            // 
            this.refreshPrintersButton.BadgeBack = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(90)))), ((int)(((byte)(158)))));
            this.refreshPrintersButton.BorderWidth = 1F;
            this.refreshPrintersButton.Location = new System.Drawing.Point(9, 642);
            this.refreshPrintersButton.Name = "refreshPrintersButton";
            this.refreshPrintersButton.Radius = 4;
            this.refreshPrintersButton.Size = new System.Drawing.Size(112, 32);
            this.refreshPrintersButton.TabIndex = 116;
            this.refreshPrintersButton.Text = "上报打印机";
            this.refreshPrintersButton.Type = AntdUI.TTypeMini.Primary;
            this.refreshPrintersButton.WaveSize = 0;
            this.refreshPrintersButton.Click += new System.EventHandler(this.refreshPrintersButton_Click);
            // 
            // toggleButton
            // 
            this.toggleButton.BadgeBack = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(90)))), ((int)(((byte)(158)))));
            this.toggleButton.BorderWidth = 1F;
            this.toggleButton.Location = new System.Drawing.Point(129, 642);
            this.toggleButton.Name = "toggleButton";
            this.toggleButton.Radius = 4;
            this.toggleButton.Size = new System.Drawing.Size(112, 32);
            this.toggleButton.TabIndex = 104;
            this.toggleButton.Text = "重连云端";
            this.toggleButton.Type = AntdUI.TTypeMini.Primary;
            this.toggleButton.WaveSize = 0;
            this.toggleButton.Click += new System.EventHandler(this.toggleButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(260, 688);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cloudServerUrlInput);
            this.Controls.Add(this.deviceTokenLabel);
            this.Controls.Add(this.cloudDeviceTokenInput);
            this.Controls.Add(this.activationCodeLabel);
            this.Controls.Add(this.activationCodeInput);
            this.Controls.Add(this.deviceIdLabel);
            this.Controls.Add(this.cloudDeviceIdInput);
            this.Controls.Add(this.deviceNameLabel);
            this.Controls.Add(this.cloudDeviceNameInput);
            this.Controls.Add(this.toggleButton);
            this.Controls.Add(this.activateButton);
            this.Controls.Add(this.cloudEnabledCheckbox);
            this.Controls.Add(this.allowPreviewCheckbox);
            this.Controls.Add(this.refreshPrintersButton);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.activationSectionLabel);
            this.Controls.Add(this.connectionSectionLabel);
            this.Controls.Add(this.deviceSectionLabel);
            this.Controls.Add(this.printerSectionLabel);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.defaultPrinterSelect);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "报表助手";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.ApplyCompactLayout();
            this.mainContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

        private void ApplyCompactLayout()
        {
            this.ClientSize = new System.Drawing.Size(360, 226);

            statusLabel.Location = new System.Drawing.Point(16, 16);
            statusLabel.Size = new System.Drawing.Size(328, 26);
            statusLabel.Font = new Font("微软雅黑", 11, FontStyle.Bold);

            deviceSummaryLabel.Location = new System.Drawing.Point(16, 50);
            deviceSummaryLabel.Size = new System.Drawing.Size(328, 48);

            activateButton.Location = new System.Drawing.Point(16, 114);
            activateButton.Size = new System.Drawing.Size(158, 34);
            activateButton.Text = "激活设备";

            toggleButton.Location = new System.Drawing.Point(186, 114);
            toggleButton.Size = new System.Drawing.Size(158, 34);
            toggleButton.Radius = 4;
            toggleButton.BorderWidth = 1F;
            toggleButton.Text = "连接云端";

            settingsButton.Location = new System.Drawing.Point(16, 164);
            settingsButton.Size = new System.Drawing.Size(158, 34);
            settingsButton.Radius = 4;
            settingsButton.BorderWidth = 1F;
            settingsButton.Type = AntdUI.TTypeMini.Default;
            settingsButton.WaveSize = 0;
            settingsButton.Text = "连接设置";
            settingsButton.Click += new System.EventHandler(this.settingsButton_Click);

            refreshPrintersButton.Location = new System.Drawing.Point(186, 164);
            refreshPrintersButton.Size = new System.Drawing.Size(158, 34);
            refreshPrintersButton.Text = "打印机管理";

            this.Controls.Clear();
            this.Controls.Add(statusLabel);
            this.Controls.Add(deviceSummaryLabel);
            this.Controls.Add(activateButton);
            this.Controls.Add(toggleButton);
            this.Controls.Add(settingsButton);
            this.Controls.Add(refreshPrintersButton);

            this.ClientSize = new System.Drawing.Size(360, 226);
        }

		private void btnExit_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void btnDefaultPreview_Click(object sender, System.EventArgs e)
		{
			Report.PrintPreview(true);
		}

		private void btnCustomPreview_Click(object sender, System.EventArgs e)
		{
			PreviewForm theForm = new PreviewForm();
			theForm.AttachReport(Report);
			theForm.ShowDialog();
		}

		private void btnPrint_Click(object sender, System.EventArgs e)
		{
			Report.Print(true);
		}

		private void btnDesign_Click(object sender, System.EventArgs e)
		{
			DesignForm theForm = new DesignForm();
			theForm.AttachReport(Report);
			theForm.ShowDialog();
		}

		private void btnDisplay_Click(object sender, System.EventArgs e)
		{
			DisplayForm theForm = new DisplayForm();
			theForm.AttachReport(Report);
			theForm.ShowDialog();
		}

		private void btnPageSetup_Click(object sender, System.EventArgs e)
		{
			Report.Printer.PageSetupDialog();
		}

		private void btnPrinterSetup_Click(object sender, System.EventArgs e)
		{
			Report.Printer.PrinterSetupDialog();
		}

		private void btnPrintSetup_Click(object sender, System.EventArgs e)
		{
			Report.Printer.PrintDialog();
		}

		private void ckbShowPrintDlg_CheckedChanged(object sender, System.EventArgs e)
		{
		
		}


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void cmbLanguage_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			int Language;
			Language = 0x0804;
			//Language = 0x0404;
			//Language = 0x0409;
			Report.Language = Language;
		}

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 注意判断关闭事件reason来源于窗体按钮，否则用菜单退出时无法退出!
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (ViewModel.CloseMode == "Tray")
                {
                    //取消"关闭窗口"事件
                    e.Cancel = true;

                    //使关闭时窗口向右下角缩小的效果
                    this.WindowState = FormWindowState.Minimized;
                    this.mainNotifyIcon.Visible = true;
                    //this.m_cartoonForm.CartoonClose();
                    this.Hide();
                }
                else
                {
                    this.ExitApp();
                }
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
        }

        private void mainNotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                logger.Debug("Notify icon double clicked. WindowState={WindowState}", WindowState);
                if (this.WindowState == FormWindowState.Normal)
                {
                    this.WindowState = FormWindowState.Minimized;
                    this.Hide();
                }
                else if (this.WindowState == FormWindowState.Minimized)
                {
                    this.Show();
                    this.WindowState = FormWindowState.Normal;
                    this.Activate();
                }
            }
            catch (Exception objException)
            {
                logger.Error(objException, "Notify icon double click handling failed.");
                throw new Exception(objException.Message);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("你确定要退出程序吗？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.OK)
                {
                    logger.Info("Application exit confirmed from tray menu.");
                    this.mainNotifyIcon.Visible = false;
                    this.mainNotifyIcon.Dispose();
                    this.Dispose();
                    Application.Exit();
                }
            }
            catch (Exception objException)
            {
                logger.Error(objException, "Application exit from tray menu failed.");
                MessageBox.Show(objException.Message);
            }
        }

        private void autoStartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ViewModel.AutoStartUp = autoStartToolStripMenuItem.Checked;
            logger.Info("Auto startup changed. AutoStartUp={AutoStartUp}", ViewModel.AutoStartUp);
        }

        private async void checkUpdateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await CheckForApplicationUpdateAsync(true);
        }

        private void appDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            logger.Info("Opening application directory. Directory={Directory}", AppDomain.CurrentDomain.BaseDirectory);
            Process.Start("explorer.exe", AppDomain.CurrentDomain.BaseDirectory);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            logger.Info("Main form loaded.");
            if (Helper.AlreadyRun())
            {
                logger.Warn("Another GridReportForm instance is already running. Exiting current process.");
                MessageBox.Show("打印助手已运行");
                this.ExitApp();
                return;
            }
            StartAutoUpdateCheck();
        }


        private void ExitApp()
        {
            try
            {
                Application.Exit();
            }
            catch (Exception)
            {
                logger.Warn("Application.Exit failed. Forcing process exit.");
                Environment.Exit(0);
            }
        }

        private void StartAutoUpdateCheck()
        {
            if (!ViewModel.AutoUpdate)
            {
                logger.Info("Auto update check skipped because AutoUpdate is disabled.");
                return;
            }

            Timer timer = new Timer
            {
                Interval = 5000
            };
            timer.Tick += async (sender, args) =>
            {
                timer.Stop();
                timer.Dispose();
                await CheckForApplicationUpdateAsync(false);
            };
            timer.Start();
            logger.Info("Auto update check scheduled.");
        }

        private async Task CheckForApplicationUpdateAsync(bool manual)
        {
            if (updateCheckRunning)
            {
                logger.Debug("Application update check ignored because another check is running. Manual={Manual}", manual);
                return;
            }

            updateCheckRunning = true;
            checkUpdateToolStripMenuItem.Enabled = false;
            string originalText = checkUpdateToolStripMenuItem.Text;
            checkUpdateToolStripMenuItem.Text = manual ? "检查中..." : originalText;

            try
            {
                logger.Info("Application update check started. Manual={Manual}", manual);
                ApplicationUpdateCheckResult result = await updateService.CheckLatestAsync();
                if (result.Status == ApplicationUpdateStatus.NoUpdate)
                {
                    if (manual)
                    {
                        MessageBox.Show("当前已是最新版本", "检查更新", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    return;
                }
                if (result.Status == ApplicationUpdateStatus.Unavailable)
                {
                    logger.Warn("Application update unavailable. Manual={Manual}, Message={Message}", manual, result.Message);
                    if (manual)
                    {
                        MessageBox.Show(result.Message ?? "当前版本无法自动更新", "检查更新", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    return;
                }

                await PromptInstallUpdateAsync(result.Update);
            }
            catch (Exception exception)
            {
                logger.Error(exception, "Application update check failed. Manual={Manual}", manual);
                if (manual)
                {
                    MessageBox.Show(exception.Message, "检查更新失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            finally
            {
                checkUpdateToolStripMenuItem.Text = originalText;
                checkUpdateToolStripMenuItem.Enabled = true;
                updateCheckRunning = false;
            }
        }

        private async Task PromptInstallUpdateAsync(ApplicationUpdateInfo update)
        {
            DialogResult confirm = MessageBox.Show(
                $"发现新版本 {ApplicationUpdateService.FormatVersion(update.LatestVersion)}\r\n\r\n当前版本：{ApplicationUpdateService.FormatVersion(update.CurrentVersion)}\r\n是否立即下载并安装？",
                "发现新版本",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1);
            if (confirm != DialogResult.Yes)
            {
                logger.Info("Application update declined by user. LatestVersion={LatestVersion}", update.LatestVersion);
                return;
            }

            try
            {
                checkUpdateToolStripMenuItem.Text = "下载中...";
                logger.Info("Application update accepted by user. LatestVersion={LatestVersion}", update.LatestVersion);
                string installerPath = await updateService.DownloadInstallerAsync(update);
                MessageBox.Show("更新安装包已下载，即将退出并启动安装程序。", "准备安装更新", MessageBoxButtons.OK, MessageBoxIcon.Information);
                logger.Info("Starting update installer. InstallerPath={InstallerPath}", installerPath);
                cloudTransport.Stop();
                Process.Start(new ProcessStartInfo(installerPath)
                {
                    UseShellExecute = true
                });
                mainNotifyIcon.Visible = false;
                Application.Exit();
            }
            catch (Exception exception)
            {
                logger.Error(exception, "Application update install preparation failed. LatestVersion={LatestVersion}", update.LatestVersion);
                MessageBox.Show(exception.Message, "更新失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void toggleButton_Click(object sender, EventArgs e)
        {
            logger.Info("Connect cloud button clicked. CloudEnabled={CloudEnabled}, Status={Status}", ViewModel.CloudEnabled, cloudTransport.Status);
            cloudTransport.Stop();
            if (ViewModel.CloudEnabled)
            {
                cloudTransport.Start();
            }
        }

        private void activateButton_Click(object sender, EventArgs e)
        {
            logger.Info("Activation button clicked. HasToken={HasToken}, DeviceId={DeviceId}", !string.IsNullOrWhiteSpace(ViewModel.CloudDeviceToken), ViewModel.CloudDeviceId);
            ShowActivationDialog();
        }

        private void settingsButton_Click(object sender, EventArgs e)
        {
            logger.Info("Settings button clicked.");
            ShowSettingsDialog();
        }

        private void ShowActivationDialog()
        {
            bool activated = !string.IsNullOrWhiteSpace(ViewModel.CloudDeviceToken);
            logger.Info("Opening activation dialog. Reactivation={Reactivation}, DeviceId={DeviceId}, DeviceName={DeviceName}", activated, ViewModel.CloudDeviceId, ViewModel.CloudDeviceName);
            Form form = CreateDialog(activated ? "重新激活设备" : "激活设备", 330, 310);
            AntdUI.Label serverLabel = CreateDialogLabel("云服务器地址", 16, 16);
            AntdUI.Input serverInput = CreateDialogInput(ViewModel.CloudServerUrl, "wss://example.com/admin/platform/gridReport/cloud/ws", 16, 42);
            AntdUI.Label nameLabel = CreateDialogLabel("设备名称", 16, 86);
            AntdUI.Input nameInput = CreateDialogInput(ViewModel.CloudDeviceName, "前台收银机01", 16, 112);
            AntdUI.Label codeLabel = CreateDialogLabel("激活码", 16, 156);
            AntdUI.Input codeInput = CreateDialogInput("", "后台生成的 6 位激活码", 16, 182);
            AntdUI.Checkbox previewCheckbox = new AntdUI.Checkbox
            {
                Location = new Point(16, 224),
                Size = new Size(160, 24),
                Text = "允许云端预览",
                Checked = ViewModel.AllowPreview
            };
            AntdUI.Button activate = CreateDialogButton(activated ? "重新激活" : "激活", 176, 242, AntdUI.TTypeMini.Primary);
            activate.Click += (o, args) =>
            {
                try
                {
                    logger.Info("Submitting activation dialog. ServerUrl={ServerUrl}, DeviceName={DeviceName}, AllowPreview={AllowPreview}, HasActivationCode={HasActivationCode}", serverInput.Text.Trim(), nameInput.Text.Trim(), previewCheckbox.Checked, !string.IsNullOrWhiteSpace(codeInput.Text));
                    ActivateDevice(serverInput.Text.Trim(), nameInput.Text.Trim(), codeInput.Text.Trim(), previewCheckbox.Checked);
                    form.DialogResult = DialogResult.OK;
                    form.Close();
                }
                catch (Exception exception)
                {
                    logger.Error(exception, "Device activation dialog submission failed.");
                    MessageBox.Show(exception.Message, "设备激活失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            form.Controls.Add(serverLabel);
            form.Controls.Add(serverInput);
            form.Controls.Add(nameLabel);
            form.Controls.Add(nameInput);
            form.Controls.Add(codeLabel);
            form.Controls.Add(codeInput);
            form.Controls.Add(previewCheckbox);
            form.Controls.Add(activate);
            form.ShowDialog(this);
        }

        private void ActivateDevice(string serverUrl, string deviceName, string activationCode, bool allowPreview)
        {
            try
            {
                logger.Info("Activating device from main form. ServerUrl={ServerUrl}, DeviceId={DeviceId}, DeviceName={DeviceName}, AllowPreview={AllowPreview}, HasActivationCode={HasActivationCode}", serverUrl, ViewModel.CloudDeviceId, deviceName, allowPreview, !string.IsNullOrWhiteSpace(activationCode));
                ViewModel.CloudServerUrl = serverUrl;
                ViewModel.CloudDeviceName = deviceName;
                ViewModel.AllowPreview = allowPreview;

                CloudActivationResult result = CloudActivationClient.Activate(new CloudActivationRequest
                {
                    ServerUrl = serverUrl,
                    ActivationCode = activationCode,
                    DeviceId = ViewModel.CloudDeviceId,
                    DeviceName = deviceName,
                    AllowPreview = allowPreview,
                    DefaultPrinter = MyLocalPrinter.DefaultPrinter(),
                    Printers = MyLocalPrinter.GetLocalPrinters()
                });

                if (!string.IsNullOrWhiteSpace(result.DeviceId))
                {
                    ViewModel.CloudDeviceId = result.DeviceId;
                }
                if (!string.IsNullOrWhiteSpace(result.DeviceName))
                {
                    ViewModel.CloudDeviceName = result.DeviceName;
                }
                ViewModel.CloudDeviceToken = result.Token;
                logger.Info("Device activation saved. DeviceId={DeviceId}, DeviceName={DeviceName}, HasToken={HasToken}", ViewModel.CloudDeviceId, ViewModel.CloudDeviceName, !string.IsNullOrWhiteSpace(ViewModel.CloudDeviceToken));
                MessageBox.Show("设备激活成功");

                cloudTransport.Stop();
                if (ViewModel.CloudEnabled)
                {
                    cloudTransport.Start();
                }
                RefreshCloudStatus();
            }
            catch (Exception exception)
            {
                logger.Error(exception, "Device activation failed before completion.");
                throw;
            }
        }

        private void ShowSettingsDialog()
        {
            logger.Info("Opening settings dialog. DeviceId={DeviceId}, DeviceName={DeviceName}, CloudEnabled={CloudEnabled}, AllowPreview={AllowPreview}", ViewModel.CloudDeviceId, ViewModel.CloudDeviceName, ViewModel.CloudEnabled, ViewModel.AllowPreview);
            Form form = CreateDialog("连接与设备设置", 330, 410);
            AntdUI.Label serverLabel = CreateDialogLabel("云服务器地址", 16, 16);
            AntdUI.Input serverInput = CreateDialogInput(ViewModel.CloudServerUrl, "wss://example.com/admin/platform/gridReport/cloud/ws", 16, 42);
            AntdUI.Label tokenLabel = CreateDialogLabel("设备 Token", 16, 86);
            AntdUI.Input tokenInput = CreateDialogInput(ViewModel.CloudDeviceToken, "激活后自动写入", 16, 112);
            AntdUI.Label idLabel = CreateDialogLabel("设备 ID", 16, 156);
            AntdUI.Input idInput = CreateDialogInput(ViewModel.CloudDeviceId, "device id", 16, 182);
            AntdUI.Label nameLabel = CreateDialogLabel("设备名称", 16, 226);
            AntdUI.Input nameInput = CreateDialogInput(ViewModel.CloudDeviceName, "前台收银机01", 16, 252);
            AntdUI.Checkbox cloudCheckbox = new AntdUI.Checkbox
            {
                Location = new Point(16, 294),
                Size = new Size(120, 24),
                Text = "启用云模式",
                Checked = ViewModel.CloudEnabled
            };
            AntdUI.Checkbox previewCheckbox = new AntdUI.Checkbox
            {
                Location = new Point(150, 294),
                Size = new Size(150, 24),
                Text = "允许云端预览",
                Checked = ViewModel.AllowPreview
            };
            AntdUI.Button save = CreateDialogButton("保存", 176, 342, AntdUI.TTypeMini.Primary);
            save.Click += (o, args) =>
            {
                logger.Info("Saving settings dialog. ServerUrl={ServerUrl}, DeviceId={DeviceId}, DeviceName={DeviceName}, CloudEnabled={CloudEnabled}, AllowPreview={AllowPreview}, HasToken={HasToken}", serverInput.Text.Trim(), idInput.Text.Trim(), nameInput.Text.Trim(), cloudCheckbox.Checked, previewCheckbox.Checked, !string.IsNullOrWhiteSpace(tokenInput.Text));
                ViewModel.CloudServerUrl = serverInput.Text.Trim();
                ViewModel.CloudDeviceToken = tokenInput.Text.Trim();
                ViewModel.CloudDeviceId = idInput.Text.Trim();
                ViewModel.CloudDeviceName = nameInput.Text.Trim();
                ViewModel.CloudEnabled = cloudCheckbox.Checked;
                ViewModel.AllowPreview = previewCheckbox.Checked;
                cloudTransport.Stop();
                if (ViewModel.CloudEnabled)
                {
                    cloudTransport.Start();
                }
                RefreshCloudStatus();
                MessageBox.Show("设置已保存", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                form.Close();
            };

            form.Controls.Add(serverLabel);
            form.Controls.Add(serverInput);
            form.Controls.Add(tokenLabel);
            form.Controls.Add(tokenInput);
            form.Controls.Add(idLabel);
            form.Controls.Add(idInput);
            form.Controls.Add(nameLabel);
            form.Controls.Add(nameInput);
            form.Controls.Add(cloudCheckbox);
            form.Controls.Add(previewCheckbox);
            form.Controls.Add(save);
            form.ShowDialog(this);
        }

        private Form CreateDialog(string title, int width, int height)
        {
            return new Form
            {
                Text = title,
                Font = new Font("微软雅黑", 10),
                BackColor = Color.White,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                StartPosition = FormStartPosition.CenterParent,
                ClientSize = new Size(width, height)
            };
        }

        private AntdUI.Label CreateDialogLabel(string text, int x, int y)
        {
            return new AntdUI.Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(180, 22)
            };
        }

        private AntdUI.Input CreateDialogInput(string text, string placeholder, int x, int y)
        {
            return new AntdUI.Input
            {
                Text = text,
                PlaceholderText = placeholder,
                Location = new Point(x, y),
                Size = new Size(292, 34),
                Radius = 4
            };
        }

        private AntdUI.Button CreateDialogButton(string text, int x, int y, AntdUI.TTypeMini type)
        {
            return new AntdUI.Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(132, 34),
                Radius = 4,
                BorderWidth = 1F,
                Type = type,
                WaveSize = 0
            };
        }

        private void refreshPrintersButton_Click(object sender, EventArgs e)
        {
            logger.Info("Printer management button clicked.");
            ShowPrinterDialog();
        }

        private void ShowPrinterDialog()
        {
            logger.Info("Opening printer dialog. DefaultPrinter={DefaultPrinter}", MyLocalPrinter.DefaultPrinter());
            Form form = CreateDialog("打印机管理", 330, 235);
            AntdUI.Label printerLabel = CreateDialogLabel("默认打印机", 16, 16);
            AntdUI.Select printerSelect = new AntdUI.Select
            {
                Location = new Point(16, 42),
                Size = new Size(292, 38)
            };
            printerSelect.Items.AddRange(MyLocalPrinter.GetLocalPrinters().ToArray<object>());
            printerSelect.SelectedValue = MyLocalPrinter.DefaultPrinter();
            Observable.FromEventPattern(printerSelect, nameof(AntdUI.Select.SelectedValueChanged)).Subscribe(x =>
            {
                var args = x as EventPattern<object>;
                var eventArgs = args.EventArgs as AntdUI.ObjectNEventArgs;
                logger.Info("Default printer changed from printer dialog. PrinterName={PrinterName}", eventArgs.Value);
                MyLocalPrinter.SetDefaultPrinter(eventArgs.Value.ToString());
            });

            AntdUI.Label noteLabel = new AntdUI.Label
            {
                Location = new Point(16, 96),
                Size = new Size(292, 46),
                Text = "上报后，云端会更新此设备可用打印机列表。"
            };
            AntdUI.Button reportButton = CreateDialogButton("上报打印机", 176, 160, AntdUI.TTypeMini.Primary);
            reportButton.Click += (o, args) =>
            {
                logger.Info("Printer report submitted from printer dialog. DefaultPrinter={DefaultPrinter}", MyLocalPrinter.DefaultPrinter());
                cloudTransport.ReportPrintersNow();
                MessageBox.Show("已提交打印机上报请求");
                form.Close();
            };

            form.Controls.Add(printerLabel);
            form.Controls.Add(printerSelect);
            form.Controls.Add(noteLabel);
            form.Controls.Add(reportButton);
            form.ShowDialog(this);
        }

        private CloudConnectionOptions CreateCloudOptions()
        {
            return new CloudConnectionOptions
            {
                ServerUrl = ViewModel.CloudServerUrl,
                DeviceToken = ViewModel.CloudDeviceToken,
                DeviceId = ViewModel.CloudDeviceId,
                DeviceName = ViewModel.CloudDeviceName,
                AllowPreview = ViewModel.AllowPreview
            };
        }

        private void CloudTransport_StatusChanged(string status)
        {
            logger.Debug("Cloud transport status changed event received. Status={Status}", status);
            if (IsDisposed)
            {
                return;
            }
            BeginInvoke((MethodInvoker)RefreshCloudStatus);
        }

        private void RefreshCloudStatus()
        {
            if (statusLabel == null)
            {
                return;
            }

            bool activated = !string.IsNullOrWhiteSpace(ViewModel.CloudDeviceToken);
            string status = activated ? cloudTransport.Status : "设备未激活";
            statusLabel.Text = "状态：" + status;
            activateButton.Visible = true;
            activateButton.Text = activated ? "重新激活" : "激活设备";
            toggleButton.Enabled = activated;
            toggleButton.Text = "连接云端";
            deviceSummaryLabel.Text = activated
                ? $"设备：{ViewModel.CloudDeviceName}\r\n编号：{ViewModel.CloudDeviceId}"
                : "设备尚未绑定，请点击右侧按钮完成激活。";

            bool failed = status == "设备未激活" || status == "连接失败" || status == "未配置云服务器";
            statusLabel.ForeColor = status == "已连接"
                ? Color.ForestGreen
                : status == "连接中" ? Color.RoyalBlue
                : failed ? Color.Firebrick : Color.FromArgb(30, 30, 30);
            if (!failed)
            {
                lastAlertedConnectionError = null;
                return;
            }

            string error = cloudTransport.LastError;
            if (!string.IsNullOrWhiteSpace(error) && error != lastAlertedConnectionError)
            {
                lastAlertedConnectionError = error;
                logger.Warn("Showing cloud connection failure message. Status={Status}, Error={Error}", status, error);
                MessageBox.Show(error, "云连接失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

    }
}

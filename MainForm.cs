using System;
using System.Drawing;
using System.Windows.Forms;
using gregn6Lib;
using Fleck;
using ReactiveUI;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Drawing.Printing;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Masuit.Tools;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Reactive;

namespace GridReportForm
{
	/// <summary>
	/// MainForm 的摘要说明。
	/// </summary>
	public class MainForm : Form, IViewFor<MainViewModel>
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private WebSocketServer server;
        private readonly AutoResetEvent taskEvent;
        private readonly AutoResetEvent taskDesignEvent;
        private readonly PrintDocument printDocument;
        private readonly RegistryKey registryKey;
        private static bool designOpened = false;


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
        private ToolStripMenuItem exitToolStripMenuItem;
        private AntdUI.FlowPanel flowPanel1;
        private AntdUI.Tabs tabs1;
        private AntdUI.TabPage tabPage1;
        private AntdUI.Panel panel1;
        private AntdUI.Label socketCountLabel;
        private AntdUI.Label label1;
        private AntdUI.Button toggleButton;
        private AntdUI.Label label2;
        private AntdUI.InputNumber portInput;
        private AntdUI.TabPage tabPage2;
        private AntdUI.Label label3;
        private AntdUI.Alert alertState;
        private AntdUI.Button button1;
        private AntdUI.Panel panel2;
        private AntdUI.In.FlowLayoutPanel flowLayoutPanel1;
        private AntdUI.Checkbox autoStartUpCheckbox;
        private AntdUI.Checkbox autoUpdateCheckbox;
        private AntdUI.Checkbox startServerOnLaunchCheckbox;
        private System.ComponentModel.IContainer components;
        static object lockDesignForm = new object();
        private AntdUI.Select defaultPrinterSelect;
        private AntdUI.Label label4;
        private AntdUI.FlowPanel flowPanel2;
        private AntdUI.Radio radioExit;
        private AntdUI.Radio radioTray;
        private AntdUI.Label label5;
        private AntdUI.Checkbox notifyOnConnectCheckbox;
        static DesignForm designForm;

        public MainForm()
		{
            //
            // Windows 窗体设计器支持所必需的
            //
            this.Font = new Font("微软雅黑", 10);
            InitializeComponent();

            this.taskEvent = new AutoResetEvent(true);
            this.taskDesignEvent = new AutoResetEvent(true);
            this.printDocument = new PrintDocument();
            this.registryKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
            Init();
            if (ViewModel.StartServerOnLaunch)
            {
                StartServer();
            }
        }

        private void Init()
        {
            this.WhenActivated(d =>
            {
                d(this.OneWayBind(ViewModel, vm => vm.Port, v => v.portInput.Value));
                d(this.OneWayBind(ViewModel, vm => vm.State, v => v.toggleButton.Text, (state) =>
                {
                    if (state)
                    {
                        return "停止服务";
                    }
                    else
                    {
                        return "启动服务";
                    }
                }));
                d(this.OneWayBind(ViewModel, vm => vm.CloseMode, v => v.radioTray.Checked, ViewModelCloseModeToViewRadioTrayConverterFunc));
                d(this.OneWayBind(ViewModel, vm => vm.CloseMode, v => v.radioExit.Checked, ViewModelCloseModeToViewRadioExitConverterFunc));
                d(this.OneWayBind(ViewModel, vm => vm.AutoStartUp, v => v.autoStartUpCheckbox.Checked));
                d(this.OneWayBind(ViewModel, vm => vm.NotifyOnConnect, v => v.notifyOnConnectCheckbox.Checked));
                d(this.OneWayBind(ViewModel, vm => vm.AutoUpdate, v => v.autoUpdateCheckbox.Checked));
                d(this.OneWayBind(ViewModel, vm => vm.StartServerOnLaunch, v => v.startServerOnLaunchCheckbox.Checked));
                defaultPrinterSelect.Items.AddRange(MyLocalPrinter.GetLocalPrinters().ToArray<object>());
                defaultPrinterSelect.SelectedValue = MyLocalPrinter.DefaultPrinter();
                this.WhenAnyValue(x => x.ViewModel.State).Subscribe(state =>
                {
                    if (state)
                    {
                        toggleButton.Type = AntdUI.TTypeMini.Error;
                        toggleButton.BackHover = Color.FromArgb(0, 192, 57, 43);
                        alertState.Icon = AntdUI.TType.Success;
                        alertState.Text = "运行中";
                    }
                    else
                    {
                        toggleButton.Type = AntdUI.TTypeMini.Primary;
                        alertState.Icon = AntdUI.TType.Error;
                        alertState.Text = "已停止";
                    }
                });
                this.WhenAnyValue(x => x.ViewModel.CloseMode).Subscribe(value =>
                {
                    radioTray.Checked = (value == "Tray");
                });
                ViewModel.Sockets.CollectionChanged += Sockets_CollectionChanged;
            });
            Observable.FromEventPattern(defaultPrinterSelect, nameof(AntdUI.Select.SelectedValueChanged)).Subscribe(x =>
            {
                var args = x as EventPattern<object>;
                var e = args.EventArgs as AntdUI.ObjectNEventArgs;
                MyLocalPrinter.SetDefaultPrinter(e.Value.ToString());
            });
            Observable.FromEventPattern(radioTray, nameof(AntdUI.Radio.CheckedChanged)).Subscribe(x =>
            {
                ViewModel.CloseMode = (x.Sender as AntdUI.Radio).Checked ? "Tray" : "Exit";
            });
            Observable.FromEventPattern(autoStartUpCheckbox, nameof(AntdUI.Checkbox.CheckedChanged)).Subscribe(x =>
            {
                ViewModel.AutoStartUp = (x.Sender as AntdUI.Checkbox).Checked;
            });
            Observable.FromEventPattern(autoUpdateCheckbox, nameof(AntdUI.Checkbox.CheckedChanged)).Subscribe(x =>
            {
                ViewModel.AutoUpdate = (x.Sender as AntdUI.Checkbox).Checked;
            });
            Observable.FromEventPattern(startServerOnLaunchCheckbox, nameof(AntdUI.Checkbox.CheckedChanged)).Subscribe(x =>
            {
                ViewModel.StartServerOnLaunch = (x.Sender as AntdUI.Checkbox).Checked;
            });
            Observable.FromEventPattern(notifyOnConnectCheckbox, nameof(AntdUI.Checkbox.CheckedChanged)).Subscribe(x =>
            {
                ViewModel.NotifyOnConnect = (x.Sender as AntdUI.Checkbox).Checked;
            });
        }

        private bool ViewModelCloseModeToViewRadioTrayConverterFunc(string value)
        {
            return value == "Tray";
        }

        private bool ViewModelCloseModeToViewRadioExitConverterFunc(string value)
        {
            return value == "Exit";
        }

        private void Sockets_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            socketCountLabel.Text = ViewModel.Sockets.Count.ToString();
        }

        private void StartServer()
        {
            int port = decimal.ToInt32(ViewModel.Port);
            if (Helper.PortInUse(port))
            {
                logger.Error($"端口[{port}]被占用");
                MessageBox.Show("端口被占用");
            }
            else
            {
                this.server = new WebSocketServer($"ws://0.0.0.0:{port}");
                server.RestartAfterListenError = true;
                server.Start(socket =>
                {
                    string origin = this.GetSocketOrigin(socket);
                    socket.OnOpen = () =>
                    {
                        ViewModel.Sockets.Add(socket);
                        logger.Info($"终端{socket.ConnectionInfo.ClientIpAddress}:{socket.ConnectionInfo.ClientPort}已连接, {{{socket.ConnectionInfo.Id}}}");
                        if (designForm != null)
                        {
                            designForm.Socket = socket;
                            logger.Info($"重新设置设计窗口Socket {{{socket.ConnectionInfo.Id}}}");
                        }
                        if (ViewModel.NotifyOnConnect)
                        {
                            mainNotifyIcon.Visible = true;
                            // timoeout参数已经无效，通知的显示时间基于系统的辅助功能设置
                            mainNotifyIcon.ShowBalloonTip(0, "连接通知", $"终端{socket.ConnectionInfo.ClientIpAddress}:{socket.ConnectionInfo.ClientPort}已连接", ToolTipIcon.Info);
                            Helper.Delay(5000);
                            mainNotifyIcon.Visible = false;
                        }

                    };
                    socket.OnClose = () =>
                    {
                        try
                        {
                            logger.Info($"终端{socket.ConnectionInfo.ClientIpAddress}:{socket.ConnectionInfo.ClientPort}已断开连接, {{{socket.ConnectionInfo.Id}}}");
                            ViewModel.Sockets.Remove(socket);
                            if (ViewModel.NotifyOnConnect)
                            {
                                if (!mainNotifyIcon.Visible)
                                {
                                    mainNotifyIcon.Visible = true;
                                    // timoeout参数已经无效，通知的显示时间基于系统的辅助功能设置
                                    mainNotifyIcon.ShowBalloonTip(0, "连接通知", $"终端{socket.ConnectionInfo.ClientIpAddress}:{socket.ConnectionInfo.ClientPort}断开连接", ToolTipIcon.Error);
                                    Helper.Delay(5000);
                                    mainNotifyIcon.Visible = false;
                                }
           
                            }
                        }
                        catch
                        {

                        }
                    };
                    socket.OnPing = message =>
                    {
                    };
                    socket.OnMessage = message =>
                    {
                        if(!string.IsNullOrEmpty(message) && message != "ping")
                        {
                            logger.Info($"接收消息 {Regex.Replace(message, @"[\r\n]", "")}");
                            Thread thread1 = new Thread(() => this.MessageHandle(socket, message));
                            thread1.SetApartmentState(ApartmentState.STA);
                            thread1.Start();
                        }
                    };
                });
                ViewModel.State = true;
            }
        }

        private void CloseServer()
        {
            this.server.Dispose();
            ViewModel.Sockets.ForEach(row => row.Close());
            ViewModel.Sockets.Clear();
            ViewModel.State = false;
        }

        private string GetSocketOrigin(IWebSocketConnection socket)
        {
            if (string.IsNullOrEmpty(socket.ConnectionInfo.Origin))
                return "";

            return new UriBuilder(socket.ConnectionInfo.Origin).Host;
            /*
            string host = new UriBuilder(socket.ConnectionInfo.Origin).Host;
            int index = socket.ConnectionInfo.Path.IndexOf("?");
            if (index >= 0)
            {
                string str2 = HttpUtility.ParseQueryString(socket.ConnectionInfo.Path.Substring(index + 1))["origin"];
                if (str2 != null)
                {
                    host = str2;
                }
            }
            return host;
            */
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            AntdUI.Tabs.StyleCard styleCard1 = new AntdUI.Tabs.StyleCard();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.mainNotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.mainContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.flowPanel1 = new AntdUI.FlowPanel();
            this.button1 = new AntdUI.Button();
            this.toggleButton = new AntdUI.Button();
            this.portInput = new AntdUI.InputNumber();
            this.label2 = new AntdUI.Label();
            this.alertState = new AntdUI.Alert();
            this.label3 = new AntdUI.Label();
            this.tabs1 = new AntdUI.Tabs();
            this.tabPage1 = new AntdUI.TabPage();
            this.panel2 = new AntdUI.Panel();
            this.flowPanel2 = new AntdUI.FlowPanel();
            this.radioExit = new AntdUI.Radio();
            this.radioTray = new AntdUI.Radio();
            this.label4 = new AntdUI.Label();
            this.flowLayoutPanel1 = new AntdUI.In.FlowLayoutPanel();
            this.autoStartUpCheckbox = new AntdUI.Checkbox();
            this.autoUpdateCheckbox = new AntdUI.Checkbox();
            this.startServerOnLaunchCheckbox = new AntdUI.Checkbox();
            this.notifyOnConnectCheckbox = new AntdUI.Checkbox();
            this.panel1 = new AntdUI.Panel();
            this.socketCountLabel = new AntdUI.Label();
            this.label1 = new AntdUI.Label();
            this.tabPage2 = new AntdUI.TabPage();
            this.label5 = new AntdUI.Label();
            this.defaultPrinterSelect = new AntdUI.Select();
            this.mainContextMenuStrip.SuspendLayout();
            this.flowPanel1.SuspendLayout();
            this.tabs1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.flowPanel2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageList1
            // 
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
            this.exitToolStripMenuItem});
            this.mainContextMenuStrip.Name = "mainContextMenuStrip";
            this.mainContextMenuStrip.Size = new System.Drawing.Size(101, 26);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(100, 22);
            this.exitToolStripMenuItem.Text = "退出";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // flowPanel1
            // 
            this.flowPanel1.Controls.Add(this.button1);
            this.flowPanel1.Controls.Add(this.toggleButton);
            this.flowPanel1.Controls.Add(this.portInput);
            this.flowPanel1.Controls.Add(this.label2);
            this.flowPanel1.Controls.Add(this.alertState);
            this.flowPanel1.Controls.Add(this.label3);
            this.flowPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowPanel1.Location = new System.Drawing.Point(0, 213);
            this.flowPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowPanel1.Name = "flowPanel1";
            this.flowPanel1.Size = new System.Drawing.Size(494, 33);
            this.flowPanel1.TabIndex = 27;
            this.flowPanel1.Text = "flowPanel1";
            // 
            // button1
            // 
            this.button1.BadgeBack = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(90)))), ((int)(((byte)(158)))));
            this.button1.BorderWidth = 1F;
            this.button1.Location = new System.Drawing.Point(403, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "检查更新";
            this.button1.Type = AntdUI.TTypeMini.Primary;
            this.button1.WaveSize = 0;
            // 
            // toggleButton
            // 
            this.toggleButton.BadgeBack = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(90)))), ((int)(((byte)(158)))));
            this.toggleButton.BorderWidth = 1F;
            this.toggleButton.Location = new System.Drawing.Point(322, 3);
            this.toggleButton.Name = "toggleButton";
            this.toggleButton.Size = new System.Drawing.Size(75, 23);
            this.toggleButton.TabIndex = 3;
            this.toggleButton.Text = "启动";
            this.toggleButton.Type = AntdUI.TTypeMini.Primary;
            this.toggleButton.WaveSize = 0;
            this.toggleButton.Click += new System.EventHandler(this.toggleButton_Click);
            // 
            // portInput
            // 
            this.portInput.BackColor = System.Drawing.Color.Transparent;
            this.portInput.Location = new System.Drawing.Point(244, 0);
            this.portInput.Margin = new System.Windows.Forms.Padding(0);
            this.portInput.Name = "portInput";
            this.portInput.PlaceholderText = "端口";
            this.portInput.Radius = 4;
            this.portInput.ReadOnly = true;
            this.portInput.SelectionColor = System.Drawing.Color.Empty;
            this.portInput.ShowControl = false;
            this.portInput.Size = new System.Drawing.Size(75, 29);
            this.portInput.TabIndex = 99;
            this.portInput.TabStop = false;
            this.portInput.Text = "0";
            this.portInput.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(170, 0);
            this.label2.Margin = new System.Windows.Forms.Padding(5, 0, 0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 32);
            this.label2.TabIndex = 1;
            this.label2.Text = "服务端口：";
            // 
            // alertState
            // 
            this.alertState.Icon = AntdUI.TType.Success;
            this.alertState.Location = new System.Drawing.Point(87, 3);
            this.alertState.Name = "alertState";
            this.alertState.Size = new System.Drawing.Size(75, 23);
            this.alertState.TabIndex = 3;
            this.alertState.Text = "运行中";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(10, 0);
            this.label3.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 32);
            this.label3.TabIndex = 2;
            this.label3.Text = "服务状态：";
            // 
            // tabs1
            // 
            this.tabs1.Controls.Add(this.tabPage1);
            this.tabs1.Controls.Add(this.tabPage2);
            this.tabs1.Cursor = System.Windows.Forms.Cursors.Default;
            this.tabs1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabs1.Location = new System.Drawing.Point(0, 0);
            this.tabs1.Name = "tabs1";
            this.tabs1.Pages.Add(this.tabPage1);
            this.tabs1.Pages.Add(this.tabPage2);
            this.tabs1.Size = new System.Drawing.Size(494, 213);
            styleCard1.BorderActive = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(223)))), ((int)(((byte)(221)))));
            styleCard1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(223)))), ((int)(((byte)(221)))));
            styleCard1.Gap = 4;
            this.tabs1.Style = styleCard1;
            this.tabs1.TabIndex = 28;
            this.tabs1.Text = "tabs1";
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.panel2);
            this.tabPage1.Controls.Add(this.panel1);
            this.tabPage1.Location = new System.Drawing.Point(3, 27);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(488, 183);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "常规设置";
            // 
            // panel2
            // 
            this.panel2.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(204)))), ((int)(((byte)(204)))));
            this.panel2.BorderWidth = 1F;
            this.panel2.Controls.Add(this.flowPanel2);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.flowLayoutPanel1);
            this.panel2.Location = new System.Drawing.Point(3, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(373, 153);
            this.panel2.TabIndex = 7;
            this.panel2.Text = "panel2";
            // 
            // flowPanel2
            // 
            this.flowPanel2.Controls.Add(this.radioExit);
            this.flowPanel2.Controls.Add(this.radioTray);
            this.flowPanel2.Location = new System.Drawing.Point(0, 86);
            this.flowPanel2.Name = "flowPanel2";
            this.flowPanel2.Size = new System.Drawing.Size(248, 23);
            this.flowPanel2.TabIndex = 8;
            this.flowPanel2.Text = "flowPanel2";
            // 
            // radioExit
            // 
            this.radioExit.Location = new System.Drawing.Point(126, 3);
            this.radioExit.Name = "radioExit";
            this.radioExit.Size = new System.Drawing.Size(112, 23);
            this.radioExit.TabIndex = 6;
            this.radioExit.Text = "退出程序";
            // 
            // radioTray
            // 
            this.radioTray.Location = new System.Drawing.Point(3, 3);
            this.radioTray.Name = "radioTray";
            this.radioTray.Size = new System.Drawing.Size(117, 23);
            this.radioTray.TabIndex = 7;
            this.radioTray.Text = "最小化到托盘";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(6, 63);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(110, 23);
            this.label4.TabIndex = 5;
            this.label4.Text = "关闭主窗口动作";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.autoStartUpCheckbox);
            this.flowLayoutPanel1.Controls.Add(this.autoUpdateCheckbox);
            this.flowLayoutPanel1.Controls.Add(this.startServerOnLaunchCheckbox);
            this.flowLayoutPanel1.Controls.Add(this.notifyOnConnectCheckbox);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 11);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(366, 50);
            this.flowLayoutPanel1.TabIndex = 4;
            // 
            // autoStartUpCheckbox
            // 
            this.autoStartUpCheckbox.Location = new System.Drawing.Point(0, 0);
            this.autoStartUpCheckbox.Margin = new System.Windows.Forms.Padding(0);
            this.autoStartUpCheckbox.Name = "autoStartUpCheckbox";
            this.autoStartUpCheckbox.Size = new System.Drawing.Size(114, 23);
            this.autoStartUpCheckbox.TabIndex = 1;
            this.autoStartUpCheckbox.Text = "开机启动";
            // 
            // autoUpdateCheckbox
            // 
            this.autoUpdateCheckbox.Location = new System.Drawing.Point(114, 0);
            this.autoUpdateCheckbox.Margin = new System.Windows.Forms.Padding(0);
            this.autoUpdateCheckbox.Name = "autoUpdateCheckbox";
            this.autoUpdateCheckbox.Size = new System.Drawing.Size(97, 23);
            this.autoUpdateCheckbox.TabIndex = 0;
            this.autoUpdateCheckbox.Text = "自动更新";
            // 
            // startServerOnLaunchCheckbox
            // 
            this.startServerOnLaunchCheckbox.Location = new System.Drawing.Point(211, 0);
            this.startServerOnLaunchCheckbox.Margin = new System.Windows.Forms.Padding(0);
            this.startServerOnLaunchCheckbox.Name = "startServerOnLaunchCheckbox";
            this.startServerOnLaunchCheckbox.Size = new System.Drawing.Size(135, 23);
            this.startServerOnLaunchCheckbox.TabIndex = 3;
            this.startServerOnLaunchCheckbox.Text = "启动后运行服务";
            // 
            // notifyOnConnectCheckbox
            // 
            this.notifyOnConnectCheckbox.Location = new System.Drawing.Point(0, 23);
            this.notifyOnConnectCheckbox.Margin = new System.Windows.Forms.Padding(0);
            this.notifyOnConnectCheckbox.Name = "notifyOnConnectCheckbox";
            this.notifyOnConnectCheckbox.Size = new System.Drawing.Size(114, 23);
            this.notifyOnConnectCheckbox.TabIndex = 4;
            this.notifyOnConnectCheckbox.Text = "连接通知";
            // 
            // panel1
            // 
            this.panel1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(69)))), ((int)(((byte)(120)))));
            this.panel1.BorderWidth = 2F;
            this.panel1.Controls.Add(this.socketCountLabel);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(382, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(102, 61);
            this.panel1.TabIndex = 6;
            this.panel1.Text = "panel1";
            // 
            // socketCountLabel
            // 
            this.socketCountLabel.BackColor = System.Drawing.Color.Transparent;
            this.socketCountLabel.Font = new System.Drawing.Font("微软雅黑", 11F);
            this.socketCountLabel.Location = new System.Drawing.Point(0, 34);
            this.socketCountLabel.Name = "socketCountLabel";
            this.socketCountLabel.Size = new System.Drawing.Size(102, 23);
            this.socketCountLabel.TabIndex = 1;
            this.socketCountLabel.Text = "0";
            this.socketCountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 11F);
            this.label1.Location = new System.Drawing.Point(0, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(102, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "连接终端";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.label5);
            this.tabPage2.Controls.Add(this.defaultPrinterSelect);
            this.tabPage2.Location = new System.Drawing.Point(-488, -183);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(488, 183);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "打印机设置";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(9, 4);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(75, 23);
            this.label5.TabIndex = 1;
            this.label5.Text = "默认打印机";
            // 
            // defaultPrinterSelect
            // 
            this.defaultPrinterSelect.Location = new System.Drawing.Point(7, 24);
            this.defaultPrinterSelect.Name = "defaultPrinterSelect";
            this.defaultPrinterSelect.Size = new System.Drawing.Size(468, 38);
            this.defaultPrinterSelect.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(494, 246);
            this.Controls.Add(this.tabs1);
            this.Controls.Add(this.flowPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "报表助手";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.mainContextMenuStrip.ResumeLayout(false);
            this.flowPanel1.ResumeLayout(false);
            this.tabs1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.flowPanel2.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

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

        private void mainNotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
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
                throw new Exception(objException.Message);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("你确定要退出程序吗？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.OK)
                {
                    this.mainNotifyIcon.Visible = false;
                    this.mainNotifyIcon.Dispose();
                    this.Dispose();
                    Application.Exit();
                }
            }
            catch (Exception objException)
            {
                MessageBox.Show(objException.Message);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (Helper.AlreadyRun())
            {
                MessageBox.Show("打印助手已运行");
                this.ExitApp();
            }
            else
            {
                string[] commandLineArgs = Environment.GetCommandLineArgs();
                if (commandLineArgs.Length == 2)
                {
                    if (commandLineArgs[1].Contains("run"))
                    {
                        this.StartServer();
                    }
                    if (commandLineArgs[1].Contains("serve"))
                    {
                        this.StartServer();
                        return;
                    }

                }
            }
        }


        private void ExitApp()
        {
            try
            {
                Application.Exit();
            }
            catch (Exception)
            {
                Environment.Exit(0);
            }
        }

        private void SocketSendMessage(string socketId, object obj)
        {
            IWebSocketConnection socket = ViewModel.Sockets.FirstOrDefault(row => row.ConnectionInfo.Id.ToString() == socketId);
            if (socket != null)
            {
                if (obj is string str)
                {
                    socket.Send(str);
                }
                else
                {
                    socket.Send(JObject.FromObject(obj).ToString(0, Array.Empty<JsonConverter>()));
                }
            }
        }


        private void MessageHandle(IWebSocketConnection socket, string message)
        {
            try
            {
                if (message == "ping")
                {
                    SocketSendMessage(socket.ConnectionInfo.Id.ToString(), "pong");
                    taskEvent.Set();
                    return;
                }
                string socketOrigin = this.GetSocketOrigin(socket);
                if (!this.taskEvent.WaitOne(0x7530))
                {
                    throw new Exception("任务超时已取消");
                }
                JObject jsonData = JObject.Parse(message);
                //指令
                string cmd = jsonData.Value<string>("cmd");
                //数据源
                string source = jsonData.Value<string>("source");
                //模板数据
                string template = jsonData.Value<string>("template");
                //扩展数据
                JObject extInfo = jsonData.Value<JObject>("extInfo");
                if (string.IsNullOrEmpty(cmd))
                {
                    throw new MessageHandleException("指令未传输");
                }
                switch(cmd)
                {
                    case "get-printers":
                    {
                        TaskItem item = new TaskItem();
                        item.TaskName = cmd;
                        item.SocketId = socket.ConnectionInfo.Id.ToString();
                        item.ThreadId = Thread.CurrentThread.ManagedThreadId.ToString();
                        item.Template = template;
                        item.Source = source;
                        ViewModel.Tasks.Add(item);
                        List<string> printers = MyLocalPrinter.GetLocalPrinters();
                        string defaultPrinter = MyLocalPrinter.DefaultPrinter();
                        object obj = new
                        {
                            //保证消息的唯一性
                            ticketId = Guid.NewGuid().ToString(),
                            success = true,
                            type = cmd,
                            code = 200,
                            state = "success",
                            data = new
                            {
                                defaultPrinter,
                                printers
                            }
                        };
                        socket.Send(JObject.FromObject(obj).ToString(0, Array.Empty<JsonConverter>()));
                        ViewModel.Tasks.Add(item);
                        this.taskEvent.Set();
                        break;
                    }
                    case "print":
                    {
                        if (string.IsNullOrEmpty(template))
                        {
                            throw new MessageHandleException("模板信息未传输");
                        }
                        TaskItem item = new TaskItem();
                        item.TaskName = cmd;
                        item.SocketId = socket.ConnectionInfo.Id.ToString();
                        item.ThreadId = Thread.CurrentThread.ManagedThreadId.ToString();
                        item.Template = template;
                        item.Source = source;
                        ViewModel.Tasks.Add(item);
                        this.Print(template, source, extInfo);
                        ViewModel.Tasks.Add(item);
                        break;
                    }
                    case "preview":
                    {
                        if (string.IsNullOrEmpty(template))
                        {
                            throw new MessageHandleException("模板信息未传输");
                        }
                        TaskItem item = new TaskItem();
                        item.TaskName = cmd;
                        item.SocketId = socket.ConnectionInfo.Id.ToString();
                        item.ThreadId = Thread.CurrentThread.ManagedThreadId.ToString();
                        item.Template = template;
                        item.Source = source;
                        ViewModel.Tasks.Add(item);
                        this.Preview(template, source, extInfo);
                        break;
                    }
                    case "document":
                    {
                        TaskItem item = new TaskItem();
                        item.TaskName = cmd;
                        item.SocketId = socket.ConnectionInfo.Id.ToString();
                        item.ThreadId = Thread.CurrentThread.ManagedThreadId.ToString();
                        ViewModel.Tasks.Add(item);
                        break;
                    }
                    case "design":
                    {
                        TaskItem item = new TaskItem();
                        item.TaskName = cmd;
                        item.SocketId = socket.ConnectionInfo.Id.ToString();
                        item.ThreadId = Thread.CurrentThread.ManagedThreadId.ToString();
                        item.Template = template;
                        item.Source = source;
                        ViewModel.Tasks.Add(item);
                        this.Design(socket, template, source, extInfo);
                        break;
                    }
                    default:
                    {
                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                logger.Error(exception);
                this.taskEvent.Set();
                object obj = new
                {
                    //保证消息的唯一性
                    ticketId = Guid.NewGuid().ToString(),
                    success = false,
                    code = 500,
                    state = "error",
                    message = exception.Message.ToString()
                };
                socket.Send(JObject.FromObject(obj).ToString(0, Array.Empty<JsonConverter>()));
                TaskItem item = ViewModel.Tasks.Find(row => (row.ThreadId == Thread.CurrentThread.ManagedThreadId.ToString()));
                if (item != null)
                {
                    ViewModel.Tasks.Remove(item);
                }
            }
        }

        private void Design(IWebSocketConnection socket, string template, string source, JObject extInfo)
        {
            lock (lockDesignForm)
            {
                if (designOpened && designForm != null)
                {
                    /*
                    this.Invoke((MethodInvoker)delegate
                    {
                        designForm.TopMost = true;
                        designForm.Activate();
                        designForm.TopMost = false;
                    });
                    */
                    throw new MessageHandleException("设计器繁忙");
                }
                //taskDesignEvent.WaitOne();
                // 使用 Invoke 来确保UI操作在UI线程上执行
                this.Invoke((MethodInvoker)delegate
                {
                    TaskItem item = ViewModel.Tasks.Find(row => row.ThreadId == Thread.CurrentThread.ManagedThreadId.ToString());
                    designForm = new DesignForm(socket, template, source, extInfo);
                    designForm.FormClosed += (o, e) =>
                    {
                        //taskDesignEvent.Set();
                        designOpened = false;
                        designForm = null;
                    };
                    designForm.Show();
                    designForm.TopMost = true;
                    designForm.Activate();
                    designForm.TopMost = false;
                    designOpened = true;
                    taskEvent.Set();
                    ViewModel.Tasks.Remove(item);
                });
            }
        }

        private void Preview(string template, string source, JObject extInfo)
        {
            // 使用 Invoke 来确保UI操作在UI线程上执行
            this.Invoke((MethodInvoker)delegate
            {
                TaskItem item = ViewModel.Tasks.Find(row => row.ThreadId == Thread.CurrentThread.ManagedThreadId.ToString());
                Form form = new PreviewForm(template, source, extInfo);
                form.Show();
                form.TopMost = true;
                form.Activate();
                form.TopMost = false;
                ViewModel.Tasks.Remove(item);
                taskEvent.Set();
            });
        }

        private void Print(string template, string source, JObject extInfo)
        {
            // 使用 Invoke 来确保UI操作在UI线程上执行
            this.Invoke((MethodInvoker)delegate
            {
                TaskItem item = ViewModel.Tasks.Find(row => row.ThreadId == Thread.CurrentThread.ManagedThreadId.ToString());
                Form form = new PrintForm(template, source, extInfo);
                form.Show();
                form.TopMost = true;
                form.Activate();
                form.TopMost = false;
                ViewModel.Tasks.Remove(item);
                taskEvent.Set();
            });
        }

        private void toggleButton_Click(object sender, EventArgs e)
        {
            if (!ViewModel.State)
            {
                StartServer();
            }
            else
            {
                CloseServer();
            }
        }
    }
}

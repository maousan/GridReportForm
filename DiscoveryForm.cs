using System;
using System.Drawing;
using System.Windows.Forms;

namespace GridReportForm
{
    /// <summary>
    /// Settings dialog for the local device discovery service (docs/adr/0009).
    /// Opened from the tray menu. Keeps discovery configuration out of the
    /// already crowded main window.
    /// </summary>
    internal class DiscoveryForm : Form
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly MainViewModel viewModel;
        private readonly Func<bool> isRunningProvider;
        private readonly Action applyRestart;

        private Label sectionLabel;
        private AntdUI.Label statusLabel;
        private AntdUI.Checkbox enabledCheckbox;
        private Label portLabel;
        private AntdUI.Input portInput;
        private Label originsLabel;
        private AntdUI.Input originsInput;
        private AntdUI.Button applyButton;

        public DiscoveryForm(MainViewModel viewModel, Func<bool> isRunningProvider, Action applyRestart)
        {
            this.viewModel = viewModel;
            this.isRunningProvider = isRunningProvider;
            this.applyRestart = applyRestart;
            BuildUi();
            RefreshStatus();
        }

        private void BuildUi()
        {
            Text = "本地发现服务";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(340, 290);
            BackColor = Color.White;
            Font = new Font("微软雅黑", 10);

            sectionLabel = new Label
            {
                Location = new Point(16, 16),
                AutoSize = true,
                Text = "本地发现服务",
                Font = new Font("微软雅黑", 11, FontStyle.Bold)
            };
            statusLabel = new AntdUI.Label
            {
                Location = new Point(190, 16),
                Size = new Size(134, 24),
                Text = "○ 未监听"
            };

            enabledCheckbox = new AntdUI.Checkbox
            {
                Location = new Point(16, 52),
                Size = new Size(160, 24),
                Checked = viewModel.DiscoveryEnabled,
                Text = "启用本地发现"
            };

            portLabel = new Label { Location = new Point(16, 90), AutoSize = true, Text = "端口" };
            portInput = new AntdUI.Input
            {
                Location = new Point(16, 112),
                Size = new Size(308, 34),
                Radius = 4,
                Text = viewModel.DiscoveryPort.ToString()
            };

            originsLabel = new Label { Location = new Point(16, 156), AutoSize = true, Text = "允许的前端来源（逗号分隔）" };
            originsInput = new AntdUI.Input
            {
                Location = new Point(16, 178),
                Size = new Size(308, 34),
                Radius = 4,
                Text = viewModel.DiscoveryAllowedOrigins,
                PlaceholderText = "http://biz.example.com"
            };

            applyButton = new AntdUI.Button
            {
                Location = new Point(190, 236),
                Size = new Size(134, 34),
                Radius = 4,
                Text = "应用并重启"
            };
            applyButton.Click += ApplyButton_Click;

            Controls.Add(sectionLabel);
            Controls.Add(statusLabel);
            Controls.Add(enabledCheckbox);
            Controls.Add(portLabel);
            Controls.Add(portInput);
            Controls.Add(originsLabel);
            Controls.Add(originsInput);
            Controls.Add(applyButton);
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            viewModel.DiscoveryEnabled = enabledCheckbox.Checked;
            if (int.TryParse(portInput.Text.Trim(), out int port))
            {
                viewModel.DiscoveryPort = port;
            }
            viewModel.DiscoveryAllowedOrigins = originsInput.Text.Trim();
            logger.Info("Discovery settings applied. Enabled={Enabled}, Port={Port}, AllowedOrigins={AllowedOrigins}", viewModel.DiscoveryEnabled, viewModel.DiscoveryPort, viewModel.DiscoveryAllowedOrigins);
            applyRestart?.Invoke();
            RefreshStatus();
        }

        private void RefreshStatus()
        {
            if (isRunningProvider != null && isRunningProvider())
            {
                statusLabel.Text = "● 监听中";
                statusLabel.ForeColor = Color.FromArgb(0, 120, 60);
            }
            else if (!viewModel.DiscoveryEnabled)
            {
                statusLabel.Text = "○ 未启用";
                statusLabel.ForeColor = Color.Gray;
            }
            else
            {
                statusLabel.Text = "○ 未监听";
                statusLabel.ForeColor = Color.FromArgb(180, 80, 20);
            }
        }
    }
}

using IniParser;
using IniParser.Model;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;

namespace GridReportForm
{
    public class MainViewModel : ReactiveObject
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IniData _config;
        private string _ip;
        private bool _autoUpdate;
        private bool _autoStartUp;
        private string _closeMode;
        private bool _cloudEnabled;
        private string _cloudServerUrl;
        private string _cloudDeviceToken;
        private string _cloudDeviceId;
        private string _cloudDeviceName;
        private bool _allowPreview;
        private FileIniDataParser parser = new FileIniDataParser();
        private string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");


        public MainViewModel()
        {
            logger.Info("Loading application config. ConfigFile={ConfigFile}", configFilePath);
            Config = parser.ReadFile(configFilePath);
            LocalIp = Helper.GetLocalIP();
            AutoStartUp = bool.Parse(Config["App"]["AutoStartUp"]);
            AutoUpdate = bool.Parse(Config["App"]["AutoUpdate"]);
            CloseMode = Config["App"]["CloseMode"];
            CloudEnabled = ReadBool("CloudEnabled", true);
            CloudServerUrl = ReadString("CloudServerUrl", "");
            CloudDeviceToken = ReadString("CloudDeviceToken", ReadOptionalString("CloudToken", ReadOptionalString("CloudApiKey", "")));
            CloudDeviceId = ReadString("CloudDeviceId", "");
            if (string.IsNullOrWhiteSpace(CloudDeviceId))
            {
                CloudDeviceId = Guid.NewGuid().ToString("N");
                logger.Info("Generated new cloud device id. DeviceId={DeviceId}", CloudDeviceId);
            }
            else if (CloudDeviceId.StartsWith("device-", StringComparison.OrdinalIgnoreCase))
            {
                CloudDeviceId = CloudDeviceId.Substring("device-".Length);
                logger.Info("Migrated cloud device id by removing legacy prefix. DeviceId={DeviceId}", CloudDeviceId);
            }
            CloudDeviceName = ReadString("CloudDeviceName", Environment.MachineName);
            if (string.IsNullOrWhiteSpace(CloudDeviceName))
            {
                CloudDeviceName = Environment.MachineName;
            }
            AllowPreview = ReadBool("AllowPreview", false);
            logger.Info("Application config loaded. CloudEnabled={CloudEnabled}, ServerUrl={ServerUrl}, DeviceId={DeviceId}, DeviceName={DeviceName}, HasToken={HasToken}, AllowPreview={AllowPreview}", CloudEnabled, CloudServerUrl, CloudDeviceId, CloudDeviceName, !string.IsNullOrWhiteSpace(CloudDeviceToken), AllowPreview);
        }

        public List<TaskItem> Tasks { get; } = new List<TaskItem>();

        public bool AutoUpdate
        {
            get => _autoUpdate;
            set
            {
                this.RaiseAndSetIfChanged(ref _autoUpdate, value);
                if (value.ToString() != Config["App"]["AutoUpdate"])
                {
                    Config["App"]["AutoUpdate"] = value.ToString();
                    parser.WriteFile(configFilePath, Config);
                }
            }
        }

        public string CloseMode
        {
            get => _closeMode;
            set
            {
                this.RaiseAndSetIfChanged(ref _closeMode, value);
                if (value.ToString() != Config["App"]["CloseMode"])
                {
                    Config["App"]["CloseMode"] = value.ToString();
                    parser.WriteFile(configFilePath, Config);
                }
            }
        }


        public bool AutoStartUp
        {
            get => _autoStartUp;
            set
            {
                this.RaiseAndSetIfChanged(ref _autoStartUp, value);
                if (value.ToString() != Config["App"]["AutoStartUp"])
                {
                    Config["App"]["AutoStartUp"] = value.ToString();
                    parser.WriteFile(configFilePath, Config);
                    Helper.SetMeStart(value);
                }
            }
        }

        public string LocalIp
        {
            get => _ip;
            set => this.RaiseAndSetIfChanged(ref _ip, value);
        }

        public IniData Config
        {
            get => _config;
            set
            {
                _config = value;
            }
        }

        public bool CloudEnabled
        {
            get => _cloudEnabled;
            set
            {
                this.RaiseAndSetIfChanged(ref _cloudEnabled, value);
                WriteString("CloudEnabled", value.ToString());
            }
        }

        public string CloudServerUrl
        {
            get => _cloudServerUrl;
            set
            {
                this.RaiseAndSetIfChanged(ref _cloudServerUrl, value);
                WriteString("CloudServerUrl", value ?? "");
            }
        }

        public string CloudDeviceToken
        {
            get => _cloudDeviceToken;
            set
            {
                this.RaiseAndSetIfChanged(ref _cloudDeviceToken, value);
                WriteString("CloudDeviceToken", value ?? "");
            }
        }

        public string CloudDeviceId
        {
            get => _cloudDeviceId;
            set
            {
                this.RaiseAndSetIfChanged(ref _cloudDeviceId, value);
                WriteString("CloudDeviceId", value ?? "");
            }
        }

        public string CloudDeviceName
        {
            get => _cloudDeviceName;
            set
            {
                this.RaiseAndSetIfChanged(ref _cloudDeviceName, value);
                WriteString("CloudDeviceName", value ?? "");
            }
        }

        public bool AllowPreview
        {
            get => _allowPreview;
            set
            {
                this.RaiseAndSetIfChanged(ref _allowPreview, value);
                WriteString("AllowPreview", value.ToString());
            }
        }

        private string ReadString(string key, string defaultValue)
        {
            if (!Config["App"].ContainsKey(key))
            {
                logger.Info("Config key missing. Writing default value. Key={Key}", key);
                WriteString(key, defaultValue);
            }
            return Config["App"][key];
        }

        private string ReadOptionalString(string key, string defaultValue)
        {
            return Config["App"].ContainsKey(key) ? Config["App"][key] : defaultValue;
        }

        private bool ReadBool(string key, bool defaultValue)
        {
            return bool.Parse(ReadString(key, defaultValue.ToString()));
        }

        private void WriteString(string key, string value)
        {
            if (Config["App"][key] != value)
            {
                logger.Debug("Writing config value. Key={Key}, Value={Value}", key, IsSensitiveKey(key) ? "***" : value);
                Config["App"][key] = value;
                parser.WriteFile(configFilePath, Config);
            }
        }

        private static bool IsSensitiveKey(string key)
        {
            return key != null && key.IndexOf("Token", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}

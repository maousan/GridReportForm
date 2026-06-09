using DynamicData;
using DynamicData.Binding;
using Fleck;
using IniParser;
using IniParser.Model;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridReportForm
{
    public class MainViewModel : ReactiveObject
    {
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private decimal _port;
        private bool _state;
        private ObservableCollection<IWebSocketConnection> _sockets;
        private IniData _config;
        private string _ip;
        private bool _autoUpdate;
        private bool _autoStartUp;
        private bool _notifyOnConnect;
        private bool _startServerOnLaunch;
        private string _closeMode;
        private FileIniDataParser parser = new FileIniDataParser();
        private string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");


        public MainViewModel()
        {
            Config = parser.ReadFile(configFilePath);
            Port = int.Parse(Config["App"]["Port"]);
            LocalIp = Helper.GetLocalIP();
            NotifyOnConnect = bool.Parse(Config["App"]["NotifyOnConnect"]);
            AutoStartUp = bool.Parse(Config["App"]["AutoStartUp"]);
            AutoUpdate = bool.Parse(Config["App"]["AutoUpdate"]);
            StartServerOnLaunch = bool.Parse(Config["App"]["StartServerOnLaunch"]);
            CloseMode = Config["App"]["CloseMode"];
            Sockets = new ObservableCollection<IWebSocketConnection>();
        }

        public List<TaskItem> Tasks { get; } = new List<TaskItem>();

        public bool StartServerOnLaunch
        {
            get => _startServerOnLaunch;
            set
            {
                this.RaiseAndSetIfChanged(ref _startServerOnLaunch, value);
                if (value.ToString() != Config["App"]["StartServerOnLaunch"])
                {
                    Config["App"]["StartServerOnLaunch"] = value.ToString();
                    parser.WriteFile(configFilePath, Config);
                }
            }
        }

        public bool NotifyOnConnect
        {
            get => _notifyOnConnect;
            set
            {
                this.RaiseAndSetIfChanged(ref _notifyOnConnect, value);
                if (value.ToString() != Config["App"]["NotifyOnConnect"])
                {
                    Config["App"]["NotifyOnConnect"] = value.ToString();
                    parser.WriteFile(configFilePath, Config);
                }
            }
        }

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

        public decimal Port
        {
            get => _port;
            set
            {
                this.RaiseAndSetIfChanged(ref _port, value);
                if (value.ToString() != Config["App"]["Port"])
                {
                    Config["App"]["Port"] = value.ToString();
                    parser.WriteFile(configFilePath, Config);
                }
            }
        }

        public bool State
        {
            get => _state;
            set => this.RaiseAndSetIfChanged(ref _state, value);
        }


        public ObservableCollection<IWebSocketConnection> Sockets
        {
            get => _sockets;
            set => this.RaiseAndSetIfChanged(ref _sockets, value);
        }
    }
}

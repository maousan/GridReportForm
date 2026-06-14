using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace GridReportForm
{
    internal class CloudWebSocketTransport : IDisposable
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly Func<CloudConnectionOptions> optionsFactory;
        private readonly Func<CloudReportTask, CloudTaskResult> taskHandler;
        private readonly object socketLock = new object();
        private readonly object sendLock = new object();
        private CancellationTokenSource cancellation;
        private WebSocket socket;
        private Task worker;

        public CloudWebSocketTransport(Func<CloudConnectionOptions> optionsFactory, Func<CloudReportTask, CloudTaskResult> taskHandler)
        {
            this.optionsFactory = optionsFactory;
            this.taskHandler = taskHandler;
        }

        public bool IsRunning => worker != null && !worker.IsCompleted && cancellation != null && !cancellation.IsCancellationRequested;

        public string Status { get; private set; } = "未连接";

        public string LastError { get; private set; }

        public event Action<string> StatusChanged;

        public void Start()
        {
            if (IsRunning)
            {
                logger.Debug("Cloud transport start ignored because worker is already running. Status={Status}", Status);
                return;
            }

            logger.Info("Starting cloud transport.");
            cancellation = new CancellationTokenSource();
            worker = Task.Run(() => RunAsync(cancellation.Token));
        }

        public void Stop()
        {
            if (cancellation == null)
            {
                logger.Debug("Cloud transport stop ignored because cancellation source is null. Status={Status}", Status);
                return;
            }

            logger.Info("Stopping cloud transport. Status={Status}", Status);
            cancellation.Cancel();
            CloseSocket();
            cancellation.Dispose();
            cancellation = null;
            SetStatus("未连接");
        }

        public void ReportPrintersNow()
        {
            logger.Info("Manual printer report requested.");
            Task.Run(() => SendPrinterReport());
        }

        public void Dispose()
        {
            Stop();
        }

        private async Task RunAsync(CancellationToken token)
        {
            CloudConnectionOptions options = optionsFactory();
            if (string.IsNullOrWhiteSpace(options.ServerUrl))
            {
                LastError = "云服务器地址未配置";
                logger.Warn("Cloud transport cannot start because server url is empty. DeviceId={DeviceId}", options.DeviceId);
                SetStatus("未配置云服务器");
                return;
            }
            if (string.IsNullOrWhiteSpace(options.DeviceToken))
            {
                LastError = "设备未激活，请先输入激活码完成激活";
                logger.Warn("Cloud transport cannot start because device token is empty. DeviceId={DeviceId}, ServerUrl={ServerUrl}", options.DeviceId, options.ServerUrl);
                SetStatus("未激活");
                return;
            }

            try
            {
                SetStatus("连接中");
                Uri serverUri = BuildServerUri(options);
                logger.Info("Connecting cloud WebSocket. ServerUri={ServerUri}, DeviceId={DeviceId}, DeviceName={DeviceName}, HasToken={HasToken}", MaskTokenQuery(serverUri), options.DeviceId, options.DeviceName, !string.IsNullOrWhiteSpace(options.DeviceToken));
                WebSocket activeSocket = CreateSocket(serverUri);
                lock (socketLock)
                {
                    socket = activeSocket;
                }
                activeSocket.Connect();
                if (activeSocket.ReadyState != WebSocketState.Open)
                {
                    throw new MessageHandleException("WebSocket 连接未打开");
                }
                if (token.IsCancellationRequested)
                {
                    logger.Info("Cloud WebSocket connected after cancellation. Closing immediately. DeviceId={DeviceId}", options.DeviceId);
                    activeSocket.Close();
                    return;
                }

                LastError = null;
                SetStatus("已连接");
                logger.Info("Cloud WebSocket connected. DeviceId={DeviceId}, DeviceName={DeviceName}", options.DeviceId, options.DeviceName);
                SendHello(options);
                SendPrinterReport();
                await RunConnectedAsync(token);
            }
            catch (OperationCanceledException)
            {
                logger.Info("Cloud transport run cancelled.");
            }
            catch (Exception exception)
            {
                logger.Error(exception, "Cloud transport connection failed. ServerUrl={ServerUrl}, DeviceId={DeviceId}", options.ServerUrl, options.DeviceId);
                LastError = FormatConnectionError(exception);
                SetStatus("连接失败");
            }
            finally
            {
                logger.Debug("Cloud transport run finished. Status={Status}", Status);
                CloseSocket();
            }
        }

        private async Task RunConnectedAsync(CancellationToken token)
        {
            WebSocket activeSocket = socket;
            if (activeSocket == null)
            {
                return;
            }

            DateTime nextHeartbeatAt = DateTime.UtcNow.AddSeconds(30);
            DateTime nextPrinterReportAt = DateTime.UtcNow.AddMinutes(5);

            while (!token.IsCancellationRequested && activeSocket.ReadyState == WebSocketState.Open)
            {
                if (DateTime.UtcNow >= nextHeartbeatAt)
                {
                    logger.Trace("Sending cloud heartbeat.");
                    SendEnvelope(CloudEnvelope.Create("heartbeat", null, new { }));
                    nextHeartbeatAt = DateTime.UtcNow.AddSeconds(30);
                }
                if (DateTime.UtcNow >= nextPrinterReportAt)
                {
                    SendPrinterReport();
                    nextPrinterReportAt = DateTime.UtcNow.AddMinutes(5);
                }

                await Task.Delay(1000, token);
            }
        }

        private void HandleMessage(string message)
        {
            CloudEnvelope envelope = JsonConvert.DeserializeObject<CloudEnvelope>(message);
            if (envelope == null || envelope.Type != "task")
            {
                logger.Debug("Ignored cloud message. MessageType={MessageType}, Length={Length}", envelope?.Type, message?.Length ?? 0);
                return;
            }

            CloudReportTask task = envelope.Data?.ToObject<CloudReportTask>();
            logger.Info("Received cloud task. RequestId={RequestId}, TaskId={TaskId}, Cmd={Cmd}", envelope.RequestId, task?.TaskId, task?.Cmd);
            CloudTaskResult result = taskHandler(task);
            logger.Info("Cloud task executed. RequestId={RequestId}, TaskId={TaskId}, Cmd={Cmd}, Success={Success}, ErrorCode={ErrorCode}, Message={Message}", envelope.RequestId, result.TaskId, result.Cmd, result.Success, result.ErrorCode, result.Message);
            object data = new
            {
                taskId = result.TaskId,
                cmd = result.Cmd,
                success = result.Success,
                message = result.Message,
                errorCode = result.ErrorCode,
                result = result.Result ?? new { }
            };
            SendEnvelope(CloudEnvelope.Create("task-result", envelope.RequestId, data));
        }

        private void SendHello(CloudConnectionOptions options)
        {
            logger.Debug("Sending cloud hello. DeviceId={DeviceId}, DeviceName={DeviceName}, AllowPreview={AllowPreview}", options.DeviceId, options.DeviceName, options.AllowPreview);
            SendEnvelope(CloudEnvelope.Create("hello", null, new
            {
                deviceId = options.DeviceId,
                deviceName = options.DeviceName,
                token = options.DeviceToken,
                allowPreview = options.AllowPreview
            }));
        }

        private void SendPrinterReport()
        {
            if (socket == null || socket.ReadyState != WebSocketState.Open)
            {
                return;
            }

            CloudConnectionOptions options = optionsFactory();
            List<string> printers = MyLocalPrinter.GetLocalPrinters();
            logger.Info("Sending printer report. DeviceId={DeviceId}, DefaultPrinter={DefaultPrinter}, PrinterCount={PrinterCount}", options.DeviceId, MyLocalPrinter.DefaultPrinter(), printers.Count);
            SendEnvelope(CloudEnvelope.Create("printer-report", null, new
            {
                deviceId = options.DeviceId,
                defaultPrinter = MyLocalPrinter.DefaultPrinter(),
                printers
            }));
        }

        private void SendEnvelope(CloudEnvelope envelope)
        {
            WebSocket activeSocket = socket;
            if (activeSocket == null || activeSocket.ReadyState != WebSocketState.Open)
            {
                logger.Trace("Skip sending envelope because socket is not open. Type={Type}, RequestId={RequestId}", envelope?.Type, envelope?.RequestId);
                return;
            }

            string json = JsonConvert.SerializeObject(envelope);
            try
            {
                lock (sendLock)
                {
                    logger.Trace("Sending cloud envelope. Type={Type}, RequestId={RequestId}, Length={Length}", envelope.Type, envelope.RequestId, json.Length);
                    activeSocket.Send(json);
                }
            }
            catch (InvalidOperationException exception)
            {
                logger.Warn(exception, "Sending cloud envelope failed because socket state changed. Type={Type}, RequestId={RequestId}", envelope.Type, envelope.RequestId);
            }
        }

        private void CloseSocket()
        {
            WebSocket activeSocket;
            lock (socketLock)
            {
                activeSocket = socket;
                socket = null;
            }

            if (activeSocket == null || activeSocket.ReadyState != WebSocketState.Open)
            {
                logger.Debug("Skip closing cloud socket. SocketExists={SocketExists}, State={State}", activeSocket != null, activeSocket?.ReadyState.ToString());
                return;
            }

            try
            {
                logger.Debug("Closing cloud socket. State={State}", activeSocket.ReadyState);
                activeSocket.Close();
            }
            catch (Exception exception)
            {
                logger.Warn(exception, "Cloud socket close failed.");
            }
        }

        private static Uri BuildServerUri(CloudConnectionOptions options)
        {
            UriBuilder builder = new UriBuilder(options.ServerUrl);
            string tokenQuery = "token=" + Uri.EscapeDataString(options.DeviceToken ?? "");
            if (string.IsNullOrWhiteSpace(builder.Query))
            {
                builder.Query = tokenQuery;
            }
            else if (builder.Query.IndexOf("token=", StringComparison.OrdinalIgnoreCase) < 0)
            {
                builder.Query = builder.Query.TrimStart('?') + "&" + tokenQuery;
            }
            return builder.Uri;
        }

        private WebSocket CreateSocket(Uri serverUri)
        {
            WebSocket webSocket = new WebSocket(serverUri.ToString());
            webSocket.OnMessage += (sender, args) =>
            {
                if (args.IsText)
                {
                    logger.Trace("Cloud WebSocket message received. Length={Length}", args.Data?.Length ?? 0);
                    Task.Run(() => HandleMessage(args.Data));
                }
            };
            webSocket.OnError += (sender, args) =>
            {
                logger.Error(args.Exception, "Cloud WebSocket error. Message={Message}", args.Message);
                LastError = args.Message;
                SetStatus("连接失败");
            };
            webSocket.OnClose += (sender, args) =>
            {
                logger.Info("Cloud WebSocket closed. Code={Code}, Reason={Reason}, WasClean={WasClean}, Status={Status}", args.Code, args.Reason, args.WasClean, Status);
                if (Status == "已连接")
                {
                    SetStatus("未连接");
                }
            };
            return webSocket;
        }

        private static string MaskTokenQuery(Uri uri)
        {
            UriBuilder builder = new UriBuilder(uri);
            if (!string.IsNullOrWhiteSpace(builder.Query))
            {
                string[] parts = builder.Query.TrimStart('?').Split('&');
                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i].StartsWith("token=", StringComparison.OrdinalIgnoreCase))
                    {
                        parts[i] = "token=***";
                    }
                }
                builder.Query = string.Join("&", parts);
            }
            return builder.Uri.ToString();
        }

        private static string FormatConnectionError(Exception exception)
        {
            string message = exception.Message;
            if (message != null && message.IndexOf("Connection", StringComparison.OrdinalIgnoreCase) >= 0
                && message.IndexOf("upgrade, keep-alive", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "WebSocket 握手失败：服务端或网关返回了无效的 Connection 响应头 \"upgrade, keep-alive\"。请将云打印 WebSocket 地址对应的网关配置改为只返回 \"Connection: Upgrade\"。";
            }
            return message;
        }

        private void SetStatus(string status)
        {
            if (Status != status)
            {
                logger.Info("Cloud transport status changed. From={OldStatus}, To={NewStatus}", Status, status);
            }
            Status = status;
            StatusChanged?.Invoke(status);
        }
    }

    internal class CloudConnectionOptions
    {
        public string ServerUrl { get; set; }
        public string DeviceToken { get; set; }
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public bool AllowPreview { get; set; }
    }
}

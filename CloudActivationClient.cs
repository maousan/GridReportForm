using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace GridReportForm
{
    internal static class CloudActivationClient
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static CloudActivationResult Activate(CloudActivationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ServerUrl))
            {
                throw new MessageHandleException("云服务器地址未配置");
            }
            if (string.IsNullOrWhiteSpace(request.ActivationCode))
            {
                throw new MessageHandleException("请输入激活码");
            }

            string url = BuildActivationUrl(request.ServerUrl);
            logger.Info("Activating cloud device. ActivationUrl={ActivationUrl}, DeviceId={DeviceId}, DeviceName={DeviceName}, AllowPreview={AllowPreview}, DefaultPrinter={DefaultPrinter}, PrinterCount={PrinterCount}, HasActivationCode={HasActivationCode}",
                url,
                request.DeviceId,
                request.DeviceName,
                request.AllowPreview,
                request.DefaultPrinter,
                request.Printers?.Count ?? 0,
                !string.IsNullOrWhiteSpace(request.ActivationCode));
            object body = new
            {
                activationCode = request.ActivationCode,
                deviceId = request.DeviceId,
                deviceName = request.DeviceName,
                allowPreview = request.AllowPreview,
                defaultPrinter = request.DefaultPrinter,
                printers = request.Printers
            };

            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                client.Headers[HttpRequestHeader.ContentType] = "application/json; charset=UTF-8";
                client.Headers[HttpRequestHeader.Accept] = "application/json";
                try
                {
                    string response = client.UploadString(url, "POST", JsonConvert.SerializeObject(body));
                    JObject result = JObject.Parse(response);
                    int code = result.Value<int>("code");
                    if (code != 200)
                    {
                        string message = result.Value<string>("message") ?? "设备激活失败";
                        logger.Warn("Cloud device activation rejected. Code={Code}, Message={Message}, DeviceId={DeviceId}", code, message, request.DeviceId);
                        throw new MessageHandleException(message);
                    }

                    JObject data = result.Value<JObject>("data");
                    if (data == null)
                    {
                        logger.Warn("Cloud device activation response missing data. DeviceId={DeviceId}", request.DeviceId);
                        throw new MessageHandleException("设备激活响应缺少数据");
                    }

                    CloudActivationResult activationResult = new CloudActivationResult
                    {
                        DeviceId = data.Value<string>("deviceId"),
                        DeviceName = data.Value<string>("deviceName"),
                        Token = data.Value<string>("token")
                    };
                    logger.Info("Cloud device activation succeeded. DeviceId={DeviceId}, DeviceName={DeviceName}, HasToken={HasToken}", activationResult.DeviceId, activationResult.DeviceName, !string.IsNullOrWhiteSpace(activationResult.Token));
                    return activationResult;
                }
                catch (Exception exception)
                {
                    logger.Error(exception, "Cloud device activation failed. ActivationUrl={ActivationUrl}, DeviceId={DeviceId}", url, request.DeviceId);
                    throw;
                }
            }
        }

        private static string BuildActivationUrl(string serverUrl)
        {
            Uri uri = new Uri(serverUrl);
            string scheme = uri.Scheme == "wss" ? "https" : uri.Scheme == "ws" ? "http" : uri.Scheme;
            UriBuilder builder = new UriBuilder(uri)
            {
                Scheme = scheme,
                Path = "/admin/platform/cloudPrint/devices/activate",
                Query = "",
                Fragment = ""
            };
            return builder.Uri.ToString();
        }
    }

    internal class CloudActivationRequest
    {
        public string ServerUrl { get; set; }
        public string ActivationCode { get; set; }
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public bool AllowPreview { get; set; }
        public string DefaultPrinter { get; set; }
        public List<string> Printers { get; set; }
    }

    internal class CloudActivationResult
    {
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string Token { get; set; }
    }
}

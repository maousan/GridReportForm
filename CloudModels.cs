using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace GridReportForm
{
    internal class CloudEnvelope
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("requestId")]
        public string RequestId { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("data")]
        public JObject Data { get; set; }

        public static CloudEnvelope Create(string type, string requestId, object data)
        {
            return new CloudEnvelope
            {
                Type = type,
                RequestId = string.IsNullOrWhiteSpace(requestId) ? Guid.NewGuid().ToString() : requestId,
                Timestamp = DateTimeOffset.Now.ToString("o"),
                Data = data == null ? new JObject() : JObject.FromObject(data)
            };
        }
    }

    internal class CloudReportTask
    {
        [JsonProperty("taskId")]
        public string TaskId { get; set; }

        [JsonProperty("cmd")]
        public string Cmd { get; set; }

        [JsonProperty("templateUrl")]
        public string TemplateUrl { get; set; }

        [JsonProperty("dataUrl")]
        public string DataUrl { get; set; }

        [JsonProperty("printerId")]
        public string PrinterId { get; set; }

        [JsonProperty("printerName")]
        public string PrinterName { get; set; }

        [JsonProperty("extInfo")]
        public JObject ExtInfo { get; set; }
    }

    internal class CloudTaskResult
    {
        public string TaskId { get; private set; }
        public string Cmd { get; private set; }
        public bool Success { get; private set; }
        public string Message { get; private set; }
        public string ErrorCode { get; private set; }
        public object Result { get; private set; }

        public static CloudTaskResult Ok(string taskId, string cmd, string message, object result = null)
        {
            return new CloudTaskResult
            {
                TaskId = taskId,
                Cmd = cmd,
                Success = true,
                Message = message,
                Result = result
            };
        }

        public static CloudTaskResult Fail(string taskId, string cmd, string message, string errorCode)
        {
            return new CloudTaskResult
            {
                TaskId = taskId,
                Cmd = cmd,
                Success = false,
                Message = message,
                ErrorCode = errorCode
            };
        }
    }
}

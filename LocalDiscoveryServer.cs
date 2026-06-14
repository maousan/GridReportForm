using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace GridReportForm
{
    /// <summary>
    /// Read-only local HTTP service that lets the web frontend discover the
    /// Cloud Device identity and printer list of the machine it is running on.
    /// See docs/adr/0009-add-local-device-discovery-service.md.
    /// Binds 127.0.0.1 only, validates request Origin against a configured
    /// allowlist, and exposes a single GET /discover endpoint with no
    /// side effects. Task delivery is not handled here.
    /// </summary>
    internal sealed class LocalDiscoveryServer : IDisposable
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private const string DiscoverPath = "/discover";

        private readonly int port;
        private readonly HashSet<string> allowedOrigins;
        private readonly Func<DiscoveryInfo> infoProvider;
        private HttpListener listener;
        private Thread worker;
        private volatile bool stopping;

        public LocalDiscoveryServer(int port, IEnumerable<string> allowedOrigins, Func<DiscoveryInfo> infoProvider)
        {
            this.port = port;
            this.allowedOrigins = new HashSet<string>(allowedOrigins ?? new string[0], StringComparer.OrdinalIgnoreCase);
            this.infoProvider = infoProvider;
        }

        public bool IsRunning { get; private set; }

        public void Start()
        {
            try
            {
                HttpListener newListener = new HttpListener();
                newListener.Prefixes.Add(string.Format("http://127.0.0.1:{0}/", port));
                newListener.Start();
                listener = newListener;
                IsRunning = true;
                stopping = false;
                worker = new Thread(RunLoop) { IsBackground = true, Name = "LocalDiscoveryServer" };
                worker.Start();
                logger.Info("Local discovery server started. Port={Port}, AllowedOrigins={AllowedOriginCount}, Prefix=127.0.0.1", port, allowedOrigins.Count);
                if (allowedOrigins.Count == 0)
                {
                    logger.Warn("Local discovery server has no allowed origins configured. Browser callers will be rejected until DiscoveryAllowedOrigins is set in config.ini.");
                }
            }
            catch (Exception exception)
            {
                logger.Warn(exception, "Local discovery server failed to start. Port={Port}. Device discovery will be unavailable. On Windows this may require a urlacl reservation (netsh http add urlacl url=http://127.0.0.1:{Port}/ user=Everyone).", port);
                TryCloseListener();
                IsRunning = false;
            }
        }

        public void Dispose()
        {
            stopping = true;
            TryCloseListener();
        }

        private void RunLoop()
        {
            HttpListener active = listener;
            while (!stopping && active != null)
            {
                HttpListenerContext context;
                try
                {
                    context = active.GetContext();
                }
                catch (HttpListenerException) when (stopping)
                {
                    break;
                }
                catch (Exception exception)
                {
                    if (!stopping)
                    {
                        logger.Warn(exception, "Local discovery server accept failed.");
                    }
                    break;
                }

                try
                {
                    Handle(context);
                }
                catch (Exception exception)
                {
                    logger.Warn(exception, "Local discovery server request handling failed.");
                }
            }
        }

        private void Handle(HttpListenerContext context)
        {
            string origin = context.Request.Headers["Origin"];
            bool hasOrigin = !string.IsNullOrEmpty(origin);
            bool allowed = OriginAllowed(origin);

            // CORS preflight
            if (hasOrigin && context.Request.HttpMethod == "OPTIONS")
            {
                if (allowed)
                {
                    ApplyCorsHeaders(context.Response, origin);
                    context.Response.Headers["Access-Control-Allow-Methods"] = "GET, OPTIONS";
                    context.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type";
                    context.Response.Headers["Access-Control-Max-Age"] = "86400";
                }
                context.Response.StatusCode = allowed ? (int)HttpStatusCode.OK : (int)HttpStatusCode.Forbidden;
                context.Response.Close();
                return;
            }

            if (!allowed)
            {
                logger.Warn("Local discovery request rejected by origin allowlist. Origin={Origin}, Path={Path}", origin ?? "(none)", context.Request.Url?.AbsolutePath);
                WriteJson(context.Response, HttpStatusCode.Forbidden, new { error = "origin not allowed" });
                return;
            }

            string path = context.Request.Url?.AbsolutePath ?? string.Empty;
            if (!string.Equals(path.TrimEnd('/'), DiscoverPath, StringComparison.OrdinalIgnoreCase))
            {
                WriteJson(context.Response, HttpStatusCode.NotFound, new { error = "not found" });
                return;
            }

            if (!string.Equals(context.Request.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase))
            {
                WriteJson(context.Response, HttpStatusCode.MethodNotAllowed, new { error = "method not allowed" });
                return;
            }

            DiscoveryInfo info = infoProvider() ?? new DiscoveryInfo();
            if (hasOrigin)
            {
                ApplyCorsHeaders(context.Response, origin);
            }
            WriteJson(context.Response, HttpStatusCode.OK, new
            {
                deviceId = info.DeviceId,
                deviceName = info.DeviceName,
                defaultPrinter = info.DefaultPrinter,
                printers = info.Printers ?? new List<string>(),
                online = info.Online
            });
        }

        private bool OriginAllowed(string origin)
        {
            // Non-browser callers (curl, same-machine tools) do not send an Origin header.
            if (string.IsNullOrEmpty(origin))
            {
                return true;
            }
            if (allowedOrigins.Count == 0)
            {
                return false;
            }
            if (allowedOrigins.Contains(origin))
            {
                return true;
            }
            // Host-based match: an entry like "127.0.0.1" or "http://biz.example.com"
            // matches any browser Origin sharing that host (any scheme/port), e.g.
            // Origin "http://127.0.0.1:8080" is matched by configured "127.0.0.1".
            string originHost = ExtractHost(origin);
            if (string.IsNullOrEmpty(originHost))
            {
                return false;
            }
            foreach (string allowed in allowedOrigins)
            {
                if (string.Equals(allowed, originHost, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                string allowedHost = ExtractHost(allowed);
                if (!string.IsNullOrEmpty(allowedHost)
                    && string.Equals(allowedHost, originHost, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private static string ExtractHost(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            value = value.Trim();
            if (Uri.TryCreate(value, UriKind.Absolute, out Uri uri))
            {
                return uri.Host;
            }
            // Bare host like "127.0.0.1" (no scheme) — return as-is.
            return value;
        }

        private static void ApplyCorsHeaders(HttpListenerResponse response, string origin)
        {
            response.Headers["Access-Control-Allow-Origin"] = origin;
            response.Headers["Vary"] = "Origin";
        }

        private static void WriteJson(HttpListenerResponse response, HttpStatusCode status, object body)
        {
            response.StatusCode = (int)status;
            response.ContentType = "application/json; charset=utf-8";
            byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body));
            response.ContentLength64 = bytes.Length;
            try
            {
                response.OutputStream.Write(bytes, 0, bytes.Length);
            }
            catch (HttpListenerException exception)
            {
                logger.Warn(exception, "Local discovery response write failed. Status={Status}", status);
            }
            finally
            {
                try { response.Close(); } catch (IOException) { }
            }
        }

        private void TryCloseListener()
        {
            IsRunning = false;
            HttpListener active = listener;
            listener = null;
            if (active != null)
            {
                try { active.Stop(); } catch (Exception exception) { logger.Warn(exception, "Local discovery listener stop failed."); }
                try { active.Close(); } catch (Exception exception) { logger.Warn(exception, "Local discovery listener close failed."); }
            }
        }
    }

    internal sealed class DiscoveryInfo
    {
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string DefaultPrinter { get; set; }
        public List<string> Printers { get; set; }
        public bool Online { get; set; }
    }
}

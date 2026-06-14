using System;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Threading.Tasks;

namespace GridReportForm
{
    internal class ApplicationUpdateService
    {
        private const string LatestReleaseUrl = "https://github.com/maousan/GridReportForm/releases/latest";
        private const string ReleaseDownloadBaseUrl = "https://github.com/maousan/GridReportForm/releases/download";
        private const string InstallerNamePrefix = "ReportHelperSetup-";
        private const string InstallerNameSuffix = ".exe";
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public async Task<ApplicationUpdateCheckResult> CheckLatestAsync()
        {
            Version currentVersion = GetCurrentVersion();
            logger.Info("Checking application update. CurrentVersion={CurrentVersion}, LatestReleaseUrl={LatestReleaseUrl}", currentVersion, LatestReleaseUrl);

            Uri latestReleaseUri = await ResolveLatestReleaseUriAsync();
            string tagName = ExtractTagName(latestReleaseUri);
            Version latestVersion;
            if (!TryParseReleaseVersion(tagName, out latestVersion))
            {
                return ApplicationUpdateCheckResult.Unavailable(currentVersion, $"GitHub Release tag 无法识别：{tagName}");
            }

            if (CompareVersions(latestVersion, currentVersion) <= 0)
            {
                logger.Info("Application is up to date. CurrentVersion={CurrentVersion}, LatestVersion={LatestVersion}", currentVersion, latestVersion);
                return ApplicationUpdateCheckResult.NoUpdate(currentVersion, latestVersion);
            }

            string expectedAssetName = $"{InstallerNamePrefix}{FormatVersion(latestVersion)}{InstallerNameSuffix}";
            string downloadUrl = $"{ReleaseDownloadBaseUrl}/{tagName}/{expectedAssetName}";
            if (!await AssetExistsAsync(downloadUrl))
            {
                logger.Warn("Latest release does not contain expected installer asset. LatestVersion={LatestVersion}, ExpectedAssetName={ExpectedAssetName}", latestVersion, expectedAssetName);
                return ApplicationUpdateCheckResult.Unavailable(currentVersion, $"未找到更新安装包：{expectedAssetName}", latestVersion);
            }

            logger.Info("Application update available. CurrentVersion={CurrentVersion}, LatestVersion={LatestVersion}, AssetName={AssetName}", currentVersion, latestVersion, expectedAssetName);
            return ApplicationUpdateCheckResult.Available(currentVersion, latestVersion, expectedAssetName, downloadUrl);
        }

        public async Task<string> DownloadInstallerAsync(ApplicationUpdateInfo update)
        {
            if (update == null)
            {
                throw new ArgumentNullException(nameof(update));
            }
            ValidateUpdateInfo(update);

            string downloadDirectory = Path.Combine(Path.GetTempPath(), "GridReportForm", "updates", FormatVersion(update.LatestVersion));
            Directory.CreateDirectory(downloadDirectory);
            string installerPath = Path.Combine(downloadDirectory, update.AssetName);
            logger.Info("Downloading update installer. Version={Version}, Url={Url}, InstallerPath={InstallerPath}", update.LatestVersion, update.DownloadUrl, installerPath);

            if (File.Exists(installerPath))
            {
                File.Delete(installerPath);
            }

            using (WebClient client = CreateGitHubClient())
            {
                await client.DownloadFileTaskAsync(update.DownloadUrl, installerPath);
            }

            FileInfo fileInfo = new FileInfo(installerPath);
            if (!fileInfo.Exists || fileInfo.Length <= 0)
            {
                throw new MessageHandleException("安装包下载失败，文件为空");
            }

            logger.Info("Update installer downloaded. Version={Version}, InstallerPath={InstallerPath}, Size={Size}", update.LatestVersion, installerPath, fileInfo.Length);
            return installerPath;
        }

        private static WebClient CreateGitHubClient()
        {
            WebClient client = new WebClient();
            client.Encoding = System.Text.Encoding.UTF8;
            client.Headers[HttpRequestHeader.UserAgent] = "GridReportForm";
            return client;
        }

        private static async Task<Uri> ResolveLatestReleaseUriAsync()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(LatestReleaseUrl);
            request.AllowAutoRedirect = false;
            request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
            request.Headers[HttpRequestHeader.CacheControl] = "no-cache";
            request.Headers[HttpRequestHeader.Pragma] = "no-cache";
            request.Method = "GET";
            request.UserAgent = "GridReportForm";
            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            {
                string location = response.Headers[HttpResponseHeader.Location];
                if (string.IsNullOrWhiteSpace(location))
                {
                    return response.ResponseUri;
                }
                return new Uri(location);
            }
        }

        private static async Task<bool> AssetExistsAsync(string downloadUrl)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(downloadUrl);
                request.AllowAutoRedirect = false;
                request.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);
                request.Headers[HttpRequestHeader.CacheControl] = "no-cache";
                request.Headers[HttpRequestHeader.Pragma] = "no-cache";
                request.Method = "HEAD";
                request.UserAgent = "GridReportForm";
                using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    return response.StatusCode == HttpStatusCode.OK
                        || response.StatusCode == HttpStatusCode.Found
                        || response.StatusCode == HttpStatusCode.Redirect;
                }
            }
            catch (WebException exception)
            {
                HttpWebResponse response = exception.Response as HttpWebResponse;
                if (response != null && response.StatusCode == HttpStatusCode.NotFound)
                {
                    return false;
                }
                throw;
            }
        }

        private static string ExtractTagName(Uri releaseUri)
        {
            string marker = "/releases/tag/";
            string path = releaseUri.AbsolutePath;
            int index = path.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                return null;
            }
            return Uri.UnescapeDataString(path.Substring(index + marker.Length));
        }

        private static Version GetCurrentVersion()
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);
            return new Version(version.Major, version.Minor, version.Build < 0 ? 0 : version.Build);
        }

        private static bool TryParseReleaseVersion(string tagName, out Version version)
        {
            version = null;
            if (string.IsNullOrWhiteSpace(tagName))
            {
                return false;
            }

            string value = tagName.Trim();
            if (value.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring(1);
            }
            return Version.TryParse(value, out version) && version.Major >= 0 && version.Minor >= 0 && version.Build >= 0;
        }

        private static int CompareVersions(Version left, Version right)
        {
            Version normalizedLeft = new Version(left.Major, left.Minor, left.Build < 0 ? 0 : left.Build);
            Version normalizedRight = new Version(right.Major, right.Minor, right.Build < 0 ? 0 : right.Build);
            return normalizedLeft.CompareTo(normalizedRight);
        }

        private static void ValidateUpdateInfo(ApplicationUpdateInfo update)
        {
            string expectedAssetName = $"{InstallerNamePrefix}{FormatVersion(update.LatestVersion)}{InstallerNameSuffix}";
            if (!string.Equals(update.AssetName, expectedAssetName, StringComparison.OrdinalIgnoreCase))
            {
                throw new MessageHandleException($"安装包名称不匹配：{update.AssetName}");
            }
            if (string.IsNullOrWhiteSpace(update.DownloadUrl))
            {
                throw new MessageHandleException("安装包下载地址为空");
            }
            Uri uri = new Uri(update.DownloadUrl);
            if (!string.Equals(uri.Host, "github.com", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(uri.Host, "objects.githubusercontent.com", StringComparison.OrdinalIgnoreCase)
                && !uri.Host.EndsWith(".githubusercontent.com", StringComparison.OrdinalIgnoreCase))
            {
                throw new MessageHandleException("安装包下载地址不是 GitHub Release 地址");
            }
        }

        public static string FormatVersion(Version version)
        {
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }
    }

    internal class ApplicationUpdateCheckResult
    {
        public ApplicationUpdateStatus Status { get; private set; }
        public Version CurrentVersion { get; private set; }
        public Version LatestVersion { get; private set; }
        public ApplicationUpdateInfo Update { get; private set; }
        public string Message { get; private set; }

        public static ApplicationUpdateCheckResult NoUpdate(Version currentVersion, Version latestVersion)
        {
            return new ApplicationUpdateCheckResult
            {
                Status = ApplicationUpdateStatus.NoUpdate,
                CurrentVersion = currentVersion,
                LatestVersion = latestVersion
            };
        }

        public static ApplicationUpdateCheckResult Available(Version currentVersion, Version latestVersion, string assetName, string downloadUrl)
        {
            return new ApplicationUpdateCheckResult
            {
                Status = ApplicationUpdateStatus.Available,
                CurrentVersion = currentVersion,
                LatestVersion = latestVersion,
                Update = new ApplicationUpdateInfo
                {
                    CurrentVersion = currentVersion,
                    LatestVersion = latestVersion,
                    AssetName = assetName,
                    DownloadUrl = downloadUrl
                }
            };
        }

        public static ApplicationUpdateCheckResult Unavailable(Version currentVersion, string message, Version latestVersion = null)
        {
            return new ApplicationUpdateCheckResult
            {
                Status = ApplicationUpdateStatus.Unavailable,
                CurrentVersion = currentVersion,
                LatestVersion = latestVersion,
                Message = message
            };
        }
    }

    internal class ApplicationUpdateInfo
    {
        public Version CurrentVersion { get; set; }
        public Version LatestVersion { get; set; }
        public string AssetName { get; set; }
        public string DownloadUrl { get; set; }
    }

    internal enum ApplicationUpdateStatus
    {
        NoUpdate,
        Available,
        Unavailable
    }
}

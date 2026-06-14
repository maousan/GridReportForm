// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GridReportForm
{
    internal class HttpClientUtils
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void DownloadFile(string url, string filePath)
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    logger.Info("Downloading file. Url={Url}, FilePath={FilePath}", url, filePath);
                    client.DownloadFile(url, filePath);
                    logger.Info("File downloaded. Url={Url}, FilePath={FilePath}", url, filePath);
                }
                catch (Exception exception)
                {
                    logger.Error(exception, "File download failed. Url={Url}, FilePath={FilePath}", url, filePath);
                    throw;
                }
            }
        }
    }
}



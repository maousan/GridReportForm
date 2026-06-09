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
        public static void DownloadFile(string url, string filePath)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(url, filePath);
            }
        }
    }
}



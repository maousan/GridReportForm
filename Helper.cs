using Microsoft.Win32;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GridReportForm
{
    internal class Helper
    {
        public static void Delay(double time)
        {
            double strart = Environment.TickCount;
            while (Math.Abs(Environment.TickCount - strart) < time)
            {
                Application.DoEvents();
            }
        }

        public static bool AlreadyRun()
        {
            bool flag = false;
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                flag = true;
            }
            return flag;

        }
        public static string GetLocalIP()
        {
            string str = string.Empty;
            foreach (IPAddress address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                AddressFamily addressFamily = address.AddressFamily;
                if (addressFamily.ToString() == "InterNetwork")
                {
                    str = address.ToString();
                }
            }
            return str;

        }
        public static bool PortInUse(int port)
        {
            bool flag = false;
            IPEndPoint[] activeTcpListeners = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners();
            int index = 0;
            while (true)
            {
                if (index < activeTcpListeners.Length)
                {
                    if (activeTcpListeners[index].Port != port)
                    {
                        index++;
                        continue;
                    }
                    flag = true;
                }
                return flag;
            }

        }
        public static string StreamToBase64(Stream stream)
        {
            stream.Position = 0L;
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            stream.Seek(0L, SeekOrigin.Begin);
            return Convert.ToBase64String(buffer);

        }
        public static string TokenDecode(string product, string str)
        {
            string str2;
            try
            {
                if (product == "report")
                {
                    byte[] bytes = Convert.FromBase64String("LS0tLS1CRUdJTiBQVUJMSUMgS0VZLS0tLS0KTUlHZk1BMEdDU3FHU0liM0RRRUJBUVVBQTRHTkFEQ0JpUUtCZ1FEZktIWGJJUm9KbFJWZmJSRXFzdU5MUGwzNgpRSUZGcnl1ZlFOd0gzVWs3M0F3MFQ0d25rQlM1QWp5eGsvVFdXVUNxVFk2Q3o0UkFSaHhreWhmZjJCT2tJb3pnCmxEN1VNV3ZRMjFnNWJzUVAzRmI0MjNXajRPenl2ZDJjRGcxRkpxc1NqdGtnQzUxY3B5dkk5bCtYRTVJSklnSEYKVndNa0hKbGdvTG04QXBzcnNRSURBUUFCCi0tLS0tRU5EIFBVQkxJQyBLRVktLS0tLQ==");
                    AsymmetricKeyParameter parameters = (AsymmetricKeyParameter)new PemReader(new StringReader(Encoding.UTF8.GetString(bytes))).ReadObject();
                    IBufferedCipher cipher = CipherUtilities.GetCipher("RSA/ECB/PKCS1Padding");
                    cipher.Init(false, parameters);
                    byte[] buffer3 = cipher.DoFinal(Convert.FromBase64String(str));
                    str2 = Encoding.UTF8.GetString(buffer3);
                }
                else if (product != "erp")
                {
                    str2 = "";
                }
                else
                {
                    byte[] bytes = Convert.FromBase64String("LS0tLS1CRUdJTiBQVUJMSUMgS0VZLS0tLS0KTUlHZk1BMEdDU3FHU0liM0RRRUJBUVVBQTRHTkFEQ0JpUUtCZ1FEUG5CUXBRYVdSM1BiWS9SaGlDTWhlZDViZgpZYitlY1VOMHVzaHc5THBKZlYwdjYxMU9zeUNaQ1pIRWROOVh0MFB4a3JtcWRlUTc4OWVJY1FsU0VSbFZwTW5JClI0WXk3SkVHUWtOTU0vTG1pSWQ5MEpVam9KMzY0WWxpU0FHM2ozSDVGQXZyeitLVjBxNS9HRnhMRDBCR2l6VXoKdUVhS2FORGRlbUNWZXdWUHl3SURBUUFCCi0tLS0tRU5EIFBVQkxJQyBLRVktLS0tLQ==");
                    AsymmetricKeyParameter parameters = (AsymmetricKeyParameter)new PemReader(new StringReader(Encoding.UTF8.GetString(bytes))).ReadObject();
                    IBufferedCipher cipher = CipherUtilities.GetCipher("RSA/ECB/PKCS1Padding");
                    cipher.Init(false, parameters);
                    byte[] buffer6 = cipher.DoFinal(Convert.FromBase64String(str));
                    str2 = Encoding.UTF8.GetString(buffer6);
                }
            }
            catch (Exception)
            {
                str2 = "error";
            }
            return str2;

        }

        /// <summary>
        /// 将本程序设为开启自启
        /// </summary>
        /// <param name="onOff">自启开关</param>
        /// <returns></returns>
        public static bool SetMeStart(bool onOff)
        {
            bool isOk = false;
            string appName = Process.GetCurrentProcess().MainModule.ModuleName;
            string appPath = Process.GetCurrentProcess().MainModule.FileName;
            isOk = SetAutoStart(onOff, appName, appPath);
            return isOk;
        }

        /// <summary>
        /// 将应用程序设为或不设为开机启动
        /// </summary>
        /// <param name="onOff">自启开关</param>
        /// <param name="appName">应用程序名</param>
        /// <param name="appPath">应用程序完全路径</param>
        public static bool SetAutoStart(bool onOff, string appName, string appPath)
        {
            bool isOk = true;
            //如果从没有设为开机启动设置到要设为开机启动
            if (!IsExistKey(appName) && onOff)
            {
                isOk = SelfRunning(onOff, appName, @appPath);
            }
            //如果从设为开机启动设置到不要设为开机启动
            else if (IsExistKey(appName) && !onOff)
            {
                isOk = SelfRunning(onOff, appName, @appPath);
            }
            return isOk;
        }

        /// <summary>
        /// 判断注册键值对是否存在，即是否处于开机启动状态
        /// </summary>
        /// <param name="keyName">键值名</param>
        /// <returns></returns>
        private static bool IsExistKey(string keyName)
        {
            try
            {
                bool _exist = false;
                RegistryKey local = Registry.LocalMachine;
                RegistryKey runs = local.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (runs == null)
                {
                    RegistryKey key2 = local.CreateSubKey("SOFTWARE");
                    RegistryKey key3 = key2.CreateSubKey("Microsoft");
                    RegistryKey key4 = key3.CreateSubKey("Windows");
                    RegistryKey key5 = key4.CreateSubKey("CurrentVersion");
                    RegistryKey key6 = key5.CreateSubKey("Run");
                    runs = key6;
                }
                string[] runsName = runs.GetValueNames();
                foreach (string strName in runsName)
                {
                    if (strName.ToUpper() == keyName.ToUpper())
                    {
                        _exist = true;
                        return _exist;
                    }
                }
                return _exist;

            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 写入或删除注册表键值对,即设为开机启动或开机不启动
        /// </summary>
        /// <param name="isStart">是否开机启动</param>
        /// <param name="exeName">应用程序名</param>
        /// <param name="path">应用程序路径带程序名</param>
        /// <returns></returns>
        private static bool SelfRunning(bool isStart, string exeName, string path)
        {
            try
            {
                RegistryKey local = Registry.LocalMachine;
                RegistryKey key = local.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (key == null)
                {
                    local.CreateSubKey("SOFTWARE//Microsoft//Windows//CurrentVersion//Run");
                }
                //若开机自启动则添加键值对
                if (isStart)
                {
                    key.SetValue(exeName, path);
                    key.Close();
                }
                else//否则删除键值对
                {
                    string[] keyNames = key.GetValueNames();
                    foreach (string keyName in keyNames)
                    {
                        if (keyName.ToUpper() == exeName.ToUpper())
                        {
                            key.DeleteValue(exeName);
                            key.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string ss = ex.Message;
                return false;
                //throw;
            }

            return true;
        }

    }

}

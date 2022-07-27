using System;
using System.IO;
using System.Management;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SmartCar
{
    static class TheveVerify
    {
        private static byte[] _KEY = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };
        private static byte[] _IV = new byte[] { 0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01 };
        /// <summary>
        /// 验证过程
        /// </summary>
        /// <param name="concents">CID</param>
        /// <param name="code">输入的序列号</param>
        /// <returns></returns>
        public static bool Verify(string concents, string code)
        {
            string sor = concents + "3";
            DESCryptoServiceProvider cp = new DESCryptoServiceProvider();
            MemoryStream ms = new MemoryStream();
            CryptoStream cst = new CryptoStream(ms, cp.CreateEncryptor(_KEY, _IV), CryptoStreamMode.Write);
            StreamWriter sw = new StreamWriter(cst);
            sw.Write(sor);
            sw.Flush();
            cst.FlushFinalBlock();
            sw.Flush();
            string strRet = Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
            if (strRet == code)
            {
                TheveIniFiles.IniWriteValue("验证", "CID", concents);
                TheveIniFiles.IniWriteValue("验证", "序列号", code);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取INI文件的序列号进行验证
        /// </summary>
        /// <returns>验证成功返回true</returns>
        public static bool VerifyIniFile()
        {
            if (TheveIniFiles.ExistINIFile() == true)
            {
                if (TheveIniFiles.GetItemsKeys("验证") != null)
                {
                    string cid = GetHardInfo();
                    string num = TheveIniFiles.IniReadValue("验证", "序列号");
                    if (Verify(cid, num) == true)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static bool VerifyLoad()
        {
            return VerifyIniFile();
        }
        /// <summary>
        /// 获取CPU序列号
        /// </summary>
        /// <returns></returns>
        private static string GetCPUSerialNumber()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Processor");
                string sCPUSerialNumber = "";
                foreach (ManagementObject mo in searcher.Get())
                {
                    sCPUSerialNumber = mo["ProcessorId"].ToString().Trim();
                    break;
                }
                return sCPUSerialNumber;
            }
            catch
            {
                return "";
            }
        }
        /// <summary>
        /// 获取UUID
        /// </summary>
        /// <returns></returns>
        private static string GetHardUUID()
        {
            string code = null;
            SelectQuery query = new SelectQuery("select * from Win32_ComputerSystemProduct");
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (var item in searcher.Get())
                {
                    using (item) code = item["UUID"].ToString();
                }
            }
            return code.Replace("-", "");
        }
        /// <summary>
        /// 获取数字化电脑硬件信息
        /// </summary>
        /// <returns>数子序列号</returns>
        public static string GetHardInfo()
        {
            int rand = 10;
            //获取UUID缩位和
            string SNID = GetCPUSerialNumber();
            string UUID = GetHardUUID();
            UUID = Regex.Replace(UUID, @"[^1-9]+", "");
            string nonNumUUID = UUID;
            while (rand > 9)
            {
                rand = 0;
                foreach (char i in nonNumUUID)
                {
                    rand += Convert.ToInt32(i) - 48;
                }
                nonNumUUID = rand.ToString();
            }
            string newUUID = "";
            for (int i = 0; i < UUID.Length; i++)
            {
                newUUID += UUID[i] + rand - 48;
            }
            string newSNID = "";
            for (int i = 0; i < SNID.Length; i++)
            {
                newSNID += SNID[i] + rand - 48;
            }
            return rand + newSNID + newUUID;
        }



    }
}
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace SmartCar
{
    public static class TheveIniFiles
    {
        private static string inipath;
        /// <summary>
        /// 初始化保存路径
        /// </summary>
        /// <param name="path">默认为相对路径</param>
        public static void InitPath(string path)
        {
            inipath = path;
        }

        //声明API函数
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        /// <summary>
        /// 将指定的键值对写到指定的节点，如果已经存在则替换。
        /// </summary>
        /// <param name="section">节点，如果不存在此节点，则创建此节点</param>
        /// <param name="Item">Item键值对，多个用\0分隔,形如key1=value1\0key2=value2
        /// <para>如果为string.Empty，则删除指定节点下的所有内容，保留节点</para>
        /// <para>如果为null，则删除指定节点下的所有内容，并且删除该节点</para>
        /// </param>
        /// <param name="path">INI文件</param>
        /// <returns>是否成功写入</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool WritePrivateProfileSection(string section, string Item, string path);

        /// <summary>
        /// 读取INI文件中指定的Key的值
        /// </summary>
        /// <param name="section">节点名称。如果为null,则读取INI中所有节点名称,每个节点名称之间用\0分隔</param>
        /// <param name="key">Key名称。如果为null,则读取INI中指定节点中的所有KEY,每个KEY之间用\0分隔</param>
        /// <param name="Default">读取失败时的默认值</param>
        /// <param name="retVal">读取的内容缓冲区，读取之后，多余的地方使用\0填充</param>
        /// <param name="size">内容缓冲区的长度</param>
        /// <param name="filePath">INI文件名</param>
        /// <returns>实际读取到的长度</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetPrivateProfileString(string AppName, string KeyName, string Default, [In, Out] char[] ReturnedString, uint Size, string FileName);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string Default, StringBuilder retVal, int size, string filePath);

        /// <summary> 
        /// 写入INI文件 
        /// </summary> 
        /// <param name="Section">项目名称(如 [TypeName] )</param> 
        /// <param name="Key">键</param> 
        /// <param name="Value">值</param> 
        public static void IniWriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, inipath);
        }
        /// <summary> 
        /// 读出INI文件 
        /// </summary> 
        /// <param name="Section">项目名称(如 [TypeName] )</param> 
        /// <param name="Key">键</param> 
        public static string IniReadValue(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(500);
            int i = GetPrivateProfileString(Section, Key, "", temp, 500, inipath);
            return temp.ToString();
        }
        /// <summary> 
        /// 验证文件是否存在 
        /// </summary> 
        /// <returns>布尔值</returns> 
        public static bool ExistINIFile()
        {
            return File.Exists(inipath);
        }

        /// <summary>
        /// 获取INI文件中指定节点(Section)中的所有条目的Key列表
        /// </summary>
        /// <param name="section">节点名称</param>
        /// <returns>如果没有内容,反回null</returns>
        public static string[] GetItemsKeys(string section)
        {
            char[] chars = new char[10240];
            uint byteNum = GetPrivateProfileString(section, null, null, chars, 10240, inipath);
            if (byteNum != 0)
            {
                return new string(chars).Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 删除指定节点中的所有内容。
        /// </summary>
        /// <param name="iniFile">INI文件</param>
        /// <param name="section">节点</param>
        /// <returns>操作是否成功</returns>
        public static bool DeleteSection(string section)
        {
            return WritePrivateProfileSection(section, string.Empty, inipath);
        }
        /// <summary>
        /// 初始化载入INI
        /// </summary>
        /// <param name="comboBox"></param>
        public static void IniLoad(ComboBox comboBox)
        {
            if (ExistINIFile() == true)
            {
                string[] iniNameArray = GetItemsKeys("端口另起名");
                if (iniNameArray != null)
                {
                    foreach (string iniName in iniNameArray)
                    {
                        int index = comboBox.Items.IndexOf(iniName);
                        if (index >= 0)
                        {
                            comboBox.Items[index] += IniReadValue("端口另起名", iniName.ToString());
                            comboBox.SelectedItem = comboBox.Items[index];
                        }
                    }
                }
            }
        }

    }
}

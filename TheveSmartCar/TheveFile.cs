using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TheveSmartCar
{
    static class TheveFile
    {
        /// <summary>
        /// 弹出窗口选择文件夹
        /// </summary>
        /// <returns>文件夹绝对路径</returns>
        public static string SelectDirection()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "请选择文件夹：";
            folderBrowserDialog.RootFolder = Environment.SpecialFolder.Desktop;
            folderBrowserDialog.ShowNewFolderButton = true;
            folderBrowserDialog.ShowDialog();
            string path = folderBrowserDialog.SelectedPath;
            folderBrowserDialog.Dispose();
            return path;
        }

        /// <summary>
        /// 选择文件(.bmp)
        /// </summary>
        /// <param name="basePath">设置打开的基路径</param>
        /// <returns>文件绝对路径</returns>
        public static string SelectFile(string basePath = @"D:")
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "请选择一个文件";
            openFileDialog.Filter = "位图文件|*.bmp";
            openFileDialog.InitialDirectory = basePath;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.ShowDialog();
            string path = openFileDialog.FileName;
            openFileDialog.Dispose();
            return path;
        }

        /// <summary>
        /// 获取文件夹内全部指定类型文件(.bmp)的路径
        /// </summary>
        /// <param name="path">文件夹的绝对路径</param>
        /// <returns>文件夹内的文件的绝对路径的列表</returns>
        public static List<string> GetAllFilePath(string path)
        {
            List<string> fileList = new List<string>();
            string filename = String.Empty;
            DirectoryInfo dir = new DirectoryInfo(path);
            FileInfo[] file = dir.GetFiles();
            Array.Sort(file, new Asort());
            foreach (FileInfo f in file)
            {
                filename = f.FullName;
                if (filename.EndsWith("bmp") || filename.EndsWith("BMP"))//判断文件后缀，并获取指定格式的文件全路径增添至fileList
                {
                    fileList.Add(filename);
                }
            }
            return fileList;
        }

    }
    /// <summary>
    /// 按照数字排序
    /// </summary>
    /// <param name="param1"></param>
    /// <param name="param2"></param>
    /// <returns></returns>
    class Asort : IComparer
    {
        [System.Runtime.InteropServices.DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern int StrCmpLogicalW(string param1, string param2);
        public int Compare(object name1, object name2)
        {
            if (name1 == null || name2 == null)
            {
                return -1;
            }
            else
            {
                return StrCmpLogicalW(name1.ToString(), name2.ToString());
            }
        }
    }
}

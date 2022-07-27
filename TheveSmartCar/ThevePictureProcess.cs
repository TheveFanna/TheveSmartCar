
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ThevePictureProcessDll;

namespace SmartCar
{
    static class ThevePictureProcess
    {
        private static int _height = 120;
        private static int _Width = 188;
        private static int _picCount = 0;
        private static int _SetWhite = 255;
        public static string error = "未知错误";
        public static string Log = "正常";
        public static string Resoultion = "";
        public static string Depth = "";
        public static bool RunCodeFlag = false;
        public static bool CodeLock = false;
        public static bool AutoPlay = false;
        public static bool ChangeFlag = false;
        public static bool antiColorFlag = false;
        public static bool modeFlag = true;

        public static int Height { get => _height; set => _height = value; }
        public static int Width { get => _Width; set => _Width = value; }
        public static int PicCount { get => _picCount; set => _picCount = value; }
        public static int SetWhite { get => _SetWhite; set => _SetWhite = value; }

        //初始化时间间隔
        public static void InitInterval(ComboBox comboBoxAutoInterval)
        {
            for (double i = 1.0; i > 0.02; i -= 0.01)
            {
                comboBoxAutoInterval.Items.Add(i.ToString("f2"));
            }
            comboBoxAutoInterval.SelectedItem = "0.50";
        }
        /// <summary>
        /// 选择文件夹
        /// </summary>
        /// <returns>文件绝对路径</returns>
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
        /// 选择文件
        /// </summary>
        /// <param name="basePath">设置打开的基路径</param>
        /// <returns>文件绝对路径</returns>
        public static string SelectFile(string basePath = @"D:")
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "请选择一个BMP图片的文件作为初始图片";
            openFileDialog.Filter = "位图文件|*.bmp";
            openFileDialog.InitialDirectory = basePath;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.ShowDialog();
            string path = openFileDialog.FileName;
            openFileDialog.Dispose();
            return path;

        }

        /// <summary>
        /// 获取文件夹内全部文件路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<string> GetAllPicPath(string path)
        {
            List<string> fileList = new List<string>();
            string filename;
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

        /// <summary>
        /// 将BMP转化为数组
        /// </summary>
        /// <param name="path">BMP所在的路径</param>
        /// <returns>成功有否，false则打印日志在error中</returns>
        public static bool GetBMP(string path)
        {
            try
            {
                Bitmap bmp = new Bitmap(path);
                Width = bmp.Width;
                Height = bmp.Height;
                Resoultion = "分辨率：" + Width.ToString() + "x" + Height.ToString();
                Depth = "位深度：" + Regex.Replace(bmp.PixelFormat.ToString(), @"[^0-9]+", "");
                Stream stream = File.OpenRead(path);
                int bmpCheck = stream.ReadByte();
                bmpCheck = stream.ReadByte() << 8 | bmpCheck;
                if (bmpCheck != 0x4d42)
                {
                    MessageBox.Show("这不是BMP图片");
                    return false;
                }
                stream.Position = 10;
                int bmphead = stream.ReadByte();
                bmphead = stream.ReadByte() << 8 | bmphead;
                stream.Position = bmphead;
                int supple = Width % 4;

                switch (bmp.PixelFormat)
                {
                    case PixelFormat.Format1bppIndexed:
                        break;
                    case PixelFormat.Format4bppIndexed:
                        break;
                    case PixelFormat.Format8bppIndexed:
                        for (int i = Height - 1; i >= 0; i--)
                        {
                            for (int j = 0; j < Width; j++)
                            {
                                int a = stream.ReadByte();
                                PicPro.img[i, j] = a;
                            }
                            for (int ii = 0; ii < supple; ii++)
                            {
                                stream.ReadByte();
                            }
                        }
                        break;
                    case PixelFormat.Format24bppRgb:
                        for (int i = Height - 1; i >= 0; i--)
                        {
                            for (int j = 0; j < Width; j++)
                            {
                                int a = stream.ReadByte();
                                PicPro.img[i, j] = a;
                                stream.ReadByte();
                                stream.ReadByte();
                            }
                            for (int ii = 0; ii < supple; ii++)
                            {
                                stream.ReadByte();
                            }
                        }
                        break;
                    default:
                        MessageBox.Show("不能识别的图像类型。");
                        break;
                }


                //int length = 3 * bmp.Width * bmp.Height;
                //byte[] bitData = new byte[length];
                //BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                //IntPtr Scan0 = data.Scan0;
                //Marshal.Copy(Scan0, bitData, 0, length);
                //bmp.UnlockBits(data);

                //for (int i = 0; i < bmp.Height; i++)
                //{
                //    for (int j = 0; j < bmp.Width; j++)
                //    {
                //        PicPro.img[i, j] = bitData[3 * (bmp.Width * i + j)];
                //    }

                //}
                bmp.Dispose();
                stream.Dispose();
                //保存原始数据
                PicPro.imgOriginal = PicPro.img;
                error = "";
            }
            catch
            {
                error = "请检查：\n是否最后一张\n图片名是否连续。";
            }

            return true;
        }

        /// <summary>
        /// 动态编译并运行代码
        /// </summary>
        /// <param name="codes">代码内容</param>
        static object obj = new object();

        static string start = @"using ThevePictureProcessDll; using System.Windows.Forms;using System; public class Test { public void Hello() {";
        static string end = "}}";
        static string start2 = @"using ThevePictureProcessDll; using System.Windows.Forms;using System; public class PicProSpace { ";
        static string end2 = "}";
        public static bool RunCode(string codes)
        {
            TempFileCollection tc = new TempFileCollection();
            CompilerResults cr = new CompilerResults(tc);
            if (RunCodeFlag == false)//第一次执行需要编译
            {
                CSharpCodeProvider cs = new CSharpCodeProvider();
#pragma warning disable CS0618 // 类型或成员已过时
                ICodeCompiler ic = cs.CreateCompiler();
#pragma warning restore CS0618 // 类型或成员已过时
                CompilerParameters cp = new CompilerParameters();
                cp.ReferencedAssemblies.Add("System.dll"); //添加需要引用的dll
                cp.ReferencedAssemblies.Add("ThevePictureProcessDll.dll");
                cp.ReferencedAssemblies.Add("System.Drawing.dll");
                cp.ReferencedAssemblies.Add("System.Windows.Forms.dll");
                cp.GenerateExecutable = false; //是否生成可执行文件
                cp.GenerateInMemory = true;//是否生成在内存中
                codes = codes.Replace("][", ",");
                if (modeFlag == true)
                {
                    cr = ic.CompileAssemblyFromSource(cp, start2 + codes + end2);
                }
                else
                {
                    cr = ic.CompileAssemblyFromSource(cp, start + codes + end);
                }
                //cr = ic.CompileAssemblyFromSource(cp,  codes  );
                obj = cr;
                RunCodeFlag = true;

            }
            cr = (CompilerResults)obj;
            if (cr.Errors.HasErrors)
            {
                Log = "编译错误：" + string.Join(Environment.NewLine, cr.Errors.Cast<CompilerError>().Select(err => err.ErrorText));
                return false;
            }
            else
            {
                try
                {
                    Assembly objAssembly = cr.CompiledAssembly;
                    object objHelloWorld = objAssembly.CreateInstance("PicProSpace");
                    MethodInfo objMI = objHelloWorld.GetType().GetMethod("PicProMain");
                    objMI.Invoke(objHelloWorld, null);
                    Log = " 完成。";
                    return true;
                }
                catch
                {
                    Log = "数组越界。";
                    return false;
                }
            }
        }

        /// <summary>
        /// 显示处理后图片
        /// </summary>
        /// <returns></returns> 
        public static Bitmap ShowBMP(bool hide = false)
        {
            Bitmap bmpShow = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            for (int i = 0; i < bmpShow.Height; i++)
            {
                for (int j = 0; j < bmpShow.Width; j++)
                {
                    int a = PicPro.img[i, j];
                    a = a < 0 ? 0 : a;
                    switch (a)
                    {
                        case 255 + 1:
                            bmpShow.SetPixel(j, i, Color.Red);
                            break;
                        case 255 + 2:
                            bmpShow.SetPixel(j, i, Color.Green);
                            break;
                        case 255 + 3:
                            bmpShow.SetPixel(j, i, Color.Blue);
                            break;
                        case 255 + 4:
                            bmpShow.SetPixel(j, i, Color.Yellow);
                            break;
                        case 255 + 5:
                            bmpShow.SetPixel(j, i, Color.Orange);
                            break;
                        case 255 + 6:
                            bmpShow.SetPixel(j, i, Color.Cyan);
                            break;
                        case 255 + 7:
                            bmpShow.SetPixel(j, i, Color.Purple);
                            break;
                        case 255 + 8:
                            bmpShow.SetPixel(j, i, Color.Pink);
                            break;
                        case 255 + 9:
                            bmpShow.SetPixel(j, i, Color.Magenta);
                            break;
                        default:
                            if (hide == false)//隐藏原图，只有彩色
                            {
                                if (antiColorFlag == true)
                                {
                                    if (a == 0)
                                    {
                                        bmpShow.SetPixel(j, i, Color.White);
                                    }
                                    else
                                    {
                                        bmpShow.SetPixel(j, i, Color.Black);
                                    }
                                }
                                else
                                {
                                    bmpShow.SetPixel(j, i, Color.FromArgb(a, a, a));
                                }
                            }
                            else
                            {
                                bmpShow.SetPixel(j, i, Color.White);
                            }
                            break;
                    }
                }
            }
            return bmpShow;
        }
        /// <summary>
        /// PictureBox取消平滑模糊方法
        /// </summary>
        public static void PaintWithoutFuzzy(PictureBox pictureBox, PaintEventArgs e)
        {
            var pic = pictureBox;
            if (pic.Image == null)
                return;
            var state = e.Graphics.Save();
            e.Graphics.SmoothingMode = SmoothingMode.None;
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.Clear(pictureBox.BackColor);
            e.Graphics.DrawImage(pic.Image, new Rectangle(0, 0, pic.Width, pic.Height), new Rectangle(0, 0, pic.Image.Width, pic.Image.Height), GraphicsUnit.Pixel);
            e.Graphics.Restore(state);
        }
        /// <summary>
        /// 自动匹配等宽长比
        /// </summary>
        /// <param name="pictureShow">图片框</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void AdaptSize(PictureBox pictureShow, int x, int y)
        {
            if (x * Height - y * Width > 0)
            {
                pictureShow.Height = y;
                pictureShow.Width = (int)(y * (double)Width / Height);
            }
            else
            {
                pictureShow.Width = x;
                pictureShow.Height = (int)(x * (double)Height / Width);
            }
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

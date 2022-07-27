
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

namespace TheveSmartCar
{
    static class ThevePictureProcess
    {
        private static int _height = 120;
        private static int _Width = 188;
        private static int _picCount = 0;
        private static int _SetWhite = 255;
        public static string Log = "正常";
        public static string Resoultion = "";
        public static string Depth = "";
        public static bool isCompiledCode = false;
        public static bool AutoPlay = false;
        public static bool ChangeFlag = false;
        public static bool isAntiColor = false;
        public static bool isMultiFuncMode = true; 
        public enum EModeDisplay
        {
            Processed,
            Original,
            Trajectory
        }

        public static int Height { get => _height; set => _height = value; }
        public static int Width { get => _Width; set => _Width = value; }
        public static int PicCount { get => _picCount; set => _picCount = value; }
        public static int SetWhite { get => _SetWhite; set => _SetWhite = value; }

        //初始化时间间隔
        public static void InitInterval(ComboBox comboBoxAutoInterval)
        {
            for (double i = 1.0; i >= 0.01; i -= 0.01)
            {
                comboBoxAutoInterval.Items.Add(i.ToString("f2"));
            }
            comboBoxAutoInterval.SelectedItem = "0.50";
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
                //获取图像的基本信息
                Bitmap bmp = new Bitmap(path);
                Width = bmp.Width;
                Height = bmp.Height;
                Resoultion = "分辨率：" + Height.ToString() + "x" + Width.ToString();
                Depth = "位深度：" + Regex.Replace(bmp.PixelFormat.ToString(), @"[^0-9]+", "");
                //读取图像数据
                Stream stream = File.OpenRead(path);
                int bmpCheck = stream.ReadByte();
                bmpCheck = stream.ReadByte() << 8 | bmpCheck;
                //检查是否为BMP图像
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
                //获取数据
                PicPro.CreatImg(Height, Width);
                switch (bmp.PixelFormat)
                {
                    case PixelFormat.Format8bppIndexed:
                        for (int i = Height - 1; i >= 0; i--)
                        {
                            for (int j = 0; j < Width; j++)
                            {
                                int a = stream.ReadByte();
                                PicPro.img[i, j] = a;
                                PicPro.imgOriginal[i, j] = a;
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
                                PicPro.imgOriginal[i, j] = a;
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
                bmp.Dispose();
                stream.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 动态编译并运行代码
        /// </summary>
        /// <param name="codes">代码内容</param>
        static object obj = new object();
        static string startCodeSingle = @"using ThevePictureProcessDll; using System.Windows.Forms;using System; public class PicProSpace { public void PicProMain() {";
        static string endCodeSingle = "}}";
        static string startCodeMulti = @"using ThevePictureProcessDll; using System.Windows.Forms;using System; public class PicProSpace { ";
        static string endCodeMulti = "}";
        public static bool RunCode(string codes)
        {
            if (codes == "")
            {
                return true;
            }
            TempFileCollection tc = new TempFileCollection();
            CompilerResults cr = new CompilerResults(tc);
            if (isCompiledCode == false)//第一次执行需要编译
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
                //多函数模式或单函数模式
                if (isMultiFuncMode == true)
                {
                    cr = ic.CompileAssemblyFromSource(cp, startCodeMulti + codes + endCodeMulti);
                }
                else
                {
                    cr = ic.CompileAssemblyFromSource(cp, startCodeSingle + codes + endCodeSingle);
                }
                obj = cr;
                isCompiledCode = true;
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
                    object objSmartCar = objAssembly.CreateInstance("PicProSpace");
                    MethodInfo objMI = objSmartCar.GetType().GetMethod("PicProMain");
                    objMI.Invoke(objSmartCar, null);
                    Log = " 完成。";
                    return true;
                }
                catch (Exception ex)
                {
                    Log = ex.Message;
                    return false;
                }
            }
        }

        /// <summary>
        /// 显示处理后图片
        /// </summary>
        /// <returns></returns> 
        public static Bitmap ShowBMP(EModeDisplay modeDisplay = EModeDisplay.Processed)
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
                            switch (modeDisplay)
                            {
                                case EModeDisplay.Original://原图
                                case EModeDisplay.Processed://处理图
                                    if (isAntiColor == true)
                                    {
                                        a = 255 - a; 
                                    }
                                    bmpShow.SetPixel(j, i, Color.FromArgb(a, a, a));

                                    break;
                                case EModeDisplay.Trajectory://标记图
                                    bmpShow.SetPixel(j, i, Color.White);
                                    break;

                                default:
                                    break;
                            }
                            break;
                    }
                }
            }
            return bmpShow;
        }
        public static Bitmap ShowBMP(string path)
        {
            return (Bitmap)Image.FromFile(path);
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
}

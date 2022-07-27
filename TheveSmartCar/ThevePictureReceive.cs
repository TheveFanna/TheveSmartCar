using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThevePictureProcessDll;

namespace TheveSmartCar
{
    static class ThevePictureReceive
    {
        private static int _Height = 120;
        private static int _Width = 188;
        private static int _Size = 188*120;
        private static int _picCount = 0;
        private static int _picAutoCount = 0;
        private static Bitmap bmpSave;
        public static bool GrayFlag = false;


        public static int Height { get => _Height; set => _Height = value; }
        public static int Width { get => _Width; set => _Width = value; }
        public static int PicCount { get => _picCount; set => _picCount = value; }
        public static int PicAutoCount { get => _picAutoCount; set => _picAutoCount = value; }
        public static int Size { get => _Size; set => _Size = value; }

        /// <summary>
        /// 显示出BMP图像
        /// </summary>
        /// <param name="buffer">图像数据</param>
        /// <returns></returns>
        public static Bitmap BmpShow(byte[] buffer)
        {
            Bitmap bmp = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)
                { 
                        int a = buffer[i * Width + j];
                        bmp.SetPixel(j, i, Color.FromArgb(a, a, a));
                  
                }
            }
            bmpSave = bmp.Clone(new Rectangle(0, 0, Width, Height), PixelFormat.Format24bppRgb);
            return bmp;
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
                                if (ThevePictureProcess.antiColorFlag == true)
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

            bmpSave = bmpShow.Clone(new Rectangle(0, 0, Width, Height), PixelFormat.Format24bppRgb);

            PicPro.watch.Clear();
            return bmpShow;
        }
        /// <summary>
        /// 保存BMP图像到而本地
        /// </summary>
        /// <param name="path">字符型非路径，除去标号和后缀</param>
        /// <returns>保存的全路径</returns>
        public static string BmpSave(string path,int cnt)
        {
            string allPath = path + cnt.ToString() + ".bmp";
            bmpSave.Save(allPath, ImageFormat.Bmp);
            return allPath;
        }
        /// <summary>
        /// 检测并创建所需目录
        /// </summary>
        public static void CheckAndCreatDirection()
        {
            if (Directory.Exists("BmpSave") == false)
            {
                Directory.CreateDirectory("BmpSave");
            }
            PicCount = Directory.GetFiles("BmpSave").Length;
            if (Directory.Exists("BmpAutoSave") == false)
            {
                Directory.CreateDirectory("BmpAutoSave");
            }
            PicAutoCount = Directory.GetFiles("BmpAutoSave").Length;
        }
    }
}

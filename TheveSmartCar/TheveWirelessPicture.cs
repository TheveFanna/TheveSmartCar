using System;
using System.Windows.Forms;
using System.IO.Ports;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace SmartCar
{
    static class TheveWirelessPicture
    {
        private static bool _start = false;
        private static bool _autoSave = false;
        private static int _hight = 120;
        private static int _weight = 188;
        private static int _byteBuffer = 22562;
        private static int _picCount = 0;
        private static int _depth = 8;
        private static Bitmap bmpSave;
        public static bool Start { get => _start; set => _start = value; }
        public static int Hight { get => _hight; set => _hight = value; }
        public static int Weight { get => _weight; set => _weight = value; }
        public static int PicCount { get => _picCount; set => _picCount = value; }
        public static int Depth { get => _depth; set => _depth = value; }
        public static int ByteBuffer { get => _byteBuffer; set => _byteBuffer = value; }
        public static bool AutoSave { get => _autoSave; set => _autoSave = value; }

        /// <summary>
        /// PictureBox取消平滑模糊方法
        /// </summary>
        public static void PaintWithoutFuzzy(PictureBox pictureBox, PaintEventArgs e)
        {
            var pic = pictureBox;
            if (pic.Image == null)
                return;
            var state = e.Graphics.Save();
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            e.Graphics.Clear(pictureBox.BackColor);
            e.Graphics.DrawImage(pic.Image, new Rectangle(0, 0, pic.Width, pic.Height), new Rectangle(0, 0, pic.Image.Width, pic.Image.Height), GraphicsUnit.Pixel);
            e.Graphics.Restore(state);
        }
        /// <summary>
        /// 分辨率和为深度的设置方法
        /// </summary>
        /// <param name="comboBoxResoultion">分辨率下拉栏</param>
        /// <param name="comboBoxDepth">位深度下拉栏</param>
        public static void InitWiressResoultionDepth(ComboBox comboBoxResoultion, ComboBox comboBoxDepth)
        {
            comboBoxResoultion.Items.Add("188*120");
            comboBoxResoultion.Items.Add("160*120");
            comboBoxResoultion.SelectedIndex = 0;

            comboBoxDepth.Items.Add("4");
            comboBoxDepth.Items.Add("8");
            comboBoxDepth.Items.Add("24");
            comboBoxDepth.Items.Add("32");
            comboBoxDepth.SelectedIndex = 1;
        }

        /// <summary>
        /// 根据字节数组获得Bitmap类型
        /// </summary>
        /// <param name="buffer">Weight*Hight的一维字节数组</param>
        /// <returns>Bitmap类</returns>
        public static Bitmap GetBmpPicture(byte[] buffer)
        {
            Bitmap bmp = new Bitmap(Weight, Hight, PixelFormat.Format24bppRgb);
            for (int i = 0; i < Hight; i++)
            {
                for (int j = 0; j < Weight; j++)
                {
                    if (buffer[i * Weight + j] == 0)
                    {
                        bmp.SetPixel(j, i, Color.White);
                    }
                }
            }
            bmpSave = (Bitmap)bmp.Clone(new Rectangle(0, 0, Weight, Hight),PixelFormat.Format8bppIndexed);
            return bmp;
        }
        /// <summary>
        /// 保存到本地
        /// </summary>
        /// <param name="path">路径</param>
        public static void BmpSave(string path)
        {
            bmpSave.Save(path, ImageFormat.Bmp);
        }
        /// <summary>
        /// 创建一个空Bitmap图片，用于填充PictureBox
        /// </summary>
        /// <param name="pictureBox"></param>
        public static void BmpClear(PictureBox pictureBox)
        {
            pictureBox.Image = new Bitmap(Weight, Hight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        }
    }
}

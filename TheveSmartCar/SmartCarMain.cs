using SmartCar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;
using ThevePictureProcessDll;
using TheveSmartCar.Properties;

namespace TheveSmartCar
{
    public partial class SmartCarMain : Form
    {
        bool Unique = false;
        int transFlag = 0;
        int PictureReceiveCount = 0;
        List<Byte> listRecvPicData = new List<byte>();
        int ReceiveDataCount = 0;
        bool StartReceiveFlag = false;
        Toolss t;
        List<string> listFilePath = new List<string>();
        private void Form1_Load(object sender, EventArgs e)
        {
            TheveIniFiles.InitPath(Application.StartupPath + @"\SmartCarConfig.ini");
            VerifyState();
            //初始化串口
            TheveSerialPort.Scan(comboBoxCom);
            //自恢复 
            buttonIniRecover_Click(null, null);
            ButtonEnable(false);
            //连续时间刻度
            ThevePictureProcess.InitInterval(comboBoxAutoInterval);
            //串口获取
            TheveSerialPort.Scan(comboBoxCom);
            TheveSerialPort.SetBund(comboBoxBaud);
            ThevePictureReceive.CheckAndCreatDirection();
            HelpText();
            labelOrinial.Text = "";

        }
        #region 图像处理
        //浏览文件夹
        private void buttonDirectionBmp_Click(object sender, EventArgs e)
        {

            textBoxDirectionBmp.Text = TheveFile.SelectDirection();
            if (textBoxDirectionBmp.Text != "")
            {
                GetDirectionFiles(textBoxDirectionBmp.Text);
            }
        }
        //获取文件夹内的文件列表
        private void GetDirectionFiles(string directionPath)
        {
            listFilePath = TheveFile.GetAllFilePath(directionPath);
            ThevePictureProcess.PicCount = 0;
            labelPicAllNum.Text = "图片总数：" + listFilePath.Count;
            if (listFilePath.Count > 0)
            {
                TheveMenuStrip.DirectionRecent(ToolStripMenuItemFile, textBoxDirectionBmp.Text, true);
            }
        }
        //跳转
        private void buttonJump_Click(object sender, EventArgs e)
        {
            try
            {
                int num = Convert.ToInt32(textBoxStartNum.Text);
                if (num >= 0)
                {
                    ThevePictureProcess.PicCount = num;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("请输入正确数字", "错误");
                throw;
            }
            GetBmpInfo();
        }
        //图像处理处理
        private void GetBmpInfo(bool passFlag = false)
        {
            //防止序号为负数
            if (ThevePictureProcess.PicCount < 0)
            {
                ThevePictureProcess.PicCount = 0;
                return;
            }
            //最后一张判断
            if (ThevePictureProcess.PicCount == listFilePath.Count)
            {
                ThevePictureProcess.PicCount = listFilePath.Count - 1;
                if (ThevePictureProcess.AutoPlay == true)
                {
                    //取消连续
                    buttonAutoInterval.Text = "连续(&A)";
                    timer1.Stop();
                    ThevePictureProcess.AutoPlay = false;
                    DebugLog("当前是最后一张，连续播放结束。");
                }
                return;
            }
            //防止序号超过图片总数
            if (ThevePictureProcess.PicCount >= listFilePath.Count)
            {
                MessageBox.Show("超过文件内图片总数。", "错误");
                return;
            }
            //获取图片的数据到PicPro.img
            if (ThevePictureProcess.GetBMP(listFilePath[ThevePictureProcess.PicCount]) == false)
            {
                return;
            }

            labelDepth.Text = ThevePictureProcess.Depth;
            labelResoultion.Text = ThevePictureProcess.Resoultion;
            labelPicNumShow.Text = "当前图片编号：" + ThevePictureProcess.PicCount;
            //是否跳过编译
            if (passFlag == false)
            {
                if (richTextBoxPicPro.Text != "" && ThevePictureProcess.ChangeFlag == false)
                {
                    if (ThevePictureProcess.RunCode(richTextBoxPicPro.Text))
                    {
                        DebugLog(listFilePath[ThevePictureProcess.PicCount].ToString() + ThevePictureProcess.Log);
                        labelState.Text = "状态：" + "正常";
                    }
                    else
                    {
                        MessageBox.Show(ThevePictureProcess.Log, "程序错误");
                        labelState.Text = "状态：" + "错误";
                        return;
                    }
                }
                else
                {
                    labelState.Text = "状态：" + "原图";
                }
            }

            if (transFlag == 2)//隐藏黑白
            {
                pictureShow.Image = ThevePictureProcess.ShowBMP(true);
            }
            else
            {
                pictureShow.Image = ThevePictureProcess.ShowBMP();
            }
            int a = PicPro.watch.Keys.Count;
            WatchShow();
            DebugLog(ThevePictureProcess.error);
        }
        //数据监视器
        private void WatchShow()
        {
            richTextBoxWatch.Text = "";
            foreach (string name in PicPro.watch.Keys)
            {
                richTextBoxWatch.AppendText(name + " = " + PicPro.watch[name] + "\r\n");
            }
            PicPro.WatchClear();
        }
        //锁定
        private void buttonCodeLock_Click(object sender, EventArgs e)
        {
            if (ThevePictureProcess.CodeLock == false)
            {
                ThevePictureProcess.CodeLock = true;
                buttonCodeLock.Text = "解锁(&D)";
                ButtonEnable(true);
                richTextBoxPicPro.BackColor = Color.Gainsboro;
                if (t != null)
                {
                    t.UniqueCodeBackColor(Color.Gainsboro);
                }
            }
            else
            {
                if (黑色ToolStripMenuItem.Checked == true)
                {
                    richTextBoxPicPro.BackColor = Color.FromArgb(51, 51, 55);
                    if (t != null)
                    {
                        t.UniqueCodeBackColor(Color.FromArgb(51, 51, 55));
                    }
                }
                else
                {
                    richTextBoxPicPro.BackColor = Color.White;
                    if (t != null)
                    {
                        t.UniqueCodeBackColor(Color.White);
                    }
                }
                buttonCodeLock.Text = "锁定(&D)";
                ThevePictureProcess.RunCodeFlag = false;
                ButtonEnable(false);
                ThevePictureProcess.CodeLock = false;
                if (Unique == true)
                {
                    richTextBoxPicPro.Text = t.synStr;
                }
            }
        }
        //上一张
        private void buttonLast_Click(object sender, EventArgs e)
        {
            ThevePictureProcess.PicCount--;
            GetBmpInfo();
        }
        //下一张
        private void buttonNext_Click(object sender, EventArgs e)
        {
            ThevePictureProcess.PicCount++;
            GetBmpInfo();
        }
        //Debug
        private void DebugLog(string contenes)
        {
            if (contenes != "" && groupBox3.Visible == true)
            {
                textBoxDebug.AppendText(contenes + "\r\n");
            }

        }
        //按键状态
        private void ButtonEnable(bool state)
        {
            richTextBoxPicPro.ReadOnly = state;

            switch (tabControlMode.SelectedIndex)
            {
                case 0:
                    buttonAutoInterval.Enabled = state;
                    buttonLast.Enabled = state;
                    buttonNext.Enabled = state;
                    buttonJump.Enabled = state;
                    buttonChange.Enabled = state;
                    textBoxDirectionBmp.Enabled = !state;
                    buttonDirectionBmp.Enabled = !state;
                    buttonAntiColor.Enabled = state;
                    break;
                case 1:
                    textBoxPicWidth.Enabled = !state;
                    textBoxPicHeight.Enabled = !state;
                    buttonStartReceive.Enabled = state;
                    buttonClearBmp.Enabled = state;
                    buttonSaveBmp.Enabled = state;
                    buttonTrans.Enabled = state;
                    buttonCodeRun2.Enabled = state;
                    break;
                default:
                    break;
            }
        }
        #endregion
        #region 连续功能
        //连续
        private void buttonAutoInterval_Click(object sender, EventArgs e)
        {
            if (ThevePictureProcess.AutoPlay == false)
            {
                timer1.Tag = "连续放图";
                buttonAutoInterval.Text = "停止(&A)";
                timer1.Start();
                ThevePictureProcess.AutoPlay = true;
            }
            else
            {
                buttonAutoInterval.Text = "连续(&A)";
                timer1.Stop();
                ThevePictureProcess.AutoPlay = false;
            }
        }
        //定时器中断
        private void timer1_Tick(object sender, EventArgs e)
        {
            switch (timer1.Tag)
            {
                case "连续放图":
                    ThevePictureProcess.PicCount++;
                    GetBmpInfo();
                    break;
                default:
                    timer1.Stop();
                    break;
            }
        }
        //连续时间更改
        private void comboBoxAutoInterval_SelectedIndexChanged(object sender, EventArgs e)
        {
            timer1.Interval = (int)(Convert.ToDouble(comboBoxAutoInterval.Text) * 1000);
        }
        #endregion
        #region INI设置
        //保存INI设置
        private void buttonIniSave_Click(object sender, EventArgs e)
        {
            TheveIniFiles.IniWriteValue("图像处理", "显示模式", TheveMenuStrip.GetMenuCheck(显示模式ToolStripMenuItem).Name);
            TheveIniFiles.IniWriteValue("图像处理", "编译模式", TheveMenuStrip.GetMenuCheck(编译模式ToolStripMenuItem).Name);
            TheveIniFiles.IniWriteValue("图像处理", "鼠标样式", TheveMenuStrip.GetMenuCheck(鼠标样式ToolStripMenuItem).Name);

            int cnt = 0;
            foreach (var i in ToolStripMenuItemFile.DropDownItems)
            {
                cnt++;
                TheveIniFiles.IniWriteValue("文件夹记录", "最近打开的文件夹" + cnt.ToString(), i.ToString());
            }
            File.WriteAllText(@"PicProCode.c", richTextBoxPicPro.Text);
            DebugLog("已保存设置。");
            DebugLog("图像处理程序已保存在 PicProCode.txt 文件中。");

        }
        //恢复INI设置
        private void buttonIniRecover_Click(object sender, EventArgs e)
        {
            if (TheveIniFiles.ExistINIFile() == true)
            {
                //最近浏览文件夹
                string[] pathCount = TheveIniFiles.GetItemsKeys("文件夹记录");
                if (pathCount != null)
                {
                    foreach (var i in pathCount)
                    {
                        TheveMenuStrip.DirectionRecent(ToolStripMenuItemFile, TheveIniFiles.IniReadValue("文件夹记录", i));
                    }
                }
                //上次文件夹路径
                textBoxDirectionBmp.Text = TheveIniFiles.IniReadValue("图像处理", "文件夹路径");
                //恢复代码
                if (File.Exists(@"PicProCode.c"))
                {
                    richTextBoxPicPro.Text = File.ReadAllText(@"PicProCode.c");
                }
                //全屏启动
                if (TheveIniFiles.IniReadValue("图像处理", "全屏启动") == "True")
                {
                    this.WindowState = FormWindowState.Maximized;
                    全屏启动ToolStripMenuItem.Checked = true;
                }
                //隐藏相关信息栏
                if (TheveIniFiles.IniReadValue("图像处理", "隐藏相关信息栏") == "False")
                {
                    隐藏相关信息栏ToolStripMenuItem_Click(null, null);
                }
                //菜单栏
                显示模式ToolStripMenuItem_DropDownItemClicked(null, new ToolStripItemClickedEventArgs(TheveMenuStrip.NameToObject(显示模式ToolStripMenuItem, TheveIniFiles.IniReadValue("图像处理", "显示模式"))));
                编译模式ToolStripMenuItem_DropDownItemClicked(null, new ToolStripItemClickedEventArgs(TheveMenuStrip.NameToObject(编译模式ToolStripMenuItem, TheveIniFiles.IniReadValue("图像处理", "编译模式"))));
                鼠标样式ToolStripMenuItem_DropDownItemClicked(null, new ToolStripItemClickedEventArgs(TheveMenuStrip.NameToObject(鼠标样式ToolStripMenuItem, TheveIniFiles.IniReadValue("图像处理", "鼠标样式"))));
                if (TheveIniFiles.IniReadValue("图像处理", "主题") == "黑色ToolStripMenuItem")
                {
                    主题ToolStripMenuItem_DropDownItemClicked(null, new ToolStripItemClickedEventArgs(黑色ToolStripMenuItem));
                }

                textBoxPicWidth.Text = TheveIniFiles.IniReadValue("串口传图", "图像宽");
                textBoxPicHeight.Text = TheveIniFiles.IniReadValue("串口传图", "图像高");

                DebugLog("已恢复设置。");
            }

        }
        //代码同步
        private void buttonSynchronization_Click(object sender, EventArgs e)
        {
            //恢复代码
            if (File.Exists(@"PicProCode.c"))
            {
                richTextBoxPicPro.Text = File.ReadAllText(@"PicProCode.c");
            }
        }

        #endregion
        #region 代码框和图片框分权
        //去模糊
        private void pictureShow_Paint(object sender, PaintEventArgs e)
        {
            ThevePictureProcess.PaintWithoutFuzzy(pictureShow, e);
            if (等宽长缩放ToolStripMenuItem.Checked == true)
            {
                显示模式ToolStripMenuItem_DropDownItemClicked(null, new ToolStripItemClickedEventArgs(等宽长缩放ToolStripMenuItem));
            }
        }
        //改变代码框和图片框占比
        private Button buttonSplitMove;
        private int buttonSplit_X;
        private void buttonSplit_MouseDown(object sender, MouseEventArgs e)
        {
            buttonSplit_X = e.Location.X;
            buttonSplitMove = sender as Button;
        }
        //改变代码框和图片框占比
        private void buttonSplit_MouseMove(object sender, MouseEventArgs e)
        {
            int pos_x;
            if (e.Button == MouseButtons.Left)
            {
                pos_x = buttonSplitMove.Location.X + (e.X - buttonSplit_X);
                buttonSplitMove.Location = new Point(pos_x, buttonSplit.Location.Y);

                tabControl1.Location = new Point(pos_x + 13, tabControl1.Location.Y);
                tabControl1.Width = this.Width - pos_x - 13 - 34;

                groupBox2.Width = pos_x - groupBox2.Location.X - 3;
                groupBox3.Width = pos_x - groupBox3.Location.X - 3;

            }
        }

        #endregion
        #region 环境验证

        private void VerifyState()
        {
            buttonCodeLock.Enabled = true;
            buttonSerialOpen.Enabled = true;
            buttonLockInfo.Enabled = true;
        }
        #endregion
        #region 帮助
        //help
        void HelpText()
        {
            textBoxHelp.Text = @"注意：快捷键的使用方法：Alt+某键
一维数组声明方式为:
类型[] 变量名 = new 类型[数组大小]
例如：int[] bord_L = new int[120];
二维数组声明方式为:
类型[,] 变量名 = new 类型[数组大小,数组大小]
例如：int[,] test = new int[188,120];
其他类型可以按照C语言的格式。
支持：char,short,int,double 
不支持：指针,unsigned,float,#define宏定义，数组定义时初始化

可视化颜色修改方式如下： 
修改颜色：PicPro.img[i][j]=255+数字；默认如下
1-红色
2-绿色
3-蓝色
4-黄色
5-橙色
6-青色
7-紫色
8-粉色
9-洋红
采用位图标准数据定义黑白色：
0-黑色
255-白色
修改颜色：PicPro.img[i][j]=255+数字；
例：PicPro.img[1][1]=255+1;点（1,1）为红色

参数监视器语句调用方法为：
PicPro.Watch(“名称”，变量);
例：PicPro.Watch(“bord_L_flag”，bord_L_flag);
例：PicPro.Watch(“左边界标志位”，bord_L_flag);
程序内的代码必须为英文格式，包括上面的双引号。

支持数学库：(例)
Math.Sqrt();
Math.Abs();
Math.Sin();
...
具体内容查看C#的Math类。

如有疑问、建议和任何反馈请联系闲鱼作者“落落仪”";
        }
        //解释性说明以下
        private void buttonDirectionBmp_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "点击选择包含BMP图集的文件夹。";
        }
        private void buttonJump_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "点击跳转至指定序号的图像。";
        }
        private void buttonAutoInterval_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "点击即可连续播放图像，至最后一张时停止。";
        }
        private void buttonIniSave_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "点击保存界面的一些设置到本地。";
        }
        private void buttonIniRecover_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "点击从本地恢复上次的界面设置。";
        }
        private void buttonCodeLock_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "需要先点击本按钮将图像处理程序锁定，再使用其他功能。";
        }
        private void buttonChange_MouseEnter(object sender, EventArgs e)
        {

            toolStripStatusLabel1.Text = "点击则转换当前图像为原图或处理后的图像。";
        }
        private void buttonAntiColor_MouseEnter(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = "点击则黑白反色当前图片，只是显示反色，逻辑不反。";
        }
        private void buttonUnique_MouseEnter(object sender, EventArgs e)
        {

            toolStripStatusLabel1.Text = "点击出现独立的程序框，可修改代码，点击锁定可同步。";
        }
        public SmartCarMain()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }
        #endregion
        #region 实用小功能
        //反色
        private void buttonAntiColor_Click(object sender, EventArgs e)
        {
            ThevePictureProcess.antiColorFlag = !ThevePictureProcess.antiColorFlag;
            labelAntiColor.Visible = ThevePictureProcess.antiColorFlag;
            GetBmpInfo();
        }
        //转换前后图
        private void buttonChange_Click(object sender, EventArgs e)
        {
            transFlag = ++transFlag > 2 ? 0 : transFlag;
            switch (transFlag)
            {
                case 0:
                    labelOrinial.Text = "";
                    ThevePictureProcess.ChangeFlag = false;
                    GetBmpInfo();
                    break;
                case 1:
                    labelOrinial.Text = "原图";
                    ThevePictureProcess.ChangeFlag = true;
                    GetBmpInfo(true);
                    break;
                case 2:
                    labelOrinial.Text = "标记图";
                    ThevePictureProcess.ChangeFlag = false;
                    GetBmpInfo();
                    break;
                default:
                    break;
            }
        }
        //实时灰度值+鼠标样式
        private void pictureShow_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (pictureShow.Image != null)
                {
                    Bitmap bm = (Bitmap)pictureShow.Image;
                    float a, b, c, d;
                    c = ((float)pictureShow.Width / ThevePictureProcess.Width);
                    d = ((float)pictureShow.Height / ThevePictureProcess.Height);
                    a = e.X / c;
                    b = e.Y / d;
                    if (ThevePictureProcess.antiColorFlag)
                    {
                        label1.Text = string.Format("像素信息：PicPro.img[{1}][{0}]={2}", (int)a, (int)b, 255 - bm.GetPixel((int)a, (int)b).R);
                    }
                    else
                    {
                        label1.Text = string.Format("像素信息：PicPro.img[{1}][{0}]={2}", (int)a, (int)b, bm.GetPixel((int)a, (int)b).R);
                    }
                }
            }
            catch
            {
            }
        }
        private void buttonUniqueTool_Click(object sender, EventArgs e)
        {
            if (Unique == false)
            {
                Unique = true;
                buttonUnique.Text = "关闭独立程序栏(&U)";
                t = new Toolss(richTextBoxPicPro.Text, 黑色ToolStripMenuItem.Checked);
                t.Show();
            }
            else
            {
                Unique = false;
                buttonUnique.Text = "开启独立程序栏(&U)";
                richTextBoxPicPro.Text = t.synStr;
                t.Dispose();
            }
        }
        //动态获取文件数
        private void textBoxDirectionBmp_TextChanged(object sender, EventArgs e)
        {
            string path = textBoxDirectionBmp.Text;
            if (Directory.Exists(path) && path != "")
            {
                TheveIniFiles.IniWriteValue("图像处理", "文件夹路径", textBoxDirectionBmp.Text);
                GetDirectionFiles(path);
            }
            else
            {
                labelPicAllNum.Text = "图片总数：0";
            }
        }
        #endregion
        #region 菜单栏 
        //隐藏Debug 
        private void 隐藏相关信息栏ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TheveMenuStrip.SingleCheckTrans(sender);
            TheveIniFiles.IniWriteValue("图像处理", "隐藏相关信息栏", 隐藏相关信息栏ToolStripMenuItem.Checked.ToString());

            if (隐藏相关信息栏ToolStripMenuItem.Checked)
            {
                groupBox2.Height = tabControl1.Height;
                groupBox3.Visible = false;
            }
            else
            {
                groupBox2.Height = groupBox3.Location.Y - groupBox2.Location.Y - 10;
                groupBox3.Visible = true;
            }
            switch (tabControlMode.SelectedIndex)
            {
                case 0:
                    if (pictureShow.Image != null)
                    {
                        GetBmpInfo();
                    }
                    break;
                case 1:
                    if (pictureShow.Image != null)
                    {
                        ThevePictureReceive.ShowBMP();
                    }
                    break;
                default:
                    break;
            }
        }
        //全屏启动
        private void 全屏启动ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TheveMenuStrip.SingleCheckTrans(sender);
            TheveIniFiles.IniWriteValue("图像处理", "全屏启动", 全屏启动ToolStripMenuItem.Checked.ToString());
        }
        //最近浏览文件夹点击时
        private void ToolStripMenuItemFile_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (buttonCodeLock.Text == "解锁(&D)")
            {
                buttonCodeLock_Click(null, null);
            }
            textBoxDirectionBmp.Text = e.ClickedItem.Text;
        }
        //窗体黑色主题

        void MainThemeChange()
        {
            ChangeColor(this);
        }
        void ChangeColor(Control c)
        {
            ThemeChange(c);
            foreach (Control d in c.Controls)
            {
                ThemeChange(d);
                if (d is Panel || d is TabControl || d is GroupBox)
                {
                    ChangeColor(d);
                }
            }
        }
        //控件颜色改变
        void ThemeChange(Control c, int level = 2)
        {
            switch (level)
            {
                case 0:
                    c.BackColor = Color.FromArgb(25, 25, 28);
                    break;
                case 1:
                    c.BackColor = Color.FromArgb(30, 30, 30);
                    break;
                case 2:
                    c.BackColor = Color.FromArgb(51, 51, 55);
                    break;
                default:
                    break;
            }
            c.ForeColor = Color.White;
        }
        private void 显示模式ToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == null)
            {
                return;
            }
            TheveMenuStrip.SetMenuCheck(显示模式ToolStripMenuItem, e.ClickedItem);
            TheveIniFiles.IniWriteValue("图像处理", "显示模式", e.ClickedItem.Name);
            switch (e.ClickedItem.ToString())
            {
                case "拉伸模式":
                    pictureShow.Height = groupBox2.Height - 50;
                    pictureShow.Width = groupBox2.Width;
                    break;

                case "等宽长缩放":
                    ThevePictureProcess.AdaptSize(pictureShow, groupBox2.Width, groupBox2.Height - 50);
                    break;

                default:
                    break;
            }
        }
        private void 编译模式ToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

            if (e.ClickedItem == null)
            {
                return;
            }
            TheveMenuStrip.SetMenuCheck(编译模式ToolStripMenuItem, e.ClickedItem);
            TheveIniFiles.IniWriteValue("图像处理", "编译模式", e.ClickedItem.Name);
            if (buttonCodeLock.Text == "解锁(&D)")
            {
                buttonCodeLock_Click(null, null);
            }
            switch (e.ClickedItem.ToString())
            {
                case "单函数模式":
                    ThevePictureProcess.modeFlag = false;
                    break;

                case "多函数模式":
                    ThevePictureProcess.modeFlag = true;
                    break;

                default:
                    break;
            }
        }

        private void 鼠标样式ToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem == null)
            {
                return;
            }
            TheveMenuStrip.SetMenuCheck(鼠标样式ToolStripMenuItem, e.ClickedItem);
            TheveIniFiles.IniWriteValue("图像处理", "鼠标样式", e.ClickedItem.Name);

            switch (e.ClickedItem.ToString())
            {
                case "箭头":
                    pictureShow.Cursor = Cursors.Arrow;
                    break;
                case "十字":
                    pictureShow.Cursor = Cursors.Cross;
                    break;
                case "圆圈":
                    pictureShow.Cursor = Cursors.WaitCursor;
                    break;
                case "隐藏":
                    //pictureShow.Cursor = Cursors.WaitCursor;
                    break;
                default:
                    break;
            }
        }

        private void 主题ToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            TheveMenuStrip.SetMenuCheck(主题ToolStripMenuItem, e.ClickedItem);
            TheveIniFiles.IniWriteValue("图像处理", "主题", e.ClickedItem.Name);
            switch (e.ClickedItem.ToString())
            {
                case "白色":
                    Application.Restart();
                    break;
                case "黑色":
                    MainThemeChange();
                    break;
                default:
                    break;
            }
        }

        private void 赞赏ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form f = new Form();
            f.Icon = Resources.car;
            f.StartPosition = FormStartPosition.CenterScreen;
            f.Size = new Size(1000, 1000);
            f.BackgroundImage = Resources.赞赏码;
            f.BackgroundImageLayout = ImageLayout.Stretch;
            f.Show();
        }
        #endregion
        #region 串口接收图像
        //打开串口
        private void buttonSerialOpen_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxCom.Text != "")
                {
                    if (buttonSerialOpen.Text == "打开(&O)")
                    {
                        serialPort1.PortName = comboBoxCom.Text;
                        serialPort1.BaudRate = Convert.ToInt32(comboBoxBaud.Text);
                        serialPort1.Open();
                        while (true)
                        {
                            if (serialPort1.IsOpen == true)
                            {
                                buttonSerialOpen.Text = "关闭(&O)";
                                comboBoxCom.Enabled = false;
                                comboBoxBaud.Enabled = false;
                                buttonLockInfo.Enabled = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        serialPort1.Close();
                        while (true)
                        {
                            if (serialPort1.IsOpen == false)
                            {
                                buttonSerialOpen.Text = "打开(&O)";
                                comboBoxCom.Enabled = true;
                                comboBoxBaud.Enabled = true;
                                if (buttonLockInfo.Text == "解锁(&D)")
                                {
                                    buttonLockInfo_Click(null, null);
                                }

                                break;
                            }
                        }
                    }
                }

            }
            catch
            {
                DebugLog("串口开关出错。");
            }
        }
        Stopwatch stopwatch;
        //接收
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (serialPort1.BytesToRead > 0 && StartReceiveFlag == true)
            {
                if (stopwatch == null)//计时系统
                {
                    stopwatch = new Stopwatch();
                    stopwatch.Start();
                }
                int recvCount = serialPort1.BytesToRead;
                byte[] buff = new byte[recvCount];
                if (serialPort1.Read(buff, 0, recvCount) > 0)
                {
                    for (int i = 0; i < recvCount; i++)
                    {
                        listRecvPicData.Add(buff[i]);
                        ReceiveDataCount++;
                    }
                }
            }
            else
            {
                serialPort1.DiscardInBuffer();
            }
        }
        //开始接收图像
        private void buttonStartReceive_Click(object sender, EventArgs e)
        {
            if (StartReceiveFlag == false)//停止
            {
                buttonStartReceive.Text = "停止接收图像(&G)";
                DebugLog("开始接收图像");
                timer2.Start();
            }
            else
            {
                buttonStartReceive.Text = "开始接收图像(&G)";
                DebugLog("停止接收图像");
                listRecvPicData.Clear();
            }
            StartReceiveFlag = !StartReceiveFlag;
        }
        //保存当前图像
        private void buttonSaveBmp_Click(object sender, EventArgs e)
        {
            if (pictureShow.Image != null)
            {
                DebugLog("已保存至：" + ThevePictureReceive.BmpSave(@"BmpSave/img", ThevePictureReceive.PicCount++));

            }
        }
        //锁定
        private void buttonLockInfo_Click(object sender, EventArgs e)
        {
            if (ThevePictureProcess.CodeLock == false)
            {
                if (textBoxPicHeight.Text == "" || textBoxPicWidth.Text == "")
                {
                    MessageBox.Show("请先填写接收图像的宽度和高度");
                }
                else
                {
                    ThevePictureReceive.Width = Convert.ToInt32(textBoxPicWidth.Text);
                    ThevePictureReceive.Height = Convert.ToInt32(textBoxPicHeight.Text);
                    ThevePictureReceive.Size = ThevePictureReceive.Width * ThevePictureReceive.Height;

                    buttonLockInfo.Text = "解锁(&D)";
                    ButtonEnable(true);
                    ThevePictureProcess.CodeLock = true;

                    TheveIniFiles.IniWriteValue("串口传图", "串口号", comboBoxCom.SelectedItem.ToString());
                    TheveIniFiles.IniWriteValue("串口传图", "波特率", comboBoxBaud.Text);
                    TheveIniFiles.IniWriteValue("串口传图", "图像宽", textBoxPicWidth.Text);
                    TheveIniFiles.IniWriteValue("串口传图", "图像高", textBoxPicHeight.Text);
                }
            }
            else
            {
                buttonLockInfo.Text = "锁定(&D)";
                ThevePictureProcess.RunCodeFlag = false;
                ButtonEnable(false);
                ThevePictureProcess.CodeLock = false;
                if (Unique == true)
                {
                    richTextBoxPicPro.Text = t.synStr;
                }
                if (buttonStartReceive.Text == "停止接收图像(&G)")
                {
                    StartReceiveFlag = false;
                    buttonStartReceive.Text = "开始接收图像(&G)";
                    DebugLog("停止接收图像\r\n");
                }
            }

        }
        //任务栏页面更改时
        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ButtonEnable(false);

            switch (tabControlMode.SelectedIndex)
            {
                case 0:
                    labelByte.Visible = false;

                    if (buttonLockInfo.Text != "锁定(&D)")
                    {
                        buttonLockInfo_Click(null, null);
                    }

                    if (buttonSerialOpen.Text != "打开(&O)")
                    {
                        buttonSerialOpen_Click(null, null);
                    }

                    break;
                case 1:

                    TheveSerialPort.Scan(comboBoxCom);
                    labelByte.Visible = true;
                    if (buttonSerialOpen.Text == "打开(&O)")
                    {
                        buttonLockInfo.Enabled = false;
                    }
                    if (ThevePictureProcess.CodeLock == true)
                    {
                        buttonCodeLock_Click(null, null);
                    }
                    break;
                default:
                    break;
            }
        }
        //清除图片
        private void buttonClearBmp_Click(object sender, EventArgs e)
        {
            pictureShow.Image = null;
        }
        //定时器处理
        private void timer2_Tick(object sender, EventArgs e)
        {
            CheckImage();
            labelByte.Text = "传输字节量：" + listRecvPicData.Count.ToString();
        }
        //检查是否满足图像条件
        void CheckImage()
        {
            int index = 0;
            while (index <= listRecvPicData.Count - ThevePictureReceive.Size - 4)
            {
                if (listRecvPicData[index] == 0xFC && listRecvPicData[index + 1] == 0xCF)
                {
                    if (listRecvPicData[index + ThevePictureReceive.Size + 2] == 0xCF && listRecvPicData[index + ThevePictureReceive.Size + 3] == 0xFC)
                    {
                        for (int i = 0; i < ThevePictureReceive.Height; i++)
                        {
                            for (int j = 0; j < ThevePictureReceive.Width; j++)
                            {
                                PicPro.img[i, j] = listRecvPicData[index + j + i * ThevePictureReceive.Width];
                                PicPro.imgOriginal[i, j] = PicPro.img[i, j];
                            }
                        }
                        listRecvPicData.RemoveRange(0, index + ThevePictureReceive.Size + 3);
                        pictureShow.Image = ThevePictureReceive.ShowBMP();
                        PictureReceiveCount++;
                        labelPicNumShow.Text = "当前图片编号：" + PictureReceiveCount.ToString();
                        //计时
                        stopwatch.Stop();
                        TimeSpan dt = stopwatch.Elapsed;
                        DebugLog("结束此帧图像用时：" + dt.TotalSeconds.ToString());
                        stopwatch = null;

                        if (checkBoxAutoSave.Checked == true)//自动保存
                        {
                            DebugLog("已保存至：" + ThevePictureReceive.BmpSave(@"BmpAutoSave\img", ThevePictureReceive.PicAutoCount++));
                        }

                        if (checkBoxRunCode.Checked == true)//自动运行程序
                        {
                            if (ThevePictureProcess.RunCode(richTextBoxPicPro.Text))
                            {
                                DebugLog("接收图像序号" + PictureReceiveCount.ToString() + ThevePictureProcess.Log);
                            }
                            else
                            {
                                if (buttonStartReceive.Text == "停止接收图像(&G)")
                                {
                                    buttonStartReceive.Text = "开始接收图像(&G)";
                                    DebugLog("停止接收图像");
                                    listRecvPicData.Clear();
                                    StartReceiveFlag = false;
                                }
                                MessageBox.Show(ThevePictureProcess.Log, "程序错误");
                            }
                            pictureShow.Image = ThevePictureProcess.ShowBMP();
                        }
                    }
                    else
                    {
                        listRecvPicData.RemoveRange(0, index + ThevePictureReceive.Size + 3);
                        DebugLog("此帧图像数据错乱,已丢弃。");
                        stopwatch = null;
                    }
                }
                index++;
            }
        }
        //接收图像运行程序
        private void buttonCodeRun2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ThevePictureReceive.Height; i++)
            {
                for (int j = 0; j < ThevePictureReceive.Width; j++)
                {
                    PicPro.img[i, j] = PicPro.imgOriginal[i, j];
                }
            }

            if (ThevePictureProcess.RunCode(richTextBoxPicPro.Text))
            {
                DebugLog("接收图像序号" + PictureReceiveCount.ToString() + ThevePictureProcess.Log);
            }
            else
            {
                if (buttonStartReceive.Text == "停止接收图像(&G)")
                {
                    buttonStartReceive.Text = "开始接收图像(&G)";
                    DebugLog("停止接收图像");
                    listRecvPicData.Clear();
                    StartReceiveFlag = false;
                }
                MessageBox.Show(ThevePictureProcess.Log, "程序错误");
            }
            pictureShow.Image = ThevePictureReceive.ShowBMP();
        }
        //转换
        private void buttonTrans_Click(object sender, EventArgs e)
        {
        }
        #endregion


    }
}

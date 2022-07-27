using SmartCar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using ThevePictureProcessDll;
using TheveSmartCar.Properties;
using static TheveSmartCar.ThevePictureProcess;


namespace TheveSmartCar
{
    public partial class SmartCarMain : Form
    {
        //开关：是否进入Debug模式
        bool IsDebugMode = false;
        //跳过运行程序
        bool isPassCompileRun = false;

        //接收图像数据
        List<Byte> listRecvPicData = new List<byte>();
        private void Form1_Load(object sender, EventArgs e)
        {

            TheveIniFiles.InitPath(Application.StartupPath + @"\SmartCarConfig.ini");
            VerifyState();
            //初始化串口
            TheveSerialPort.Scan(comboBoxCom);
            //自恢复 
            buttonIniRecover_Click(null, null);
            //连续时间刻度
            ThevePictureProcess.InitInterval(comboBoxAutoInterval);
            //串口获取 
            TheveSerialPort.SetBund(comboBoxBaud);
            ThevePictureReceive.CheckAndCreatDirection();
            TheveGridView.Init(dataGridView1);
            HelpText();
        }
        #region 本地图像处理
        //显示模式
        int modeDisplayTrans = 0;
        //文件夹文件路径
        List<string> listFilePath = new List<string>();
        //浏览文件夹
        private void buttonDirectionBmp_Click(object sender, EventArgs e)
        {

            textBoxDirectionBmp.Text = TheveFile.SelectDirection();
            if (textBoxDirectionBmp.Text != "")
            {
                GetDirectionFiles(textBoxDirectionBmp.Text);
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
                return;
            }
            GetBmpInfo();
        }
        //锁定
        private void buttonCodeLock_Click(object sender, EventArgs e)
        {
            if (buttonCodeLock.Text == "锁定(&D)")
            {
                ButtonLocalEnable(true);
                buttonCodeLock.Text = "解锁(&D)";
            }
            else
            {
                //取消连续
                if (buttonAutoInterval.Text != "连续(&A)")
                {
                    buttonAutoInterval_Click(null, null);
                }
                ThevePictureProcess.isCompiledCode = false;
                PicPro.ClearInherit();
                ButtonLocalEnable(false);
                buttonCodeLock.Text = "锁定(&D)";
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
        //连续
        private void buttonAutoInterval_Click(object sender, EventArgs e)
        {
            if (buttonAutoInterval.Text == "连续(&A)")
            {
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
        //显示转换图
        private void buttonChange_Click(object sender, EventArgs e)
        {
            modeDisplayTrans = ++modeDisplayTrans > 2 ? 0 : modeDisplayTrans;
            switch (modeDisplayTrans)
            {
                case 0:
                    isPassCompileRun = false;
                    labelPicMode.Text = "图像模式：处理图";
                    break;
                case 1:
                    isPassCompileRun = true;
                    labelPicMode.Text = "图像模式：原图";
                    break;
                case 2:
                    isPassCompileRun = false;
                    labelPicMode.Text = "图像模式：标记图";
                    break;
                default:
                    break;
            }
            GetBmpInfo();
        }
        //按钮使能
        private void ButtonLocalEnable(bool state)
        {
            //锁定后 不能改 
            textBoxDirectionBmp.Enabled = !state;
            buttonDirectionBmp.Enabled = !state;
            //锁定后 才能改 true
            buttonAntiColor.Enabled = state;
            buttonDisplayChange.Enabled = state;
            buttonLastPic.Enabled = state;
            buttonNextPic.Enabled = state;
            RichTextReadState(state);
        }
        //获取文件夹内的文件列表
        private void GetDirectionFiles(string directionPath)
        {
            listFilePath = TheveFile.GetAllFilePath(directionPath);
            ThevePictureProcess.PicCount = 0;
            labelPicAllCnt.Text = "图片总数：" + listFilePath.Count;
            if (listFilePath.Count > 0)
            {
                TheveMenuStrip.DirectionRecent(ToolStripMenuItemFile, textBoxDirectionBmp.Text, true);
            }
        }
        //图像处理处理
        private void GetBmpInfo()
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
            labelPicIndex.Text = "当前图片编号：" + ThevePictureProcess.PicCount;
            //是否跳过 编译和运行
            if (isPassCompileRun == false)
            {
                if (CompileRunCode())
                {
                    DebugLog(listFilePath[ThevePictureProcess.PicCount].ToString() + ThevePictureProcess.Log);
                }
                else
                {
                    return;
                }
            }
            //显示  
            pictureShow.Image = ThevePictureProcess.ShowBMP((EModeDisplay)modeDisplayTrans);
            //参数监视器
            WatchShow();
            //数组查看器
            WatchArrayShow();
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
        //数组查看器
        private void WatchArrayShow()
        {
            try
            {
                foreach (string name in PicPro.arrayTable.Keys)
                {
                    if (!dataGridView1.Columns.Contains(name))
                    {
                        int index = dataGridView1.Columns.Add(name, name);
                        DataGridViewColumn dataGridViewColumn = dataGridView1.Columns[index];
                        dataGridViewColumn.ReadOnly = true;
                        dataGridViewColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
                    }
                    TheveGridView.WriteColData(name, (int[])PicPro.arrayTable[name]);
                }
                PicPro.WatchArrayClear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //定时器中断
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (ThevePictureProcess.AutoPlay)
            {
                ThevePictureProcess.PicCount++;
                GetBmpInfo();
            }
        }
        //连续时间更改
        private void comboBoxAutoInterval_SelectedIndexChanged(object sender, EventArgs e)
        {
            timer1.Interval = (int)(Convert.ToDouble(comboBoxAutoInterval.Text) * 1000);
        }
        //关闭本地界面所有功能
        private void DisadbleLocalForm()
        {
            if (buttonCodeLock.Text != "锁定(&D)")
            {
                buttonCodeLock_Click(null, null);
            }
        }

        #endregion
        #region INI设置
        //保存程序
        private void buttonIniSave_Click(object sender, EventArgs e)
        {
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
                textBoxWifiWidth.Text = TheveIniFiles.IniReadValue("WIFI传图", "图像宽");
                textBoxWifiHeight.Text = TheveIniFiles.IniReadValue("WIFI传图", "图像高");

                DebugLog("已恢复设置。");
            }

        }


        #endregion
        #region 代码框和图片框分权
        void RichTextReadState(bool isReadOnly)
        {
            richTextBoxPicPro.ReadOnly = isReadOnly;
            if (isReadOnly)
            {
                richTextBoxPicPro.BackColor = Color.Gainsboro;
            }
            else
            {
                //主题颜色
                if (黑色ToolStripMenuItem.Checked == true)
                {
                    richTextBoxPicPro.BackColor = Color.FromArgb(51, 51, 55);
                }
                else
                {
                    richTextBoxPicPro.BackColor = Color.White;
                }
            }
        }
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
            buttonWifiLink.Enabled = true;
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
类型[][] 变量名 = new 类型[数组大小][数组大小]
例如：int[][] test = new int[188][120];
其他类型可以按照C语言的格式，详情查看用户手册
支持：char,short,int,float,double 
不支持：指针,unsigned,#define，数组定义时初始化

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
修改像素颜色：PicPro.img[i][j]=255+数字；
例：PicPro.img[1][1]=255+1;点（1,1）为红色

参数监视器 语句调用方法为：
PicPro.Watch(“名称”，变量);
例：PicPro.Watch(“bord_L_flag”，bord_L_flag);
例：PicPro.Watch(“左边界标志位”，bord_L_flag); 

数组查看器（一维）语句调用方法为：
PicPro.WatchArray(“名称”，数组名);
例：PicPro.WatchArray(“bord_L”，bord_L);
例：PicPro.WatchArray(“左边界数组”，bord_L); 

全局继承器 语句调用方法为：
PicPro.Inherit(“名称”，变量);
例：PicPro.Inherit(“abc”，bord_L_flag);
int a = (int)PicPro.GetInherit(“abc”);
(返回值为float，需强制转换为你需要的类型)
例：PicPro.Inherit(“平均斜率”，ave_gradient);
float ave_gradient = PicPro.GetInherit(“平均斜率”);

程序内的代码必须为英文格式，包括上面的双引号。

支持数学库：(例)
Math.Sqrt();
Math.Abs();
Math.Sin();
...
具体内容查看用户手册。

如有任何疑问、建议和反馈请联系闲鱼作者“落落仪”";
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
        public SmartCarMain()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }
        #endregion
        #region 实用小功能
        //编译代码
        bool CompileRunCode()
        {
            if (ThevePictureProcess.RunCode(richTextBoxPicPro.Text))
            {
                labelCompileState.Text = "状态：编译成功";
                return true;
            }
            else
            {
                labelCompileState.Text = "状态：编译错误";
                MessageBox.Show(ThevePictureProcess.Log, "错误");
                return false;
            }
        }
        //反色
        private void buttonAntiColor_Click(object sender, EventArgs e)
        {
            if (ThevePictureProcess.isAntiColor)
            {
                label01State.Text = "二值化显示状态：标准色";
                ThevePictureProcess.isAntiColor = false;
            }
            else
            {
                label01State.Text = "二值化显示状态：反色";
                ThevePictureProcess.isAntiColor = true;
            }
            GetBmpInfo();
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
                    if (ThevePictureProcess.isAntiColor)
                    {
                        labelPixelInfo.Text = string.Format("像素信息：PicPro.img[{1}][{0}]={2}", (int)a, (int)b, 255 - bm.GetPixel((int)a, (int)b).R);
                    }
                    else
                    {
                        labelPixelInfo.Text = string.Format("像素信息：PicPro.img[{1}][{0}]={2}", (int)a, (int)b, bm.GetPixel((int)a, (int)b).R);
                    }
                }
            }
            catch
            {
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
                labelPicAllCnt.Text = "图片总数：0";
            }
        }

        //Debug
        public void DebugLog(string str, bool isDebugMode = true)
        {
            if (isDebugMode == true)
            {
                if (str != "" && groupBox3.Visible == true)
                {
                    textBoxDebug.AppendText(str + "\r\n");
                }
            }
        }


        //同步程序
        private void buttonSyncCode_Click(object sender, EventArgs e)
        {
            //恢复代码
            if (File.Exists(@"PicProCode.c"))
            {
                richTextBoxPicPro.Text = File.ReadAllText(@"PicProCode.c");
            }
        }
        //保存当前图像
        private void buttonSaveBmp_Click(object sender, EventArgs e)
        {
            if (pictureShow.Image != null)
            {
                DebugLog("已保存至：" + ThevePictureReceive.BmpSave(@"BmpSave/img", ThevePictureReceive.PicCount++));
            }
        }
        //清除图片和信息
        private void buttonClearBmp_Click(object sender, EventArgs e)
        {
            pictureShow.Image = null;
            if (stopwatchTime.IsRunning)
            {
                stopwatchTime.Reset();
            }
            cntRecvAllPic = 0;
            cntRecvFailPic = 0;
            rateError = 0;
            rateFrame = 0;
            timeSumFrame = 0;
            timeOneFrame = 0;
            timeWifiRun = 0;
        }

        // 显示DataGridView序号
        private void dataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            Rectangle rectangle = new Rectangle(e.RowBounds.Location.X, e.RowBounds.Location.Y, dgv.RowHeadersWidth - 4, e.RowBounds.Height);
            TextRenderer.DrawText(e.Graphics, e.RowIndex.ToString(), dgv.RowHeadersDefaultCellStyle.Font, rectangle, dgv.RowHeadersDefaultCellStyle.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
        }
        #endregion
        #region 菜单栏  
        int IndexTabControlLast = 0;
        //任务栏页面更改时
        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (IndexTabControlLast)
            {
                case 0:
                    DisadbleLocalForm();
                    break;
                case 1:
                    DisadbleUartForm();
                    break;
                case 2:
                    DisadbleWifiForm();
                    break;
                default:
                    break;
            }
            IndexTabControlLast = tabControlMode.SelectedIndex;
            switch (tabControlMode.SelectedIndex)
            {
                case 1:
                    TheveSerialPort.Scan(comboBoxCom);
                    break;
                case 2:
                    TheveWifi.IpScan(comboBoxIP);
                    break;
                default:
                    break;
            }
        }
        //隐藏Debug 
        private void 隐藏相关信息栏ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TheveMenuStrip.SingleCheckTrans(sender);
            TheveIniFiles.IniWriteValue("图像处理", "隐藏相关信息栏", 隐藏相关信息栏ToolStripMenuItem.Checked.ToString());

            if (隐藏相关信息栏ToolStripMenuItem.Checked)
            {
                groupBox2.Height = tabControl1.Height;
                groupBoxInfo.Height = groupBox2.Height;
                groupBox3.Visible = false;
            }
            else
            {
                groupBox2.Height = groupBox3.Location.Y - groupBox2.Location.Y - 10;
                groupBoxInfo.Height = groupBox2.Height;
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
                    pictureShow.Dock = DockStyle.Fill;
                    break;

                case "等宽长缩放":
                    pictureShow.Dock = DockStyle.None;
                    ThevePictureProcess.AdaptSize(pictureShow, groupBox2.Width - 6, groupBox2.Height - 24);
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
                    ThevePictureProcess.isMultiFuncMode = false;
                    break;

                case "多函数模式":
                    ThevePictureProcess.isMultiFuncMode = true;
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
        //窗口大小改变时
        private void SmartCarMain_SizeChanged(object sender, EventArgs e)
        {
            if (拉伸模式ToolStripMenuItem.Checked)
            {
                pictureShow.Height = groupBox2.Height;
                pictureShow.Width = groupBox2.Width;
            }
            else
            {
                ThevePictureProcess.AdaptSize(pictureShow, groupBox2.Width, groupBox2.Height);
            }
        }
        //关闭软件
        private void SmartCarMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (thrRecv != null && thrRecv.IsAlive)
            {
                thrRecv.Abort();
                UdpStopReceive();
            }
        }
        #endregion
        #region 串口图传

        int PictureReceiveCount = 0;
        //接收数据字节
        int CntUartRecvBytes = 0;
        //开始接收标志
        bool IsUartRecvStart = false;

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
                                buttonUartLock.Enabled = true;

                                if (thrCheck == null)
                                {
                                    thrCheck = new Thread(UartCheck);
                                    thrCheck.Start();
                                }
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
                                buttonUartLock.Enabled = false;
                                if (buttonUartLock.Text == "解锁(&D)")
                                {
                                    buttonUartLock_Click(null, null);
                                }
                                if (thrCheck != null)
                                {
                                    thrCheck.Abort();
                                }
                                break;
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //锁定
        private void buttonUartLock_Click(object sender, EventArgs e)
        {
            if (buttonUartLock.Text == "锁定(&D)")
            {
                if (textBoxPicHeight.Text == "" || textBoxPicWidth.Text == "")
                {
                    MessageBox.Show("请先填写接收图像的宽度和高度");
                }
                else
                {
                    try
                    {
                        ThevePictureReceive.CreatBmpSize(Convert.ToInt32(textBoxPicWidth.Text), Convert.ToInt32(textBoxPicHeight.Text));
                        ButtonUartEnable(true);
                        //保存信息
                        if (comboBoxCom.Text != "")
                        {
                            TheveIniFiles.IniWriteValue("串口传图", "串口号", comboBoxCom.SelectedItem.ToString());
                            TheveIniFiles.IniWriteValue("串口传图", "波特率", comboBoxBaud.Text);
                        }
                        TheveIniFiles.IniWriteValue("串口传图", "图像宽", textBoxPicWidth.Text);
                        TheveIniFiles.IniWriteValue("串口传图", "图像高", textBoxPicHeight.Text);
                        buttonUartLock.Text = "解锁(&D)";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                }

            }
            else
            {
                ThevePictureProcess.isCompiledCode = false;
                PicPro.ClearInherit();
                ButtonUartEnable(false);
                //解锁时关闭接收图像功能
                if (buttonUartRecvStart.Text == "停止接收图像(&G)")
                {
                    IsUartRecvStart = false;
                    buttonUartRecvStart.Text = "开始接收图像(&G)";
                    DebugLog("停止接收图像\r\n");
                }
                buttonUartLock.Text = "锁定(&D)";
            }

        }
        //开始接收图像
        private void buttonStartReceive_Click(object sender, EventArgs e)
        {
            if (buttonUartRecvStart.Text == "开始接收图像(&G)")
            {
                IsUartRecvStart = true;
                DebugLog("开始接收图像");
                timerUartCheck.Start();
                buttonUartRecvStart.Text = "停止接收图像(&G)";
            }
            else
            {
                IsUartRecvStart = false;
                listRecvPicData.Clear();
                DebugLog("停止接收图像");
            }
        }
        //按钮使能
        private void ButtonUartEnable(bool state)
        {
            //锁定后 不能改  
            textBoxPicHeight.Enabled = !state;
            textBoxPicWidth.Enabled = !state;
            //锁定后 才能改 true 
            buttonUartRecvStart.Enabled = state;
            buttonClearBmp.Enabled = state;
            buttonUartSaveBmp.Enabled = state;
            buttonUartRunCode.Enabled = state;
            listRecvPicData.Clear();
            RichTextReadState(state);
        }
        //接收中断
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (serialPort1.BytesToRead > 0 && IsUartRecvStart == true)
                {
                    //计时系统
                    if (IsRecvFrame)
                    {
                        TimerEnd(stopwatchSum, "帧间隔用时：", ref timeSumFrame);
                    }
                    TimerStart(stopwatchSum);
                    TimerStart(stopwatch);
                    TimerStart(stopwatchTime);
                    //接收数据
                    int CntRecvBytes = serialPort1.BytesToRead;
                    byte[] buff = new byte[CntRecvBytes];
                    CntRecvBytes = serialPort1.Read(buff, 0, CntRecvBytes);
                    if (CntRecvBytes > 0)
                    {
                        for (int i = 0; i < CntRecvBytes; i++)
                        {
                            listRecvPicData.Add(buff[i]);
                        }
                        CntUartRecvBytes += CntRecvBytes;
                    }
                }
                else//杂乱数据
                {
                    serialPort1.DiscardInBuffer();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        //定时器处理函数
        private void timerUartCheck_Tick(object sender, EventArgs e)
        {
            //CheckImage();
            //labelRecvByte.Text = "传输字节量：" + listRecvPicData.Count.ToString();
        }
        //检查是否满足图像条件
        private void CheckImage()
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
                                PicPro.img[i, j] = listRecvPicData[index + 2 + j + i * ThevePictureReceive.Width];
                                PicPro.imgOriginal[i, j] = PicPro.img[i, j];
                            }
                        }
                        listRecvPicData.RemoveRange(0, index + ThevePictureReceive.Size + 3);
                        pictureShow.Image = ThevePictureReceive.ShowBMP();
                        PictureReceiveCount++;
                        labelPicIndex.Text = "当前图片编号：" + PictureReceiveCount.ToString();

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
                                if (buttonUartRecvStart.Text == "停止接收图像(&G)")
                                {
                                    buttonUartRecvStart.Text = "开始接收图像(&G)";
                                    DebugLog("停止接收图像");
                                    listRecvPicData.Clear();
                                    IsUartRecvStart = false;
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
        void UartCheck()
        {
            DebugLog("[检查线程开始]", IsDebugMode);
            while (IsUdpcRecvStart)
            {
                while (IsWifiRecvStop)
                {
                    timeWifiRun = stopwatchTime.Elapsed.TotalSeconds;
                    labelWifiTime.Text = "图传启动时间：" + timeWifiRun.ToString("#0.0 s");
                    rateFrame = timeWifiRun == 0 ? 0 : cntRecvAllPic / timeWifiRun;
                    labelRecvFrameRate.Text = "平均帧率：" + rateFrame.ToString("#0.0");
                }
                //显示数据
                rateError = cntRecvAllPic == 0 ? 0 : (float)cntRecvFailPic / cntRecvAllPic * 100;
                timeWifiRun = stopwatchTime.Elapsed.TotalSeconds;
                rateFrame = timeWifiRun == 0 ? 0 : cntRecvAllPic / timeWifiRun;
                labelRecvErrRate.Text = "错误率：" + rateError.ToString("#0.000%");
                labelRecvByte.Text = "接收字节数：" + listRecvPicData.Count;
                labelRecvCnt.Text = "接收图片数：" + cntRecvAllPic;
                labelRecvOneFrameTime.Text = "平均单帧接收时间：" + (cntRecvAllPic == 0 ? 0 : (timeOneFrame / cntRecvAllPic)).ToString("#0.000 000 s");
                labelRecvSumFrameTime.Text = "平均接收时间：" + (cntRecvAllPic == 0 ? 0 : (timeSumFrame / cntRecvAllPic)).ToString("#0.000 s");
                labelRecvFrameRate.Text = "平均帧率：" + rateFrame.ToString("#0.0 fps");
                labelRecvFrameRateEff.Text = "平均有效帧率：" + (timeSumFrame == 0 ? 0 : (cntRecvAllPic / timeSumFrame)).ToString("#0.0 fps");
                labelWifiTime.Text = "图传启动时间：" + timeWifiRun.ToString("#0.0 s");
                //查找协议
                int index = 0;
                while (index <= listRecvPicData.Count - ThevePictureReceive.Size - 4)
                {
                    if (listRecvPicData[index] == 0xFC && listRecvPicData[index + 1] == 0xCF)
                    {
                        cntRecvAllPic++;
                        if (listRecvPicData[index + ThevePictureReceive.Size + 2] == 0xCF && listRecvPicData[index + ThevePictureReceive.Size + 3] == 0xFC)
                        {
                            for (int i = 0; i < ThevePictureReceive.Height; i++)
                            {
                                for (int j = 0; j < ThevePictureReceive.Width; j++)
                                {
                                    PicPro.img[i, j] = listRecvPicData[index + 2 + j + i * ThevePictureReceive.Width];
                                    PicPro.imgOriginal[i, j] = PicPro.img[i, j];
                                }
                            }
                            //帧间隔 计时处理
                            IsRecvFrame = true;
                            TimerEnd(stopwatch, "此帧用时：", ref timeOneFrame);
                            try
                            {
                                listRecvPicData.RemoveRange(0, index + ThevePictureReceive.Size + 4);
                                pictureShow.Image = ThevePictureReceive.ShowBMP();
                            }
                            catch { }
                            //自动保存
                            if (checkBoxWifiAutoSave.Checked == true)
                            {
                                DebugLog("已保存至：" + ThevePictureReceive.BmpSave(@"BmpAutoSave\img", ThevePictureReceive.PicAutoCount++));
                            }
                            //自动运行程序
                            if (checkBoxWifiRunCode.Checked == true)
                            {
                                if (ThevePictureProcess.RunCode(richTextBoxPicPro.Text))
                                {
                                    DebugLog("接收图像序号" + cntRecvAllPic.ToString() + ThevePictureProcess.Log);
                                }
                                else//编译错误
                                {
                                    if (buttonUartRecvStart.Text == "停止接收图像(&G)")
                                    {
                                        buttonUartRecvStart.Text = "开始接收图像(&G)";
                                        DebugLog("停止接收图像");
                                        listRecvPicData.Clear();
                                        IsUartRecvStart = false;
                                    }
                                    MessageBox.Show(ThevePictureProcess.Log, "程序错误");
                                }
                                pictureShow.Image = ThevePictureReceive.ShowBMP();
                                //参数监视器
                                WatchShow();
                                //数组查看器
                                WatchArrayShow();
                            }
                        }
                        else
                        {
                            cntRecvFailPic++;
                            listRecvPicData.RemoveRange(0, index + ThevePictureReceive.Size + 4);
                            DebugLog("此帧图像数据错乱,已丢弃。");
                        }
                    }
                    index++;
                }
            }
            DebugLog("[检查线程关闭]", IsDebugMode);
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

            if (CompileRunCode())
            {
                DebugLog("接收图像序号" + PictureReceiveCount.ToString() + ThevePictureProcess.Log);
            }
            else
            {
                if (buttonUartLock.Text != "锁定(&D)")
                {
                    buttonUartLock_Click(null, null);
                }
            }
            pictureShow.Image = ThevePictureReceive.ShowBMP();
            //参数监视器
            WatchShow();
            //数组查看器
            WatchArrayShow();
        }
        //关闭串口所有功能
        private void DisadbleUartForm()
        {
            if (buttonSerialOpen.Text != "打开(&O)")
            {
                buttonSerialOpen_Click(null, null);
            }


        }


        #endregion
        #region WiFi图传 

        //开关：在连接UDP报文阶段为true，否则为false 
        bool IsUdpcRecvStart = false;//开始接受
        bool IsRecvFrame = false;//接收到了一帧
        bool IsWifiRecvStop = true;//停止接收
        //UDP对象
        UdpClient udpcRecv;//UDP客户端
        IPEndPoint localIpep;
        //线程：连接UDP报文 检查传输协议
        Thread thrRecv;//接收数据线程
        Thread thrCheck;//检查协议线程

        //计时器
        Stopwatch stopwatch = new Stopwatch();
        Stopwatch stopwatchSum = new Stopwatch();
        Stopwatch stopwatchTime = new Stopwatch();
        double timeOneFrame = 0;//接收一帧的时间
        double timeSumFrame = 0;//帧间隔时间
        double timeWifiRun = 0;//接收字节时运行时间
        //参数
        double rateFrame = 0;//帧率
        double rateError = 0;//错误率
        //计数器
        int cntRecvAllPic = 0;//接收的图片总数
        int cntRecvFailPic = 0;//接收失败的图片


        //连接
        private void buttonWifiLink_Click(object sender, EventArgs e)
        {
            if (buttonWifiLink.Text == "连接(&L)")
            {
                if (comboBoxIP.Text != null && textBoxPort.Text != null)
                {
                    if (UdpStartReceive(comboBoxIP.Text, textBoxPort.Text))
                    {
                        comboBoxIP.Enabled = false;
                        textBoxPort.Enabled = false;
                        buttonWifiLock.Enabled = true;
                        buttonWifiLink.Text = "断开(&L)";
                    }
                }
            }
            else
            {
                comboBoxIP.Enabled = true;
                textBoxPort.Enabled = true;
                buttonWifiLock.Enabled = false;
                UdpStopReceive();
                buttonWifiLink.Text = "连接(&L)";
                if (buttonWifiLock.Text == "解锁(&D)")
                {
                    buttonWifiLock_Click(null, null);
                }
            }
        }
        //锁定
        private void buttonWifiLock_Click(object sender, EventArgs e)
        {
            if (buttonWifiLock.Text == "锁定(&D)")
            {
                if (textBoxWifiHeight.Text == "" || textBoxWifiWidth.Text == "")
                {
                    MessageBox.Show("请先填写接收图像的宽度和高度");
                }
                else
                {
                    try
                    {
                        ThevePictureReceive.CreatBmpSize(Convert.ToInt32(textBoxWifiWidth.Text), Convert.ToInt32(textBoxWifiHeight.Text));
                        PicPro.CreatImg(ThevePictureReceive.Height, ThevePictureReceive.Width);
                        ButtonWifiEnable(true);
                        buttonWifiLock.Text = "解锁(&D)";
                        //保存信息 
                        TheveIniFiles.IniWriteValue("WIFI传图", "端口号", textBoxPort.Text);
                        TheveIniFiles.IniWriteValue("WIFI传图", "图像宽", textBoxWifiWidth.Text);
                        TheveIniFiles.IniWriteValue("WIFI传图", "图像高", textBoxWifiHeight.Text);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "错误");
                    }
                }
            }
            else
            {
                ThevePictureProcess.isCompiledCode = false;
                PicPro.ClearInherit();
                ButtonWifiEnable(false);
                if (buttonWifiRecvStart.Text != "开始接收图像(&G)")
                {
                    buttonWifiRecvStart_Click(null, null);
                }
                buttonWifiLock.Text = "锁定(&D)";
            }
        }
        //开始/暂停接收图像
        private void buttonWifiRecvStart_Click(object sender, EventArgs e)
        {
            if (buttonWifiRecvStart.Text == "开始接收图像(&G)")
            {
                IsWifiRecvStop = false;
                buttonWifiRecvStart.Text = "暂停接收图像(&G)";
            }
            else
            {
                IsWifiRecvStop = true;
                buttonWifiRecvStart.Text = "开始接收图像(&G)";
            }
        }

        //运行程序
        private void buttonWifiRun_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ThevePictureReceive.Height; i++)
            {
                for (int j = 0; j < ThevePictureReceive.Width; j++)
                {
                    PicPro.img[i, j] = PicPro.imgOriginal[i, j];
                }
            }

            if (CompileRunCode())
            {
                DebugLog("接收图像序号" + cntRecvAllPic.ToString() + ThevePictureProcess.Log);
            }
            else
            {
                if (buttonWifiRecvStart.Text == "停止接收图像(&G)")
                {
                    buttonWifiRecvStart.Text = "开始接收图像(&G)";
                    DebugLog("停止接收图像");
                    listRecvPicData.Clear();
                    IsWifiRecvStop = true;
                }
            }
            pictureShow.Image = ThevePictureReceive.ShowBMP();
            //参数监视器
            WatchShow();
            //数组查看器
            WatchArrayShow();
        }
        //按钮使能
        private void ButtonWifiEnable(bool state)
        {
            //锁定后 不能改   
            textBoxWifiHeight.Enabled = !state;
            textBoxWifiWidth.Enabled = !state;
            //锁定后 才能改 true  
            buttonWifiRecvStart.Enabled = state;
            buttonWifiClear.Enabled = state;
            buttonWifiSaveBmp.Enabled = state;
            buttonWifiRunCode.Enabled = state;
            listRecvPicData.Clear();
            RichTextReadState(state);
        }
        // 开始接收UDP信息 
        public bool UdpStartReceive(string userIPadress, string userPort)
        {
            try
            {
                if (!IsUdpcRecvStart) // 未连接的情况，开始连接
                {
                    localIpep = new IPEndPoint(IPAddress.Parse(userIPadress), Convert.ToInt32(userPort)); // 本机IP和连接端口号
                    udpcRecv = new UdpClient(localIpep);
                    thrRecv = new Thread(UdpReceiveInfo);
                    thrRecv.Start();
                    thrCheck = new Thread(UdpCheckInfo);
                    thrCheck.Start();
                    IsUdpcRecvStart = true;
                    DebugLog("WiFi数据接收器启动成功");
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误");
            }
            return false;
        }

        // 关闭UDP
        public void UdpStopReceive()
        {
            if (IsUdpcRecvStart)
            {
                thrRecv.Abort();
                udpcRecv.Close();
                thrCheck.Abort();
                IsUdpcRecvStart = false;
                DebugLog("WiFi数据接收器关闭成功。");
            }
        }

        // 接收数据
        private void UdpReceiveInfo(object obj)
        {
            DebugLog("[接收线程开始]", IsDebugMode);
            while (IsUdpcRecvStart)
            {
                while (IsWifiRecvStop) ;
                //阻塞接收数据
                byte[] bytRecv = udpcRecv.Receive(ref localIpep);
                if (IsWifiRecvStop) continue;
                //计时系统
                if (IsRecvFrame)
                {
                    TimerEnd(stopwatchSum, "帧间隔用时：", ref timeSumFrame);
                }
                TimerStart(stopwatchSum);
                TimerStart(stopwatch);
                TimerStart(stopwatchTime);
                //加入数据
                for (int i = 0; i < bytRecv.Length; i++)
                {
                    listRecvPicData.Add(bytRecv[i]);
                }
            }
            DebugLog("[接收线程开始]", IsDebugMode);
        }
        //检查数据
        private void UdpCheckInfo(object obj)
        {
            DebugLog("[检查线程开始]", IsDebugMode);
            while (IsUdpcRecvStart)
            {
                while (IsWifiRecvStop)
                {
                    timeWifiRun = stopwatchTime.Elapsed.TotalSeconds;
                    labelWifiTime.Text = "图传启动时间：" + timeWifiRun.ToString("#0.0 s");
                    rateFrame = timeWifiRun == 0 ? 0 : cntRecvAllPic / timeWifiRun;
                    labelRecvFrameRate.Text = "平均帧率：" + rateFrame.ToString("#0.0 fps");
                }
                try
                {

                    //显示数据
                    rateError = cntRecvAllPic == 0 ? 0 : (float)cntRecvFailPic / cntRecvAllPic * 100;
                    timeWifiRun = stopwatchTime.Elapsed.TotalSeconds;
                    rateFrame = timeWifiRun == 0 ? 0 : cntRecvAllPic / timeWifiRun;
                    labelRecvErrRate.Text = "错误率：" + rateError.ToString("#0.000%");
                    labelRecvByte.Text = "接收字节数：" + listRecvPicData.Count;
                    labelRecvCnt.Text = "接收图片数：" + cntRecvAllPic;
                    labelRecvOneFrameTime.Text = "平均单帧接收时间：" + (cntRecvAllPic == 0 ? 0 : (timeOneFrame / cntRecvAllPic)).ToString("#0.000 000 s");
                    labelRecvSumFrameTime.Text = "平均接收时间：" + (cntRecvAllPic == 0 ? 0 : (timeSumFrame / cntRecvAllPic)).ToString("#0.000 s");
                    labelRecvFrameRate.Text = "平均帧率：" + rateFrame.ToString("#0.0 fps");
                    labelRecvFrameRateEff.Text = "平均有效帧率：" + (timeSumFrame == 0 ? 0 : (cntRecvAllPic / timeSumFrame)).ToString("#0.0 fps");
                    labelWifiTime.Text = "图传启动时间：" + timeWifiRun.ToString("#0.0 s");
                }
                catch
                {
                }
                //查找协议
                int index = 0;
                while (index <= listRecvPicData.Count - ThevePictureReceive.Size - 4)
                {
                    if (listRecvPicData[index] == 0xFC && listRecvPicData[index + 1] == 0xCF)
                    {
                        cntRecvAllPic++;
                        if (listRecvPicData[index + ThevePictureReceive.Size + 2] == 0xCF && listRecvPicData[index + ThevePictureReceive.Size + 3] == 0xFC)
                        {
                            for (int i = 0; i < ThevePictureReceive.Height; i++)
                            {
                                for (int j = 0; j < ThevePictureReceive.Width; j++)
                                {
                                    PicPro.img[i, j] = listRecvPicData[index + 2 + j + i * ThevePictureReceive.Width];
                                    PicPro.imgOriginal[i, j] = PicPro.img[i, j];
                                }
                            }
                            //帧间隔 计时处理
                            IsRecvFrame = true;
                            TimerEnd(stopwatch, "此帧用时：", ref timeOneFrame);
                            try
                            {
                                listRecvPicData.RemoveRange(0, index + ThevePictureReceive.Size + 4);
                                pictureShow.Image = ThevePictureReceive.ShowBMP();
                            }
                            catch { }
                            //自动保存
                            if (checkBoxWifiAutoSave.Checked == true)
                            {
                                DebugLog("已保存至：" + ThevePictureReceive.BmpSave(@"BmpAutoSave\img", ThevePictureReceive.PicAutoCount++));
                            }
                            //自动运行程序
                            if (checkBoxWifiRunCode.Checked == true)
                            {
                                if (ThevePictureProcess.RunCode(richTextBoxPicPro.Text))
                                {
                                    DebugLog("接收图像序号" + cntRecvAllPic.ToString() + ThevePictureProcess.Log);
                                }
                                else//编译错误
                                {
                                    if (buttonWifiRecvStart.Text == "停止接收图像(&G)")
                                    {
                                        buttonWifiRecvStart.Text = "开始接收图像(&G)";
                                        DebugLog("停止接收图像");
                                        listRecvPicData.Clear();
                                        IsWifiRecvStop = true;
                                    }
                                    MessageBox.Show(ThevePictureProcess.Log, "程序错误");
                                }
                                pictureShow.Image = ThevePictureReceive.ShowBMP();
                                //参数监视器
                                WatchShow();
                                //数组查看器
                                WatchArrayShow();
                            }
                        }
                        else
                        {
                            cntRecvFailPic++;
                            listRecvPicData.RemoveRange(0, index + ThevePictureReceive.Size + 4);
                            DebugLog("此帧图像数据错乱,已丢弃。");
                        }
                    }
                    index++;
                }
            }
            DebugLog("[检查线程关闭]", IsDebugMode);
        }
        //开始计时
        void TimerStart(Stopwatch stopwatch)
        {
            if (!stopwatch.IsRunning)//计时系统
            {
                stopwatch.Restart();
            }
        }
        //结束计时
        void TimerEnd(Stopwatch stopwatch, string txt, ref double timeCnt)
        {
            if (stopwatch.IsRunning)
            {
                stopwatch.Stop();
                TimeSpan dt = stopwatch.Elapsed;
                if (dt.TotalSeconds < 1)
                {
                    timeCnt += dt.TotalSeconds;
                }
                DebugLog(txt + dt.TotalSeconds.ToString("#0.000 s"));
            }
        }
        //关闭计时
        void TimerClose(Stopwatch stopwatch)
        {
            try
            {
                if (stopwatch.IsRunning)
                {
                    stopwatch.Stop();
                }
            }
            catch { }
        }
        //关闭wifi所有功能
        private void DisadbleWifiForm()
        {
            TimerClose(stopwatch);
            TimerClose(stopwatchSum);
            TimerClose(stopwatchTime);
            UdpStopReceive();
            if (buttonWifiLink.Text != "连接(&L)")
            {
                buttonWifiLink_Click(null, null);
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


    }
}

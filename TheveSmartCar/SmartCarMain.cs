using SmartCar;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ThevePictureProcessDll;
using TheveSmartCar.Properties;

namespace TheveSmartCar
{
   public partial class SmartCarMain : Form
    {
        bool Unique = false;
        int transFlag = 0;
        Toolss t;
        List<string> fileList = new List<string>();
        private void Form1_Load(object sender, EventArgs e)
        {
            TheveIniFiles.InitPath(Application.StartupPath + @"\SmartCarConfig.ini");
            VerifyState(); 
            //自恢复
            if (TheveIniFiles.IniReadValue("图像处理", "自恢复") == "True")
            {
                buttonIniRecover_Click(null, null);
            }
            ButtonEnable(false);
            //连续时间刻度
            ThevePictureProcess.InitInterval(comboBoxAutoInterval);
            HelpText();
            labelOrinial.Text = "";

        }
        #region 图像处理
        //浏览文件夹
        private void buttonDirectionBmp_Click(object sender, EventArgs e)
        {
            textBoxDirectionBmp.Text = ThevePictureProcess.SelectDirection();
            if (textBoxDirectionBmp.Text != "")
            {
                //fileList = ThevePictureProcess.GetAllPicPath(textBoxDirectionBmp.Text);
                //ThevePictureProcess.PicCount = 0;
                //labelPicAllNum.Text = "图片总数：" + fileList.Count.ToString();
                //if(fileList.Count>0)
                //{
                //    TheveToolStrip.DirectionRecent(ToolStripMenuItemFile,textBoxDirectionBmp.Text);
                //}

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
                throw;
            }
            GetBmpInfo();
        }
        //图像处理处理
        private void GetBmpInfo(bool pass = false)
        {
            if (ThevePictureProcess.PicCount < 0)
            {
                ThevePictureProcess.PicCount = 0;
                return;
            }
            if (ThevePictureProcess.PicCount == fileList.Count)
            {
                ThevePictureProcess.PicCount = fileList.Count - 1;
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
            if (ThevePictureProcess.PicCount >= fileList.Count)
            {
                MessageBox.Show("超过文件内图片总数。", "错误");
                return;
            }

            if (ThevePictureProcess.GetBMP(fileList[ThevePictureProcess.PicCount]) == false)
            {
                return;
            }

            labelDepth.Text = ThevePictureProcess.Depth;
            labelResoultion.Text = ThevePictureProcess.Resoultion;
            labelPicNumShow.Text = "当前图片编号：" + ThevePictureProcess.PicCount;

            if (pass == false)
            {
                if (richTextBoxPicPro.Text != "" && ThevePictureProcess.ChangeFlag == false)
                {
                    if (ThevePictureProcess.RunCode(richTextBoxPicPro.Text))
                    {
                        DebugLog(fileList[ThevePictureProcess.PicCount].ToString() + ThevePictureProcess.Log);
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
            if (transFlag == 2)
            {
                pictureShow.Image = ThevePictureProcess.ShowBMP(true);
            }
            else
            {
                pictureShow.Image = ThevePictureProcess.ShowBMP();
            }
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
                buttonCodeLock.Text = "解锁(&D)";
                ButtonEnable(true);
            }
            else
            {
                buttonCodeLock.Text = "锁定(&D)";
                ThevePictureProcess.RunCodeFlag = false;
                ButtonEnable(false);
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
            ThevePictureProcess.CodeLock = state;
            richTextBoxPicPro.ReadOnly = state;
            buttonAutoInterval.Enabled = state;
            buttonLast.Enabled = state;
            buttonNext.Enabled = state;
            buttonJump.Enabled = state;
            buttonChange.Enabled = state;
            textBoxDirectionBmp.Enabled = !state;
            buttonDirectionBmp.Enabled = !state;
            buttonAntiColor.Enabled = state;
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
            TheveIniFiles.IniWriteValue("图像处理", "自恢复", toolStripMenuItemIniRecover.Checked.ToString());
            TheveIniFiles.IniWriteValue("图像处理", "文件夹路径", textBoxDirectionBmp.Text);
            TheveIniFiles.IniWriteValue("图像处理", "主题", 黑色ToolStripMenuItem.Checked.ToString()); 

            int cnt = 0;
            foreach (var i in ToolStripMenuItemFile.DropDownItems)
            {
                cnt++;
                TheveIniFiles.IniWriteValue("文件夹记录", "最近打开的文件夹" + cnt.ToString(), i.ToString());
            }
            //DebugLog("文件路径" + textBoxDirectionBmp.Text + @"\" + ThevePictureProcess.PicCount.ToString() + ".bmp 已保存。");
            File.WriteAllText(@"PicProCode.c", richTextBoxPicPro.Text);
            DebugLog("图像处理程序已保存在 PicProCode.txt 文件中。");
        }
        //恢复INI设置
        private void buttonIniRecover_Click(object sender, EventArgs e)
        {
            if (TheveIniFiles.ExistINIFile() == true)
            {
                if (TheveIniFiles.GetItemsKeys("图像处理") != null)
                {
                    string[] pathCount = TheveIniFiles.GetItemsKeys("文件夹记录");
                    if (pathCount != null)
                    {
                        foreach (var i in pathCount)
                        {
                            TheveMenuStrip.DirectionRecent(ToolStripMenuItemFile, TheveIniFiles.IniReadValue("文件夹记录", i));
                        }
                    }

                    textBoxDirectionBmp.Text = TheveIniFiles.IniReadValue("图像处理", "文件夹路径");

                    if (File.Exists(@"PicProCode.c"))
                    {
                        richTextBoxPicPro.Text = "";
                        richTextBoxPicPro.AppendText(File.ReadAllText(@"PicProCode.c"));
                    }
                    if (TheveIniFiles.IniReadValue("图像处理", "自恢复") == "True")
                    {
                        toolStripMenuItemIniRecover.Checked = true;
                    }
                    if (TheveIniFiles.IniReadValue("图像处理", "显示模式") == "拉伸模式")
                    {
                        显示模式ToolStripMenuItem_DropDownItemClicked(null, new ToolStripItemClickedEventArgs(拉伸模式ToolStripMenuItem));
                    }

                    if (TheveIniFiles.IniReadValue("图像处理", "编译模式") == "多函数模式")
                    {
                        编译模式ToolStripMenuItem_DropDownItemClicked(null, new ToolStripItemClickedEventArgs(多函数模式ToolStripMenuItem));
                    }

                    if (TheveIniFiles.IniReadValue("图像处理", "主题") == "黑色")
                    {
                        主题ToolStripMenuItem_DropDownItemClicked(null, new ToolStripItemClickedEventArgs(黑色ToolStripMenuItem));
                    }

                    string name = TheveIniFiles.IniReadValue("图像处理", "鼠标样式");
                    foreach (ToolStripItem i in 鼠标样式ToolStripMenuItem.DropDownItems)
                    {
                        if(i.Name == name)
                        {
                            鼠标样式ToolStripMenuItem_DropDownItemClicked(null, new ToolStripItemClickedEventArgs(i));
                            break;
                        }
                    }
                    DebugLog("已恢复设置。");
                }

            }
        }
        #endregion
        #region 界面设置
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
当“灰度”框未勾选时：
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
程序内的代码在一个默认的函数中，不能再写函数。（有待完善）

支持数学库：(例)
Math.Sqrt();
Math.Abs();
Math.Sin();
...
具体内容查看C#的Math语法。

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

            //if (ThevePictureProcess.ChangeFlag == false)
            //{
            //    labelOrinial.Visible = true;
            //    ThevePictureProcess.ChangeFlag = true;
            //    GetBmpInfo(true);
            //}
            //else
            //{
            //    labelOrinial.Visible = false;
            //    ThevePictureProcess.ChangeFlag = false;
            //    GetBmpInfo();
            //} 
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
                    label1.Text = string.Format("像素信息：PicPro.img[{1}][{0}]={2}", (int)a, (int)b, bm.GetPixel((int)a, (int)b).R);
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
                GetDirectionFiles(path);
            }
            else
            {
                labelPicAllNum.Text = "图片总数：0";
            }
        }
        //获取文件夹内的文件列表
        private void GetDirectionFiles(string path)
        {
            fileList = ThevePictureProcess.GetAllPicPath(path);
            ThevePictureProcess.PicCount = 0;
            labelPicAllNum.Text = "图片总数：" + fileList.Count.ToString();
            if (fileList.Count > 0)
            {
                TheveMenuStrip.DirectionRecent(ToolStripMenuItemFile, textBoxDirectionBmp.Text, true);
            }
        }


        #endregion
        #region 菜单栏
        //是否自定义设置启动
        private void toolStripMenuItemIniRecover_Click(object sender, EventArgs e)
        {
            toolStripMenuItemIniRecover.Checked = !toolStripMenuItemIniRecover.Checked;
        }
        //隐藏Debug 
        private void ToolStripMenuItemHideDebug_Click(object sender, EventArgs e)
        {
            if (ToolStripMenuItemHideDebug.Checked)
            {
                groupBox2.Height = groupBox3.Location.Y - groupBox2.Location.Y - 10;
                groupBox3.Visible = true;
                ToolStripMenuItemHideDebug.Checked = false;
            }
            else
            {
                groupBox2.Height = tabControl1.Height;
                groupBox3.Visible = false;
                ToolStripMenuItemHideDebug.Checked = true;
            }
            if (pictureShow.Image != null)
            {
                GetBmpInfo();
            }
        }

        private void ToolStripMenuItemFile_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        { 
            if (buttonCodeLock.Text == "解锁(&D)")
            {
                buttonCodeLock_Click(null, null); 
                textBoxDirectionBmp.Text = e.ClickedItem.Text;
            } 
        }
        //窗体黑色主题
        void MainThemeChange()
        {
            foreach (Control c in this.Controls)
            {
                ThemeChange(c);
            }
            foreach (Control b in tabControl1.TabPages)
            {
                foreach (Control c in b.Controls)
                {
                    ThemeChange(c);
                }
            }
            foreach (Control c in groupBox1.Controls)
            {
                if (c is Button)
                {
                    (c as Button).FlatAppearance.BorderSize = 0;
                    (c as Button).FlatStyle = FlatStyle.Flat;
                    ThemeChange(c);
                }
            }
            ThemeChange(textBoxDirectionBmp);
            ThemeChange(textBoxStartNum);
            ThemeChange(textBoxDebug);
            ThemeChange(comboBoxAutoInterval);
            ToolStripMenuItemFile.BackColor = Color.FromArgb(51, 51, 55);
            this.BackColor = Color.FromArgb(51, 51, 55);

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
            TheveMenuStrip.RadioCheck(显示模式ToolStripMenuItem, e.ClickedItem);
            TheveIniFiles.IniWriteValue("图像处理", "显示模式", e.ClickedItem.ToString());
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

            TheveMenuStrip.RadioCheck(编译模式ToolStripMenuItem, e.ClickedItem);
            TheveIniFiles.IniWriteValue("图像处理", "编译模式", e.ClickedItem.ToString());
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
        private void 主题ToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            TheveMenuStrip.RadioCheck(主题ToolStripMenuItem, e.ClickedItem);
            TheveIniFiles.IniWriteValue("图像处理", "主题", e.ClickedItem.ToString());
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
         
        private void 鼠标样式ToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        { 
            TheveMenuStrip.RadioCheck(鼠标样式ToolStripMenuItem, e.ClickedItem);
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
        private void 赞赏ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form f = new Form();
            f.Icon = Resources.car;
            f.StartPosition = FormStartPosition.CenterScreen;
            f.Size = new Size(1000, 1000);
            f.BackgroundImage = Resources.赞赏码;
            f.BackgroundImageLayout = ImageLayout.Stretch;
            //PictureBox pictureBox = new PictureBox();
            //pictureBox.Parent = f;
            //pictureBox.Image = Resources.赞赏码;
            f.Show();
        }
        #endregion

    }

}

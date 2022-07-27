using System;
using System.Drawing;
using System.Windows.Forms;

namespace TheveSmartCar
{
    public partial class Toolss : Form
    {
        public Toolss()
        {
            InitializeComponent();
        }
        public Toolss(string info, bool theme)
            : this()
        {
            richTextBoxUnique.Text = info;
            if (theme)
            { 
                MainThemeChange();
            }
        }
        //主窗体同步
        public string synStr
        {
            get { return richTextBoxUnique.Text; }
        }

        //自动换行
        private void checkBoxAutoLine_CheckedChanged(object sender, EventArgs e)
        {
            richTextBoxUnique.WordWrap = !checkBoxAutoLine.Checked;
        }
        //字体
        private void button1_Click(object sender, EventArgs e)
        {
            if (fontDialog1.ShowDialog() == DialogResult.OK)
            {
                richTextBoxUnique.Font = fontDialog1.Font;
                richTextBoxUnique.ForeColor = fontDialog1.Color;
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
            this.BackColor = Color.FromArgb(51, 51, 55);
            tabPage1.BackColor = Color.FromArgb(51, 51, 55);
            tabPage2.BackColor = Color.FromArgb(51, 51, 55);
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

    }
}

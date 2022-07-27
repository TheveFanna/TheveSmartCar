
namespace TheveSmartCar
{
    partial class Toolss
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.richTextBoxUnique = new System.Windows.Forms.RichTextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.checkBoxAutoLine = new System.Windows.Forms.CheckBox();
            this.fontDialog1 = new System.Windows.Forms.FontDialog();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(710, 1016);
            this.tabControl1.TabIndex = 61;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.richTextBoxUnique);
            this.tabPage1.Location = new System.Drawing.Point(4, 28);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(702, 984);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "独立程序栏";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // richTextBoxUnique
            // 
            this.richTextBoxUnique.BackColor = System.Drawing.SystemColors.Window;
            this.richTextBoxUnique.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxUnique.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxUnique.Location = new System.Drawing.Point(3, 3);
            this.richTextBoxUnique.Name = "richTextBoxUnique";
            this.richTextBoxUnique.Size = new System.Drawing.Size(696, 978);
            this.richTextBoxUnique.TabIndex = 0;
            this.richTextBoxUnique.Text = "";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.richTextBox1);
            this.tabPage2.Controls.Add(this.button1);
            this.tabPage2.Controls.Add(this.checkBoxAutoLine);
            this.tabPage2.Location = new System.Drawing.Point(4, 28);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(702, 984);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "设置";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBox1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.richTextBox1.Location = new System.Drawing.Point(15, 10);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(673, 213);
            this.richTextBox1.TabIndex = 3;
            this.richTextBox1.Text = "独立程序框始终保持可写状态。\n当在独立程序框内编辑内容时，主程序框不会立即同步，\n这时需要手动确认同步，有两种方法，在主程序框内点击\n解锁-锁定时，独立程序框的内" +
    "容会同步到主程序框内；\n或者点击主程序框内的“关闭独立程序框”也可实现同步。\n";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(25, 280);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(106, 46);
            this.button1.TabIndex = 1;
            this.button1.Text = "字体设置";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // checkBoxAutoLine
            // 
            this.checkBoxAutoLine.AutoSize = true;
            this.checkBoxAutoLine.Location = new System.Drawing.Point(25, 231);
            this.checkBoxAutoLine.Name = "checkBoxAutoLine";
            this.checkBoxAutoLine.Size = new System.Drawing.Size(106, 22);
            this.checkBoxAutoLine.TabIndex = 0;
            this.checkBoxAutoLine.Text = "自动换行";
            this.checkBoxAutoLine.UseVisualStyleBackColor = true;
            this.checkBoxAutoLine.CheckedChanged += new System.EventHandler(this.checkBoxAutoLine_CheckedChanged);
            // 
            // Toolss
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(710, 1016);
            this.ControlBox = false;
            this.Controls.Add(this.tabControl1);
            this.Name = "Toolss";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "独立程序框";
             this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.RichTextBox richTextBoxUnique;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.CheckBox checkBoxAutoLine;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.FontDialog fontDialog1;
        private System.Windows.Forms.RichTextBox richTextBox1;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TheveSmartCar
{
    static class TheveMenuStrip
    {
        /// <summary>
        /// 添加工具栏文件下的子目录
        /// </summary>
        /// <param name="toolStripMenuItem">工具栏下的某项</param>
        /// <param name="path"></param>
        /// <param name="inversion">是否倒置加入</param>
        public static void DirectionRecent(ToolStripMenuItem toolStripMenuItem, string path, bool inversion = false)
        {
            if (toolStripMenuItem.DropDownItems.Count != 0)
            {
                foreach (var i in toolStripMenuItem.DropDownItems)
                {
                    if (path == i.ToString())
                    {
                        return;
                    }
                }
            }
            ToolStripItem toolStripItem = toolStripMenuItem.DropDownItems.Add(path);
            if (inversion)
            {
                toolStripMenuItem.DropDownItems.Insert(0, toolStripItem);
            }

            while (toolStripMenuItem.DropDownItems.Count >= 25)
            {
                toolStripMenuItem.DropDownItems.RemoveAt(24);
            }
        }


        public static void RadioCheck(ToolStripMenuItem toolStripMenuItem, ToolStripItem t)
        {
            foreach (ToolStripMenuItem i in toolStripMenuItem.DropDownItems)
            {
                if (i != t)
                {
                    i.Checked = false;
                }
                else
                {
                    i.Checked = true;
                }
            }

        }
    }
}

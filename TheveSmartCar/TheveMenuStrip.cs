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

        /// <summary>
        /// 勾选，多子菜单,单一勾选
        /// </summary>
        /// <param name="toolStripMenuItem">一级菜单项</param>
        /// <param name="t">二级菜单勾选项</param>
        public static void SetMenuCheck(ToolStripMenuItem toolStripMenuItem, ToolStripItem t)
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
        /// <summary>
        /// 获取一级菜单下的二级菜单哪个被勾选了
        /// </summary>
        /// <param name="toolStripMenuItem">一级菜单</param>
        /// <param name="t">被勾选的二级菜单</param>
        public static ToolStripMenuItem GetMenuCheck(ToolStripMenuItem toolStripMenuItem)
        {
            foreach (ToolStripMenuItem i in toolStripMenuItem.DropDownItems)
            {
                if (i.Checked)
                {
                    return i;
                }
            }
            return null;
        }

        /// <summary>
        /// 通过字符串获取二级菜单的对象
        /// </summary>
        /// <param name="toolStripMenuItem">一级菜单</param>
        /// <param name="toolStripItemName">二级菜单的名字</param>
        /// <returns></returns>
        public static ToolStripItem NameToObject(ToolStripMenuItem toolStripMenuItem, string toolStripItemName)
        {
            foreach (ToolStripItem i in toolStripMenuItem.DropDownItems)
            {
                if (i.Name == toolStripItemName)
                {
                    return i;
                }
            }
            return null;
        }

        /// <summary>
        /// 勾选状态改变
        /// </summary>
        /// <param name="sender"></param>
        public static void SingleCheckTrans(object sender)
        {
            if (sender is ToolStripMenuItem)
            {
                ToolStripMenuItem toolStripMenuItem = sender as ToolStripMenuItem;
                toolStripMenuItem.Checked = !toolStripMenuItem.Checked;
            }
        }

    }
}

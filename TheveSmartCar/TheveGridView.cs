using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartCar
{
    public static class TheveGridView
    { 
        static DataGridView dataGridView;
        /// <summary>
        /// 初始化相关参数
        /// </summary>
        /// <param name="dataGridView1"></param>
        public static void Init(DataGridView dataGridView1)
        {
            dataGridView = dataGridView1;
            //列标题居中
            dataGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            //无边界
            dataGridView.BorderStyle = BorderStyle.None;
            dataGridView.CellBorderStyle = DataGridViewCellBorderStyle.None;

            //取消自动增加新行
            dataGridView.AllowUserToAddRows = false;
            dataGridView.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            //初始化表格
            //dataGridView.Rows.Add(120);

        }
        /// <summary>
        /// 写入一列数据
        /// </summary>
        /// <param name="col">列序号</param>
        /// <param name="array">数组</param>
        public static void WriteColData(int colIndex, int[] array)
        {
            if (dataGridView.Rows.Count < array.Length)
            { 
                dataGridView.Rows.Add(array.Length- dataGridView.Rows.Count);
            }
            for (int i = 0; i < array.Length; i++)
            { 
                  dataGridView.Rows[i].Cells[colIndex].Value = array[i];
            }
        }
        public static void WriteColData(string colName, int[] array)
        {
            if (dataGridView.Rows.Count < array.Length)
            {
                dataGridView.Rows.Add(array.Length - dataGridView.Rows.Count);
            }
            for (int i = 0; i < array.Length; i++)
            {
                dataGridView.Rows[i].Cells[colName].Value = array[i];
            }
        }

    }
}

using System.Collections;
using System.Collections.Generic;

namespace ThevePictureProcessDll
{
    public static class PicPro
    {
        public static int[,] img;
        public static int[,] imgOriginal;
        public static Hashtable watch = new Hashtable();
        public static Hashtable arrayTable = new Hashtable();
        public static Hashtable inherit = new Hashtable(); 
        /// <summary>
        /// 初始化合适的数组大小
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        public static void CreatImg(int i, int j)
        {
            img = new int[i, j];
            imgOriginal = new int[i, j];
        }
        /// <summary>
        /// 添加变量
        /// </summary>
        /// <param name="name">变量名</param>
        /// <param name="num">变量数据</param>
        public static void Watch(string name, int num)
        {
            watch.Add(name, num);
        }
        /// <summary>
        /// 写入数组数据
        /// </summary>
        /// <param name="name">数组名</param>
        /// <param name="array">可以是char、short、int类型</param> 
        public static void WatchArray(string name, byte[] array)
        {
            arrayTable.Add(name,array);
        }
        public static void WatchArray(string name, short[] array)
        {
            arrayTable.Add(name, array);
        }
        public static void WatchArray(string name, int[] array)
        {
            arrayTable.Add(name, array);
        }
        public static void WatchArray(string name, float[] array)
        {
            arrayTable.Add(name, array);
        }
        /// <summary>
        /// 全局变量继承
        /// </summary>
        /// <param name="name"></param>
        /// <param name="global"></param> 
        public static void Inherit(string name, float global)
        {
            if (inherit.ContainsKey(name))
            {
                inherit[name] = global;
            }
            else
            {
                inherit.Add(name, global);
            }
        }
        /// <summary>
        /// 获取全局继承变量
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static float GetInherit(string name)
        {
            if (inherit.ContainsKey(name))
            {
                return (float)inherit[name];
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// 清除所有内容
        /// </summary>
        public static void WatchClear()
        {
            watch.Clear();
        }
        /// <summary>
        /// 清除储存的数组
        /// </summary>
        public static void WatchArrayClear()
        {
            arrayTable.Clear();
        }
        /// <summary>
        /// 清除全局继承变量
        /// </summary>
        public static void ClearInherit()
        {
            inherit.Clear();
        }

    }
}

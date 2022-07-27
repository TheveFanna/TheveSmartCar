using System;
using System.Windows.Forms;
using System.IO.Ports;
using System.Text;
using System.IO;

namespace SmartCar
{
    static class TheveSerialPort
    {
        private static SerialPort serial;
        private static bool isOpen = false;
        private static string error = "未知错误";
        private static long receiveAmount = 0;
        private static long sendAmount = 0;
        public enum ChangeType
        {
            String = 0,
            Hex,
            bmp4,
            bmp8,
            ChangeTypeMax
        }

        //System.Timers.Timer t = new System.Timers.Timer(10000);//实例化Timer类，设置间隔时间为10000毫秒；
        //t.Elapsed += new System.Timers.ElapsedEventHandler(theout);//到达时间的时候执行事件；
        //    t.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；
        //    t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
        //public void theout(object source, System.Timers.ElapsedEventArgs e)
        //{
        //    MessageBox.Show("OK!");
        //}

        public static SerialPort Serial { get => serial; set => serial = value; }
        public static bool IsOpen { get => isOpen; set => isOpen = value; }
        public static string Error { get => error; set => error = value; }
        public static long SendAmount { get => sendAmount; set => sendAmount = value; }
        public static long ReceiveAmount { get => receiveAmount; set => receiveAmount = value; }

        /// <summary>
        /// 扫描端口 结果反馈给下拉表
        /// </summary>
        /// <param name="combobox"></param>
        public static void Scan(ComboBox combobox)
        {
            string[] COMNames = SerialPort.GetPortNames();
            combobox.Items.Clear();
            for (int i = 0; i < COMNames.Length; i++)
            {
                combobox.Items.Add(COMNames[i]);
            }
            if (combobox.Items.Count != 0)
            {
                combobox.SelectedIndex = 0;
            }

            TheveIniFiles.IniLoad(combobox);
        }
        /// <summary>
        /// 扫描端口
        /// </summary>
        /// <returns>string[] 字符串数组</returns>
        public static string[] Scan()
        {
            return SerialPort.GetPortNames();
        }
        /// <summary>
        /// 配置端口名 波特率
        /// </summary>
        /// <param name="com">端口名</param>
        /// <param name="baud">波特率</param>
        public static bool Config(string com, int baud)
        {
            serial.PortName = com;
            serial.BaudRate = baud;
            if (com != "")
            {
                serial.PortName = com;
            }
            else
            {
                Error = "端口不正确";
                return false;
            }
            if (baud <= 0)
            {
                Error = "波特率不正确";
                return false;
            }
            else
            {
                serial.BaudRate = baud;
            }
            return true;
        }
        /// <summary>
        /// 配置端口名 波特率
        /// </summary>
        /// <param name="com">端口名</param>
        /// <param name="baud">波特率</param>
        public static bool Config(string com, string baud)
        {
            if (com != "")
            {
                serial.PortName = com;
            }
            else
            {
                Error = "端口不正确";
                return false;
            }
            if (baud != "")
            {
                serial.BaudRate = Convert.ToInt32(baud);
            }
            else
            {
                Error = "波特率不正确";
                return false;
            }
            return true;
        }
        /// <summary>
        /// 打开串口
        /// </summary>
        /// <param name="reconnectNum">重试次数</param>
        /// <returns>打开成功返回true 否则返回false</returns>
        public static bool Open(int reconnectNum = 10)
        {
            try
            {
                Serial.Open();
                for (int i = 0; i < reconnectNum; i++)
                {
                    if (serial.IsOpen == true)
                    {
                        isOpen = true;
                        return true;
                    }
                }
                error = "打开超时";
                return false;
            }
            catch
            {
                error = "串口被占用";
                return false;
            }
        }
        /// <summary>
        /// 关闭串口
        /// </summary>
        /// <param name="reconnectNum"></param>
        /// <returns></returns>
        public static bool Close(int reconnectNum = 10)
        {
            try
            {
                Serial.Close();
                for (int i = 0; i < reconnectNum; i++)
                {
                    if (serial.IsOpen == false)
                    {
                        isOpen = false;
                        return true;
                    }
                }
                error = "关闭超时";
                return false;
            }
            catch
            {
                error = "串口无法解除占用";
                return false;
            }
        }
        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="type">选择字符串还是16进制文本</param>
        /// <returns>字符串数据</returns>
        public static string Receive(ChangeType type)
        {
            string receiveData = "";
            switch (type)
            {
                case ChangeType.String:
                    receiveData = serial.ReadExisting();
                    receiveAmount += Encoding.Default.GetBytes(receiveData.ToCharArray()).Length;
                    break;
                case ChangeType.Hex:
                    byte[] readbuffer = new byte[serial.BytesToRead];
                    receiveAmount += readbuffer.Length;
                    serial.Read(readbuffer, 0, readbuffer.Length);
                    foreach (byte b in readbuffer)
                    {
                        string strTemp = Convert.ToString(b, 16).ToUpper();
                        if (strTemp.Length == 1)
                        {
                            strTemp = "0" + strTemp;
                        }
                        receiveData += strTemp + " ";
                    }
                    break;
            }
            return receiveData;
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="type">选择字符串还是16进制文本</param>
        /// <param name="sendData">需要发送的字符串</param>
        public static void Write(ChangeType type, string sendData)
        {
            switch (type)
            {
                case ChangeType.String:
                    serial.Write(sendData);
                    sendAmount += Encoding.Default.GetBytes(sendData.ToCharArray()).Length;
                    break;
                case ChangeType.Hex:
                    string[] s = sendData.Trim().Split(' ');
                    byte[] b = new byte[s.Length];
                    for (int i = 0; i < s.Length; i++)
                    {
                        b[i] = Convert.ToByte(s[i], 16);
                    }
                    serial.Write(b, 0, b.Length);
                    sendAmount += b.Length;
                    break;
            }
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="isHex">true则发送16进制 false发送字符串</param>
        /// <param name="sendData">字符串数据</param>
        public static void Write(bool isHex, string sendData)
        {
            if (sendData != "")
            {
                if (isHex == true)
                {
                    string[] s = sendData.Trim().Split(' ');
                    byte[] b = new byte[s.Length];
                    for (int i = 0; i < s.Length; i++)
                    {
                        b[i] = Convert.ToByte(s[i], 16);
                    }
                    serial.Write(b, 0, b.Length);
                    sendAmount += b.Length;
                }
                else
                {
                    serial.Write(sendData);
                    sendAmount += Encoding.Default.GetBytes(sendData.ToCharArray()).Length;

                }
            }
        }
        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="Filter">文件滤波器</param>
        /// <returns>文件路径</returns>
        public static string SaveFile(string Filter = "所有文件|*.*|文本文件|*.txt")
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "选择保存路径";
            saveFileDialog.Filter = Filter;
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                return saveFileDialog.FileName;
            }
            return "error";
        }
        /// <summary>
        /// 选择文件对话框
        /// </summary>
        /// <param name="Filter">文件滤波器</param>
        /// <returns>选择文件的路径</returns>
        public static string SelectFile(string Filter = "所有文件|*.*|文本文件|*.txt")
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "选择要发送的文件";
            openFileDialog.Filter = Filter;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                return openFileDialog.FileName;
            }
            return "error";
        }
        /// <summary>
        /// 发送文件
        /// </summary>
        /// <param name="path">文件路径</param>
        public static void SendFile(string path)
        {
            Write(ChangeType.String, File.ReadAllText(path));
        }



    }
}

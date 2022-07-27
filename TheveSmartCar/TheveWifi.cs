using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace TheveSmartCar
{
    static class TheveWifi
    {
        //获取本地IP
        public static List<string> GetLocalIp()
        {
            List<string> IpAddress = new List<string>();
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    IpAddress.Add(_IPAddress.ToString());
                }
            }
            return IpAddress;
        }
        //扫描IP
        public static void IpScan(ComboBox comboBoxIP)
        {
            List<string> IpAddress = GetLocalIp();
            foreach (string i in IpAddress)
            {
                comboBoxIP.Items.Add(i);
            }
            comboBoxIP.SelectedIndex = comboBoxIP.Items.Count - 1; 
        }
        /// <summary>
        /// 开始监听
        /// </summary>
        /// <param name="socketListen">监听socket</param>
        /// <param name="comboBoxIP">IP下拉栏</param>
        /// <param name="textBoxPort">端口文本框</param>
        public static void Listen(ref Socket socketListen, ComboBox comboBoxIP, TextBox textBoxPort)
        {
            string adress = comboBoxIP.SelectedItem.ToString();
            int port = Convert.ToInt32(textBoxPort.Text);
            socketListen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketListen.Bind(new IPEndPoint(IPAddress.Parse(adress), port));
            socketListen.Listen(1);
        }


    }
}

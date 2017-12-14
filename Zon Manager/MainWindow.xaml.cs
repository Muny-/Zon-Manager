using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Elysium.Notifications.Client;
using Zön_Manager.Notifications;
using System.IO;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using InTheHand.Net;

namespace Zön_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Elysium.Controls.Window
    {
        public MainWindow()
        {
            InitializeComponent();
            timer.Interval = 1000;
            timer.Tick += timer_Tick;
        }

        int oldBytes = 0;
        int currentBytes = 0;
        Stopwatch watch;

        void timer_Tick(object sender, EventArgs e)
        {
            int bytesPerSecond = currentBytes - oldBytes;
            transferSpeed.Content = (bytesPerSecond / 1024) + " KB/s";

            oldBytes = currentBytes;
        }

        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        void Window1_Activated(object sender, EventArgs e)
        {
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            BluetoothServer.instance.SendMessage(textBox1.Text, true);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            BluetoothServer.instance.stopwatch = System.Diagnostics.Stopwatch.StartNew();
            BluetoothServer.instance.SendMessage(Constants.Commands.PING.ToString(), true);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            

            /*timer.Start();
            watch = Stopwatch.StartNew();
            new Thread(delegate()
            {
                FileStream fs = new FileStream("C:\\Users\\Kevin\\Desktop\\10mb.test", FileMode.Open, FileAccess.Read);
                int packets = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(fs.Length) / Convert.ToDouble(1024)));
                byte[] sendBuf = null;
                int totLength = (int)fs.Length;
                int currentPacketLength;

                

                Console.WriteLine("Starting transfer...");

                for (int i = 0; i < packets; i++)
                {
                    if (totLength > 1024)
                    {
                        currentPacketLength = 1024;
                        totLength -= currentPacketLength;
                    }
                    else
                        currentPacketLength = totLength;

                    sendBuf = new byte[currentPacketLength];
                    fs.Read(sendBuf, 0, currentPacketLength);
                    BluetoothServer.instance.Client.BTClient.GetStream().Write(sendBuf, 0, (int)sendBuf.Length);

                    currentBytes = i * 1024;
                }
                timer.Stop();
                watch.Stop();
                Console.WriteLine("Done, took " + watch.ElapsedMilliseconds/1000 + "s to transfer " + totLength + " bytes (" + packets + " KB)");
                fs.Close();

            }).Start();*/
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //BluetoothServer.instance.SendMessage(Constants.Commands.GENERIC_MULTIPARTDATA + Constants.DATA_DELIMETER + "Hello this is message number one!" + Constants.DATA_DELIMETER + "Hello, this is message number two!");
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            //textBox1.Text += Constants.DATA_DELIMETER;
        }
    }
}

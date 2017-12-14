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
using System.Windows.Shapes;
using Zön_Manager.Notifications;
using CoreAudioApi;
using System.Management;
using System.Diagnostics;

namespace Zön_Manager
{
    /// <summary>
    /// Interaction logic for BackgroundWindow.xaml
    /// </summary>
    public partial class BackgroundWindow : Window
    {
        public static BackgroundWindow instance;
        public NotificationManager notificationManager = new NotificationManager();
        public BluetoothServer bluetoothServer;

        public BackgroundWindow()
        {
            instance = this;
            bluetoothServer = new BluetoothServer();
            bluetoothServer.Start();

            MainWindow mw = new MainWindow();
            mw.Show();

            MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();

            /*MMDevice defind = DevEnum.GetDefaultAudioEndpoint(EDataFlow.eCapture, ERole.eCommunications);

            PropertyStore ps = defind.Properties;

            for (int i = 0; i < ps.Count; i++ )
            {
                PropertyKey pk = ps[i].Key;

                //Console.WriteLine(pk.fmtid + " (" + pk.pid + "): " + ps[i].Value);

                if (ps[i].Value.GetType() == typeof(Byte[]))
                {
                    Byte[] b = (Byte[])ps[i].Value;

                    
                }
            }*/

            //MMDevice devic = DevEnum.GetDevice(data.data);
            

            //Console.WriteLine(output3);

            
        }

        void audioseslist_VolumeChanged(object sender, VolumeEventArgs e)
        {
            AudioSessionListener asl = (AudioSessionListener)sender;
            try
            {
                Console.WriteLine(Process.GetProcessById(asl.ProcessID).ProcessName + ": " + e.NewVolume);
            }
            catch { }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

namespace Zön_Manager
{
    public class ZönClient
    {
        public string UUID;
        public BluetoothClient BTClient;
        public string Accepted;
        public string CurrentPage;

        public ZönClient()
        {

        }

        public ZönClient(string UUID)
        {
            this.UUID = UUID;
        }

        public ZönClient(BluetoothClient BTClient)
        {
            this.BTClient = BTClient;
        }

        public ZönClient(string UUID, BluetoothClient BTClient)
        {
            this.UUID = UUID;
            this.BTClient = BTClient;
        }
    }
}

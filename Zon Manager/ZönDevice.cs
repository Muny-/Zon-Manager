using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InTheHand.Net;

namespace Zön_Manager
{
    [Serializable]
    public class ZönDevice
    {
        public string Name;
        public BluetoothAddress Address;

        public ZönDevice()
        {

        }

        public ZönDevice(string Name, BluetoothAddress Address)
        {
            this.Name = Name;
            this.Address = Address;
        }
    }
}

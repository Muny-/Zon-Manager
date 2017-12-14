using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zön_Manager
{
    public class NotificationClient : CoreAudioApi.Interfaces.IMMNotificationClient
    {
        public void OnPropertyValueChanged(string DeviceID, CoreAudioApi.PropertyKey key)
        {
            Console.WriteLine("Property value changed: " + DeviceID + ", " + key.fmtid + "(" + key.pid + ")");
        }

        public void OnDeviceStateChanged(string DeviceID, CoreAudioApi.EDeviceState state)
        {
            if (state == CoreAudioApi.EDeviceState.DEVICE_STATE_ACTIVE)
            {
                BluetoothServer.instance.OnDeviceAdded(DeviceID);
            }
            else
            {
                BluetoothServer.instance.OnDeviceRemoved(DeviceID);
            }
            Console.WriteLine("Device state change: " + DeviceID + ", " + state.ToString());
        }

        public void OnDeviceRemoved(string DeviceID)
        {
            Console.WriteLine("Device removed: " + DeviceID);
        }

        public void OnDeviceAdded(string DeviceID)
        {
            Console.WriteLine("Device added: " + DeviceID);
        }

        public void OnDefaultDeviceChanged(CoreAudioApi.EDataFlow dataFlow, CoreAudioApi.ERole role, string DefaultDeviceID)
        {
            Console.WriteLine("Default device changed: " + DefaultDeviceID);
        }
    }
}

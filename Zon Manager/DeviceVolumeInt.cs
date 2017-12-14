using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zön_Manager
{
    public class DeviceVolumeInt
    {
        public string did;
        public int vol;
        public bool isMuted;

        public DeviceVolumeInt(string did, int vol, bool isMuted)
        {
            this.did = did;
            this.vol = vol;
            this.isMuted = isMuted;
        }
    }
}

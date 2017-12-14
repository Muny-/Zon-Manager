using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zön_Manager
{
    public class ProcessVolumeInt
    {
        public int pid;
        public int vol;
        public bool isMuted;

        public ProcessVolumeInt(int pid, int vol, bool isMuted)
        {
            this.pid = pid;
            this.vol = vol;
            this.isMuted = isMuted;
        }
    }
}

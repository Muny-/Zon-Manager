using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreAudioApi;

namespace Zön_Manager
{
    public class VolumeEventArgs : EventArgs
    {
        public int NewVolume;
        public Guid Context;

        public VolumeEventArgs(int newVolume, Guid Context) {NewVolume = newVolume; this.Context = Context; }
    }

    public class AudioSessionListener : CoreAudioApi.Interfaces.IAudioSessionEvents
    {
        public int ProcessID;
        public AudioSessionControl ASC;
        public string Name;
        public delegate void VolumeEventHandler(object sender, VolumeEventArgs e);

        public AudioSessionListener(int ProcessID, AudioSessionControl asc, string Name)
        {
            this.ASC = asc;
            this.ProcessID = ProcessID;
            this.Name = Name;
        }

        public int OnSimpleVolumeChanged(float newVol, bool isMuted, Guid context)
        {
            //Console.WriteLine(newVol);
            /*
            if (VolumeChanged != null)
            {
                VolumeChanged(this, new VolumeEventArgs((int)(newVol*100), context));
            }*/

            BluetoothServer.instance.AudioSessionVolumeChanged(this.ProcessID, newVol, isMuted);

            return 0;
        }

        public int OnStateChanged(CoreAudioApi.AudioSessionState newState)
        {
            return 0;
        }

        public int OnSessionDisconnected(CoreAudioApi.AudioSessionDisconnectReason reason)
        {
            return 0;
        }

        public int OnIconPathChanged(string newPath, System.Guid context)
        {
            return 0;
        }

        public int OnGroupingParamChanged(System.Guid groupingParam, Guid context)
        {
            return 0;
        }

        public int OnDisplayNameChanged(string newName, System.Guid context)
        {
            return 0;
        }

        public int OnChannelVolumeChanged(uint ChannelCount, System.IntPtr NewChannelVolumeArray, uint ChannelChanged, Guid context)
        {
            return 0;
        }

        public void Dispose()
        {
            ASC = null;
        }
    }
}

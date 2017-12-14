using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using InTheHand.Net.Bluetooth.AttributeIds;
using System.Net.Sockets;
using System.IO;
using Zön_Manager.Notifications;
using System.Diagnostics;
using CoreAudioApi;

namespace Zön_Manager
{
    public static class Extension
    {
        public static IEnumerable<T1> GetKeysByValue<T1, T2>(this Dictionary<T1, T2> dico, T2 value)
        {
            return from kvp in dico
                   where kvp.Value.Equals(value)
                   select kvp.Key;
        }
    }

    public class BluetoothServer
    {
        private Thread ServerThread;
        private BluetoothListener BTListener;
        public ZönClient Client;
        public static BluetoothServer instance;
        public Stopwatch stopwatch;

        public Dictionary<int, object> Objects = new Dictionary<int, object>();
        MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
        public List<AudioSessionListener> listeners = new List<AudioSessionListener>();
        public MMDevice lastDevice;
        public string lastDeviceID;
        public ProcessVolumeInt lastChangedVol;
        public ProcessVolumeInt lastSentVol;
        public DeviceVolumeInt lastChangedVolD;
        public DeviceVolumeInt lastSentVolD;
        public System.Windows.Forms.Timer AudioSessionTimer = new System.Windows.Forms.Timer();
        public System.Windows.Forms.Timer GCColTimer = new System.Windows.Forms.Timer();
        public string lastSentPacket = "";
        public List<int> deadSessionProcessIDs = new List<int>();
        public Dictionary<int, string> InputDevices = new Dictionary<int, string>();
        NotificationClient NOCL = new NotificationClient();
        OlsonMapper olsonmap = new OlsonMapper();

        public long lastChangeSessionLevelTime = (long)(DateTime.UtcNow-new DateTime (1970, 1, 1)).TotalMilliseconds;

        public BluetoothServer()
        {
            DevEnum.RegisterEndpointNotificationCallback(NOCL);
            AudioSessionTimer.Interval = 1000;
            AudioSessionTimer.Tick += AudioSessionTimer_Tick;
            AudioSessionTimer.Start();
            GCColTimer.Interval = 60000;
            GCColTimer.Tick += GCColTimer_Tick;
            GCColTimer.Start();

            lastChangedVol = new ProcessVolumeInt(-99999, -99999, false);
            lastSentVol = new ProcessVolumeInt(-99999, -99999, false);
            lastChangedVolD = new DeviceVolumeInt("", -99999, false);
            lastSentVolD = new DeviceVolumeInt("", -99999, false);
            instance = this;
            if (BluetoothRadio.IsSupported)
            {
                Console.WriteLine("Bluetooth Supported");
                Console.WriteLine("-------------------");
                Console.WriteLine("Primary Bluetooth Radio Name: " + BluetoothRadio.PrimaryRadio.Name);
                Console.WriteLine("Primary Bluetooth Radio Address: " + BluetoothRadio.PrimaryRadio.LocalAddress);
                Console.WriteLine("Primary Bluetooth Radio Manufacturer: " + BluetoothRadio.PrimaryRadio.Manufacturer);
                Console.WriteLine("Primary Bluetooth Radio Mode: " + BluetoothRadio.PrimaryRadio.Mode);
                Console.WriteLine("Primary Bluetooth Radio Software Manufacturer: " + BluetoothRadio.PrimaryRadio.SoftwareManufacturer);
                Console.WriteLine("Primary Bluetooth Radio Version: " + BluetoothRadio.PrimaryRadio.HciVersion);
                Console.WriteLine("-------------------");
            }
            else
            {
                BackgroundWindow.instance.notificationManager.SendNotification(Constants.Notifications.BluetoothNotSupported);
            }
        }

        void GCColTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                DevEnum.UnregisterEndpointNotificationCallback(NOCL);
            }
            catch
            { }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            NOCL = new NotificationClient();
            DevEnum.RegisterEndpointNotificationCallback(NOCL);
        }

        public void Start()
        {
            if (BluetoothRadio.IsSupported)
            {
                ServerThread = new Thread(AcceptAndListen);
                ServerThread.Start();
            }
        }

        public void Stop()
        {
            try
            {
                Client.BTClient.GetStream().Close();
                Client.BTClient.Close();
                Client.BTClient.Dispose();
                ServerThread.Abort();
                ServerThread.Join();
            }
            catch
            {

            }
        }

        public void AcceptAndListen()
        {
            while(true)
            {
                // Handle Client interaction
                if (Client != null && Client.BTClient.Connected)
                {
                    /*try
                    {

                    }
                    catch
                    {

                    }*/

                    if (Client.Accepted == null)
                    {
                        Notification ntf = null;
                        /*BackgroundWindow.instance.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate()
                        {
                        
                        }));*/

                        BackgroundWindow.instance.Dispatcher.Invoke(new Action(() => {
                            ntf = BackgroundWindow.instance.notificationManager.SendNotification(Constants.Notifications.DeviceAttemptingConnection, Client.BTClient.RemoteMachineName);
                        }));

                        bool accepted = false;
                        bool waiting = true;
                        while (waiting)
                        {
                            bool isnull = false;
                            BackgroundWindow.instance.Dispatcher.Invoke(new Action(() =>
                            {
                                accepted = ntf.HasBeenClicked;
                                if (!ntf.IsVisible)
                                    isnull = true;
                            }));

                            if (isnull)
                                waiting = false;
                            else if (accepted)
                            {
                                waiting = false;
                            }
                        }
                        if (accepted)
                        {
                            Client.Accepted = "true";
                            if (Zön_Manager.Properties.Settings.Default.AllowedDevices == null)
                                Zön_Manager.Properties.Settings.Default.AllowedDevices = new System.Collections.Specialized.StringCollection();
                            Zön_Manager.Properties.Settings.Default.AllowedDevices.Add(Client.BTClient.RemoteMachineName + "@" + Client.BTClient.RemoteEndPoint.Address.ToString());
                            Zön_Manager.Properties.Settings.Default.Save();
                        }
                        else
                        {
                            Client.Accepted = "false";
                            Client.BTClient.Close();
                            Client = null;
                        }
                    }
                    else if (Client.Accepted == "true")
                    {
                        /*try
                        {*/
                            while (Client.BTClient.Connected)
                            {
                                string raw_data = ReadNetStream(Client.BTClient.GetStream(), 2048);

                                if (raw_data == "")
                                {
                                    BackgroundWindow.instance.Dispatcher.Invoke(new Action(() =>
                                    {
                                        BackgroundWindow.instance.notificationManager.SendNotification(Constants.Notifications.DeviceLostConnection, Client.BTClient.RemoteMachineName);
                                    }));
                                    DisconnectClient();
                                }
                                else
                                {


                                    handleMergedPackets(raw_data);

                                    /*BackgroundWindow.instance.Dispatcher.Invoke(new Action(() =>
                                    {
                                        BackgroundWindow.instance.notificationManager.AddNotification("Message Received", myCompleteMessage.ToString(), 10000);
                                    }));*/
                                }
                            }
                        /*}
                        catch (Exception ex)
                        {
                            Console.WriteLine("There was an error while listening connection!");
                            Console.WriteLine(ex.Message);
                        }*/
                    }
                    else
                    {
                        Client.Accepted = "false";
                        DisconnectClient();
                        Client = null;
                    }
                    
                }
                else
                {
                    // Listen and accept connections
                    /*try
                    {*/
                        BTListener = new BluetoothListener(Constants.ZönServiceGUID);
                        BTListener.Start();
                        Console.WriteLine("Listening for connection...");
                        BluetoothClient btcli = BTListener.AcceptBluetoothClient();
                        Console.WriteLine("Accepted connection: " + btcli.RemoteMachineName);
                        Client = new ZönClient(btcli);
                        if (Zön_Manager.Properties.Settings.Default.AllowedDevices == null)
                            Zön_Manager.Properties.Settings.Default.AllowedDevices = new System.Collections.Specialized.StringCollection();

                        if (Zön_Manager.Properties.Settings.Default.AllowedDevices.Contains(Client.BTClient.RemoteMachineName + "@" + Client.BTClient.RemoteEndPoint.Address.ToString()))
                        {
                            Client.Accepted = "true";
                        }
                    /*}
                    catch (Exception ex)
                    {
                        Console.WriteLine("There was an error while listening connection!");
                        Console.WriteLine(ex.Message);
                    }*/
                }
            }
        }

        string fixMergedPackets(string input)
        {
            if (input.StartsWith("\""))
            {
                input = input.Insert(0, "{");
                return fixMergedPackets(input);
            }
            else if (input.EndsWith("}}"))
            {
                input = input.Remove(input.Length - 2, 1);
                return fixMergedPackets(input);
            }
            else if (input.EndsWith("\""))
            {
                input += "}";
                return fixMergedPackets(input);
            }
            else
                return input;
        }

        void handleMergedPackets(string input)
        {
            if (input.Contains("}{"))
            {
                List<string> packets = input.Split(new string[] { "}{" }, StringSplitOptions.RemoveEmptyEntries).ToList();

                foreach (string str in packets)
                {
                    handleMergedPackets(fixMergedPackets(str));
                }
            }
            else
                ParseJSONPacket(input);
        }

        void ParseJSONPacket(string json)
        {
            Console.WriteLine("Parsing: " + json);

            dynamic data = DynamicJson.Parse(json);

            string command = "";

            try
            {

                command = data.c;
            }
            catch
            {
                Console.WriteLine("Command identifier not found...");
            }

            switch (command)
            {
                case Constants.Commands.DISCONNECT:
                    DisconnectClient();
                break;

                case Constants.Commands.UPDATE_OBJECT:

                break;

                case Constants.Commands.UPDATE_PAGE:
                    Client.CurrentPage = data.d;
                    Notif("Changed client's current page to: " + Client.CurrentPage);
                break;

                case Constants.Commands.PING:
                    SendMessage("{\"" + Constants.Commands.PONG + "\": \"\"}", true);
                break;

                case Constants.Commands.PONG:
                    if (stopwatch.IsRunning)
                    {
                        stopwatch.Stop();
                        Console.WriteLine("Ping to client: " + stopwatch.ElapsedMilliseconds + "ms");
                    }
                    else
                    {
                        Error(Constants.ErrorType.InvalidlyTimedCommand, "Unable to output client response time due to the timer not running.");
                    }
                break;

                case Constants.Commands.REQUEST_OUTPUT_DEVICES:
                    MMDeviceCollection coll = DevEnum.EnumerateAudioEndPoints(EDataFlow.eRender, EDeviceState.DEVICE_STATE_ACTIVE);
                    MMDevice defoutd = DevEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);

                    string output = "{\"" + Constants.Commands.REQUEST_OUTPUT_DEVICES + "\":[";

                    for (int i = 0; i < coll.Count; i++)
                    {
                        output += "[\"" + coll[i].ID + "\",\"" + coll[i].FriendlyName + "\", " + (coll[i].ID == defoutd.ID).ToString().ToLower() + "],";
                    }

                    output = output.Remove(output.Length - 1, 1);

                    output += "]}";
                    SendMessage(output, true);
                break;

                case Constants.Commands.REQUEST_INPUT_DEVICES:
                    foreach (KeyValuePair<int,string> pair in InputDevices)
                    {
                        DevEnum.GetDevice(pair.Value).AudioEndpointVolume.OnVolumeNotification -= InputDeviceVolChange;
                    }
                    InputDevices.Clear();
                    MMDeviceCollection coll2 = DevEnum.EnumerateAudioEndPoints(EDataFlow.eCapture, EDeviceState.DEVICE_STATE_ACTIVE);

                    string output2 = "{\"" + Constants.Commands.REQUEST_INPUT_DEVICES + "\":[";

                    for (int i = 0; i < coll2.Count; i++)
                    {
                        InputDevices.Add(i, coll2[i].ID);
                        coll2[i].AudioEndpointVolume.OnVolumeNotification += InputDeviceVolChange; 
                        output2 += "[\"" + i.ToString() + "\",\"" + coll2[i].FriendlyName + "\", " + coll2[i].AudioEndpointVolume.MasterVolumeLevelScalar*100 + "],";
                    }

                    output2 = output2.Remove(output2.Length - 1, 1);

                    output2 += "]}";

                    SendMessage(output2, true);
                break;

                case Constants.Commands.REQUEST_AUDIO_SESSIONS:

                    foreach (AudioSessionListener asl in listeners)
                    {
                        asl.ASC.UnregisterAudioSessionNotification(asl);
                        asl.Dispose();
                    }

                    listeners.Clear();
                    SessionCollection scol = null;
                    string output3 = "";

                    try
                    {
                        lastDevice = DevEnum.GetDevice(data.d);
                        lastDeviceID = lastDevice.ID;
                        lastDevice.AudioEndpointVolume.OnVolumeNotification += AudioEndpointVolume_OnVolumeNotification;
                        scol = lastDevice.AudioSessionManager.Sessions;
                    

                        output3 = "{\"" + Constants.Commands.REQUEST_AUDIO_SESSIONS + "\":{\"" + lastDevice.ID + "\": {";

                        output3 += "\"master\": " + lastDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100 + ",";

                    }
                    catch { }

                    for (int i = 0; i < scol.Count; i++)
                    {
                        if (!deadSessionProcessIDs.Contains((int)scol[i].ProcessID))
                        {
                            string name = "";
                            try
                            {
                                name = FileVersionInfo.GetVersionInfo(Process.GetProcessById((int)scol[i].ProcessID).MainModule.FileName).FileDescription.Trim();

                                if (name.Length > 20)
                                {
                                    name = Process.GetProcessById((int)scol[i].ProcessID).ProcessName;
                                    name = name.First().ToString().ToUpper() + String.Join("", name.Skip(1));
                                }
                            }
                            catch
                            {
                                try
                                {
                                    name = Process.GetProcessById((int)scol[i].ProcessID).ProcessName;
                                    name = name.First().ToString().ToUpper() + String.Join("", name.Skip(1));
                                }
                                catch
                                {
                                    name = scol[i].DisplayName;
                                }
                            }

                            if (name == "Idle")
                                name = "System Sounds";

                            AudioSessionListener t = new AudioSessionListener((int)scol[i].ProcessID, scol[i], name);

                            scol[i].RegisterAudioSessionNotification(t);

                            listeners.Add(t);

                            output3 += "\"" + scol[i].ProcessID + "\": [\"" + name + "\", " + scol[i].SimpleAudioVolume.MasterVolume * 100 + "],";
                        }
                    }

                    output3 = output3.Remove(output3.Length - 1, 1);

                    output3 += "} } }";

                    SendMessage(output3, true);

                break;

                case Constants.Commands.CHANGE_SESSION_LEVEL:
                    if (data.d.pid == "master")
                    {
                        try
                        {
                            lastDevice = DevEnum.GetDevice(lastDeviceID);
                            lastDevice.AudioEndpointVolume.MasterVolumeLevelScalar = (float)(data.d.vol / 100);

                            if (lastDevice.AudioEndpointVolume.MasterVolumeLevelScalar == 0)
                                lastDevice.AudioEndpointVolume.Mute = true;
                            else
                                lastDevice.AudioEndpointVolume.Mute = false;

                            lastChangeSessionLevelTime = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
                            lastChangedVol = new ProcessVolumeInt(-69001, (int)data.d.vol, ((int)data.d.vol == 0));
                        }
                        catch { }
                    }
                    else
                    {
                        List<AudioSessionListener> asls = listeners.Where(value => value.ProcessID.ToString() == data.d.pid).ToList();
                        foreach (AudioSessionListener asl in asls)
                        {

                            asl.ASC.SimpleAudioVolume.MasterVolume = (float)(data.d.vol / 100);

                            if ((int)data.d.vol == 0)
                                asl.ASC.SimpleAudioVolume.Mute = true;
                            else
                                asl.ASC.SimpleAudioVolume.Mute = false;

                            lastChangedVol = new ProcessVolumeInt(asl.ProcessID, (int)data.d.vol, ((int)data.d.vol == 0));
                        }

                        lastChangeSessionLevelTime = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;

                        /*foreach (AudioSessionListener asl2 in listeners)
                        {
                            if (asl2.ProcessID.ToString() == data.d.pid)
                            {
                                asl2.ASC.SimpleAudioVolume.MasterVolume = (float)(data.d.vol / 100);
                                break;
                            }
                        }*/
                    }
                break;

                case Constants.Commands.CHANGE_INPUT_LEVEL:
                    MMDevice dev = DevEnum.GetDevice(InputDevices[Convert.ToInt32(data.d[0])]);
                    dev.AudioEndpointVolume.MasterVolumeLevelScalar = (float)(data.d[1]/100);
                    lastChangedVolD = new DeviceVolumeInt(InputDevices[Convert.ToInt32(data.d[0])], Convert.ToInt32(data.d[1]), (bool)(Convert.ToInt32(data.d[1]) == 0));
                    lastChangeSessionLevelTime = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
                break;

                case Constants.Commands.REQUEST_TIME_ZONE:
                    SendMessage("{\"" + Constants.Commands.REQUEST_TIME_ZONE + "\": \"" + olsonmap.Find(System.TimeZoneInfo.Local.Id) + "\"}", true);
                break;
            }
        }

        void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data, string DeviceID)
        {
            AudioSessionVolumeChanged(-69001, data.MasterVolume, data.Muted);
        }

        public void DisconnectClient()
        {
            try
            {
                Client.BTClient.Close();
                BTListener.Stop();
                BTListener = null;
            }
            catch
            {

            }
        }

        public static string ReadNetStream(NetworkStream stream, int maxbytes)
        {
            byte[] readBuffer = new byte[maxbytes];
            StringBuilder str = new StringBuilder();
            int bytesRead = 0;

            do
            {
                bytesRead = stream.Read(readBuffer, 0, readBuffer.Length);
                str.AppendFormat("{0}", Encoding.ASCII.GetString(readBuffer, 0, bytesRead));
            }
            while (stream.DataAvailable);

            return str.ToString();
        }

        public void Error(Constants.ErrorType errortype, string detail)
        {
            BackgroundWindow.instance.Dispatcher.Invoke(new Action(() =>
            {
                BackgroundWindow.instance.notificationManager.AddNotification("An Error Occured!", "Error Type: " + errortype.ToString() + "\nDetail: " + detail, 10000);
            }));
        }

        public void Notif(string msg)
        {
            BackgroundWindow.instance.Dispatcher.Invoke(new Action(() =>
            {
                BackgroundWindow.instance.notificationManager.AddNotification("Generic Message", msg, 10000);
            }));
        }

        public void SendMessage(string msg, bool canDupe)
        {
            try
            {
                if (!msg.Equals("") && (canDupe || lastSentPacket != msg))
                {
                    Console.WriteLine("Sending: " + msg);
                    UTF8Encoding encoder = new UTF8Encoding();
                    NetworkStream stream = Client.BTClient.GetStream();
                    stream.Write(encoder.GetBytes(msg + "\n"), 0, encoder.GetBytes(msg).Length);
                    stream.Flush();
                    lastSentPacket = msg;
                }
            }
            catch
            {
            }
        }

        public void SendBinary(byte[] bytes)
        {
            Console.WriteLine("Sending binary data with length of " + bytes.Length.ToString() + "...");
            UTF8Encoding encoder = new UTF8Encoding();
            NetworkStream stream = Client.BTClient.GetStream();
            string length = "";

            while ((length.Length + bytes.Length.ToString().Length) < 10)
            {
                length += "0";
            }

            length += bytes.Length;

            Console.WriteLine("Length prefix: " + length);

            /*stream.Write(encoder.GetBytes(length), 0, encoder.GetBytes(length).Length);
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush();*/
        }

        public void AudioSessionVolumeChanged(int ProcessID, float newVol, bool isMuted)
        {
            //Console.WriteLine(newVol*100 + ", " + isMuted.ToString().ToLower());

            if (isMuted)
                newVol = 0;

            if (Client.CurrentPage == Constants.Device_Pages.LEVELS)
            {
                if ((long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds - lastChangeSessionLevelTime >  200)
                {
                    if (!(lastChangedVol.pid == ProcessID && (int)(newVol * 100) == lastChangedVol.vol) || lastSentVol.isMuted)
                    {
                        if (lastSentVol.pid == ProcessID && (int)(newVol * 100) == lastSentVol.vol)
                        {
                            //Console.WriteLine("Not sending 2");
                        }
                        else
                        {
                            string id = ProcessID.ToString();
                            if (ProcessID == -69001)
                                id = "master";

                            SendMessage("{\"" + Constants.Commands.CHANGE_SESSION_LEVEL + "\": [\"" + id + "\", " + newVol * 100 + "]}", false);
                            lastSentVol = new ProcessVolumeInt(ProcessID, (int)(newVol * 100), isMuted);
                        }
                    }
                    else
                    {
                        //Console.WriteLine("Not sending 1");
                    }
                }
            }
            else
            {
                //Console.WriteLine("Not sending: " + Client.CurrentPage);
            }
        }

        void AudioSessionTimer_Tick(object sender, EventArgs e)
        {
            if (lastDevice != null && Client != null && Client.CurrentPage == Constants.Device_Pages.LEVELS)
            {
                lastDevice = DevEnum.GetDevice(lastDeviceID);

                SessionCollection sescol = lastDevice.AudioSessionManager.Sessions;

                List<AudioSessionControl> newsess = new List<AudioSessionControl>();

                for (int i = 0; i < sescol.Count; i++)
                {
                    newsess.Add(sescol[i]);

                    if (listeners.Where(value => value.ASC.ProcessID == sescol[i].ProcessID).Count() < 1)
                    {
                        if (!deadSessionProcessIDs.Contains((int)sescol[i].ProcessID))
                        {
                            string output = "";
                            try
                            {
                                output = "{\"" + Constants.Commands.REQUEST_AUDIO_SESSIONS + "\":{\"" + lastDevice.ID + "\": {";
                            }
                            catch { }

                            string name = "";

                            if (sescol[i].DisplayName == "@%SystemRoot%\\System32\\AudioSrv.Dll,-202")
                                name = "System Sounds";
                            else
                            {
                                try
                                {
                                    name = FileVersionInfo.GetVersionInfo(Process.GetProcessById((int)sescol[i].ProcessID).MainModule.FileName).FileDescription.Trim();

                                    if (name.Length > 20)
                                    {
                                        name = Process.GetProcessById((int)sescol[i].ProcessID).ProcessName;
                                        name = name.First().ToString().ToUpper() + String.Join("", name.Skip(1));
                                    }
                                }
                                catch
                                {
                                    Console.WriteLine("Catch 1: " + sescol[i].DisplayName);
                                    try
                                    {
                                        name = Process.GetProcessById((int)sescol[i].ProcessID).ProcessName;
                                        name = name.First().ToString().ToUpper() + String.Join("", name.Skip(1));
                                    }
                                    catch
                                    {
                                        Console.WriteLine("Catch 2: " + sescol[i].DisplayName);
                                        name = sescol[i].DisplayName;
                                    }
                                }
                            }

                            if (name != "")
                            {

                                output += "\"" + sescol[i].ProcessID + "\": [\"" + name + "\", " + sescol[i].SimpleAudioVolume.MasterVolume * 100 + "]";

                                output += "}}}";

                                Console.WriteLine("Added audio session detected...");

                                AudioSessionListener t = new AudioSessionListener((int)sescol[i].ProcessID, sescol[i], name);

                                sescol[i].RegisterAudioSessionNotification(t);

                                listeners.Add(t);

                                SendMessage(output, true);
                            }
                        }
                    }
                }

                List<AudioSessionListener> sess2remov = new List<AudioSessionListener>();
                try
                {
                    foreach (AudioSessionListener asl in listeners)
                    {
                        if (!deadSessionProcessIDs.Contains(asl.ProcessID))
                        {
                            bool remove = false;

                            string name = "";

                            try
                            {
                                if (Process.GetProcessById((int)asl.ProcessID) == null)
                                    remove = true;
                                else if (Process.GetProcessById((int)asl.ProcessID).MainModule == null)
                                    remove = true;
                                else if (Process.GetProcessById((int)asl.ProcessID).MainModule.FileName == null)
                                    remove = true;
                            }
                            catch
                            {
                                remove = true;
                            }

                            if (asl.ASC.DisplayName == "@%SystemRoot%\\System32\\AudioSrv.Dll,-202")
                                name = "System Sounds";
                            else if (!remove)
                            {
                                try
                                {
                                    name = FileVersionInfo.GetVersionInfo(Process.GetProcessById((int)asl.ProcessID).MainModule.FileName).FileDescription.Trim();

                                    if (name.Length > 20)
                                    {
                                        name = Process.GetProcessById((int)asl.ProcessID).ProcessName;
                                        name = name.First().ToString().ToUpper() + String.Join("", name.Skip(1));
                                    }
                                }
                                catch
                                {
                                    try
                                    {
                                        name = Process.GetProcessById((int)asl.ProcessID).ProcessName;
                                        name = name.First().ToString().ToUpper() + String.Join("", name.Skip(1));
                                    }
                                    catch
                                    {
                                        name = asl.ASC.DisplayName;
                                    }
                                }
                            }

                            if (name == "")
                            {
                                remove = true;
                            }

                            if (remove && name != "System Sounds")
                            {
                                deadSessionProcessIDs.Add(asl.ProcessID);
                                Console.WriteLine("Removed audio session detected...");
                                SendMessage("{\"" + Constants.Commands.REMOVE_SESSION + "\": \"" + asl.ProcessID + "\"}", true);
                                asl.ASC.UnregisterAudioSessionNotification(asl);
                                asl.Dispose();
                                sess2remov.Add(asl);
                            }

                            /*if (newsess.Where(value => value.ProcessID == asl.ProcessID).Count() < 1)
                            {
                                SendMessage("{\"" + Constants.Commands.REMOVE_SESSION + "\": \"" + asl.ProcessID + "\"}");
                                asl.ASC.UnregisterAudioSessionNotification(asl);
                                asl.Dispose();
                                listeners.Remove(asl);
                            }*/
                        }
                    }
                }
                catch { }

                foreach(AudioSessionListener asl in sess2remov)
                {
                    listeners.Remove(asl);
                }

                sess2remov.Clear();
                sess2remov = null;
            }
        }

        void InputDeviceVolChange(AudioVolumeNotificationData data, string DeviceID)
        {
            float newVol = data.MasterVolume;
            //Console.WriteLine(newVol*100 + ", " + isMuted.ToString().ToLower());

            if (data.Muted)
                newVol = 0;

            if (Client.CurrentPage == Constants.Device_Pages.INPUTS)
            {
                if ((long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds - lastChangeSessionLevelTime > 200)
                {
                    if (!(lastChangedVolD.did == DeviceID && (int)(newVol * 100) == lastChangedVolD.vol) || lastSentVolD.isMuted)
                    {
                        if (lastSentVolD.did == DeviceID && (int)(newVol * 100) == lastSentVolD.vol)
                        {
                            //Console.WriteLine("Not sending 2");
                        }
                        else
                        {
                            
                            SendMessage("{\"" + Constants.Commands.CHANGE_INPUT_LEVEL + "\": [\"" + InputDevices.GetKeysByValue(DeviceID).First() + "\", " + newVol * 100 + "]}", false);
                            lastSentVolD = new DeviceVolumeInt(DeviceID, (int)(newVol * 100), data.Muted);
                        }
                    }
                    else
                    {
                        //Console.WriteLine("Not sending 1");
                    }
                }
            }
            else
            {
                //Console.WriteLine("Not sending: " + Client.CurrentPage);
            }
        }

        public void OnDeviceAdded(string DeviceID)
        {
            if (Client.CurrentPage == Constants.Device_Pages.INPUTS)
            {
                if (!InputDevices.Values.Contains(DeviceID))
                {
                    MMDevice device = DevEnum.GetDevice(DeviceID);
                    if (device.DataFlow == EDataFlow.eCapture)
                    {
                        int newid = 0;
                        try
                        {
                            newid = InputDevices.Keys.Last() + 1;
                        }
                        catch { }

                        while (InputDevices.ContainsKey(newid))
                            newid++;

                        InputDevices.Add(newid, DeviceID);

                        string msg = "{\"" + Constants.Commands.REQUEST_INPUT_DEVICES + "\":[";
                        device.AudioEndpointVolume.OnVolumeNotification += InputDeviceVolChange;
                        msg += "[\"" + newid.ToString() + "\",\"" + device.FriendlyName + "\", " + device.AudioEndpointVolume.MasterVolumeLevelScalar * 100 + "]";

                        msg += "]}";

                        SendMessage(msg, false);
                    }
                }
                else
                    Console.WriteLine("er...idk lol");
            }
        }

        public void OnDeviceRemoved(string DeviceID)
        {
            if (Client.CurrentPage == Constants.Device_Pages.INPUTS)
            {
                if (InputDevices.Values.Contains(DeviceID))
                {
                    SendMessage("{\"" + Constants.Commands.REMOVE_INPUT_DEVICE + "\": " + InputDevices.GetKeysByValue(DeviceID).First() + "}", false);
                    MMDevice device = DevEnum.GetDevice(DeviceID);
                    device.AudioEndpointVolume.OnVolumeNotification -= InputDeviceVolChange;
                    InputDevices.Remove(InputDevices.GetKeysByValue(DeviceID).First());
                }
            }
        }
    }
}

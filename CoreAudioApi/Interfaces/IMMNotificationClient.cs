﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CoreAudioApi.Interfaces
{
    /// <summary>
    /// IMMNotificationClient
    /// </summary>
    [Guid("7991EEC9-7E89-4D85-8390-6C703CEC60C0"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMNotificationClient
    {
        /// <summary>
        /// Device State Changed
        /// </summary>
        void OnDeviceStateChanged([MarshalAs(UnmanagedType.LPWStr)] string deviceId, [MarshalAs(UnmanagedType.I4)] EDeviceState newState);

        /// <summary>
        /// Device Added
        /// </summary>
        void OnDeviceAdded([MarshalAs(UnmanagedType.LPWStr)] string pwstrDeviceId);

        /// <summary>
        /// Device Removed
        /// </summary>
        void OnDeviceRemoved([MarshalAs(UnmanagedType.LPWStr)] string deviceId);

        /// <summary>
        /// Default Device Changed
        /// </summary>
        void OnDefaultDeviceChanged(EDataFlow flow, ERole role, [MarshalAs(UnmanagedType.LPWStr)] string defaultDeviceId);

        /// <summary>
        /// Property Value Changed
        /// </summary>
        /// <param name="pwstrDeviceId"></param>
        /// <param name="key"></param>
        void OnPropertyValueChanged([MarshalAs(UnmanagedType.LPWStr)] string pwstrDeviceId, PropertyKey key);
    }
}

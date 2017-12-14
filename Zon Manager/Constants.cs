using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zön_Manager
{
    public static class Constants
    {
        public enum Notifications
        {
            BluetoothNotSupported,
            DeviceAttemptingConnection,
            DeviceLostConnection
        }

        public enum ErrorType
        {
            UnparseableData,
            UnrecognizedIdentifier,
            InvalidlyTimedCommand
        }

        public static class Commands
        {
            public const string DISCONNECT = "0";
            public const string UPDATE_OBJECT = "1";
            public const string UPDATE_PAGE = "2";
            public const string PING = "3";
            public const string PONG = "4";
            public const string GENERIC_MULTIPARTDATA = "5";
            public const string REQUEST_OUTPUT_DEVICES = "6";
            public const string REQUEST_AUDIO_SESSIONS = "8";
            public const string CHANGE_SESSION_LEVEL = "9";
            public const string REMOVE_SESSION = "10";
            public const string REQUEST_INPUT_DEVICES = "20";
            public const string CHANGE_INPUT_LEVEL = "21";
            public const string REMOVE_INPUT_DEVICE = "22";
            public const string REQUEST_TIME_ZONE = "23";

            public const string REQUEST_MONITOR_COUNT = "24";

            // Response:  {"25": }
            public const string REQUEST_MONITOR_THUMBNAILS = "25";
        }

        public static class Device_Pages
        {
            public const string MENU = "menu";
            public const string LEVELS = "levels";
            public const string DEVICE = "device";
            public const string LOCK = "lock";
            public const string SETTINGS = "settings";
            public const string MIC = "mic";
            public const string CHOOSE_OUTPUT = "choose-output";
            public const string CHOOSE_INPUT = "choose-input";
            public const string CONNECT = "connect";
            public const string SEARCH = "search";
            public const string INPUTS = "inputs";
            public const string ERROR = "error";
        }

        public static Dictionary<Notifications, object[]> NotificationsData = new Dictionary<Notifications, object[]>()
        {
            { Notifications.BluetoothNotSupported, new object[] {"Bluetooth is not supported", "You must enable bluetooth on this computer to use the Zön0.", 8000} },
            { Notifications.DeviceAttemptingConnection, new object[] {"A device is attempting to connect", "The 'device %devicename%' is attempting to connect to and control this computer's audio levels.  Click this notification to allow the connection.", 15000} },
            { Notifications.DeviceLostConnection, new object[] {"A device has lost connection", "The device '%devicename%' has lost connection to this computer unexpectedly.", 8000} }
        };

        public static Guid ZönServiceGUID = new Guid("12621182-1423-6869-face-b00bbabe1337");
        // 1-26-21-18-21                | 42368            | 69        | face b00b babe | 1337
        // --------------------------------------------------------------------------------------------
        // Azuru (Alphabetical indexes) | Azuru's IANA PEN | My fav. # | It's a babe    | leet (elite)
        //                              |                  |           | with a face    |
        //                              |                  |           | that's a boob  |

        
    }
}

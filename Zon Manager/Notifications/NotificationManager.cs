using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace Zön_Manager.Notifications
{
    public class NotificationManager
    {
        public List<Notification> Notifications = new List<Notification>();
        Timer relocateTimer = new Timer();
        public Screen rightScreen;

        public NotificationManager()
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                if (rightScreen != null)
                {
                    if (screen.Bounds.Right > rightScreen.Bounds.Right)
                        rightScreen = screen;
                }
                else
                {
                    rightScreen = screen;
                }
            }
            relocateTimer.Interval = 10;
            relocateTimer.Tick += relocateTimer_Tick;
            relocateTimer.Start();
        }

        void relocateTimer_Tick(object sender, EventArgs e)
        {
            SoftRelocateNotifications();
        }

        public Notification SendNotification(Constants.Notifications type)
        {
            return BackgroundWindow.instance.notificationManager.AddNotification((string)Constants.NotificationsData[type][0], (string)Constants.NotificationsData[type][1], (int)Constants.NotificationsData[type][2]);
        }

        public Notification SendNotification(Constants.Notifications type, string DeviceName)
        {
            return BackgroundWindow.instance.notificationManager.AddNotification(((string)Constants.NotificationsData[type][0]).Replace("%devicename%", DeviceName), ((string)Constants.NotificationsData[type][1]).Replace("%devicename%", DeviceName), (int)Constants.NotificationsData[type][2]);
        }

        public Notification AddNotification(string Title, string Description, int TimeoutMS)
        {
            System.Windows.Point loc;

            if (Notifications.Count == 0)
                loc = new System.Windows.Point(rightScreen.Bounds.Right + 430, rightScreen.Bounds.Height - 199);
            else
            {
                int highest_notif_loc = rightScreen.Bounds.Height;

                foreach (Notification new_notif in Notifications)
                {
                    if (new_notif.Top < highest_notif_loc)
                        highest_notif_loc = (int)new_notif.Top;
                }

                loc = new System.Windows.Point(rightScreen.Bounds.Right + 430, highest_notif_loc - 199);
                if (!isPointYOnscreen(loc))
                {
                    Notifications[0].CloseNotification();
                    loc = new System.Windows.Point((int)Notifications[Notifications.Count - 1].Left, (int)Notifications[Notifications.Count - 1].Top);
                }
            }
            Notification notif = new Notification(Title, Description, loc, this, TimeoutMS);
            notif.Closed += notif_Closed;
            Notifications.Add(notif);
            notif.Show();
            return notif;
        }

        void notif_Closed(object sender, EventArgs e)
        {
            Notification notif = (Notification)sender;
            int index = Notifications.IndexOf(notif);
            Notifications.Remove(notif);
        }

        public void SoftRelocateNotifications()
        {
            for (int i = 0; i < Notifications.Count; i++)
            {
                if (i == 0)
                    Notifications[i].MoveTo(new System.Windows.Point(rightScreen.Bounds.Right - Notifications[i].Width - 10, rightScreen.Bounds.Height - Notifications[i].Height - 10));
                else
                    Notifications[i].MoveTo(new System.Windows.Point(rightScreen.Bounds.Right - Notifications[i].Width - 10, Notifications[i - 1].Top - Notifications[i].Height - 10));
            }
        }

        public void RemoveNotification(Notification notif)
        {
            notif.CloseNotification();
        }

        private bool isPointYOnscreen(System.Windows.Point p)
        {
            return p.Y >= rightScreen.Bounds.Location.Y && p.Y <= rightScreen.Bounds.Location.Y + rightScreen.Bounds.Height;
        }
    }
}

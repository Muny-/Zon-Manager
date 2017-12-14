using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Zön_Manager.Notifications
{
    /// <summary>
    /// Interaction logic for Notification.xaml
    /// </summary>
    public partial class Notification : Window
    {

        public string Description = "";
        public Point TargetLocation;
        private Point startedLocation;
        private NotificationManager notificationManager;

        ExponentialEase ease2 = new ExponentialEase();
        double duration2 = 0;

        QuinticEase ease = new QuinticEase();
        double duration = 1;

        Timer moveTimer = new Timer();
        Timer hideNotificationTimer = new Timer();
        Timer showNotificationTimer = new Timer();
        Timer closeNotificationTimer = new Timer();

        public bool HasBeenClicked = false;

        public Notification(string title, string description, Point location, NotificationManager manager, int TimeoutMS)
        {
            Title = title;
            Description = description;
            notificationManager = manager;
            ease.EasingMode = EasingMode.EaseIn;
            ease2.EasingMode = EasingMode.EaseInOut;
            InitializeComponent();
            this.Left = location.X;
            this.Top = location.Y;
            label1.Content = this.Title;
            label2.Text = this.Description;
            moveTimer.Interval = 1;
            hideNotificationTimer.Interval = 1;
            showNotificationTimer.Interval = 1;
            closeNotificationTimer.Interval = TimeoutMS;
            moveTimer.Tick += moveTimer_Tick;
            hideNotificationTimer.Tick += hideNotificationTimer_Tick;
            showNotificationTimer.Tick += showNotificationTimer_Tick;
            closeNotificationTimer.Tick += closeNotificationTimer_Tick;
        }

        void closeNotificationTimer_Tick(object sender, EventArgs e)
        {
            CloseNotification();
        }

        void showNotificationTimer_Tick(object sender, EventArgs e)
        {
            if (duration > 0.01)
            {
                duration -= 0.02;
                this.Left = (int)Math.Round(ease.Ease(duration) * notificationManager.rightScreen.Bounds.Right, 0) + notificationManager.rightScreen.Bounds.Right - this.Width - 10;
            }
            else
            {
                showNotificationTimer.Stop();
            }
        }

        void hideNotificationTimer_Tick(object sender, EventArgs e)
        {
            if (this.Opacity > 0)
            {
                this.Opacity -= 0.05;
            }
            else
            {
                hideNotificationTimer.Stop();
                this.Close();
            }
        }

        void moveTimer_Tick(object sender, EventArgs e)
        {
            if (duration2 < 1)
            {
                duration2 += 0.02;

                if (TargetLocation.Y > startedLocation.Y)
                {
                    this.Top = (int)Math.Round(ease2.Ease(duration2) * (TargetLocation.Y - startedLocation.Y), 0) + startedLocation.Y;
                }
                else
                {
                    this.Top = startedLocation.Y - (int)Math.Round(ease2.Ease(duration2) * (startedLocation.Y - TargetLocation.Y), 0);
                }
            }
            else
            {
                moveTimer.Stop();
                duration2 = 0;
            }
        }

        internal void CloseNotification()
        {
            hideNotificationTimer.Start();
        }

        internal void MoveTo(Point location)
        {
            duration2 = 0.3;
            startedLocation = new Point(this.Left, this.Top);
            TargetLocation = location;
            moveTimer.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            showNotificationTimer.Start();
            closeNotificationTimer.Start();
        }

        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            HasBeenClicked = true;
            CloseNotification();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    class NotificationArea
    {
        static List<Notification> Notifications;

        public NotificationArea()
        {
            Notifications = new List<Notification>();
            Game1.Instance.graphics.DeviceReset += new EventHandler<EventArgs>(graphics_DeviceReset);
        }

        void graphics_DeviceReset(object sender, EventArgs e)
        {
            foreach (Notification not in Notifications)
                not.Validate();
        }

        public void Update()
        {
            List<Notification> temp = Notifications.ToList();
            foreach (Notification not in temp)
                not.Update();
        }

        public static void Write(string text)
        {
            Notification not = new Notification(text);
            if (Notifications.Count > 0)
            {
                foreach (Notification notification in Notifications)
                    notification.Offset.Y -= not.Height;
            }
            not.DurationFinished += new EventHandler<EventArgs>(Notification_DurationFinished);
            Notifications.Add(not);
        }

        static void Notification_DurationFinished(object sender, EventArgs e)
        {
            Notifications.Remove(sender as Notification);
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Begin();
            foreach (Notification not in Notifications)
            {
                not.Draw(sb);
            }
            sb.End();
        }
    }
}

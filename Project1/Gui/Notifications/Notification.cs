using System;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    public class Notification : Label
    {
        public Vector2 Offset = Vector2.Zero;
        public static int Duration = 10;
        float Timer;
        public bool WarpText;
        public static int WidthMax = 100;


        public event EventHandler<EventArgs> DurationFinished;
        protected void OnDurationFinished()
        {
            if (DurationFinished != null)
                DurationFinished(this, EventArgs.Empty);

            Hide();
        }

        public Notification(string text)
        {
            Text = text;
            WarpText = false;
            Timer = 60 * Notification.Duration;
            Location =  - new Vector2(0, UIManager.Height / 4);
        }
       
        public override void Update()
        {
            base.Update();
            Timer -= 1;
            if (Timer < 0)
                OnDurationFinished();
        }
    }
}

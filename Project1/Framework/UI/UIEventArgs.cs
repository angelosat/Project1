using System;

namespace Project1.Core
{
    public delegate void UIEvent(object sender, EventArgs e);

    public class UIEventArgs : EventArgs
    {
        public Element Sender;
        public int Message;

        public UIEventArgs(Element sender)
        {
            Sender = sender;
        }
        public UIEventArgs(int message)
        {
            Message = message;
        }
    }
}

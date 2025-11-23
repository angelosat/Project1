using System;

namespace Start_a_Town_
{
    public class GameEvent : EventArgs
    {
        public double TimeStamp;
        public Components.Message.Types Type;
        public object[] Parameters;
        public GameEvent(double timestamp, Components.Message.Types type, params object[] parameters)
        {
            this.TimeStamp = timestamp;
            this.Type = type;
            this.Parameters = parameters;
        }
    }
}
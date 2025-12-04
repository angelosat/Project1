using System;

namespace Start_a_Town_
{
    //public class GameEvent<T>(TimeSpan clock, int type, T parameters) : EventArgs
    //{
    //    public double TimeStamp = clock.TotalMilliseconds;
    //    public int Type = type;
    //    public T Parameters = parameters;
    //}
    public class GameEvent : EventArgs
    {
        public double TimeStamp;
        public Components.Message.Types Type;
        public object[] Parameters;
        public object Payload;
        public object this[int index] => this.Parameters[index];
        public GameEvent(double timestamp, Components.Message.Types type, params object[] parameters)
        {
            this.TimeStamp = timestamp;
            this.Type = type;
            this.Parameters = parameters;
        }
        public GameEvent(double timestamp, int eventTypeId, params object[] parameters)
        {
            this.TimeStamp = timestamp;
            this.Type = (Components.Message.Types)eventTypeId;
            this.Parameters = parameters;
        }
        public GameEvent(TimeSpan clock, Components.Message.Types type, params object[] parameters)
        {
            this.TimeStamp = clock.TotalMilliseconds;
            this.Type = type;
            this.Parameters = parameters;
        }
    }
}
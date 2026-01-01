using System;

namespace Start_a_Town_
{
    public class GameEvent : EventArgs
    {
        public double TimeStamp;
        public int Type;
        public object[] Parameters;
        public IEventPayload Payload;
        public object this[int index] => this.Parameters[index];
        public GameEvent(int id, IEventPayload payload)
        {
            this.Type = id;
            this.Payload = payload;
        }
        public GameEvent(double timestamp, IEventPayload payload)
        {
            //this.Type = id;
            this.TimeStamp = timestamp;
            this.Payload = payload;
        }
        public GameEvent(double timestamp, Components.Message.Types type, params object[] parameters)
        {
            this.TimeStamp = timestamp;
            this.Type = (int)type;
            this.Parameters = parameters;
        }
        public GameEvent(double timestamp, int eventTypeId, params object[] parameters)
        {
            this.TimeStamp = timestamp;
            this.Type = eventTypeId;
            this.Parameters = parameters;
        }
        public GameEvent(TimeSpan clock, Components.Message.Types type, params object[] parameters)
        {
            this.TimeStamp = clock.TotalMilliseconds;
            this.Type = (int)type;
            this.Parameters = parameters;
        }
    }
}
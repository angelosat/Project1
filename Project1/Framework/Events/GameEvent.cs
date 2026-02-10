using System;

namespace Project1.Framework.Events
{
    public class GameEvent : EventArgs
    {
        public double TimeStamp;
        public int Type;
        public object[] Parameters;
        public IEventPayload Payload;
        public GameEvent(int id, IEventPayload payload)
        {
            this.Type = id;
            this.Payload = payload;
        }
        public GameEvent(double timestamp, int eventTypeId, params object[] parameters)
        {
            this.TimeStamp = timestamp;
            this.Type = eventTypeId;
            this.Parameters = parameters;
        }
    }
}
using Project1.Core.Components;
using Project1.Framework.Events;
using System;

namespace Project1.Core
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
            this.TimeStamp = timestamp;
            this.Payload = payload;
        }
        public GameEvent(double timestamp, Message.Types type, params object[] parameters)
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
        public GameEvent(TimeSpan clock, Message.Types type, params object[] parameters)
        {
            this.TimeStamp = clock.TotalMilliseconds;
            this.Type = (int)type;
            this.Parameters = parameters;
        }
    }
}
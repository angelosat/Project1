using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public class ConversationNew
    {
        readonly List<Actor> Participants = new(2);

        public int Step;

        public ConversationNew(Actor initiator, params Actor[] otherParticipants)
        {
            if (otherParticipants.Contains(initiator))
                throw new Exception();
            this.Participants.Add(initiator);
            this.Participants.AddRange(otherParticipants);
        }
        public Actor CurrentTalker
        {
            get
            {
                var participantCount = this.Participants.Count;
                return this.Participants[Step % participantCount];
            }
        }
        public void Advance(Actor actor, ConversationTopic topic)
        {
            if (this.CurrentTalker != actor)
                throw new Exception();
            topic.Apply(actor, this);
            actor.Net.Report(string.Format("conversation advanced {0}", topic.ToString()));
            this.Step++;
        }
        public IEnumerable<Actor> GetParticipants()
        {
            foreach (var p in this.Participants)
                yield return p;
        }
    }
}

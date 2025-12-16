using System.Collections.Generic;
using Start_a_Town_.Components;

namespace Start_a_Town_.AI
{
    public class AIConversationManager
    {
        public List<Conversation> Conversations = new();
        public Conversation Start(GameObject initiator, GameObject target)
        {
            var c = new Conversation(initiator, target);
            this.Conversations.Add(c);
            return c;
        }
        public class Conversation
        {
            public enum States { Requested, Accepted, Declined, Started, Finished}
            public States State;
            public GameObject Initiator, Target;
            public Progress Progress = new Progress(0, Ticks.PerSecond, 0);
            public Conversation(GameObject initiator, GameObject target)
            {
                this.Initiator = initiator;
                this.Target = target;
            }

            internal void Tick()
            {
                switch (this.State)
                {
                    case States.Accepted:
                        this.State = States.Started;
                        break;

                    case States.Started:
                        var prog = this.Progress;
                        prog.Value += 1;
                        if (prog.Percentage >= 1)
                        {
                            prog.Value = 0;
                            var socialInitiator = this.Initiator.GetNeed(NeedDefOf.Social);
                            NeedsComponent.ModifyNeed(this.Initiator, NeedDefOf.Social, 1);
                            if (socialInitiator.Percentage == 1)
                                this.State = States.Finished;
                            NeedsComponent.ModifyNeed(this.Target, NeedDefOf.Social, 1);
                        }
                        break;

                    default:
                        break;
                }
            }
        }
    }
}

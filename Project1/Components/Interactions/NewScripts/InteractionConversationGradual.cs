using System.IO;

namespace Start_a_Town_
{
    class InteractionConversationGradual : InteractionPerpetual
    {
        int CurrentTickInt;
        ConversationTopic Topic;
        public InteractionConversationGradual(ConversationTopic topic) : base("Chatting")
        {
            this.Topic = topic;
            this.Verb = this.Name;
        }
        public InteractionConversationGradual() : base("Chatting")
        {
            this.Verb = this.Name;
        }
        public override object Clone()
        {
            return new InteractionConversationGradual(this.Topic);
        }

        public override void OnUpdate()
        {
            var a = this.Actor;
            var t = this.Target;
            this.Topic.ApplyNew(a, t.Object as Actor);
            this.CurrentTickInt++;
            if (this.CurrentTickInt >= this.Topic.MaxTicks)
            {
                a.FinishConversation();
                this.Finish();
            }
        }

        protected override void WriteExtra(BinaryWriter w)
        {
            this.Topic.Write(w);
        }
        protected override void ReadExtra(BinaryReader r)
        {
            this.Topic = Def.GetDef<ConversationTopic>(r.ReadString());
        }
    }
}

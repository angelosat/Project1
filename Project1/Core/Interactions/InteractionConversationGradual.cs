using Project1.Framework.Interactions;
using Start_a_Town_;
using System.IO;

namespace Project1.Core.Interactions
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
        protected override void OnAddProgress(int v)

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
        //protected override SkillDef GetSkill() => SkillDefOf.Social;
        protected override void WriteExtra(IDataWriter w)
        {
            this.Topic.Write(w);
        }
        protected override void ReadExtra(IDataReader r)
        {
            this.Topic = Framework.Base.Def.GetDef<ConversationTopic>(r.ReadString());
        }
    }
}

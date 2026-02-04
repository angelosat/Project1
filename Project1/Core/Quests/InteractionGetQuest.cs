using Project1.Framework.Interactions;
using Start_a_Town_;
using System;
using System.IO;

namespace Project1.Core.Quests
{
    class InteractionGetQuest : Interaction
    {
        int QuestID = -1;
        public InteractionGetQuest()
        {

        }
        public InteractionGetQuest(int questID)
        {
            this.QuestID = questID;
        }

        public override void Perform()
        {
            var actor = this.Actor;
            if (this.QuestID == -1)
                throw new Exception();

            actor.AcceptQuest(this.QuestID);
        }
        protected override void WriteExtra(IDataWriter w)
        {
            w.Write(this.QuestID);
        }
        protected override void ReadExtra(IDataReader r)
        {
            this.QuestID = r.ReadInt32();
        }
    }
}

using System;
using System.IO;

namespace Start_a_Town_
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

        public override object Clone()
        {
            return new InteractionGetQuest(this.QuestID);
        }

        protected override void WriteExtra(BinaryWriter w)
        {
            w.Write(this.QuestID);
        }
        protected override void ReadExtra(BinaryReader r)
        {
            this.QuestID = r.ReadInt32();
        }
    }
}

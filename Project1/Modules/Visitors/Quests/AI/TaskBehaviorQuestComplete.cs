using System.Collections.Generic;
using System.IO;

namespace Start_a_Town_
{
    class TaskBehaviorQuestComplete : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            var actor = this.Actor;
            var task = this.Task;
            var qgiver = TargetIndex.A;
            yield return BehaviorHelper.MoveTo(qgiver);
            yield return new BehaviorInteractionNew(qgiver, () => new InteractionQuestDeliver(task.Quest));
        }
        public override void CleanUp()
        {
            var actor = this.Actor;
            var task = this.Task;
            actor.Town.QuestManager.RemoveQuestReceiver(task.Quest);
        }
        class InteractionQuestDeliver : Interaction
        {
            int QuestID;
            public InteractionQuestDeliver()
            {

            }
            public InteractionQuestDeliver(int qID)
            {
                this.QuestID = qID;
            }
            public override void Perform()
            {
                var actor = this.Actor;
                var q = actor.Town.GetQuest(this.QuestID);
                
                q.Deliver(actor);
            }
            protected override void AddSaveData(SaveTag tag)
            {
                this.QuestID.Save(tag, "QuestID");
            }
            public override void LoadData(SaveTag tag)
            {
                this.QuestID.TryLoad(tag, "QuestID");
            }
            protected override void WriteExtra(BinaryWriter w)
            {
                w.Write(this.QuestID);
            }
            protected override void ReadExtra(BinaryReader r)
            {
                this.QuestID = r.ReadInt32();
            }
            public override object Clone()
            {
                return new InteractionQuestDeliver(this.QuestID);
            }
        }
    }
}

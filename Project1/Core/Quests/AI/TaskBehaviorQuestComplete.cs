using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.Helpers;
using Project1.Core.Quests;
using Project1.Core.Towns;
using Project1.Core.Base;
using Project1.Core.Helpers;
using Project1.Core.Interactions;
using System.Collections.Generic;
using System.IO;
using Project1.Core.AI;
using Project1.Core.AI.Behaviors.NodeTypes;

namespace Project1.Core.Quests.AI
{
    class TaskBehaviorQuestComplete : BehaviorExecutePlan
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            var actor = this.Actor;
            var task = this.Plan;
            var qgiver = TargetIndex.A;
            yield return BehaviorHelper.MoveTo(qgiver);
            yield return new BehaviorResolveInteraction(qgiver, () => new InteractionQuestDeliver(task.Quest));
        }
        public override void CleanUp()
        {
            var actor = this.Actor;
            var task = this.Plan;
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
}

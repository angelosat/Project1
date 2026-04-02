using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.Helpers;
using Project1.Core.AI.Behaviors.NodeTypes;
using System;
using System.Collections.Generic;

namespace Project1.Core.Systems.Quests.AI
{
    class TaskBehaviorQuestComplete : BehaviorExecutePlan
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            var actor = this.Actor;
            var task = this.Plan;
            var qgiver = TargetIndex.A;
            yield return BehaviorHelper.MoveTo(qgiver);
            throw new NotImplementedException();
            //yield return new BehaviorResolveInteraction(qgiver, () => new InteractionQuestDeliver(task.Quest));
        }
        public override void CleanUp()
        {
            var actor = this.Actor;
            var task = this.Plan;
            actor.Town.QuestManager.RemoveQuestReceiver(task.Quest);
        }
    }
}

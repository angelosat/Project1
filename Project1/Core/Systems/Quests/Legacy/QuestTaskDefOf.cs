using Project1.Core.AI;
using Project1.Core.Systems.Quests.AI;

namespace Project1.Core.Systems.Quests.Legacy
{
    class QuestTaskDefOf
    {
        static public PlanDef AcceptQuest = new("AcceptQuest", typeof(TaskBehaviorGetQuest));
    }
}

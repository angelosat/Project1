using Project1.Core.AI;
using Project1.Core.Quests.AI;

namespace Project1.Core.Quests
{
    class QuestTaskDefOf
    {
        static public PlanDef AcceptQuest = new("AcceptQuest", typeof(TaskBehaviorGetQuest));
    }
}

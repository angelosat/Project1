using Project1.Core.Quests.AI;
using Start_a_Town_;

namespace Project1.Core.Quests
{
    class QuestTaskDefOf
    {
        static public PlanDef AcceptQuest = new("AcceptQuest", typeof(TaskBehaviorGetQuest));
    }
}

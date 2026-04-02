using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Planners;
using Project1.Core.Interactions;
using Project1.Framework;
using System;

namespace Project1.Core.Systems.Quests;

public class QuestDef(string name, Type runtimeType) : Def(name)
{
    public readonly Type RuntimeType = runtimeType;
}
[EnsureStaticCtorCall]
public static class QuestDefOf
{
    public static readonly QuestDef Deliver = new("Deliver", typeof(FetchQuestRuntime));
    public static readonly InteractionDef InteractionQuests = new("AcceptQuests", typeof(InteractionAcceptQuest), InteractionControllers.Timed);
    public static readonly PlannerDef PlannerQuests = new("Quests", typeof(PlannerQuest));
    public static readonly PlanDef PlanQuest = new("Quests", typeof(BehaviorExecutePlanNew), InteractionQuests);
    static QuestDefOf()
    {
        Def.Register(typeof(QuestDefOf));
    }
}
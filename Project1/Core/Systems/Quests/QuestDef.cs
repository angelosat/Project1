using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Planners;
using Project1.Core.Animations;
using Project1.Core.Interactions;
using Project1.Framework;
using Project1.Framework.Helpers;
using System;

namespace Project1.Core.Systems.Quests;

public class QuestDef(string name, Type runtimeType, Type resolverType) : Def(name)
{
    public readonly Type RuntimeType = runtimeType;
    public readonly QuestResolver Resolver = ActivatorSafe<QuestResolver>.CreateInstance(resolverType);
}
[EnsureStaticCtorCall]
public static class QuestDefOf
{
    public static readonly QuestDef Deliver = new("Deliver", typeof(QuestRuntime_Deliver), typeof(QuestResolver_Deliver));
    public static readonly InteractionDef InteractionQuests = new("AcceptQuests", typeof(InteractionAcceptQuest), InteractionControllers.Timed);
    public static readonly InteractionDef InteractionQuestComplete = new("CompleteQuest", typeof(InteractionCompleteQuest))
    {
        Animation = AnimationDefOf.TouchItem,
        Controller = InteractionControllers.FirstContact
    };
    public static readonly PlannerDef PlannerQuests = new("Quests", typeof(PlannerQuest));
    public static readonly PlanDef PlanQuest = new("Quests", typeof(BehaviorExecutePlanNew), InteractionQuests);
    public static readonly PlanDef PlanQuestComplete = new("QuestComplete", typeof(BehaviorExecutePlanNew), InteractionQuestComplete);
    static QuestDefOf()
    {
        Def.Register(typeof(QuestDefOf));
    }
}
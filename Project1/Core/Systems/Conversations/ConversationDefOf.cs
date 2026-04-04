using Project1.Core.AI;
using Project1.Core.AI.Planners;
using Project1.Core.Interactions;
using Project1.Framework;

namespace Project1.Core.Systems.Conversations;

[EnsureStaticCtorCall]
public static class ConversationDefOf
{
    public static readonly PlannerDef PlannerConvo = new("Initiator", typeof(PlannerConversation));

    public static readonly InteractionDef InteractionAdvance = new("Advance", typeof(InteractionConversationAdvance), InteractionControllers.Timed);
    public static readonly InteractionDef InteractionReceive = new("Receive", typeof(InteractionConversationReceive), InteractionControllers.ExternalFull)
    {
        Range = InteractionRange.None
    };

    public static readonly PlanDef PlanAdvance = new("Advance", InteractionAdvance);
    public static readonly PlanDef PlanReceive = new("Receive", InteractionReceive);

    static ConversationDefOf()
    {
        Def.Register(typeof(ConversationDefOf));
    }
}

using Project1.Core.AI;
using Project1.Core.AI.Planners;
using Project1.Core.Interactions;
using Project1.Framework;

namespace Project1.Core.Systems.Conversations;

[EnsureStaticCtorCall]
public static class ConversationDefOf
{
    public static readonly PlannerDef PlannerConvo = new("Initiator", typeof(PlannerConversation));

    public static readonly InteractionDef InteractionTalk = new("Talk", typeof(InteractionConversationTalk), InteractionControllers.Timed);
    public static readonly InteractionDef InteractionListen = new("Listen", typeof(InteractionConversationListen), InteractionControllers.ExternalFull)
    {
        Range = InteractionRange.None
    };

    public static readonly PlanDef PlanTalk = new("Talk", InteractionTalk);
    public static readonly PlanDef PlanListen = new("Listen", InteractionListen);

    static ConversationDefOf()
    {
        Def.Register(typeof(ConversationDefOf));
    }
}

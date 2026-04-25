using Project1.Core.Interactions;

namespace Project1.Core.Systems.Conversations;
internal class InteractionConversationTalk : InteractionLogic
{
    protected override InteractionContext_Conversation CreateContextInt() => new();
    static InteractionContext_Conversation TypedContext(Interaction i) => (InteractionContext_Conversation)i.Context;

    internal override bool HasFailed(Interaction i)
        => !TypedContext(i).Conversation.IsRunning;

    internal override void OnFinish(Interaction i)
    {
        if (i.Actor.Net.IsClient)
            return;
        var typedCtx = (InteractionContext_Conversation)i.Context;
        i.Actor.Map.Town.Conversations.Advance(i.Actor);
    }
}

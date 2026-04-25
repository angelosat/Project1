using Project1.Core.Interactions;

namespace Project1.Core.Systems.Conversations;

internal class InteractionConversationListen : InteractionLogic
{
    protected override InteractionContext_Conversation CreateContextInt() => new();
    static InteractionContext_Conversation TypedContext(InteractionContext ctx) => (InteractionContext_Conversation)ctx;
    internal override bool HasSucceeded(Interaction i)
    {
        var typedCtx = (InteractionContext_Conversation)i.Context;
        var convo = typedCtx.Conversation;
        if (convo is null)
            return true;
        if (convo.CurrentTalker == i.Actor.RefId)
            return true;
        return false;
    }
    internal override bool HasFailed(Interaction i)
        => TypedContext(i.Context).Patience.IsDepleted;

    internal override void OnFailure(Interaction i)
        => TypedContext(i.Context).Conversation.MarkFinished();

    internal override void OnTick(Interaction i)
        => TypedContext(i.Context).Patience.ApplyAccumulatorDelta(-.05f);//-1f);//.05f);
}

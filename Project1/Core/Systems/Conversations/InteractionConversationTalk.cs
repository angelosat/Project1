using Project1.Core.Interactions;
using Project1.Core.Resources;

namespace Project1.Core.Systems.Conversations;

internal sealed class InteractionContext_Conversation : InteractionContext
{
    internal ConversationRuntime Conversation => field ??= this.Actor.Map.Town.Conversations.GetConversationByActor(this.Actor);
    internal IResourceView Patience => field ??= this.Actor.Resources.View(ResourceDefOf.Patience);
    internal override float GetPercentage(Interaction i) => Patience.Percentage;
}
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

using Project1.Core.Interactions;

namespace Project1.Core.Systems.Conversations;

internal sealed class InteractionContext_Conversation : InteractionContext
{
    internal ConversationRuntime Conversation => field ??= this.Actor.Map.Town.Conversations.GetConversationByActor(this.Actor);
}
internal class InteractionConversationReceive : InteractionLogic
{
    protected override InteractionContext_Conversation CreateContextInt() => new();
    internal override bool HasSucceeded(Interaction i)
    {
        var typedCtx = (InteractionContext_Conversation)i.Context;
        var convo = typedCtx.Conversation;
        if (convo is null)
            return true;
        if (convo.CurrentTalker == i.Actor.RefId)
            return true;
        return false;
        //var typedCtx = (InteractionContext_Conversation)i.Context;
        //var convo = typedCtx.Conversation;
        //if (convo.CurrentTalker == i.Actor.RefId)
        //    return true;
        //return false;
    }
}
internal class InteractionConversationAdvance : InteractionLogic
{
    protected override InteractionContext_Conversation CreateContextInt() => new();
    internal override void OnFinish(Interaction i)
    {
        if (i.Actor.Net.IsClient)
            return;
        var typedCtx = (InteractionContext_Conversation)i.Context;
        i.Actor.Map.Town.Conversations.Advance(i.Actor);
    }
}

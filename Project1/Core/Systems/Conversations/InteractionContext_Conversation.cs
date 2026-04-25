using Project1.Core.Interactions;
using Project1.Core.Resources;

namespace Project1.Core.Systems.Conversations;

internal sealed class InteractionContext_Conversation : InteractionContext
{
    internal ConversationRuntime Conversation => field ??= this.Actor.Map.Town.Conversations.GetConversationByActor(this.Actor);
    internal IResourceView Patience => field ??= this.Actor.Resources.View(ResourceDefOf.Patience);
    internal override float GetPercentage(Interaction i) => Patience.Percentage;
}

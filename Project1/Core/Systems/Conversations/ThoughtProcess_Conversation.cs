using Project1.Core.AI;
using Project1.Core.AI.Thought;
using Project1.Core.Needs;

namespace Project1.Core.Systems.Conversations
{
    internal class ThoughtProcess_Conversation : ThoughtProcess
    {
        internal override void TickOnMap(AIState state)
        {
            var actor = state.Owner;
            var socialPercentage = actor.Needs.GetPercentage(NeedDefOf.Social);
            var threshold = .5f;
            if (socialPercentage > threshold)
                return;
            var manager = actor.Map.Town.Conversations;
            if (manager.TryGetConversation(actor, out var existing))
                return;
            var availablePartners = manager.GetAvailableActors();
            foreach(var other in availablePartners)
            {
                if (!actor.CanReachAndReserve(other))
                    continue;
                if (manager.TryStartConversation(actor, other))
                    return;
            }
        }

        internal override void TickOffMap(AIState state) { }
    }
}

using System.Linq;
using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Behaviors.Conversation;
using Project1.Core.Towns.AI.Needs;
using Project1.Core.Entities.Actors;
using Project1.Core.Towns.Duties;

namespace Project1.Core.Towns.AI.Behaviors
{
    class TaskGiverOfferGuidance : Planner
    {
        protected override Plan TryPlan(Actor actor)
        {
            if (!actor.HasDuty(DutyDefOf.Guide))
                return null;
            var visitors = actor.Map.World.Population.Find(v => v.Actor.Map == actor.Map && v.Actor.GetNeed(AdventurerNeedsDefOf.Guidance).Value < 50);

            // TODO sort visitors here by urgency
            var visitor = visitors.FirstOrDefault();
            if (visitor == null)
                return null;
            var elapsed = visitor.GetTimeElapsed();
            if (elapsed.TotalSeconds < 2)
                return null;
            if (visitor.Actor.CurrentPlan != null)
                return null;
            if (visitor.Actor.AI.State.ConversationPartner != null)
                return null;
            visitor.Actor.AI.State.ConversationPartner = actor;
            actor.AI.State.ConversationPartner = visitor.Actor;
            actor.EnqueueCommunication(visitor.Actor, ConversationTopic.Guidance);
            return new Plan(PlanDefOf.Chatting, new InteractionTarget(visitor.Actor));
        }
    }
}

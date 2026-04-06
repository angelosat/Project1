using Project1.Core.AI;
using Project1.Core.AI.Behaviors;
using Project1.Core.AI.Personality;
using Project1.Core.Entities.Actors;
using Project1.Core.Needs;
using System;

namespace Project1.Core.Systems.Conversations;

internal sealed class PlannerConversation : Planner
{
    protected override Plan TryPlan(Actor actor)
    {
        if (actor.IsHauling)
            return null;

        var manager = actor.Map.Town.Conversations;

        if (manager.TryGetConversation(actor, out var convo))
        {
            if (convo.CurrentTalker == actor.RefId)
            {
                if (actor.RefId == convo.Initiator)
                {
                    if (convo.IsRequested)
                        return null;
                    if (IsSatisfied(actor))
                    {
                        convo.MarkFinished();
                        return null;
                    }
                }
                var other = actor.Map.World.Get<Actor>(convo.CurrentListener);
                if (!actor.CanReachAndReserve(other))
                    throw new InvalidOperationException("Conversation shouldn't continue if actors can't reach eachother");
                var manners = actor.Personality.GetPercentage(TraitDefOf.Manners);

                //float signChance = 0.5f + manners * 0.5f;
                var signBase = .5f;// 66f;
                float signChance = signBase + manners * (1 - signBase);
                // Manners -1 → 0% positive, +1 → 100% positive
                var rand = actor.World.Random;
                int sign = rand.NextDouble() < signChance ? 1 : -1;
                float strength = (float)rand.NextDouble();

                float magnitude = sign * strength;

                var intent = new ConvoIntent_Compliment(magnitude);// manners * 10);
                manager.SetNextIntent(actor, intent);
                return new Plan(ConversationDefOf.PlanTalk, other);
            }
            else if (convo.CurrentListener == actor.RefId)
            {
                if (actor.RefId == convo.Target && convo.IsRequested)
                    convo.MarkAccepted();
                var other = actor.Map.World.Get<Actor>(convo.CurrentTalker);
                if (!actor.CanReachAndReserve(other))
                    throw new InvalidOperationException("Conversation shouldn't continue if actors can't reach eachother");
                return new Plan(ConversationDefOf.PlanListen, other);
            }
            return null;
        }

        if (!NeedsSocial(actor))
            return null;

        var availablePartners = manager.GetAvailableActors();
        foreach (var other in availablePartners)
        {
            if (other == actor)
                continue;
            if (!actor.CanReachAndReserve(other))
                continue;
            if (manager.TryStartConversation(actor, other))
                return null;
        }

        return null;
    }

    static bool NeedsSocial(Actor actor)
    {
        var socialPercentage = actor.Needs.GetPercentage(NeedDefOf.Social);
        var threshold = .5f;
        return socialPercentage < threshold;
    }
    static bool IsSatisfied(Actor actor)
    {
        var socialPercentage = actor.Needs.GetPercentage(NeedDefOf.Social);
        var threshold = .95f;
        return socialPercentage >= threshold;
    }
}

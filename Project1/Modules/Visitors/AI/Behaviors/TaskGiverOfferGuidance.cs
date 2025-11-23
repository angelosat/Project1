using System.Linq;

namespace Start_a_Town_
{
    class TaskGiverOfferGuidance : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            if (!actor.HasJob(JobDefOf.Guide))
                return null;
            var visitors = actor.Map.World.Population.Find(v => v.Actor.Map == actor.Map && v.Actor.GetNeed(VisitorNeedsDefOf.Guidance).Value < 50);

            // TODO sort visitors here by urgency
            var visitor = visitors.FirstOrDefault();
            if (visitor == null)
                return null;
            var elapsed = visitor.GetTimeElapsed();
            if (elapsed.TotalSeconds < 2)
                return null;
            if (visitor.Actor.CurrentTask != null)
                return null;
            if (visitor.Actor.GetState().ConversationPartner != null)
                return null;
            visitor.Actor.GetState().ConversationPartner = actor;
            actor.GetState().ConversationPartner = visitor.Actor;
            actor.EnqueueCommunication(visitor.Actor, ConversationTopic.Guidance);
            return new AITask(TaskDefOf.Chatting, new TargetArgs(visitor.Actor));
        }
    }
}

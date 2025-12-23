using System;

namespace Start_a_Town_
{
    public abstract class RoleMetaWrapper
    {
        public struct MetaDecision
        {
            ulong NextEvaluationTick;
            int FailureStreak;

            internal bool CanEvaluate(ulong currentTick)
            {
                return currentTick >= this.NextEvaluationTick;
            }

            internal void RegisterFailure()
            {
                this.FailureStreak++;
            }
            internal void RegisterSuccess()
            {
                this.FailureStreak = 0;
            }
            internal void ScheduleNext(WorldBase world)
            {
                var basedelay = (ulong)world.Random.Next(Ticks.FromHours(2), Ticks.FromHours(4));
                var damping = (ulong)(this.FailureStreak * Ticks.FromHours(2));
                this.NextEvaluationTick = world.CurrentTick + basedelay + damping;
            }
        }

        public Actor Actor;
        public RoleMetaDef Def;
        public FrontierDef TargetFrontier;
        public ulong LastMapTranTsitionTick;
        //public ulong NextDecisionTime;
        public MetaDecision LocationDecision;
        internal virtual void AssignTo(Actor actor)
        {
            this.Actor = actor;
            actor.AI.Meta?.RemoveFrom(actor);
            actor.AI.Meta = this;
            actor.Needs.Add(this.Def.Needs);
        }

        private void RemoveFrom(Actor actor)
        {
            actor.Needs.Remove(this.Def.Needs);
        }

        internal virtual void Tick() => this.Def.Worker.Tick(this);

        internal void ReturnToTown()
        {
            this.TargetFrontier = null;
        }

       
    }
}

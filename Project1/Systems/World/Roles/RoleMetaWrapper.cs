using System;

namespace Start_a_Town_
{
    public abstract class RoleMetaWrapper
    {
        public RoleMetaDef Def;
        public FrontierDef TargetFrontier;
        public ulong LastMapTransitionTick;
        internal virtual void AssignTo(Actor actor)
        {
            actor.AI.Meta?.RemoveFrom(actor);
            actor.AI.Meta = this;
            actor.Needs.Add(this.Def.Needs);
        }

        private void RemoveFrom(Actor actor)
        {
            actor.Needs.Remove(this.Def.Needs);
        }

        internal virtual void Tick(Actor actor) => this.Def.Worker.Tick(actor);

        internal void ReturnToTown()
        {
            this.TargetFrontier = null;
        }
    }
}

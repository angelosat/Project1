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
            var oldRole = actor.AI.Meta;
            if (oldRole is not null) oldRole.RemoveFrom(actor);
            actor.Needs.Add(this.Def.Needs);
        }

        private void RemoveFrom(Actor actor)
        {
            actor.Needs.Remove(this.Def.Needs);
        }

        internal virtual void Tick() { }
    }
}

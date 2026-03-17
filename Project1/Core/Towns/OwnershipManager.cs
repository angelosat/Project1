using Project1.Core.Blocks;
using Project1.Core.Entities.Actors;
using Project1.Framework;
using System.Collections.Generic;

namespace Project1.Core.Towns
{
    public sealed class OwnershipManager : TownComponent
    {
        public override string Name => "Ownership";

        readonly Dictionary<EntityRefId, HashSet<IntVec3>> _actorPossesions = [];

        public OwnershipManager(Town town) : base(town)
        {
        }

        public IEnumerable<IntVec3> GetOwnedBlocks(Actor actor)
        {
            //    this._actorPossesions[actor.RefId];
            if (this._actorPossesions.TryGetValue(actor.RefId, out var list))
                return list;
            return [];
        }

        void Add(EntityRefId actor, IntVec3 be)
        {
            if (!this._actorPossesions.TryGetValue(actor, out var list))
                this._actorPossesions[actor] = list = [];
            list.Add(be);
        }

        void Remove(EntityRefId actor, IntVec3 be)
        {
            if (!this._actorPossesions.TryGetValue(actor, out var list))
                throw new System.Exception();
            list.Remove(be);
            if (list.Count == 0)
                this._actorPossesions.Remove(actor);
        }

        void Remove(IntVec3 be)
        {
            foreach (var list in this._actorPossesions.Values)
                list.Remove(be);
            foreach (var actorid in this._actorPossesions.Keys)
                if (this._actorPossesions[actorid].Count == 0)
                    this._actorPossesions.Remove(actorid);
        }

        internal override void ResolveReferences()
        {
            this.Map.Events.ListenTo<BlockOwnerChangedEvent>(HandleBlockOwnerChanged);
            this.Map.Events.ListenTo<BlockEntityRemovedEvent>(HandleBlockEntityRemoved);

            foreach(var be in this.Map.BlockEntities)
            {
                if (!be.TryGetComp<BlockOwnershipComp>(out var comp))
                    continue;
                var owner = comp.Owner;
                if (owner == EntityRefId.Null)
                    continue;
            }
        }

        private void HandleBlockEntityRemoved(BlockEntityRemovedEvent e)
        {
            if (!e.Entity.TryGetComp<BlockOwnershipComp>(out var comp))
                return;
            var id = e.Entity.OriginGlobal;
            this.Remove(id);
        }

        private void HandleBlockOwnerChanged(BlockOwnerChangedEvent e)
        {
            var owner = e.Owner;
            var be = e.Entity;
            var previousOwner = e.PreviousOwner;
            if (previousOwner != EntityRefId.Null)
                this.Remove(previousOwner, be.OriginGlobal);
            if (owner.RefId != EntityRefId.Null)
                this.Add(owner.RefId, be.OriginGlobal);
        }
    }
}

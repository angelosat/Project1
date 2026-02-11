using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Project1.Core.Blocks.Comps;
using Project1.Core.Entities;
using Project1.Core.Helpers.Structs;
using Project1.Core.Simulation;

namespace Project1.Core.Blocks.Doors
{
    internal class BlockDoorComp : BlockComp
    {
        internal new class Spec : BlockComp.Spec
        {
            public override Type CompType => typeof(BlockDoorComp);

            public override BlockComp CreateComp() => new BlockDoorComp();
        }
        public override BlockCompDef CompDef => BlockCompDefOf.Door;

        public override string Name => "Door";
        BoundingBox? _cachedAABB;
        HashSet<EntityRefId> CurrentlyOccupying = [];
        public BoundingBox AABB => _cachedAABB ??= BlockDefOf.Door.Worker.GetBoundingBox(this.Parent.Map, this.Parent.OriginGlobal);
        public override void OnSpawned(BlockEntity entity, MapBase map)
        {
            entity.Name = "Door";
        }
        internal void OnActorEntered(Entity entity) 
        {
            this.CurrentlyOccupying.Add(entity.RefId);
        }
        internal void OnActorExited(Entity entity) 
        {
            this.CurrentlyOccupying.Remove(entity.RefId);
        }
        public override void Tick()
        {
            if (this.CurrentlyOccupying.Count == 0)
                return;
            var nextEntities = new HashSet<EntityRefId>();
            foreach(var entityID in this.CurrentlyOccupying)
            {
                var entity = this.Map.World.GetEntity(entityID);
                if (entity.Physics.CurrentAABB.Intersects(this.AABB))
                    nextEntities.Add(entityID);
            }
            this.CurrentlyOccupying = nextEntities;
        }
        public bool CanClose() => this.CurrentlyOccupying.Count == 0;
    }
}

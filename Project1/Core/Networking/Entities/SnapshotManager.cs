using Project1.Core.Simulation;
using Project1.Framework.Serialization;
using System;
using System.Collections.Generic;

namespace Project1.Core.Networking.Entities
{
    internal class SnapshotManager
    {
        private readonly Queue<WorldSnapshot> WorldStateBuffer = new();
        private readonly int WorldStateBufferSize = 20;

        internal void ApplyEntitySnapshots(IEntityProvider world, Tick tick)
        {
            // iterate through the state buffer and find position
            WorldSnapshot[] list = [.. this.WorldStateBuffer];
            for (int i = 0; i < this.WorldStateBuffer.Count - 1; i++)
            {
                WorldSnapshot
                    prev = list[i],
                    next = list[i + 1];

                //if (this.CurrentTick >= prev.Time && this.CurrentTick < next.Time)
                if (tick < next.Tick && prev.Tick <= tick)
                {
                    SnapEntityPositions(world, tick, prev, next);
                    //return;
                }
            }
        }
        internal static void SnapEntityPositions(IEntityProvider world, Tick tick, WorldSnapshot prev, WorldSnapshot next)
        {
            float t = (float)((tick - prev.Tick) /
                  (next.Tick - prev.Tick));
            t = Math.Clamp(t, 0f, 1f);

            foreach (var kv in prev.Dictionary)
            {
                var prevSnap = kv.Value;
                next.Dictionary.TryGetValue(prevSnap.RefID, out var nextSnap);
                var entity = world.GetEntity(prevSnap.RefID);
                if (entity is null) /// snapshot for entity that hasn't been spawned but the client yet? silently drop?
                    continue;
                if (nextSnap == EntitySnapshot.Empty)// is null)
                {
                    // extrapolation
                    // temporarily disabling extrapolation because of a bug
                    //double dt = CurrentTick - prev.Time;
                    //var predictedPos = entity.Global + entity.Velocity * (float)dt;
                    //entity.SetPosition(predictedPos);
                    continue;
                }
                entity.SetPosition(prevSnap.Position + (nextSnap.Position - prevSnap.Position) * t);
                entity.Velocity = prevSnap.Velocity + (nextSnap.Velocity - prevSnap.Velocity) * t;
                entity.Direction = prevSnap.Orientation + (nextSnap.Orientation - prevSnap.Orientation) * t;

                if (float.IsNaN(entity.Direction.X) || float.IsNaN(entity.Direction.Y))
                    throw new Exception();
            }

            foreach (var kv in next.Dictionary)
            {
                if (prev.Dictionary.ContainsKey(kv.Key))
                    continue;

                var nextObj = kv.Value;
                var entity = world.GetEntity(nextObj.RefID);
                if (entity == null) continue;
                if (entity.Map == null) continue; // a snapshot could have been received earlier than the packet to actually spawn an entity that actually is registered in the world but is unspawned

                // Policy for spawns: snap to the authoritative snapshot immediately.
                // Alternative: treat prev as same as next and interpolate from same => same.
                entity.SetPosition(nextObj.Position);
                entity.Velocity = nextObj.Velocity;
                entity.Direction = nextObj.Orientation;
            }
        }
     
        internal void ReadSnapshot(IDataReader reader)
        {
            var time = reader.ReadDouble();
            var worldState = new WorldSnapshot(time, reader);
            // insert world snapshot to world snapshot history
            this.WorldStateBuffer.Enqueue(worldState);
            while (this.WorldStateBuffer.Count > this.WorldStateBufferSize)
                this.WorldStateBuffer.Dequeue();
        }
    }
}

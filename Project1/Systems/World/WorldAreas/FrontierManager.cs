using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using static Start_a_Town_.FrontierManager;

namespace Start_a_Town_
{
    public interface IWorldSpaceManager
    {
        void Enter(Actor actor);
        void Exit(Actor actor);
        FrontierDef PlaceAtRandom(Actor actor);
        FrontierDef PlaceAtRandomAndSync(Actor actor);

        void Tick(StaticWorld staticWorld);
    }
    public class FrontierManager : IWorldSpaceManager
    {
        [EnsureStaticCtorCall]
        static class Packets
        {
            static readonly int _pPlaceAt;
            static Packets()
            {
                _pPlaceAt = Registry.PacketHandlers.Register(ReceivePlaceAt);
            }

            static internal void SendPlaceAt(Actor actor, float pos)
            {
                (actor.Net as Server).BeginPacket(_pPlaceAt)
                    .Write(actor.RefId)
                    .Write(pos);
            }
            private static void ReceivePlaceAt(NetEndpoint endpoint, Packet packet)
            {
                var client = endpoint as Client;
                if (client is null)
                    throw new Exception();
                var r = packet.PacketReader;
                var actor = client.World.GetEntity<Actor>(r.ReadInt32());
                var pos = r.ReadSingle();
                ((actor.World as StaticWorld).Space as FrontierManager).PlaceAt(actor, pos);
            }
        }

        Dictionary<FrontierDef, FrontierWrapper> Frontiers = [];
        Dictionary<int, FrontierWrapper> FrontiersByTier = [];
        Dictionary<Actor, float> Actors = [];

        public FrontierManager()
        {
            foreach (var areadef in Def.GetDefs<FrontierDef>())
                this.Frontiers.Add(areadef, new FrontierWrapper(areadef));

            var byTier = this.Frontiers.Values.ToList();
            byTier.Sort((a, b) => a.Def.Tier.CompareTo(b.Def.Tier));
            this.FrontiersByTier = byTier.ToDictionary(f => f.Def.Tier, f => f);
        }

        public void Tick(StaticWorld world)
        {
            //if (world.Net is not Server server)
            //    return;
            float step = 1f / Ticks.PerGameHour;
            var snapshot = Actors.ToList();
            foreach (var (actor, distance) in snapshot)
            {
                var target = actor.AI.Meta.TargetFrontier?.Tier ?? 0;
                // if current distance from town is not the target distance, step towards it
                var nextDistance = distance;
                if (distance != target)
                {
                    // actors should settle in the middle of the zone (or maybe jitter around the middle to influence the chances of encounters)
                    //nextDistance = Math.Max(Math.Max(0, target + .5f), Math.Min(distance + ((target < distance) ? -step : step), target - .5f));
                    //nextDistance = Math.Max(0, Math.Min(distance + ((target - .5f < distance) ? -step : step), target - .5f));
                    nextDistance = distance + ((target - .5f < distance) ? -step : step);
                    nextDistance = Math.Clamp(nextDistance, 0, this.Frontiers.Count);
                }
                // if current distance == 0 it means the actor has arrived in town and should spawn 
                if (nextDistance == 0)
                {
                    if (world.Net is Server server)
                    {
                        world.Map.SpawnAndSync(actor, world.Map.GetRandomEdgeCell().Above, Vector3.Zero);
                        server.SyncReport($"{actor.Name} has arrived!");
                        AILog.SyncWrite(actor, "I arrived in town!");
                    }
                    this.Exit(actor);
                    continue;
                }
                
                this.Actors[actor] = nextDistance;
                var currentFrontier = this.GetFrontier(actor);
                currentFrontier.Tick(actor);
                actor.Needs.Tick();
                actor.AI.Meta.Tick();
            }
        }

        FrontierWrapper GetFrontier(Actor actor)
        {
            var distance = (int)Math.Ceiling(this.Actors[actor]);
            for (int i = 0; i < this.FrontiersByTier.Count; i++)
            {
                if (i < distance && distance <= i + 1)
                    return this.FrontiersByTier[i+1];
            }
            throw new Exception("Actor distance out of bounds");
        }
        public void Exit(Actor actor)
        {
            this.Actors.Remove(actor);
            actor.Effects.Remove(EffectDefOf.Adventuring);
            //if (!this.Actors.Remove(actor))
            //    throw new InvalidOperationException($"Tried to remove {actor} but wasn't found");
        }
        public void Enter(Actor actor)
        {
            //var need = actor.GetNeed(AdventurerNeedsDefOf.Adventuring);
            actor.Effects.Apply(EffectDefOf.Adventuring);
            this.Actors.Add(actor, 0);
            if (actor.Net is not Server server)
                return;
            actor.Map.DespawnAndSync(actor);
            server.SyncReport($"{actor.Name} has departed for {actor.AI.Meta.TargetFrontier.Label}!");
        }
        public FrontierDef PlaceAtRandomAndSync(Actor actor)
        {
            var fr = this.PlaceAtRandom(actor);
            Packets.SendPlaceAt(actor, this.Actors[actor]);
            return fr;
        }
        public FrontierDef PlaceAtRandom(Actor actor)
        {
            var tier = 1 + actor.World.Random.Next(this.Frontiers.Count);
            this.PlaceAt(actor, tier);
            return this.GetFrontier(actor).Def;
        }
        public void PlaceAt(Actor actor, float tier)
        {
            this.Actors.Add(actor, tier);
            actor.AI.Meta.TargetFrontier = this.GetFrontier(actor).Def;
            actor.Effects.Apply(EffectDefOf.Adventuring);

            //actor.Map?.DespawnAndSync(actor);
            actor.Map?.Despawn(actor);
        }
        public class FrontierWrapper
        {
            public readonly FrontierDef Def;
            List<Entity> LootPool = [];
            public FrontierWrapper(FrontierDef def)
            {
                this.Def = def;
            }
            internal void Tick(Actor actor)
            {
                // roll encounter
                // roll random loot
                // etc
            }
        }
    }
}

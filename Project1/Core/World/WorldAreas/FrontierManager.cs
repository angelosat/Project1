using Microsoft.Xna.Framework;
using Project1.Core.Effects;
using Project1.Core.Entities;
using Project1.Core.Base;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Net;
using Project1.Core.Net;
using Project1.Core.Population;
using Project1.Core.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using static Project1.Core.World.WorldAreas.FrontierManager;
using Project1.Framework.IO;

namespace Project1.Core.World.WorldAreas
{
    public record struct WorldSpacePosition(float Value)
    {
        public static implicit operator float(WorldSpacePosition pos) => (float)pos.Value;
        public static implicit operator WorldSpacePosition(float pos) => new(pos);

        public static WorldSpacePosition ReadFrom(IDataReader r) => new(r.ReadSingle());
    }
    internal record struct FrontierTier(int Value)
    {
        public static implicit operator int(FrontierTier pos) => (int)pos.Value;
        public static implicit operator FrontierTier(int pos) => new(pos);
    }
    public interface IWorldSpaceManager
    {
        void Enter(Actor actor);
        void Exit(Actor actor);
        FrontierDef PlaceAtRandom(Entity entity);
        FrontierDef PlaceAt(Entity entity, WorldSpacePosition pos);
        FrontierWrapper GetFrontier(Entity entity);
        void Tick();
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

        readonly Dictionary<FrontierDef, FrontierWrapper> Frontiers = [];
        readonly Dictionary<FrontierTier, FrontierWrapper> FrontiersByTier = [];
        readonly Dictionary<Actor, WorldSpacePosition> ActorPositions = [];

        public StaticWorld World { get; }

        public FrontierManager(StaticWorld world)
        {
            this.World = world;
            foreach (var areadef in Def.GetDefs<FrontierDef>())
                this.Frontiers.Add(areadef, new FrontierWrapper(areadef));

            var byTier = this.Frontiers.Values.ToList();
            byTier.Sort((a, b) => a.Def.Tier.CompareTo(b.Def.Tier));
            this.FrontiersByTier = byTier.ToDictionary(f => new FrontierTier(f.Def.Tier), f => f);
        }

        public void Tick()
        {
            var world = this.World;
            float step = 1f / Ticks.PerGameHour;
            var snapshot = ActorPositions.ToList();
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
                        world.Map.Spawn(actor, world.Map.GetRandomEdgeCell().Above, Vector3.Zero);
                        server.SyncReport($"{actor.Name} has arrived!");
                        actor.AI.State.Log.Write("I arrived in town.");
                    }
                    this.Exit(actor);
                    continue;
                }
                
                this.ActorPositions[actor] = nextDistance;
                var currentFrontier = this.GetFrontier(actor);
                currentFrontier.Tick(actor);
                actor.Needs.Tick();
                actor.AI.Meta.Tick();
            }
        }

        public FrontierWrapper GetFrontier(Entity entity)
        {
            if (entity is not Actor actor)
                throw new NotImplementedException();
            if (!this.ActorPositions.ContainsKey(actor))
                return null;
            var distance = (int)Math.Ceiling(this.ActorPositions[actor]);
            if (distance == 0)
                return null;
            for (int i = 0; i < this.FrontiersByTier.Count; i++)
            {
                if (i < distance && distance <= i + 1)
                    return this.FrontiersByTier[i+1];
            }
            throw new Exception("Actor distance out of bounds");
        }
        public void Exit(Actor actor)
        {
            if (!this.ActorPositions.ContainsKey(actor))
                return;
            this.ActorPositions.Remove(actor);
            actor.Effects.Remove(EffectDefOf.Adventuring);
            //if (!this.Actors.Remove(actor))
            //    throw new InvalidOperationException($"Tried to remove {actor} but wasn't found");
        }
        public void Enter(Actor actor)
        {
            actor.Effects.Apply(EffectDefOf.Adventuring);
            this.ActorPositions.Add(actor, 0);
            actor.Map.Despawn(actor);

            if (actor.Net is not Server server)
                return;
            server.SyncReport($"{actor.Name} has departed for {actor.AI.Meta.TargetFrontier.LabelReadable}!");
        }
     
        public FrontierDef PlaceAtRandom(Entity entity)
        {
            var tier = 1 + entity.World.Random.Next(this.Frontiers.Count);
            return this.PlaceAt(entity, tier);
        }
        public FrontierDef PlaceAt(Entity entity, WorldSpacePosition pos)
        {
            // TODO sort entities to actors and non-actors. for example, if the entity is an item, place it in the target zone's treasure pool
            if (entity is not Actor actor)
                return null;
            this.ActorPositions.Add(actor, pos);
            actor.AI.Meta.SetTargetFrontier(this.GetFrontier(actor).Def);
            actor.Effects.Apply(EffectDefOf.Adventuring);

            actor.Map?.Despawn(actor);
            this.World.Events.Post(new InhabitantPlacedInWorldEvent(actor, pos));
            return this.GetFrontier(actor).Def;
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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    public interface IWorldSpaceManager
    {
        void Depart(Actor actor);
        void PlaceRandom(Actor actor);
        void Tick(StaticWorld staticWorld);
    }
    public class FrontierManager : IWorldSpaceManager
    {
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
            if (world.Net is not Server server)
                return;
            float step = 1f / Ticks.PerGameMinute;
            var snapshot = Actors.ToList();
            foreach (var (actor, distance) in snapshot)
            {
                var target = actor.AI.Meta.TargetFrontier?.Tier ?? 0;
                // if current distance from town is not the target distance, step towards it
                var nextDistance = distance;
                if (distance != target)
                    // actors should settle in the middle of the zone (or maybe jitter around the middle to influence the chances of encounters)
                    nextDistance = Math.Max(Math.Max(0, target + .5f), Math.Min(distance + ((target < distance) ? -step : step), target - .5f));

                // if current distance == 0 it means the actor has arrived in town and should spawn 
                if (nextDistance == 0)
                {
                    world.Map.SpawnAndSync(actor, world.Map.GetRandomEdgeCell().Above, Vector3.Zero);
                    this.Actors.Remove(actor);
                    server.SyncReport($"{actor.Name} has arrived!");
                    continue;
                }
                
                this.Actors[actor] = nextDistance;
                var currentFrontier = this.GetFrontier(actor);
                currentFrontier.Tick(actor);
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
            throw new Exception("actor distance out of bounds");
        }

        public void Depart(Actor actor)
        {
            if (actor.Net is not Server server)
                return;
            this.Actors.Add(actor, 0);
            actor.Map.DespawnAndSync(actor);
            server.SyncReport($"{actor.Name} has departed for {actor.AI.Meta.TargetFrontier.Label}!");
        }
        public void PlaceRandom(Actor actor)
        {
            var tier = Random.Shared.Next(this.Frontiers.Count - 1);
            this.Actors.Add(actor, tier);
            actor.Map?.DespawnAndSync(actor);
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

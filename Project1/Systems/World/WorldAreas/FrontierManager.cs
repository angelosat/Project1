using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

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
            if (world.Net.IsClient)
                return;
            float step = 1 / Ticks.PerGameMinute;
            var snapshot = Actors.ToList();
            foreach (var (actor, distance) in snapshot)
            {
                var target = actor.AI.Meta.TargetFrontier?.Tier ?? 0;
                if (distance == 0)
                {
                    world.Map.SpawnAndSync(actor, world.Map.GetRandomEdgeCell().Above, Vector3.Zero);
                    this.Actors.Remove(actor);
                    continue;
                }
                if (distance != target)
                    this.Actors[actor] = Math.Max(0, Math.Min(distance + ((target < distance) ? -step : step), this.FrontiersByTier.Last().Key));
                var currentFrontier = this.GetFrontier(actor);
                currentFrontier.Tick(actor);
            }
        }

        FrontierWrapper GetFrontier(Actor actor)
        {
            var distance = this.Actors[actor];
            for (int i = 0; i < this.FrontiersByTier.Count; i++)
            {
                if (i <= distance && distance < i + 1)
                    return this.FrontiersByTier[i];
            }
            throw new Exception("actor distance out of bounds");
        }

        public void Depart(Actor actor)
        {
            // server-authoritative or let clients despawn?
            if (actor.Net.IsClient)
                return;
            this.Actors.Add(actor, 0);
            actor.Map.DespawnAndSync(actor);
            //actor.Map.Despawn(actor);
            actor.Net.Report($"{actor.Name} has departed!");
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

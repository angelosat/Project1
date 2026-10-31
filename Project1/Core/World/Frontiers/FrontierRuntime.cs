using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using Project1.Core.Skills;
using Project1.Core.World.PointsOfInterest;
using Project1.Core.World.Rooms;
using Project1.Framework.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.World.Frontiers;

public class FrontierRuntime : WorldRoom
{
    public readonly FrontierDef Def;
    public readonly StaticWorld World;
    readonly internal List<Entity> Treasures = [];
    internal readonly Dictionary<Actor, SimulationTick> ActorsIncapacitated = [];
    //internal HashSet<Actor> ActorsActive = [];
    //internal HashSet<EntityRefId> ActorsActive = [];

    internal List<PoIRuntime> PoIs = [];
    int PoITargetCount = 1;

    internal void Tick(WorldBase world) { }

    public FrontierRuntime(StaticWorld world, FrontierDef def)
    {
        World = world;
        this.Def = def;
    }

    internal void AddActor(Actor actor)
    {
        this.ActorsActive.Add(actor.RefId);
    }

    internal void RemoveActor(Actor actor)
    {
        this.ActorsActive.Remove(actor.RefId);
    }

    internal PoIRuntime FindPoI(Actor actor)
        => this.PoIs.FirstOrDefault(p => p.Contains(actor));

    internal void AddIncapacitatedActor(Actor actor)
    {
        this.ActorsIncapacitated.Add(actor, actor.World.CurrentTick);
    }

    internal void RemoveIncapacitatedActor(Actor actor)
    {
        this.ActorsIncapacitated.Remove(actor);
    }

    internal void AddTreasure(Entity entity)
    {
        entity.Detach();
        this.Treasures.Add(entity);
    }

    internal bool TryFindTreasure(Random rand, Actor actor, out Entity treasure)
    {
        if(this.Treasures.Count == 0)
        {
            treasure = null;
            return false;
        }
        var exploration = actor.Skills.GetLevel(SkillDefOf.Exploration);
        var chance = 10 + exploration;
        var success = rand.Roll100(chance);
        if (!success)
        {
            treasure = null;
            return false;
        }
        treasure = this.Treasures.SelectRandom(rand);
        this.Treasures.Remove(treasure);
        return true;
    }

    internal void Tick(Actor actor)
    {
        // roll encounter
        // roll random loot
        // etc
    }

    internal void EnsurePoICount()
    {
        while(this.PoIs.Count < PoITargetCount)
        {
            var id = this.PoIs.Count;
            var def = PoIDefOf.Dungeon;
            var size = this.World.Random.Next(10, 20);
            this.RegisterPoI(id, def, size);
        }
    }

    internal void RegisterPoI(int id, PoIDef def, int size)
    {
        var poi = new PoIRuntime(this, id, def, size);
        this.PoIs.Add(poi);
        if (this.World.Net.IsServer)
            poi.Init(this.World);
        this.World.Events.Post(new PoIAddedEvent(this.Def, poi));
    }
}

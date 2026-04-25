using Microsoft.Xna.Framework;
using Project1.Core.AI.MetaRoles.Adventurer;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Core.Systems.Biology;
using Project1.Core.Systems.Tools;
using Project1.Framework.Events;
using Project1.Framework.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.World.WorldAreas;

public class FrontierManager : IWorldSpaceManager
{
    readonly Dictionary<FrontierDef, FrontierWrapper> Frontiers = [];
    static readonly Dictionary<Tier, FrontierDef> FrontiersByTier = [];
    readonly Dictionary<Actor, WorldSpacePosition> ActorPositions = [];
    readonly Dictionary<Actor, Scheduler> EventSchedulers = [];
    static readonly HashSet<FrontierDef> _cachedDefs;// = [];
    public StaticWorld World { get; }
    static readonly List<OffmapActivity> _offmapActivities = [];
    static internal List<FrontierDecider> Deciders = [];
    readonly Dictionary<Actor, SimulationTick> IncapacitatedActors = [];
    readonly static ulong IncapacitationDuration = (ulong)Ticks.FromMinutes(10);
    readonly List<Actor> toKill = [];
    static FrontierManager()
    {
        foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()))
        {
            if (typeof(OffmapActivity).IsAssignableFrom(type) && !type.IsAbstract)
                _offmapActivities.Add((OffmapActivity)Activator.CreateInstance(type));
        }
        foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()))
        {
            if (typeof(FrontierDecider).IsAssignableFrom(type) && !type.IsAbstract)
                Deciders.Add((FrontierDecider)Activator.CreateInstance(type));
        }

        _cachedDefs = [.. Def.Get<FrontierDef>()];
        FrontiersByTier = _cachedDefs.ToDictionary(d => new Tier(d.Tier), d => d);
    }
    public FrontierManager(StaticWorld world)
    {
        this.World = world;
        foreach (var areadef in _cachedDefs)
            this.Frontiers.Add(areadef, new FrontierWrapper(areadef));
        //this.GenerateTreasure(new Random());
        //world.Events.ListenTo<EntityKilledEvent>(HandleEntityKilled);
        world.Events.ListenTo<ActorIncapacitatedEvent>(HandleActorIncapacitated);
    }


    private void HandleActorIncapacitated(ActorIncapacitatedEvent e)
    {
        this.IncapacitatedActors.Add(e.Actor, this.World.CurrentTick);
    }

    void TickIncapacitated()
    {
        var now = this.World.CurrentTick;
        foreach(var (actor, tick) in this.IncapacitatedActors)
        {
            if(now - tick >= IncapacitationDuration)
            {
                this.toKill.Add(actor);
            }
        }
        foreach(var actor in toKill)
        {
            Kill(actor);
        }
        this.toKill.Clear();
    }

    private void Kill(Actor actor)
    {
        var frontier = this.GetFrontier(actor);
        var loot = actor.GetSelfAndChildren(includeSelf: false).ToArray();
        foreach (var item in loot)
            frontier.AddTreasure(item);
        this.IncapacitatedActors.Remove(actor);
        this.ActorPositions.Remove(actor);
    }

    private void HandleEntityKilled(EntityKilledEvent e)
    {
        if (e.Entity is not Actor actor)
            return;
        if (actor.IsSpawned)
            return;
        var frontier = this.GetFrontier(actor);
        var loot = actor.GetSelfAndChildren(includeSelf: false);
        foreach(var item in loot)
            frontier.AddTreasure(item);
    }

    void GenerateTreasure()
    {
        if (this.World.Net.IsClient)
            return;
        var targetloot = 10;
        foreach(var f in this.Frontiers.Values)
        {
            while(f.Treasures.Count < targetloot)
            //for (int i = 0; i < 10; i++)
            {
                var item = ToolSystem.CreateRandom(this.World.Random, f.Def.Tier);
                this.World.Register(item);
                f.AddTreasure(item);
            }
        }
    }

    internal ChangeNotifier Notifier = new();
    public void Tick()
    {
        var world = this.World;
        //float step = 1f / Ticks.PerGameHour;
        //float step = 1f / Ticks.PerGameMinute
        float step = 1f / Ticks.FromMinutes(5);
        
        var snapshot = ActorPositions.ToList();
        foreach (var (actor, distance) in snapshot)
        {
            var target = actor.AI.Meta.TargetFrontier?.Tier ?? 0;
            // if current distance from town is not the target distance, step towards it
            var nextDistance = distance;
            if (distance != target)
            {
                // actors should settle in the middle of the zone (or maybe jitter around the middle to influence the chances of encounters)
                nextDistance = distance + ((target - .5f < distance) ? -step : step);
                nextDistance = Math.Clamp(nextDistance, 0, this.Frontiers.Count);
                this.Notifier.Notify();
            }
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
            if(actor.Net.IsServer)
                this.EventSchedulers[actor].Tick(this.World.CurrentTick);
            actor.TickOffMap();
        }
        this.TickIncapacitated();
        this.GenerateTreasure();
    }
    static internal FrontierDef GetFrontier(Tier tier)
        => FrontiersByTier[tier];
    static internal IEnumerable<FrontierDef> AllFrontiers => FrontiersByTier.Values;
    public FrontierWrapper GetFrontier(Entity entity)
    {
        if (entity is not Actor actor)
            throw new NotImplementedException();
        if (!this.ActorPositions.TryGetValue(actor, out WorldSpacePosition value))
            return null;
        var distance = (int)Math.Ceiling(value);
        if (distance == 0)
            return null;
        //for (int i = 0; i < this.FrontiersByTier.Count; i++)
        //{
        //    if (i < distance && distance <= i + 1)
        //        return this.FrontiersByTier[i+1];
        //}
        for (byte i = 0; i < FrontiersByTier.Count; i++)
        {
            if (i < distance && distance <= i + 1)
                return this.Frontiers[FrontiersByTier[(Tier)(i + 1)]];
        }
        throw new Exception("Actor distance out of bounds");
    }
    public FrontierDef FrontierAt(WorldSpacePosition pos)
    {
        var distance = (int)Math.Ceiling(pos);
        if (distance == 0)
            return null;
        //for (int i = 0; i < this.FrontiersByTier.Count; i++)
        //{
        //    if (i < distance && distance <= i + 1)
        //        return this.FrontiersByTier[i+1];
        //}
        for (byte i = 0; i < FrontiersByTier.Count; i++)
        {
            if (i < distance && distance <= i + 1)
                return this.Frontiers[FrontiersByTier[(Tier)(i + 1)]].Def;
        }
        throw new ArgumentOutOfRangeException(nameof(pos), "World-space position out of bounds");
    }
    public void Exit(Actor actor)
    {
        if (!this.ActorPositions.ContainsKey(actor))
            return;
        this.ActorPositions.Remove(actor);
        this.EventSchedulers.Remove(actor);
    }
    //public void Enter(Actor actor)
    //{
    //    this.ActorPositions.Add(actor, 0);
    //    this.EventSchedulers.Add(
    //        actor, 
    //        new(() => this.TriggerOffmapEvent(actor), this.World.CurrentTick, Ticks.FromMinutes(1), Ticks.FromMinutes(3), new()));
    //    actor.Map.Despawn(actor);

    //    if (actor.Net is not Server server)
    //        return;
    //    server.SyncReport($"{actor.Name} has departed for {actor.AI.Meta.TargetFrontier.LabelReadable}!");
    //}
    void TriggerOffmapEvent(Actor actor)
    {
        if (actor.Biology.IsIncapacitated)
            return;
        var activity = _offmapActivities.SelectRandom(this.World.Random);
        var front = this.GetFrontier(actor);
        activity.Tick(front, actor);
    }
    public FrontierDef /*void*/ PlaceAtRandom(Entity entity)
    {
        var tier = 1 + entity.World.Random.Next(this.Frontiers.Count);
        /*return*/ this.PlaceAt(entity, tier);
        return this.GetFrontier(entity).Def;
    }
    public void PlaceAt(Entity entity, WorldSpacePosition pos)
    {
        // TODO sort entities to actors and non-actors. for example, if the entity is an item, place it in the target zone's treasure pool
        if (entity is not Actor actor)
            return;
        this.ActorPositions.Add(actor, pos);
        this.EventSchedulers.Add(
            actor,
            new(() => this.TriggerOffmapEvent(actor), this.World.CurrentTick, Ticks.FromMinutes(1), Ticks.FromMinutes(3), new()));
        //actor.AI.Meta.SetTargetFrontier(this.GetFrontier(actor).Def);

        actor.Map?.Despawn(actor);
        this.World.Events.Post(new InhabitantPlacedInWorldEvent(actor, pos));
        //return this.GetFrontier(actor).Def;
    }
    
}
using Microsoft.Xna.Framework;
using Project1.Core.AI;
using Project1.Core.AI.MetaRoles.Adventurer;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.World.WorldAreas;


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
            var client = endpoint as Client ?? throw new Exception();
            var r = packet.PacketReader;
            var actor = client.World.Get<Actor>(r.ReadInt32());
            var pos = r.ReadSingle();
            ((actor.World as StaticWorld).Space as FrontierManager).PlaceAt(actor, pos);
        }
    }

    readonly Dictionary<FrontierDef, FrontierWrapper> Frontiers = [];
    //readonly Dictionary<Tier, FrontierWrapper> FrontiersByTier = [];
    static readonly Dictionary<Tier, FrontierDef> FrontiersByTier = [];
    readonly Dictionary<Actor, WorldSpacePosition> ActorPositions = [];
    readonly Dictionary<Actor, Scheduler> EventSchedulers = [];
    static readonly HashSet<FrontierDef> _cachedDefs;// = [];
    public StaticWorld World { get; }
    static readonly List<OffmapActivity> _offmapActivities = [];
    static internal List<FrontierDecider> Deciders = [];
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

        //var byTier = this.Frontiers.Values.ToList();
        //byTier.Sort((a, b) => a.Def.Tier.CompareTo(b.Def.Tier));
        //this.FrontiersByTier = byTier.ToDictionary(f => new Tier(f.Def.Tier), f => f);
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
            actor.TickOffMap();
            if(actor.Net.IsServer)
                this.EventSchedulers[actor].Tick(this.World.CurrentTick);
        }
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
        for (int i = 0; i < FrontiersByTier.Count; i++)
        {
            if (i < distance && distance <= i + 1)
                return this.Frontiers[FrontiersByTier[i + 1]];
        }
        throw new Exception("Actor distance out of bounds");
    }
    public void Exit(Actor actor)
    {
        if (!this.ActorPositions.ContainsKey(actor))
            return;
        this.ActorPositions.Remove(actor);
        this.EventSchedulers.Remove(actor);
    }
    public void Enter(Actor actor)
    {
        this.ActorPositions.Add(actor, 0);
        this.EventSchedulers.Add(
            actor, 
            new(() => this.TriggerOffmapEvent(actor), this.World.CurrentTick, Ticks.FromMinutes(1), Ticks.FromMinutes(3), new()));
        actor.Map.Despawn(actor);

        if (actor.Net is not Server server)
            return;
        server.SyncReport($"{actor.Name} has departed for {actor.AI.Meta.TargetFrontier.LabelReadable}!");
    }
    void TriggerOffmapEvent(Actor actor)
    {
        var activity = _offmapActivities.SelectRandom(this.World.Random);
        var front = this.GetFrontier(actor);
        activity.Tick(front, actor);
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
        this.EventSchedulers.Add(
            actor,
            new(() => this.TriggerOffmapEvent(actor), this.World.CurrentTick, Ticks.FromMinutes(1), Ticks.FromMinutes(3), new()));
        actor.AI.Meta.SetTargetFrontier(this.GetFrontier(actor).Def);

        actor.Map?.Despawn(actor);
        this.World.Events.Post(new InhabitantPlacedInWorldEvent(actor, pos));
        return this.GetFrontier(actor).Def;
    }
    
}
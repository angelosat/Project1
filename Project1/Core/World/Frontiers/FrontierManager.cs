using Project1.Core.AI.MetaRoles.Adventurer;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Core.Systems.Biology;
using Project1.Core.Systems.Tools;
using Project1.Core.World.Actions;
using Project1.Core.World.PointsOfInterest;
using Project1.Core.World.Rooms;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.Helpers;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace Project1.Core.World.Frontiers;
public class FrontierManager : WorldComp, IWorldSpaceManager
{
    readonly Dictionary<FrontierDef, FrontierRuntime> Frontiers = [];
    static readonly Dictionary<Tier, FrontierDef> FrontiersByTier = [];
    readonly Dictionary<Actor, WorldSpacePosition> ActorPositions = [];
    readonly Dictionary<Actor, EncounterRuntime> ActiveEncounters = [];
    readonly Dictionary<Actor, Scheduler> EventSchedulers = [];
    readonly Dictionary<Actor, PoIRuntime> ActorsInPoIs = [];
    static readonly HashSet<FrontierDef> _cachedDefs;// = [];
    //public StaticWorld World { get; }
    static readonly List<EncounterWorker> _offmapActivities = [];
    static readonly EncounterDef[] EncounterDefs;
    static readonly List<PoIRuntime> PoIs = new();
    static internal List<FrontierDecider> Deciders = [];
    //readonly Dictionary<Actor, SimulationTick> IncapacitatedActors = [];
    //public IEnumerable<Actor> GetIncapacitatedActors(FrontierDef frontier) => this.IncapacitatedActors.Keys.Where(a => GetFrontier(a).Def == frontier);
    readonly static ulong IncapacitationDuration = (ulong)Ticks.FromHours(1); //FromMinutes(10);
    readonly List<Actor> toKill = [];
    static FrontierManager()
    {
        foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()))
        {
            if (typeof(EncounterWorker).IsAssignableFrom(type) && !type.IsAbstract)
                _offmapActivities.Add((EncounterWorker)Activator.CreateInstance(type));
        }
        foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()))
        {
            if (typeof(FrontierDecider).IsAssignableFrom(type) && !type.IsAbstract)
                Deciders.Add((FrontierDecider)Activator.CreateInstance(type));
        }

        EncounterDefs = [.. Def.Get<EncounterDef>()];

        _cachedDefs = [.. Def.Get<FrontierDef>()];
        FrontiersByTier = _cachedDefs.ToDictionary(d => new Tier(d.Tier), d => d);
    }
    public FrontierManager(WorldBase world) : base(world)
    {
        //this.World = world;
        foreach (var areadef in _cachedDefs)
            this.Frontiers.Add(areadef, new FrontierRuntime(this.World, areadef));
        //this.GenerateTreasure(new Random());
        //world.Events.ListenTo<EntityKilledEvent>(HandleEntityKilled);
        world.Events.ListenTo<ActorIncapacitatedEvent>(HandleActorIncapacitated);
        world.Events.ListenTo<ActorRecoveredEvent>(HandleActorRecovered);
    }

    private void HandleActorIncapacitated(ActorIncapacitatedEvent e)
    {
        //this.IncapacitatedActors.Add(e.Actor, this.World.CurrentTick);
        this.GetFrontier(e.Actor)?.AddIncapacitatedActor(e.Actor);//
    }

    private void HandleActorRecovered(ActorRecoveredEvent e)
    {
        this.GetFrontier(e.Actor)?.RemoveIncapacitatedActor(e.Actor);
    }

    void TickIncapacitatedActors()
    {
        var now = this.World.CurrentTick;
        foreach (var fw in this.Frontiers.Values)
        {
            foreach (var (actor, tick) in fw.ActorsIncapacitated)
            {
                if (now - tick >= IncapacitationDuration)
                {
                    this.toKill.Add(actor);
                }
            }
        }
        foreach (var actor in toKill)
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
        frontier.ActorsIncapacitated.Remove(actor);
        //frontier.ActorsActive.Remove(actor.RefId);
        frontier.RemoveActor(actor);
        this.ActorPositions.Remove(actor);
        this.World.Events.Post(new ActorDeathEvent(actor));
        this.World.DisposeEntity(actor);
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

    void EnsureFrontierContents()
    {
        //if (this.World.Net.IsClient)
        //    return;
        var targetloot = 20;
        foreach(var f in this.Frontiers.Values)
        {
            f.EnsurePoICount();
            while(f.Treasures.Count < targetloot)
            //for (int i = 0; i < 10; i++)
            {
                //var item = ToolSystem.CreateRandom(this.World.Random, f.Def.Tier);
                var item = ToolSystem.CreateRandomNew(this.World.Random);
                this.World.Register(item);
                f.AddTreasure(item);
            }
        }
    }

    public void Explore(FrontierRuntime room, Actor actor)
    {
        // TODO account for actor's exploration skill/stat?
        room.Exploration.ApplyDelta(.1f);
        room.World.Events.Post(new WorldExplorationEvent(room, actor));
        room.Notifier.Notify();
    }

    internal ChangeNotifier Notifier = new();

    public override void Tick()
    {
        var world = this.World;
        //float step = 1f / Ticks.PerGameHour;
        //float step = 1f / Ticks.PerGameMinute
        float step = 1f / Ticks.FromMinutes(5);
        var snapshot = ActorPositions.ToList();
        var server = this.World.Net as Server;
        var isServer = server?.IsServer ?? false;
        foreach (var (actor, distance) in snapshot)
        {
            //if (!this.IsEngaged(actor))
            if (!this.World.Actions.HasActiveAction(actor) && !this.World.Combat.IsEngaged(actor))
                {
                var prevFrontier = this.GetFrontier(actor);
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
                    if (isServer)
                    {
                        world.Map.Spawn(actor, world.Map.GetRandomEdgeCell().Above, IntVec3.Zero);
                        server.SyncReport($"{actor.Name} has arrived!");
                        actor.AI.State.Log.Write("I arrived in town.");
                    }
                    this.Exit(actor);
                    continue;
                }

                this.ActorPositions[actor] = nextDistance;
                var currentFrontier = this.GetFrontier(actor);
                currentFrontier.Tick(actor);
                if (currentFrontier != prevFrontier)
                {
                    currentFrontier.AddActor(actor);
                    prevFrontier.RemoveActor(actor);
                }
                if (isServer)
                    this.EventSchedulers[actor].Tick(this.World.CurrentTick);
            }
            actor.TickOffMap();
        }
        this.TickIncapacitatedActors();
        this.TickActiveEncounters(); // moved the client guard inside so i can increment its progress on clients

        if (isServer)
        {
            //this.TickActiveEncounters();
            this.CleanupActiveEncounters();
            this.EnsureFrontierContents();
        }
    }

    void TickActiveEncounters()
    {
        foreach(var (actor, encounter) in this.ActiveEncounters)
        {
            encounter.Tick(this.GetFrontier(actor), actor);
            //if(result.FollowUpEncounter is EncounterRuntime nextEncounter)
            //{

            //}
        }
    }

    void CleanupActiveEncounters()
    {
        foreach (var (actor, encounter) in this.ActiveEncounters.ToArray())
        {
            var state = encounter.State;
            if (state == EncounterStates.Finished)
            {
                $"{actor}'s {encounter.Def.LabelReadable} finished after {encounter.Current} ticks".ToConsole();
                this.EndEncounter(actor);
                //this.ActiveEncounters.Remove(actor);
                //this.World.Events.Post(new EncounterEndEvent(actor, encounter));
            }
        }
    }

    bool IsEngaged(Actor actor)
        => this.ActiveEncounters.ContainsKey(actor);

    internal EncounterRuntime GetEncounter(Actor actor)
        => this.ActiveEncounters.GetValueOrDefault(actor);

    static internal FrontierDef GetFrontier(Tier tier)
        => FrontiersByTier[tier];
    static internal IEnumerable<FrontierDef> AllFrontiers => FrontiersByTier.Values;
    public FrontierRuntime? GetFrontier(Entity entity)
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
                return this.Frontiers[FrontiersByTier[(Tier)(byte)(i + 1)]];
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
                return this.Frontiers[FrontiersByTier[(Tier)(byte)(i + 1)]].Def;
        }
        throw new ArgumentOutOfRangeException(nameof(pos), "World-space position out of bounds");
    }
    public void Exit(Actor actor)
    {
        if (!this.ActorPositions.ContainsKey(actor))
            return;
        this.ActorPositions.Remove(actor);
        this.GetFrontier(actor).RemoveActor(actor);
        this.EventSchedulers.Remove(actor);
    }

    void TriggerEncounter(Actor actor)
    {
        if (actor.Biology.IsIncapacitated)
            return;
        var frontier = this.GetFrontier(actor);
        //var encounterDef = EncounterDefs.SelectRandomWeighted(this.World.Random, f => f.Worker.GetWeight(frontier, actor));
        //var action = encounterDef.Worker.DecideAction(frontier, actor);
        //if (!action.HasValue)
        //    return;
        //this.World.Actions.Start(action.Value.actionDef, action.Value.data);

        var candidates = this.GetAvailableEncounters(actor).SelectMany(e => e.Worker.GetCandidates(frontier, actor).Where(c => c.encounter is not null)).ToArray();
        var (encounter, context, weight) = candidates.SelectRandomWeighted(this.World.Random, f => f.weight);
        //var action = encounter.Worker.DecideAction(frontier, actor);
        if (encounter.Worker.DecideAction(context) is not WorldActionDef action)
            return;
        this.World.Actions.Begin(action, context);
    }

    private IEnumerable<EncounterDef> GetAvailableEncounters(Actor actor)
    {
        if (this.GetPoi(actor) is PoIRuntime poi)
            return poi.GetEncounters();
        return EncounterDefs;
    }

    public FrontierDef PlaceAtRandom(Entity entity)
    {
        // TODO uncommment the line below
        var tier = 1;// + entity.World.Random.Next(this.Frontiers.Count);
        /*return*/ this.PlaceAt(entity, tier);
        return this.GetFrontier(entity).Def;
    }
    public void PlaceAt(Entity entity, WorldSpacePosition pos)
    {
        // TODO sort entities to actors and non-actors. for example, if the entity is an item, place it in the target zone's treasure pool
        if (entity is not Actor actor)
            return;
        this.ActorPositions.Add(actor, pos);
        this.GetFrontier(actor)
            .AddActor(actor);
        this.BeginSchedule(actor);

        actor.Map?.Despawn(actor);
        this.World.Events.Post(new InhabitantPlacedInWorldEvent(actor, pos));
        //return this.GetFrontier(actor).Def;
    }

    private void BeginSchedule(Actor actor)
    {
        this.EventSchedulers.Add(
                    actor,
                    new(() => this.TriggerEncounter(actor), 
                    this.World.CurrentTick, 
                    //Ticks.FromMinutes(1), 
                    //Ticks.FromMinutes(3),
                    Ticks.FromMinutes(10),
                    Ticks.FromMinutes(30),
                    //new()));
                    actor.World.Random));
    }

    internal void BeginEncounter(Actor actor, EncounterDef encounterDef)
    {
        var frontier = this.GetFrontier(actor);
        var runtime = new EncounterRuntime(encounterDef, actor, frontier.Def);
        this.ActiveEncounters.Add(actor, runtime);
        this.World.Events.Post(new EncounterBeginEvent(actor, runtime));
        if (this.World.Net.IsServer)
        {
            runtime.Begin(frontier);
            $"{actor} begins {runtime.Def.LabelReadable} on tick {this.World.CurrentTick}".ToConsole();
        }
        this.Notifier.Notify();
    }

    internal void EndEncounter(Actor actor)
    {
        this.ActiveEncounters.Remove(actor);
        this.World.Events.Post(new EncounterEndEvent(actor));
        this.Notifier.Notify();
    }

    internal string GetStatusText(Actor actor)
    {
        var poi = this.GetPoi(actor);
        if(poi is not null)
            return $"{poi.Def.LabelReadable}";// {frontier.Exploration.ToStringPercentage()}";
        var frontier = this.GetFrontier(actor);
        if (frontier is not null)
            return $"{frontier.Def.LabelReadable}";// {frontier.Exploration.ToStringPercentage()}";
        else
            return $"In town";
        //return $"" +
        //    $"{frontier?.Def.LabelReadable ?? "In town"} " +
        //    $"{(this.World.Combat.IsEngaged(actor) ? $"fighting: {this.World.Combat.Get(actor).Other(actor).Name}" : "")} " +
        //    //$"{this.GetEncounter(actor)?.LabelReadable ?? "idle"}";
        //    $"{this.World.Actions.GetAction(actor)?.Def.LabelReadable ?? "idle"}";
        return $"" +
            //$"{frontier?.Def.LabelReadable ?? "In town"} " +
            $"{frontier?.Def.LabelReadable ?? "In town"} " +
            $"{(this.World.Combat.IsEngaged(actor) ? $"fighting: {this.World.Combat.Get(actor).Other(actor).Name}" : "")} " +
            //$"{this.GetEncounter(actor)?.LabelReadable ?? "idle"}";
            $"{this.GetEncounter(actor)?.GetTextStatus() ?? "idle"}";
    }

    public FrontierRuntime Get(FrontierDef def)
        => this.Frontiers[def];

    protected internal override (string label, Control control)? GetGui()
    {
        var box = new ScrollableBoxNewNewNew(200, UIManager.LargeButton.Height * 8, ScrollModes.Vertical);
        var list = new ListBoxNoScroll<FrontierRuntime, Control>(props =>
        {
            var btn = new LabelNew(() => $"{props.Def.LabelReadable} {props.Exploration.ToStringPercentage()} explored")
                .InvalidateOn(props.Notifier);
            return btn;
        });

        //list.Bind(this.WorldInhabitants);
        list.AddItems(this.Frontiers.Values);
        box.AddControlsVertically(list);
        return ("Frontiers", box);
    }
    ScrollableBoxNewNewNew CreateGui()
    {
        var box = new ScrollableBoxNewNewNew(200, UIManager.LargeButton.Height * 8, ScrollModes.Vertical);
        var list = new ListBoxNoScroll<FrontierRuntime, LabelNew>(props =>
        {
            var btn = new LabelNew(() => $"{props.Def.LabelReadable} {props.Exploration.ToStringPercentage()} explored");
            return btn;
        });

        //list.Bind(this.WorldInhabitants);
        list.AddItems(this.Frontiers.Values);
        box.AddControlsVertically(list);
        return box;
    }
    internal void ActorEnterPoI(Actor actor, FrontierDef frontierDef, int id)
        => this.ActorPoIEnter(actor, this.Get(frontierDef), id);

    internal void ActorPoIEnter(Actor actor, FrontierRuntime frontier, int id)
    {
        var poi = frontier.PoIs[id];
        poi.Add(actor);
        this.ActorsInPoIs.Add(actor, poi);
        this.World.Events.Post(new ActorEnterPoIEvent(actor, frontier, poi));
        this.Notifier.Notify();
    }

    internal void ActorPoIExit(Actor actor)
    {
        var poi = this.GetPoi(actor);
        poi.Remove(actor);
        this.ActorsInPoIs.Remove(actor);
        this.World.Events.Post(new ActorExitPoIEvent(actor));
        this.Notifier.Notify();
    }

    internal PoIRuntime? GetPoi(Actor actor)
        => this.ActorsInPoIs.GetValueOrDefault(actor);
}

internal record struct EncounterBeginEvent(Entity Entity, EncounterRuntime Encounter) : IEventPayload { }

internal record struct EncounterEndEvent(Entity Entity) : IEventPayload { }

internal record struct ActorEnterPoIEvent(Actor Actor, FrontierRuntime Frontier, PoIRuntime PoI) : IEventPayload;

internal record struct ActorExitPoIEvent(Actor Actor) : IEventPayload;

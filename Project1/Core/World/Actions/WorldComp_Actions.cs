using Project1.Core.Entities.Actors;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Core.World.Frontiers;
using Project1.Framework;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace Project1.Core.World.Actions;

internal class WorldComp_Actions(WorldBase world) : WorldComp(world)
{
    readonly Dictionary<Actor, WorldActionRuntime> ActiveActions = [];

    internal override void Tick()
    {
        this.TickActiveEncounters(); // moved the client guard inside so i can increment its progress on clients
    }

    void TickActiveEncounters()
    {
        foreach (var action in this.ActiveActions.Values.ToArray())
        {
            action.Tick();
            //action.Current++;
            if (action.IsFinished && this.World.Net.IsServer)
            {
                action.Resolve();
                this.Finish(action.Actor);
            }
        }
    }

    public WorldActionRuntime? GetAction(Actor actor)
        => this.ActiveActions.GetValueOrDefault(actor);

    internal bool IsBusy(Actor actor)
        => this.ActiveActions.ContainsKey(actor);

    internal void Start(WorldActionDef def, WorldActionData data)
    {
        var runtime = new WorldActionRuntime(def, data);
        this.ActiveActions.Add(data.Actor, runtime);
        $"SERVER {data.Actor} starts {def.LabelReadable} with duration {runtime.Max} ticks".ToConsole();
        this.World.Events.Post(new WorldActionStartedEvent(data.Actor, runtime));
    }

    internal WorldActionRuntime Start(WorldActionDef def, Actor actor, FrontierDef frontier)
    {
        var runtime = new WorldActionRuntime(def, new(this.World.Space.Get(frontier), actor));
        this.ActiveActions.Add(actor, runtime);
        return runtime;
    }

    internal void Finish(Actor actor)
    {
        this.ActiveActions.Remove(actor);
        this.World.Events.Post(new WorldActionFinishedEvent(actor));
    }
}

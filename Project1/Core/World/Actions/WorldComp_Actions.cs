using Project1.Core.Entities.Actors;
using Project1.Core.Networking;
using Project1.Core.Simulation;
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

    public string GetStatusText(Actor actor)
    {
        var action = this.GetAction(actor);
        if (action is null)
            return "idle";
        return action.GetStatusText();
        //actionsManager.GetAction(npc)?.Def.LabelReadable ?? "idle");
    }

    internal bool IsBusy(Actor actor)
        => this.ActiveActions.ContainsKey(actor);

    internal WorldActionRuntime Start(WorldActionDef def, WorldActionData data)
    {
        var runtime = new WorldActionRuntime(def, data);
        this.ActiveActions.Add(data.Actor, runtime);
        $"SERVER {data.Actor} starts {def.LabelReadable} with duration {runtime.Max} ticks".ToConsole();
        this.World.Events.Post(new WorldActionStartedEvent(data.Actor, runtime));
        return runtime;
    }

    //internal WorldActionRuntime Start(WorldActionDef def, Actor actor, FrontierDef frontier)
    //{
    //    var runtime = new WorldActionRuntime(def, new(this.World.Space.Get(frontier), actor));
    //    this.ActiveActions.Add(actor, runtime);
    //    return runtime;
    //}

    internal void Finish(Actor actor)
    {
        this.ActiveActions.Remove(actor);
        this.World.Events.Post(new WorldActionFinishedEvent(actor));
    }

    internal void Interrupt(Actor actor)
    {
        this.Finish(actor);
    }
}

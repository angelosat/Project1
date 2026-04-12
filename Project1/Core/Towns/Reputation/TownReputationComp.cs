using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using Project1.Framework.Events;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;

namespace Project1.Core.Towns.Reputation;

internal record struct ReputationDeltaAppliedEvent(MapBase Map, Actor Actor, int Delta) : IEventPayload;
public sealed class TownReputationComp : TownComp, IGuiNew
{
    static readonly List<ReputationSourceDef> AllDefs = [.. Def.Get<ReputationSourceDef>()];

    public override string Name => "Reputation";
    readonly Dictionary<EntityRefId, ActorReputationEntry> _table = [];

    public TownReputationComp(Town town) : base(town)
    {
        town.Map.Events.ListenTo<EntitySpawnedEvent>(HandleEntitySpawned);

        foreach (var def in AllDefs)
            def.Worker.HookTo(town.Map);
    }

    private void HandleEntitySpawned(EntitySpawnedEvent e)
    {
        if (e.Entity is not Actor agent)
            return;
        if (this._table.ContainsKey(agent.RefId))
            return;
        if (this.Town.Members.Contains(agent))
            return;
        this._table.Add(agent.RefId, new(agent, this.Town.Map.World.CurrentTick));
    }
    internal void ApplyDelta(Actor actor, int v)
    {
        this._table[actor.RefId].ApplyDelta(v);
        this.Map.Events.Post(new ReputationDeltaAppliedEvent(this.Map, actor, v));
    }
    public Control CreateControl()
    {
        var box = new GroupBox();
        var table = new Table<EntityRefId>()
            .AddColumn("name", 128, e => new LabelNew(() => this.Map.World.Get<Actor>(e).LabelReadable))
            .AddColumn("rep", 200, e => this._table[e].CreateControl());
        table.AddItems(this._table.Keys);
        box.Controls.Add(table);
        return box;
    }
    internal override IEnumerable<(Func<string>, Action)> OnQuickMenuCreated()
    {
        yield return (() => "Reputation", () => this.CreateControl().ToWindow("Reputation").Show());
    }
}

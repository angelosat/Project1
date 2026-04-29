using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using Project1.Framework.Events;
using Project1.Framework.Helpers;
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
    internal IEnumerable<ActorReputationEntry> Entries => this._table.Values;

    internal event Action<IEnumerable<ActorReputationEntry>> Added;
    internal event Action<IEnumerable<ActorReputationEntry>> Removed;

    public TownReputationComp(Town town) : base(town)
    {
        town.Map.Events.ListenTo<EntitySpawnedEvent>(HandleEntitySpawned);
        town.Map.World.Events.ListenTo<EntityDisposedEvent>(HandleEntityDisposed);

        foreach (var def in AllDefs)
            def.Worker.HookTo(town.Map);
    }

    private void HandleEntityDisposed(EntityDisposedEvent e)
    {
        if (this._table.Remove(e.Entity.RefId, out var entry))
            this.Removed?.Invoke([entry]);
    }

    private void HandleEntitySpawned(EntitySpawnedEvent e)
    {
        if (e.Entity is not Actor actor)
            return;
        if (this._table.ContainsKey(actor.RefId))
            return;
        if (this.Town.Members.Contains(actor))
            return;
        var entry = new ActorReputationEntry(actor, this.Town.Map.World.CurrentTick);
        //entry.ApplyDelta(debugValue);
        this._table.Add(actor.RefId, entry);
        this.Added?.Invoke([entry]);

        if (this.Net.IsServer)
        {
            //var debugValue = this.World.Random.Next(-100, 100);
            var debugValue = new Random().Next(-100, 100);
            this.ApplyDelta(actor, debugValue);
        }
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
        yield return (() => "Reputation", () => UIManager.ToggleSingleton<TownComp_Reputation_Gui>("Reputation"));
    }
}
sealed class TownComp_Reputation_Gui : GroupBox
{
    readonly Table<ActorReputationEntry> Table;
    public TownComp_Reputation_Gui()
    {
        var comp = Ingame.MainViewportMap.Town.Reputation;
        var entries = comp.Entries;
        this.Table = new Table<ActorReputationEntry>()
            .AddColumn("name", 128, e => new LabelNew(() => comp.World.Get<Actor>(e.ActorId).LabelReadable))
            .AddColumn("rep", 200, e => e.CreateControl());
        this.Table.AddItems(entries);
        comp.Added += this.Table.AddItems;
        comp.Removed += this.Table.RemoveItems;
        this.Controls.Add(ScrollableBoxNewNewNew.FromWidth(this.Table, this.Table.RowWidth, 200).ToPanel());
    }
}

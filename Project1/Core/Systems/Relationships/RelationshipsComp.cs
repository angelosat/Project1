using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.UI;
using Project1.Framework.Helpers;
using Project1.Framework.Interfaces;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.Relationships
{
    internal class RelationshipsCompGui : SelectionBoundControl
    {
        readonly Table<(Actor actor, IProgressBar progress)> Table;
        RelationshipsComp Comp;
        public RelationshipsCompGui()
        {
            this.Table = new Table<(Actor actor, IProgressBar progress)>()
                .AddColumn("actor", 128, a => new Label(a.actor))
                .AddColumn("progress", 200, a => new BarSigned(a.progress) { TextFunc = () => a.progress.ToString() });//, 100, () => a.progress.ToString()));
            var scrollable = ScrollableBoxNewNewNew.FromWidth(this.Table, this.Table.RowWidth, 400).ToPanel();
            this.AddControlsVertically(scrollable);
        }
        protected internal override void OnBind(ISelectable selectable)
        {
            if (selectable is not Actor actor)
                return;
            unsub();
            this.Comp = actor.Relationships;
            this.Table.ClearControls();
            this.Table.AddItems(this.Comp.AllProgress);
            this.Comp.EntryAdded += this.OnEntryAdded;
            this.HideAction = unsub;// () => this.Comp.EntryAdded -= this.OnEntryAdded;
        }

        private void unsub()
        {
            this.Comp?.EntryAdded -= this.OnEntryAdded;
        }

        private void OnEntryAdded(Actor actor, IProgressBar progress)
        {
            this.Table.AddItem((actor, progress));
        }
    }
    sealed class RelationshipEntry
    {
        static readonly Tick baseInterval = Ticks.FromHours(1);
        ProgressIntSigned _progress { get; } = new(100);
        internal IProgressBar Progress => this._progress;
        internal int Sign => this._progress.Value >= 0 ? 1 : -1;
        internal int Value => this._progress.Value;
        internal Tick NextUpdate { get; private set; }
        internal void ApplyDelta(int value, Tick currentTick)
        {
            //this.NextUpdate = currentTick;
            var scaledInterval = baseInterval * Math.Clamp(Math.Abs(this._progress.Percentage), 0.1f, 1f);
            this.NextUpdate = currentTick + scaledInterval;
            this._progress.ApplyDelta(value);
        }
    }
    internal sealed class RelationshipsComp : EntityComp
    {
        public override EntityCompDef CompDef => EntityCompDefOf.Relationships;

        public override string Name => "Relationships";

        internal Action<Actor, IProgressBar> EntryAdded;
        internal IEnumerable<(Actor, RelationshipEntry)> AllEntries => this.Entries.Select(kv => (this.Owner.World.Get<Actor>(kv.Key), kv.Value));
        internal IEnumerable<(Actor, IProgressBar)> AllProgress => this.Entries.Select(kv => (this.Owner.World.Get<Actor>(kv.Key), kv.Value.Progress));

        readonly Dictionary<EntityRefId, RelationshipEntry> Entries = [];
        public override void Tick()
        {
            var current = this.Owner.World.CurrentTick;
            foreach(var (_, entry) in this.Entries)
            {
                var perc = entry.Progress.Percentage;
                if (Math.Abs(perc) <= .1f)
                    continue;
                if (current >= entry.NextUpdate)
                    entry.ApplyDelta(-1 * entry.Sign, current);
            }
        }

        public void ApplyDelta(Actor target, int delta)
        {
            if (!this.Entries.TryGetValue(target.RefId, out var entry))
            {
                this.Entries[target.RefId] = entry = new();// new(100);
                this.EntryAdded?.Invoke(target, entry.Progress);
            }
            entry.ApplyDelta(delta, this.Owner.World.CurrentTick);
            this.Owner.World.Events.Post(new RelationshipDeltaAppliedEvent(this.Owner.RefId, target.RefId, delta));
        }

        public int Get(Actor other)
            => this.Entries.TryGetValue(other.RefId, out var entry) ? entry.Value : 0;
    }
}

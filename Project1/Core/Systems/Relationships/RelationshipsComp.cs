using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.UI;
using Project1.Framework.Helpers;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.Relationships
{
    internal class RelationshipsCompGui : SelectionBoundControl
    {
        Table<(Actor actor, ProgressIntSigned progress)> Table;
        RelationshipsComp Comp;
        public RelationshipsCompGui()
        {
            this.Table = new Table<(Actor actor, ProgressIntSigned progress)>()
                .AddColumn("actor", 128, a => new Label(a.actor))
                .AddColumn("progress", 100, a => new BarSigned(a.progress) { TextFunc = () => a.progress.ToString() });//, 100, () => a.progress.ToString()));
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
            this.Table.AddItems(this.Comp.AllEntries);
            this.Comp.EntryAdded += this.OnEntryAdded;
            this.HideAction = unsub;// () => this.Comp.EntryAdded -= this.OnEntryAdded;
        }

        private void unsub()
        {
            this.Comp?.EntryAdded -= this.OnEntryAdded;
        }

        private void OnEntryAdded(Actor actor, ProgressIntSigned progress)
        {
            this.Table.AddItem((actor, progress));
        }
    }
    internal class RelationshipsComp : EntityComp, IGuiNew
    {
        public override EntityCompDef CompDef => EntityCompDefOf.Relationships;

        public override string Name => "Relationships";

        internal Action<Actor, ProgressIntSigned> EntryAdded;
        internal IEnumerable<(Actor, ProgressIntSigned)> AllEntries => this.Entries.Select(kv => (this.Owner.World.Get<Actor>(kv.Key), kv.Value));

        readonly Dictionary<EntityRefId, ProgressIntSigned> Entries = [];

        public void ApplyDelta(Actor target, int delta)
        {
            if (!this.Entries.TryGetValue(target.RefId, out var entry))
            {
                this.Entries[target.RefId] = entry = new(100);
                this.EntryAdded?.Invoke(target, entry);
            }
            entry.ApplyDelta(delta);
            this.Owner.World.Events.Post(new RelationshipDeltaAppliedEvent(this.Owner.RefId, target.RefId, delta));
        }

        public Control CreateControl()
        {
            var box = new GroupBox();
            var table = new Table<(Actor actor, ProgressInt progress)>()
                .AddColumn("actor", 128, a => new Label(a.actor))
                .AddColumn("progress", 100, a => new Bar(a.progress, 100, () => a.progress.ToString()));
            box.Controls.Add(table);
            return box;
        }
    }
}

using Project1.Core.Blocks.Comps;
using Project1.Core.Entities.Actors;
using Project1.Core.UI.Blocks;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;

namespace Project1.Core.Blocks
{
    internal class BlockOwnershipComp : BlockComp
    {
        readonly ChangeNotifier Notifications = new();
        internal new class Spec : BlockComp.Spec
        {
            public override Type CompType => typeof(BlockOwnershipComp);

            public override BlockComp CreateComp() => new BlockOwnershipComp();
        }
        public override BlockCompDef CompDef => BlockCompDefOf.Ownership;
        public EntityRefId Owner { get; private set; }

        internal override IEnumerable<(string label, Type type)> GetSelectionTabs()
        {
            yield return ("Owner", typeof(BlockOwnerGui));
        }
        internal override IEnumerable<Control> GetInspectorControls()
        {
            yield return new LabelNew(() => $"Owner: {(this.Owner != EntityRefId.Null ? this.Map.World.GetEntity(this.Owner).Name : "<none>")}").Bind(this.Notifications);
        }
        internal void SetOwner(Actor a)
        {
            this.Owner = a?.RefId ?? EntityRefId.Null;
            a?.Possessions.Add(this.Parent);
            this.Map.Events.Post(new BlockOwnerChangedEvent(this.Parent, a));
            this.Notifications.Notify();
        }

        // TODO serialization
        protected override void SaveExtra(SaveTag tag)
        {
            tag.Save("Owner", this.Owner);
        }
        public override void Load(SaveTag tag)
        {
            if (tag.TryLoadInt("Owner", out var ownerId)) this.Owner = ownerId;
        }
        public override void Write(IDataWriter w)
        {
            w.Write(this.Owner);
        }
        public override ISerializable Read(IDataReader r)
        {
            this.Owner = r.ReadInt32();
            return this;
        }
    }
}
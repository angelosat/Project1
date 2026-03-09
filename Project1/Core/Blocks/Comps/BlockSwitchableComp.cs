using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Serialization;
using System;

namespace Project1.Core.Blocks.Comps
{
    class BlockSwitchableComp : BlockComp
    {
        public new class Spec : BlockComp.Spec
        {
            public override Type CompType => typeof(BlockSwitchableComp);

            public override BlockSwitchableComp CreateComp() => new();
        }
        public override BlockCompDef CompDef => BlockCompDefOf.Switchable;
        public bool IsOn { get; private set; } = true;
     
        public void Toggle()
        {
            this.IsOn = !this.IsOn;
            foreach (var comp in this.Parent.Comps.Inner)
                comp.OnSwitched(this.IsOn);
            this.Map.Events.Post(new CellsInvalidatedEvent(this.Map, [this.Parent.OriginGlobal]));
        }
        protected override void SaveExtra(SaveTag tag)
        {
            tag.Add(this.IsOn.Save("SwitchedOn"));
        }
        public override void Load(SaveTag tag)
        {
            tag.TryGetTagValue<bool>("SwitchedOn", v => this.IsOn = v);
        }
        public override void Write(IDataWriter w)
        {
            w.Write(this.IsOn);
        }
        public override ISerializable Read(IDataReader r)
        {
            this.IsOn = r.ReadBoolean();
            return this;
        }
    }
}

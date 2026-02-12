using Project1.Core.Blocks.Comps;
using Project1.Core.Entities;
using Project1.Core.Towns.Designations;
using Project1.Framework;
using Project1.Framework.Serialization;
using System;

namespace Project1.Core.Blocks
{
    class BlockEntityCompSwitchable : BlockComp
    {
        public override BlockCompDef CompDef => throw new NotImplementedException();
        public bool SwitchedOn { get; private set; } = true;
        public bool IsSwitchedOn()
        {
            return this.SwitchedOn;
        }
        public void Toggle(GameObject actor, TargetArgs target)
        {
            this.SwitchedOn = !this.SwitchedOn;
            actor.Map.Town.DesignationManager.RemoveDesignation(DesignationDefOf.Switch, target.Global);
        }
        protected override void SaveExtra(SaveTag tag)
        {
            tag.Add(this.SwitchedOn.Save("SwitchedOn"));
        }
        public override void Load(SaveTag tag)
        {
            tag.TryGetTagValue<bool>("SwitchedOn", v => this.SwitchedOn = v);
        }
        public override void Write(IDataWriter w)
        {
            w.Write(this.SwitchedOn);
        }
        public override ISerializable Read(IDataReader r)
        {
            this.SwitchedOn = r.ReadBoolean();
            return this;
        }
    }
}

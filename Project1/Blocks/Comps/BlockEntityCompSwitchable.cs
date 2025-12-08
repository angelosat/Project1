using System.IO;

namespace Start_a_Town_
{
    class BlockEntityCompSwitchable : BlockEntityComp
    {
        public override string Name { get; } = "Switchable";
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
        public override void AddSaveData(SaveTag tag)
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

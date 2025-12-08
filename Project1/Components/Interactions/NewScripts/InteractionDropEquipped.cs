using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class InteractionDropEquipped : Interaction
    {
        GearType Type;
        static readonly public string InteractionName = "DropEquipped";
        public InteractionDropEquipped()
            : base("DropEquipped")
        {

        }
        public InteractionDropEquipped(GearType type):base("DropEquipped")
        {
            this.Type = type;
        }
        
        internal override void InitAction()
        {
            var a = this.Actor;
            var slot = a.Gear.GetSlot(this.Type);
            if (slot.Object == null)
                return;
            slot.Object.Spawn(a.Map, a.Global + new Vector3(0, 0, a.Physics.Height));
            slot.Clear();
        }
        public override object Clone()
        {
            return new InteractionDropEquipped(this.Type);
        }
        protected override void WriteExtra(IDataWriter w)
        {
            w.Write((int)this.Type.ID);
        }
        protected override void ReadExtra(IDataReader r)
        {
            this.Type = GearType.Dictionary[(GearType.Types)r.ReadInt32()];
        }
    }
}

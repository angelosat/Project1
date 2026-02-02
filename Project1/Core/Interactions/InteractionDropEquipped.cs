using Microsoft.Xna.Framework;
using Project1.Framework.Gear;
using Project1.Framework.Interactions;
using Start_a_Town_;

namespace Project1.Core.Interactions
{
    class InteractionDropEquipped : Interaction
    {
        GearTypeDef Type;
        static readonly public string InteractionName = "DropEquipped";
        public InteractionDropEquipped()
            : base("DropEquipped")
        {

        }
        public InteractionDropEquipped(GearTypeDef type):base("DropEquipped")
        {
            this.Type = type;
        }

        protected void OnStart()
        {
            var a = this.Actor;
            var slot = a.Gear.GetSlot(this.Type);
            if (slot.Object == null)
                return;
            //slot.Object.Spawn(a.Map, a.Global + new Vector3(0, 0, a.Physics.Height));
            a.Map.Spawn(slot.Object as Entity, a.Global + new Vector3(0, 0, a.Physics.Height), Vector3.Zero);
            slot.Clear();
        }
        protected override void WriteExtra(IDataWriter w)
        {
            //w.Write((int)this.Type.ID);
        }
        protected override void ReadExtra(IDataReader r)
        {
            //this.Type = GearTypeDef.Dictionary[(GearTypeDef.Types)r.ReadInt32()];
        }
    }
}

using Project1.Framework.Interactions;
using Project1.Framework.Net.Packets;
using Start_a_Town_;

namespace Project1.Core.Interactions
{
    class DropInventoryItem : Interaction
    {
        public DropInventoryItem()
            : base(
            "DropInventoryItem",
            0
            )
        {

        }
       
        public override void Perform()
        {
            if (this.Actor.Net.IsClient)
                return;
            //this.Actor.Inventory.Drop(this.Target.Object);
            PacketEntityDropItem.Send(this.Actor, this.Target.Object as Entity);
        }
    }
}

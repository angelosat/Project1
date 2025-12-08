using Project1.Network.Packets;

namespace Start_a_Town_.Components.Interactions
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

        public override object Clone()
        {
            return new DropInventoryItem();
        }
    }
}

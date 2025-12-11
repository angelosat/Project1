using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketDropInventoryItem
    {
        static readonly int p;
        static PacketDropInventoryItem()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }
        internal static void Send(INetEndpoint net, GameObject actor, GameObject item, int amount)
        {
            var stream = actor.Net.BeginPacket(p);
            stream.Write(actor.RefId);
            stream.Write(item.RefId);
            stream.Write(amount);
        }
        static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var actorID = r.ReadInt32();
            var itemID = r.ReadInt32();
            var amount = r.ReadInt32();
            var item = net.World.GetEntity(itemID);
            var actor = net.World.GetEntity<Actor>(actorID);
            actor.Inventory.Drop(item, amount); // TODO: this happens immediately when the game is paused. maybe create an interaction with a 1 frame duration? NO it's good that it happens while the game is paused
            if (amount == item.StackSize)
                NpcComponent.RemovePossesion(actor, item);
            if (net is Server)
            {
                actor.AI.State.ItemPreferences.OnForcedDrop(item); /// TODO better make the ItemPreferencesmanager subscribe to a entitydropitem event, or even a entityspawnevent (slower to check)
                Send(net, actor, item, amount);
            }
        }
    }
}

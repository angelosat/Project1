using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Framework;

namespace Project1.Core.Systems.Ownership;

[EnsureStaticCtorCall]
internal static class Packets_Ownership
{
    internal static readonly PacketId _pChange = Registry.PacketHandlers.Register(ReceiveOwnershipChange);

    static Packets_Ownership()
    {
        Registry.WorldEventHooksServer.Register<ItemOwnerChangedEvent>(HandleItemOwnerChanged);
    }

    private static void HandleItemOwnerChanged(ItemOwnerChangedEvent e)
    {
        SendItemOwnerChanged(e.Item, e.Item.OwnerId);
    }

    private static void SendItemOwnerChanged(Entity item, EntityRefId newOwner)
    {
        Server.Instance.BeginPacket(_pChange)
            .Write(item.RefId)
            .Write(newOwner);
    }

    private static void ReceiveOwnershipChange(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var client = endpoint as Client;
        var world = client.World;
        var item = world.Get(r.ReadEntityRefId());
        var newowner = world.Get<Actor>(r.ReadEntityRefId());
        item.SetOwnerNew(newowner);
    }
}

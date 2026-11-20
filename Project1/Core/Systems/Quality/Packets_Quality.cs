using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Framework;

namespace Project1.Core.Systems.Quality;

[EnsureStaticCtorCall]
internal static class Packets_Quality
{
    static readonly PacketId _pQualityUpgraded = Registry.PacketHandlers.Register(ReceiveQualityUpgraded);

    static Packets_Quality()
    {
        Registry.WorldEventHooksServer.Register<ItemRarityUpgradedEvent>(SendRarityUpgraded);
    }

    private static void SendRarityUpgraded(ItemRarityUpgradedEvent e)
    {
        var w = Server.Instance.BeginPacket(_pQualityUpgraded);
        e.Write(w);
    }

    private static void ReceiveQualityUpgraded(NetEndpoint endpoint, Packet packet)
    {
        var r = packet.PacketReader;
        var e = ItemRarityUpgradedEvent.Create(r);
        e.World = (StaticWorld)endpoint.World;
        QualitySystem.Upgrade(endpoint.World.Resolve(e.ItemId));
    }
}

using Project1.Core.Entities;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Systems.Consumables.Scrolls;
using Project1.Framework;

namespace Project1.Core.Systems.Consumables
{
    [EnsureStaticCtorCall]
    internal static class PacketsEffects
    {
        internal static readonly PacketId _pTeleport = Registry.PacketHandlers.Register(OnTeleport);

        static PacketsEffects()
        {
            Registry.MapEventHooksServer.Register<EntityTeleportedEvent>(HandleEntityTeleported);
        }

        private static void HandleEntityTeleported(EntityTeleportedEvent e)
        {
            SendEntityTeleported(Server.Instance, e.Entity);
        }
        private static void SendEntityTeleported(NetEndpoint endpoint, Entity entity)
        {
            endpoint.BeginPacket(_pTeleport)
                .Write(entity.RefId);
        }

        private static void OnTeleport(NetEndpoint endpoint, Packet packet)
        {
            var r = packet.PacketReader;
            var entity = endpoint.World.Get<Entity>(r.ReadEntityRefId());
            entity.Map.Events.Post(new EntityTeleportedEvent(entity));
        }
    }
}

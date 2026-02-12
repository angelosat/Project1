using Project1.Framework;
using Project1.Core.Entities.Actors;
using Project1.Core.Networking;
using Project1.Framework.Events;

namespace Project1.Core.Networking.Entities
{
    [EnsureStaticCtorCall]
    static class PacketEntityCrouchToggle
    {
        static readonly int PType;
        static PacketEntityCrouchToggle()
        {
            PType = Registry.PacketHandlers.Register(Receive);
        }
        internal static void Send(INetEndpoint net, int entityID, bool toggle)
        {
            var server = net as Server;
            server.BeginPacket(PType)
                .Write(entityID)
                .Write(toggle);
        }
        internal static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var id = r.ReadInt32();
            var entity = net.World.GetEntity(id) as Actor;
            var toggle = r.ReadBoolean();
            entity.CrouchToggle(toggle);

            if (net is Server)
                Send(net, entity.RefId, toggle);
        }
    }
}

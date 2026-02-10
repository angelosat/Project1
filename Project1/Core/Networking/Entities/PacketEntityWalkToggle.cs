using Project1.Framework;
using Project1.Core.Entities.Actors;
using Project1.Core.Net;
using Project1.Framework.Events;

namespace Project1.Core.Networking.Entities
{
    [EnsureStaticCtorCall]
    static class PacketEntityWalkToggle
    {
        static readonly int PType;
        static PacketEntityWalkToggle()
        {
            PType = Registry.PacketHandlers.Register(Receive);
        }
        internal static void Send(NetEndpoint net, int entityID, bool toggle)
        {
            return;
            var server = net as Server;
            var w = server.BeginPacket(PType);
            w.Write(entityID);
            w.Write(toggle);
        }
        internal static void Receive(NetEndpoint net, Packet p)
        {
            var r = p.PacketReader;
            var id = r.ReadInt32();
            var entity = net.World.GetEntity(id) as Actor;
            var toggle = r.ReadBoolean();
            entity.WalkToggle(toggle);

            if (net.IsServer)
                Send(net, entity.RefId, toggle);
        }
    }
}

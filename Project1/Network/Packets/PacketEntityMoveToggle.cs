using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketEntityMoveToggle
    {
        static readonly int _packetTypeId;
        static PacketEntityMoveToggle()
        {
            _packetTypeId = Registry.PacketHandlers.Register(Receive);
        }
        
        internal static void Send(NetEndpoint net, int entityID, bool toggle)
        {
            var server = net as Server;
            var w = server.BeginPacket(_packetTypeId);
            w.Write(entityID);
            w.Write(toggle);
        }
        internal static void Receive(NetEndpoint net, Packet packet)
        {
            var r = packet.PacketReader;
            var id = r.ReadInt32();
            var entity = net.World.GetEntity(id) as Actor;
            var toggle = r.ReadBoolean();
            entity.MoveToggle(toggle);
            if (net is Server)
                Send(net, entity.RefId, toggle);
        }
    }
}

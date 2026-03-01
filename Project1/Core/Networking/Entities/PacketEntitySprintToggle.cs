using Project1.Framework;
using Project1.Core.Entities.Actors;
using Project1.Core.Networking;

namespace Project1.Core.Networking.Entities
{
    [EnsureStaticCtorCall]
    static class PacketEntitySprintToggle
    {
        static readonly int _packetTypeId;
        static PacketEntitySprintToggle()
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
        internal static void Receive(NetEndpoint net, Packet p)
        {
            var r = p.PacketReader;
            var id = r.ReadInt32();
            var entity = net.World.GetEntity(id) as Actor;
            var toggle = r.ReadBoolean();
            entity.SprintToggle(toggle);

            if (net is Server)
                Send(net, entity.RefId, toggle);
        }
    }
}

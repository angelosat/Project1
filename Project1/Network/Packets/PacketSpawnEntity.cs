using Start_a_Town_.Net;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    public static class PacketSpawnEntity
    {
        static readonly int _packetTypeId;
        static PacketSpawnEntity()
        {
            _packetTypeId = Registry.PacketHandlers.Register(Receive);
        }

        public static void Send(Entity entity, Vector3 position, Vector3 velocity)//, Vector2 direction)
        {
            var server = entity.Net as Server;
            server.BeginPacket(_packetTypeId)
                .Write(entity.RefId)
                .Write(position)
                .Write(velocity);
                //.Write(direction);
        }
        private static void Receive(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var entity = client.World.GetEntity(r.ReadInt32());
            var pos = r.ReadVector3();
            var vel = r.ReadVector3();
            entity.SetPosition(pos);
            entity.Velocity = vel;
            //entity.Spawn(client.Map);
            client.Map.Spawn(entity);
        }
    }
}

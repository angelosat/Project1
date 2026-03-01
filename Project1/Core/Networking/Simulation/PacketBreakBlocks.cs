using System.Collections.Generic;
using Project1.Core.Simulation;
using Project1.Core.Networking;
using Project1.Framework;

namespace Project1.Core.Networking.Simulation
{
    [EnsureStaticCtorCall]
    internal class PacketBreakBlocks
    {
        static readonly int _packetTypeId;
        static PacketBreakBlocks()
        {
            _packetTypeId = Registry.PacketHandlers.Register(Receive);
        }
        public static void Send(MapBase map, List<IntVec3> blocks)
        {
            var server = map.Net as Server;
            server.BeginPacket(_packetTypeId)
                .Write(map.ID)
                .Write(blocks);
        }
        private static void Receive(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var mapid = r.ReadInt32();
            var map = client.Map;
            map.RemoveBlocks(packet.PacketReader.ReadListIntVec3());
        }
    }
}

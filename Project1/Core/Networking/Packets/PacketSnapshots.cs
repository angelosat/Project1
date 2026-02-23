using Project1.Core.Entities;
using Project1.Framework;
using Project1.Framework.Events;
using System.Collections.Generic;

namespace Project1.Core.Networking.Packets
{
    [EnsureStaticCtorCall]
    static class PacketSnapshots
    {
        static readonly int _packetTypeId;
        static PacketSnapshots()
        {
            _packetTypeId = Registry.PacketHandlers.Register(Receive);
        }
        static public void Send(NetEndpoint net, IReadOnlyCollection<Entity> entities)
        {
            var server = net as Server;
            var w = server.BeginPacketNew(ReliabilityType.Unreliable, _packetTypeId);
            w.Write(server.CurrentTick);
            w.Write(entities.Count);
            foreach (var obj in entities)
            {
                w.Write(obj.RefId);
                EntitySnapshot.Write(obj, w);
            }
        }
        static public void Receive(NetEndpoint net, Packet pck)
        {
            var client = net as Client;
            var r = pck.PacketReader;
            client.ReadSnapshot(r);
        }
    }
}

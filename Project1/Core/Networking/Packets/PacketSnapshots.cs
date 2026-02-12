using System.Collections.Generic;
using Project1.Framework;
using Project1.Core.Entities;
using Project1.Framework.Events;
using Project1.Core.Networking;

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
        static public void Send(NetEndpoint net, ICollection<GameObject> entities)
        {
            var server = net as Server;
            var strem = server.BeginPacketNew(ReliabilityType.Unreliable, _packetTypeId);
            strem.Write(server.CurrentTick);
            strem.Write(entities.Count);
            foreach (var obj in entities)
            {
                strem.Write(obj.RefId);
                ObjectSnapshot.Write(obj, strem);
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

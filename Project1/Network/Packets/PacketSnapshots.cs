using System.Collections.Generic;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketSnapshots
    {
        static readonly int _packetTypeId;
        static PacketSnapshots()
        {
            _packetTypeId = NetEndpoint.RegisterPacketHandler(Receive);
        }
        static public void Send(NetEndpoint net, ICollection<GameObject> entities)
        {
            var server = net as Server;
            //var strem = server.OutgoingStreamUnreliable;
            //strem.Write(_packetTypeId);
            var strem = server.BeginPacketNew(ReliabilityType.Unreliable, _packetTypeId);
            strem.Write(server.Clock.TotalMilliseconds);
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

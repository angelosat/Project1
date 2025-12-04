using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketEntityRequestSpawn
    {
        static readonly int _packetTypeId;
        static PacketEntityRequestSpawn()
        {
            _packetTypeId = PacketRegistry.Register(ReceiveTemplate);
        }
        internal static void SendTemplate(INetEndpoint net, int templateID, TargetArgs target)
        {
            //var w = net.GetOutgoingStreamOrderedReliable();
            //w.Write(p);
            var w = net.BeginPacket(ReliabilityType.OrderedReliable, _packetTypeId);

            w.Write(templateID);
            target.Write(w);
        }
        
        internal static void ReceiveTemplate(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var server = net as Server;
            var templateID = r.ReadInt32();
            var target = TargetArgs.Read(net, r);
            server.SpawnRequestFromTemplate(templateID, target);
        }
    }
}

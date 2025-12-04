using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketEntityRequestSpawn
    {
        static readonly int _packetTypeId;
        static PacketEntityRequestSpawn()
        {
            _packetTypeId = Registry.PacketHandlers.Register(ReceiveTemplate);
        }
        internal static void SendTemplate(NetEndpoint net, int templateID, TargetArgs target)
        {
            var w = net.BeginPacket(_packetTypeId);

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

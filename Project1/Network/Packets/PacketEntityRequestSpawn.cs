using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketEntityRequestSpawn
    {
        static readonly int p;
        static PacketEntityRequestSpawn()
        {
            p = Network.RegisterPacketHandler(ReceiveTemplate);
        }
        internal static void SendTemplate(INetPeer net, int templateID, TargetArgs target)
        {
            var w = net.GetOutgoingStream();
            w.Write(p);
            w.Write(templateID);
            target.Write(w);
        }
        
        internal static void ReceiveTemplate(INetPeer net, BinaryReader r)
        {
            var server = net as Server;
            var templateID = r.ReadInt32();
            var target = TargetArgs.Read(net, r);
            server.SpawnRequestFromTemplate(templateID, target);
        }
    }
}

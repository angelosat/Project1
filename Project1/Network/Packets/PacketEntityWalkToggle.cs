using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketEntityWalkToggle
    {
        static readonly int PType;
        static PacketEntityWalkToggle()
        {
            PType = Network.RegisterPacketHandler(Receive);
        }
       
        internal static void Send(INetwork net, int entityID, bool toggle)
        {
            var server = net as Server;
            var w = server.OutgoingStreamTimestamped;
            w.Write(PType);
            w.Write(entityID);
            w.Write(toggle);
        }
        internal static void Receive(INetwork net, BinaryReader r)
        {
            var id = r.ReadInt32();
            var entity = net.GetNetworkObject(id) as Actor;
            var toggle = r.ReadBoolean();
            entity.WalkToggle(toggle);

            if (net is Server)
                Send(net, entity.RefID, toggle);
        }
    }
}

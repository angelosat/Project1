using System.Collections.Generic;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketCommandNpc
    {
        static readonly int p;
        static PacketCommandNpc()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        static internal void Send(INetEndpoint net, List<int> npcIDs, TargetArgs target, bool enqueue)
        {
            var w = net.GetOutgoingStreamOrderedReliable();
            w.Write(p);
            w.Write(npcIDs);
            target.Write(w);
            w.Write(enqueue);
        }
        static void Receive(INetEndpoint net, BinaryReader r)
        {
            var npcids = r.ReadListInt();
            var target = TargetArgs.Read(net, r);
            var enqueue = r.ReadBoolean();
            foreach(var npc in net.World.GetEntities(npcids))
                npc.MoveOrder(target, enqueue);
        }
    }
}

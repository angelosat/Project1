using System.Collections.Generic;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketCommandNpc
    {
        static readonly int p;
        static PacketCommandNpc()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }
        static internal void Send(NetEndpoint net, List<int> npcIDs, TargetArgs target, bool enqueue)
        {
            var w = net.BeginPacket(p);
            w.Write(npcIDs);
            target.Write(w);
            w.Write(enqueue);
        }
        static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var npcids = r.ReadListInt();
            var target = TargetArgs.Read(net, r);
            var enqueue = r.ReadBoolean();
            foreach(var npc in net.World.GetEntities(npcids))
                npc.MoveOrder(target, enqueue);
        }
    }
}

using System.Collections.Generic;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Framework;

namespace Project1.Core.AI.Packets
{
    [EnsureStaticCtorCall]
    static class PacketCommandNpc
    {
        static readonly int p;
        static PacketCommandNpc()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }
        static internal void Send(NetEndpoint net, MapBase map, List<int> npcIDs, InteractionTarget target, bool enqueue)
        {
            var w = net.BeginPacket(p);
            w.Write(map.ID);
            w.Write(npcIDs);
            target.Write(w);
            w.Write(enqueue);
        }
        static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var map = net.World.Get(r.ReadMapId());
            var npcids = r.ReadListInt32();
            var target = InteractionTarget.Read(net.World, r);
            var enqueue = r.ReadBoolean();
            foreach(var npc in net.World.GetEntities(npcids))
                npc.MoveOrder(target, enqueue);
        }
    }
}

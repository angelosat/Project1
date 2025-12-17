using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    class PacketOrderAdd
    {
        static int p;
        static PacketOrderAdd()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }
        internal static void PlayerCreatedOrder(BlockEntity workstation, MaterialMappingDef processDef)
        {
            var net = workstation.Map.Net;
            var w = net.BeginPacket(p);
            w.Write(workstation.Map.ID);
            w.Write(workstation.OriginGlobal);
            w.Write(processDef);
        }
        internal static void Send(NetEndpoint net, Vector3 global, Reaction reaction)
        {
            var w = net.BeginPacket(p);
            w.Write(global);
            reaction.Write(w);
        }
        private static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var mapid = r.ReadInt32();
            var workstationPosition = r.ReadIntVec3();// net.Map.GetBlockEntity();
            var process = r.ReadDef<MaterialMappingDef>();
            net.Map.Town.CraftingManagerNew.CreateOrder(workstationPosition, process);
            if (net is Server server)
                PlayerCreatedOrder(net.Map.GetBlockEntity(workstationPosition), process);

            return;
            var station = r.ReadVector3();
            var reaction = r.ReadDef<Reaction>();
            net.Map.Town.CraftingManager.AddOrder(station, reaction);
            if (net is Server)
                Send(net, station, reaction);
        }
    }
}

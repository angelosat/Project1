using Project1.Framework.Base;
using Project1.Framework.Entities.Actors;
using Project1.Framework.Net;
using Start_a_Town_;

namespace Project1.Framework.Needs
{
    [EnsureStaticCtorCall]
    static class PacketNeedModify
    {
        static readonly int pModify, pSet;
        static PacketNeedModify()
        {
            pSet = Registry.PacketHandlers.Register(ReceiveSet);
        }

        
        static public void SendSet(NetEndpoint net, int agentID, NeedDef needDef, float value)
        {
            net.BeginPacketImmediate(pSet)
                .Write(agentID)
                .Write(needDef)
                .Write((int)value);
        }
        private static void ReceiveSet(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var actor = net.World.GetEntity(r.ReadInt32()) as Actor;
            var need = r.ReadDef<NeedDef>();
            var value = r.ReadInt32();
            actor.GetNeed(need).SetValue(value);
            if(net is Server server)
                SendSet(net, actor.RefId, need, value);
        }
        static public void SendModify(Server server, int agentID, NeedDef needDef, float value)
        {
            server.BeginPacket(pModify)
                .Write(agentID)
                .Write(needDef)
                .Write(value);
        }
        static public void ReceiveModify(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var entity = net.World.GetEntity(r.ReadInt32());
            var need = r.ReadDef<NeedDef>();
            var value = r.ReadInt32();
            NeedsComponent.ModifyNeed(entity, need, value);
        }
    }
}
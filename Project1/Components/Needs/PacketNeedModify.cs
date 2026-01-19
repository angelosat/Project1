using Start_a_Town_.Net;

namespace Start_a_Town_.Components.Needs
{
    [EnsureStaticCtorCall]
    static class PacketNeedModify
    {
        static readonly int pModify, pSet;
        static PacketNeedModify()
        {
            //pModify = Registry.PacketHandlers.Register(ReceiveModify);
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
            //actor.GetNeed(need).Value = value;
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
            //var need = entity.GetNeed(needName);
            NeedsComponent.ModifyNeed(entity, need, value);
            //entity.Map.EventOccured(Components.Message.Types.NeedUpdated, entity, needName, needVal);
            //entity.Map.World.Events.Post(new ActorNeedUpdatedEvent(need));
        }
    }
}
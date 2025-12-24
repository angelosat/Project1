using Start_a_Town_.Net;
using System;

namespace Start_a_Town_.Components.Needs
{
    [EnsureStaticCtorCall]
    static class PacketNeedModify
    {
        static readonly int pModify, pSet;
        static PacketNeedModify()
        {
            pModify = Registry.PacketHandlers.Register(ReceiveModify);
            pSet = Registry.PacketHandlers.Register(ReceiveSet);
        }

        
        static public void SendSet(NetEndpoint net, int agentID, NeedDef needDef, float value)
        {
            net.BeginPacket(pSet)
                .Write(agentID)
                .Write(needDef)
                .Write(value);
        }
        private static void ReceiveSet(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var actor = net.World.GetEntity(r.ReadInt32()) as Actor;
            var need = r.ReadDef<NeedDef>();
            var value = r.ReadSingle();
            actor.GetNeed(need).Value = value;
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
            var value = r.ReadSingle();
            //var need = entity.GetNeed(needName);
            NeedsComponent.ModifyNeed(entity, need, value);
            //entity.Map.EventOccured(Components.Message.Types.NeedUpdated, entity, needName, needVal);
            entity.Map.World.Events.Post(new ActorNeedUpdatedEvent(entity as Actor, need, value));
        }
    }
}

namespace Start_a_Town_
{
    class ActorNeedUpdatedEvent(Actor actor, NeedDef need, float value) : EventPayloadBase
    {
        public readonly Actor Actor = actor;
        public readonly NeedDef Need = need;
        public readonly float Value = value;
    }
}

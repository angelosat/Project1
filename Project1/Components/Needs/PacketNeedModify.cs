using Start_a_Town_.Net;

namespace Start_a_Town_.Components.Needs
{
    [EnsureStaticCtorCall]
    static class PacketNeedModify
    {
        static readonly int p;
        static PacketNeedModify()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }
        static public void Send(Server server, int agentID, NeedDef needDef, float value)
        {
            server.OutgoingStreamOrderedReliable.Write(p);
            server.OutgoingStreamOrderedReliable.Write(agentID);
            server.OutgoingStreamOrderedReliable.Write(needDef.Name);
            server.OutgoingStreamOrderedReliable.Write(value);

        }
        static public void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var entity = net.World.GetEntity(r.ReadInt32());
            var needName = r.ReadString();
            var needVal = r.ReadSingle();
            var need = entity.GetNeed(needName);
            NeedsComponent.ModifyNeed(entity, needName, needVal);
            //entity.Map.EventOccured(Components.Message.Types.NeedUpdated, entity, needName, needVal);
            entity.Map.World.Events.Post(new ActorNeedUpdatedEvent(entity as Actor, need.NeedDef, needVal));
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

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
            var entity = net.GetNetworkEntity(r.ReadInt32());
            var needName = r.ReadString();
            var needVal = r.ReadSingle();
            NeedsComponent.ModifyNeed(entity, needName, needVal);
            net.Map.EventOccured(Components.Message.Types.NeedUpdated, entity, needName, needVal);
        }
    }
}

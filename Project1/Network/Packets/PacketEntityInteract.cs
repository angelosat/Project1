using Start_a_Town_.Net;
using System;
using System.IO;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketEntityInteract
    {
        static readonly int PacketInteract;
        static PacketEntityInteract()
        {
            PacketInteract = Network.RegisterPacketHandler(Receive);
        }

        internal static void EndInteraction(INetwork net, GameObject entity, bool success)
        {
            var server = net as Server;
            var w = server.OutgoingStreamTimestamped;
            w.Write(PacketInteract);
            w.Write(entity.RefID);
            w.Write(false);
            w.Write(success);
        }
        internal static void Send(INetwork net, GameObject entity, Interaction action, TargetArgs target)
        {
            var server = net as Server;
            var w = server.OutgoingStreamTimestamped;
            w.Write(PacketInteract);
            w.Write(entity.RefID);
            w.Write(true);
            target.Write(w);
            w.Write(action.GetType().FullName);
            action.Write(w);
            w.Write(entity.Global);
            w.Write(entity.Velocity);
            w.Write(entity.Direction);
        }
        internal static void Receive(INetwork net, BinaryReader r)
        {
            if (net is Server)
                throw new Exception();
            var entity = net.GetNetworkObject<Actor>(r.ReadInt32());
            var map = net.Map;
            if (!r.ReadBoolean())
            {
                entity.Work.End(r.ReadBoolean());
                return;
            }
            var target = TargetArgs.Read(net.Map, r);
            var action = Activator.CreateInstance(Type.GetType(r.ReadString())) as Interaction;
            action.Actor = entity;
            action.Target = target;
            action.Read(r);
            var global = r.ReadVector3();
            var velocity = r.ReadVector3();
            var dir = r.ReadVector3();
            action.Synced(net.Map);
            entity.Work.Perform(action, target);
        }
    }
}

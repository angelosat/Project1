using Project1.Framework.Base;
using Project1.Framework.Entities;
using Project1.Framework.Entities.Actors;
using Project1.Framework.Interactions;
using Start_a_Town_;
using System;

namespace Project1.Framework.Net.Packets
{
    [EnsureStaticCtorCall]
    static class PacketEntityInteract
    {
        static readonly int PacketInteract;
        static PacketEntityInteract()
        {
            PacketInteract = Registry.PacketHandlers.Register(Receive);
        }

        internal static void EndInteraction(NetEndpoint net, GameObject entity, bool success)
        {
            return; // let client finish interaction?
            var server = net as Server;
            var w = server.BeginPacket(PacketInteract);
            w.Write(entity.RefId);
            w.Write(false);
            w.Write(success);
        }
        internal static void Send(NetEndpoint net, GameObject entity, Interaction action, TargetArgs target, int count)
        {
            var server = net as Server;
            var w = server.BeginPacket(PacketInteract);
            w.Write(entity.RefId);
            w.Write(true);
            target.Write(w);
            w.Write(count);
            w.Write(action.Def);
            action.Write(w);
            w.Write(entity.Global);
            w.Write(entity.Velocity);
            w.Write(entity.Direction);
        }
        internal static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            if (net is Server)
                throw new Exception();
            var entity = net.World.GetEntity<Actor>(r.ReadInt32());
            var map = net.Map;
            if (!r.ReadBoolean())
            {
                entity.Work.End(r.ReadBoolean());
                return;
            }
            var target = TargetArgs.Read(net, r);
            var count = r.ReadInt32();
            var action = r.ReadDef<InteractionDef>().Create(entity, target);
            action.Count = count;
            action.Read(r);
            var global = r.ReadVector3();
            var velocity = r.ReadVector3();
            var dir = r.ReadVector3();
            action.Resolve(net.Map);
            entity.Work.Perform(action, target);
        }
    }
}

using System;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketForceTask
    {
        static readonly int PType;
        static PacketForceTask()
        {
            PType = Registry.PacketHandlers.Register(Receive);
        }
        internal static void Send(TaskGiver def, Actor actor, TargetArgs target)
        {
            var client = actor.Map.Net as Client;
            var w = client.GetOutgoingStreamOrderedReliable();
            w.Write(PType);
            w.Write(actor.RefId);
            w.Write(def.GetType().FullName);
            target.Write(w);
        }
        static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var actor = net.World.GetEntity(r.ReadInt32()) as Actor;
            var typeName = r.ReadString();
            var taskGiver = Activator.CreateInstance(Type.GetType(typeName)) as TaskGiver;
            var target = TargetArgs.Read(actor.World.Net, r);
            actor.ForceTask(taskGiver, target);
        }
    }
}

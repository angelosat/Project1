using System;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketForceTask
    {
        static readonly int PType;
        static PacketForceTask()
        {
            PType = Network.RegisterPacketHandler(Receive);
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
        static void Receive(INetEndpoint net, BinaryReader r)
        {
            var actor = net.GetNetworkEntity(r.ReadInt32()) as Actor;
            var typeName = r.ReadString();
            var taskGiver = Activator.CreateInstance(Type.GetType(typeName)) as TaskGiver;
            var target = TargetArgs.Read(net, r);
            actor.ForceTask(taskGiver, target);
        }
    }
}

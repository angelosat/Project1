using Project1.Framework;
using Project1.Framework.Serialization;
using Project1.Core.AI.Planners;
using Project1.Core.Base;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Net;

namespace Project1.Core.AI.Packets
{
    [EnsureStaticCtorCall]
    static class PacketForceTask
    {
        static readonly int PType;
        static PacketForceTask()
        {
            PType = Registry.PacketHandlers.Register(Receive);
        }
        internal static void Send(PlannerDef plannerDef, Actor actor, TargetArgs target)
        {
            var client = actor.Map.Net as Client;
            var w = client.GetOutgoingStreamOrderedReliable();
            w.Write(PType);
            w.Write(actor.RefId);
            w.Write(plannerDef);
            target.Write(w);
        }
        static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var actor = net.World.GetEntity(r.ReadInt32()) as Actor;
            var typeName = r.ReadString();
            var planner = r.ReadDef<PlannerDef>();
            var target = TargetArgs.Read(actor.World.Net, r);
            actor.ForceTask(planner, target);
        }
    }
}

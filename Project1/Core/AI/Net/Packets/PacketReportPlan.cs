using Project1.Core.AI.Behaviors;
using Project1.Core.Base;
using Project1.Core.Entities.Actors;
using Project1.Core.Net;
using System;

namespace Project1.Core.AI.Net.Packets
{
    [EnsureStaticCtorCall]
    public static class PacketReportPlan
    {
        static readonly int pReportPlan;
        static PacketReportPlan()
        {
            pReportPlan = Registry.PacketHandlers.Register(ReceiveReportBehavior);
        }
        
        private static void ReceiveReportBehavior(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var actor = endpoint.World.GetEntity<Actor>(r.ReadInt32());
            var hasBhav = r.ReadBoolean();
            if (hasBhav)
            {
                var plan = new Plan();
                plan.SyncFromServer(endpoint, r);
                var bhav = Activator.CreateInstance(plan.Def.BehaviorClass) as BehaviorExecutePlan;
                bhav.Plan = plan;
                actor.AI.State.TaskStack.Push(bhav);
            }
            else
            {
                actor.AI.State.TaskStack.Clear();
            }
        }
        public static void SendReportBehavior(Actor actor, BehaviorExecutePlan bhav)
        {
            var server = actor.Net as Server;
            var w = server.BeginPacket(pReportPlan);
            w.Write(actor.RefId);
            var hasBhav = bhav is not null;
            w.Write(hasBhav);
            if (hasBhav)
                bhav.Plan.SyncToClients(w);
        }
    }
}

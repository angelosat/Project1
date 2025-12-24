using Start_a_Town_.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    public static class PacketReportPlan
    {
        static readonly int pReportPlan;
        static PacketReportPlan()
        {
            pReportPlan = Registry.PacketHandlers.Register(ReceiveReportBehavior);
        }
        public static void SendReportBehavior(Actor actor, BehaviorExecutePlan bhav)
        {
            var server = actor.Net as Server;
            var w = server.BeginPacket(pReportPlan);
            w.Write(actor.RefId);
            var hasBhav = bhav is not null;
            w.Write(hasBhav);
            if (hasBhav)
            {
                bhav.Task.Def.Write(w);
                bhav.SyncToClients(w);
            }
        }
        private static void ReceiveReportBehavior(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var actor = endpoint.World.GetEntity<Actor>(r.ReadInt32());
            var hasBhav = r.ReadBoolean();
            if (hasBhav)
            {
                var taskDef = r.ReadDef<TaskDef>();
                var bhav = Activator.CreateInstance(taskDef.BehaviorClass) as BehaviorExecutePlan;
                bhav.SyncFromServer(client, r);
                actor.AI.State.TaskStack.Push(bhav);
            }
            else
            {
                actor.AI.State.TaskStack.Clear();
            }
        }
    }
}

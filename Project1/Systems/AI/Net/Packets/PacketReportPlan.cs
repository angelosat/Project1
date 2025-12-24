using Start_a_Town_.Net;
using System;

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
        //public static void SendReportBehavior(Actor actor, Plan plan)
        //{
        //    var server = actor.Net as Server;
        //    var w = server.BeginPacket(pReportPlan);
        //    w.Write(actor.RefId);
        //    var hasPlan = plan is not null;
        //    w.Write(hasPlan);
        //    if (hasPlan)
        //        plan.SyncToClients(w);
        //}
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
                //var taskDef = r.ReadDef<TaskDef>();
                var bhav = Activator.CreateInstance(plan.Def.BehaviorClass) as BehaviorExecutePlan;
                //bhav.SyncFromServer(client, r);
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
            {
                bhav.Plan.SyncToClients(w);
                //bhav.Task.Def.Write(w);
                //bhav.SyncToClients(w);
            }
        }
        //private static void ReceiveReportBehavior(NetEndpoint endpoint, Packet packet)
        //{
        //    var client = endpoint as Client;
        //    var r = packet.PacketReader;
        //    var actor = endpoint.World.GetEntity<Actor>(r.ReadInt32());
        //    var hasBhav = r.ReadBoolean();
        //    if (hasBhav)
        //    {
        //        var taskDef = r.ReadDef<TaskDef>();
        //        var bhav = Activator.CreateInstance(taskDef.BehaviorClass) as BehaviorExecutePlan;
        //        bhav.SyncFromServer(client, r);
        //        actor.AI.State.TaskStack.Push(bhav);
        //    }
        //    else
        //    {
        //        actor.AI.State.TaskStack.Clear();
        //    }
        //}
    }
}

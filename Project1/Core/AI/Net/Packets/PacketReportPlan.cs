using Project1.Core.AI.Behaviors.NodeTypes;
using Project1.Core.Entities.Actors;
using Project1.Core.Networking;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.Helpers;

namespace Project1.Core.AI.Net.Packets
{
    [EnsureStaticCtorCall]
    public static class PacketReportPlan
    {
        static readonly int _pSyncPlan;
        static PacketReportPlan()
        {
            Registry.MapEventHooksServer.Register<PlanAssignedEvent>(OnPlanAssigned);
            _pSyncPlan = Registry.PacketHandlers.Register(ReceiveSyncBehavior);
        }

        private static void OnPlanAssigned(PlanAssignedEvent e)
        {
            if (e.Actor.Net.IsClient)
                return;
            SyncBehavior(e.Actor, e.Behavior);
        }

        private static void ReceiveSyncBehavior(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var actor = endpoint.World.GetEntity<Actor>(r.ReadInt32());
            var hasBhav = r.ReadBoolean();
            if (hasBhav)
            {
                var plan = new Plan();
                plan.SyncFromServer(endpoint, r);
                var bhav = ActivatorSafe<Behavior>.CreateInstance(plan.Def.BehaviorClass);
                bhav.Plan = plan;
                actor.AI.State.TaskStack.Push(bhav);
            }
            else
            {
                actor.AI.State.TaskStack.Clear();
            }
        }
        static void SyncBehavior(Actor actor, Behavior bhav)
        {
            var server = actor.Net as Server;
            var w = server.BeginPacket(_pSyncPlan);
            w.Write(actor.RefId);
            var hasBhav = bhav is not null;
            w.Write(hasBhav);
            if (hasBhav)
                bhav.Plan.SyncToClients(w);
        }
    }
}

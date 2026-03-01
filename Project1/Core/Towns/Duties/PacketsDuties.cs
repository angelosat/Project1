using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Framework;
using System.Diagnostics;

namespace Project1.Core.Towns.Duties
{
    [EnsureStaticCtorCall]
    class PacketsDuties
    {
        static readonly int pToggle = Registry.PacketHandlers.Register(HandleLaborToggle);
        //static readonly int pMod = Registry.PacketHandlers.Register(HandleJobModRequest);
        static readonly int pPriority = Registry.PacketHandlers.Register(HandleAdjustPriority);

     

        static readonly int pSync = Registry.PacketHandlers.Register(HandleJobSync);

        static PacketsDuties()
        {
            Registry.PlayerInputEventHooks.Register<PlayerDutyToggleEvent>(OnPlayerDutyToggle);
            Registry.PlayerInputEventHooks.Register<PlayerDutyAdjustPriorityEvent>(OnPlayerDutyAdjustPriority);
        }

        private static void OnPlayerDutyAdjustPriority(PlayerDutyAdjustPriorityEvent e)
        {
            if(Ingame.Net.IsServer)
                Ingame.CurrentMap.Town.DutiesManager.ApplyPriorityDelta(e.Actor, e.Duty, e.Delta);
            SendAdjustPriority(e.Actor, e.Duty, e.Delta);
        }

        private static void OnPlayerDutyToggle(PlayerDutyToggleEvent e)
        {
            if(Ingame.Net.IsServer)
                Ingame.CurrentMap.Town.DutiesManager.Toggle(e.Actor, e.Duty);
            SendLaborToggle(e.Actor, e.Duty);
        }

        private static void HandleJobModRequest(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var server = net as Server;
            //var player = server.GetPlayer(r.ReadInt32());
            var actor = server.World.GetEntity(r.ReadInt32()) as Actor;
            //var jobDef = Def.GetDef<JobDef>(r.ReadString());
            var jobDef = r.ReadDef<DutyDef>();
            var job = actor.GetDuty(jobDef);
            job.Read(r);
            net.Events.Post(new DutyUpdatedEvent(actor, job.Def));
            SyncJob(actor, job);
        }
        public static void SendAdjustPriority(Actor actor, DutyDef job, int delta)
        {
            var net = actor.Net;
            //if (net is Server)
            //{
            //    job.Priority = (byte)priority;
                //net.Events.Post(new DutyUpdatedEvent(actor, job.Def));
                //SyncJob(actor, job);
            //}
            //else
            //{
                var w = net.BeginPacketImmediate(pPriority);
                //w.Write(player.ID);//, actor.RefId, job.Def.Name, priority);
                w.Write(actor.RefId);
                w.Write(job);
                w.Write(delta);
            //}
        }
        private static void HandleAdjustPriority(NetEndpoint endpoint, Packet packet)
        {
            var r = packet.PacketReader;
            var actor = endpoint.World.GetEntity(r.ReadInt32()) as Actor;
            var jobDef = r.ReadDef<DutyDef>();
            var job = actor.GetDuty(jobDef);
            var delta = r.ReadInt32();
            job.ApplyPriorityDelta(delta);
            if (endpoint is Server)
                SendAdjustPriority(actor, job.Def, delta);
        }
        //public static void SendPriorityModify(Actor actor, Duty job, int priority)
        //{
        //    var net = actor.Net;
        //    if (net is Server)
        //    {
        //        throw new UnreachableException();
        //        //job.Priority = (byte)priority;
        //        net.Events.Post(new DutyUpdatedEvent(actor, job.Def));
        //        //net.EventOccured((int)Components.Message.Types.JobUpdated, actor, job.Def);
        //        SyncJob(actor, job);
        //    }
        //    else
        //    {
        //        var w = net.BeginPacketImmediate(pMod);
        //        //w.Write(player.ID);//, actor.RefId, job.Def.Name, priority);
        //        w.Write(actor.RefId);
        //        w.Write(job.Def);
        //        w.Write(job);
        //    }
        //}
        public static void SendLaborToggle(Actor actor, DutyDef jobDef)
        {
            var net = actor.Net;
            var w = net.BeginPacketImmediate(pToggle);
            //w.Write(player.ID);
            w.Write(actor.RefId);
            w.Write(jobDef);
        }
        private static void HandleLaborToggle(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            //var player = net.GetPlayer(r.ReadInt32());
            var actor = net.World.GetEntity(r.ReadInt32()) as Actor;
            //var jobDef = Def.GetDef<JobDef>(r);
            var jobDef = r.ReadDef<DutyDef>();
            //actor.ToggleJob(jobDef);
            //net.Events.Post(new DutyUpdatedEvent(actor, jobDef));
            net.Map.Town.DutiesManager.Toggle(actor, jobDef);
            if (net is Server)
                SendLaborToggle(actor, jobDef);
        }

        public static void SyncJob(Actor actor, Duty job)
        {
            var net = actor.Net as Server;
            var w = net.BeginPacketImmediate(pSync);
            //w.Write(player.ID);
            w.Write(actor.RefId);
            w.Write(job.Def.Name);
            job.Write(w);
        }
        private static void HandleJobSync(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var client = net as Client;
            //var player = client.GetPlayer(r.ReadInt32());
            var actor = client.World.GetEntity(r.ReadInt32()) as Actor;
            var jobDef = Def.GetDef<DutyDef>(r.ReadString());
            var job = actor.GetDuty(jobDef);
            job.Read(r);
            net.Events.Post(new DutyUpdatedEvent(actor, jobDef));
        }
    }
}

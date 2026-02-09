using Project1.Framework;
using Project1.Core.Base;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Helpers.Structs;
using Project1.Core.Net;

namespace Project1.Core.Interactions
{
    [EnsureStaticCtorCall]
    internal static class PacketsInteractions
    {
        static readonly PacketId _pStop, _pProgress, _pStarted, _pSpeed, _pFinished;
        static PacketsInteractions()
        {
            _pStop = Registry.PacketHandlers.Register(OnInteractionStopped);
            _pProgress = Registry.PacketHandlers.Register(OnInteractionProgress);
            _pStarted = Registry.PacketHandlers.Register(OnInteractionStarted);
            _pFinished = Registry.PacketHandlers.Register(OnInteractionFinished);
            _pSpeed = Registry.PacketHandlers.Register(OnInteractionSpeed);
            Registry.MapEventHooksServer.Register<InteractionStoppedEvent>(SendInteractionStopped);
            Registry.MapEventHooksServer.Register<InteractionProgressEvent>(SendInteractionProgress);
            Registry.MapEventHooksServer.Register<InteractionNextSwingSpeedEvent>(SendInteractionNextSwingSpeed);
            Registry.MapEventHooksServer.Register<InteractionStartedEvent>(SendInteractionStarted);
            Registry.MapEventHooksServer.Register<InteractionFinishedEvent>(SendInteractionFinished);
        }
        private static void SendInteractionFinished(InteractionFinishedEvent e)
        {
            Server.Instance.BeginPacket(_pFinished)
                .Write(e.Actor.RefId);
        }
        private static void SendInteractionStarted(InteractionStartedEvent e)
        {
            Server.Instance.BeginPacket(_pStarted)
                .Write(e.Actor.RefId)
                .Write(e.InteractionDef)
                .Write(e.Target);
        }
        private static void OnInteractionStarted(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var actor = client.World.GetEntity<Actor>(r.ReadInt32());
            var interaction = r.ReadDef<InteractionDef>();
            var target = r.ReadTarget(client.Map);
            actor.Work.Perform(interaction, target);
        }
        private static void OnInteractionFinished(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var actor = client.World.GetEntity<Actor>(r.ReadInt32());
            actor.Work.Task.Finish();
        }
        private static void SendInteractionNextSwingSpeed(InteractionNextSwingSpeedEvent e)
        {
            Server.Instance.BeginPacket(_pSpeed)
               .Write(e.Actor.RefId)
               .Write(e.Speed);
        }
        private static void SendInteractionProgress(InteractionProgressEvent e)
        {
            Server.Instance.BeginPacket(_pProgress)
               .Write(e.Actor.RefId)
               .Write(e.WorkAmount);
        }
        private static void OnInteractionProgress(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var actor = client.World.GetEntity<Actor>(r.ReadInt32());
            var progress = r.ReadInt32();
            actor.Work.Task.AddProgress(progress);
        }
        private static void OnInteractionSpeed(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var actor = client.World.GetEntity<Actor>(r.ReadInt32());
            var speed = r.ReadSingle();
            actor.Work.Task.SetNextSwingSpeed(speed);
        }
        private static void SendInteractionStopped(InteractionStoppedEvent e)
        {
            Server.Instance.BeginPacket(_pStop)
                .Write(e.Actor.RefId);
        }

        private static void OnInteractionStopped(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var actor = client.World.GetEntity<Actor>(r.ReadInt32());
            actor.Work.Stop();
        }
    }


}

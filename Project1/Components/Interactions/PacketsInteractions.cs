using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal static class PacketsInteractions
    {
        static readonly PacketId _pStop, _pProgress, _pStarted;
        static PacketsInteractions()
        {
            _pStop = Registry.PacketHandlers.Register(OnInteractionStopped);
            _pProgress = Registry.PacketHandlers.Register(OnInteractionProgress);
            _pStarted = Registry.PacketHandlers.Register(OnInteractionStarted);

            Registry.MapEventHooksServer.Register<InteractionStoppedEvent>(SendInteractionStopped);
            Registry.MapEventHooksServer.Register<InteractionProgressEvent>(SendInteractionProgress);
            Registry.MapEventHooksServer.Register<InteractionStartedEvent>(SendInteractionStarted);
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
            actor.Work.Task.AddProgress(r.ReadInt32());
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

    internal sealed record InteractionProgressEvent(Actor Actor, int WorkAmount) : IEventPayload { }
    internal sealed record InteractionStartedEvent(Actor Actor, InteractionDef InteractionDef, TargetArgs Target) : IEventPayload { }
}

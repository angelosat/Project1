using Start_a_Town_.Net;
using System;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal static class PacketsInteractions
    {
        static readonly PacketId _pStop, _pProgress;
        static PacketsInteractions()
        {
            _pStop = Registry.PacketHandlers.Register(OnInteractionStopped);
            _pProgress = Registry.PacketHandlers.Register(OnInteractionProgress);

            Registry.MapEventHooks.Register<InteractionStoppedEvent>(SendInteractionStopped);
            Registry.MapEventHooks.Register<InteractionProgressEvent>(SendInteractionProgress);
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

    public readonly struct PacketId(int value)
    {
        public readonly int Value = value;
        public static implicit operator PacketId(int v) => new(v);
        public static implicit operator int(PacketId v) => v.Value;
    }
    public readonly struct EntityRefId(int value)
    {
        internal static readonly EntityRefId Null = new(0);
        public readonly int Value = value;
        public static implicit operator EntityRefId(int v) => new(v);
        public static implicit operator int(EntityRefId v) => v.Value;
        public override string ToString() => $"{nameof(EntityRefId)}: {this.Value}";
    }
    public readonly struct SlotIndex(int value)
    {
        internal static readonly SlotIndex Null = new(-1);
        public readonly int Value = value;
        public static implicit operator SlotIndex(int v) => new(v);
        public static implicit operator int(SlotIndex v) => v.Value;
        public override string ToString() => $"{nameof(SlotIndex)}: {this.Value}";
    }

    internal class InteractionProgressEvent : EventPayloadBase
    {
        public readonly Actor Actor;
        public readonly int WorkAmount;

        public InteractionProgressEvent(Actor actor, int workAmount)
        {
            this.Actor = actor;
            this.WorkAmount = workAmount;
        }
    }
}

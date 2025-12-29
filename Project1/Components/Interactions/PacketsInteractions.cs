using Start_a_Town_.Net;
using System;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal static class PacketsInteractions
    {
        static readonly PacketId _pStop;
        static PacketsInteractions()
        {
            _pStop = Registry.PacketHandlers.Register(OnInteractionStopped);

            Registry.MapEventHooks.Register<InteractionStoppedEvent>(SendInteractionStopped);
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
}

using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Framework;

namespace Project1.Core.Effects
{
    [EnsureStaticCtorCall]
    internal static class EffectPackets
    {
        internal static readonly PacketId _pEffectApplied = Registry.PacketHandlers.Register(ReceiveEffectApplied);
        internal static readonly PacketId _pEffectAborted = Registry.PacketHandlers.Register(ReceiveEffectAborted);

        static EffectPackets()
        {
            Registry.WorldEventHooksServer.Register<ActorEffectAppliedEvent>(HandleActorEffectApplied);
            Registry.WorldEventHooksServer.Register<ActorEffectAbortedEvent>(HandleActorEffectAborted);
        }

        private static void HandleActorEffectApplied(ActorEffectAppliedEvent e)
        {
            SendEffectApplied(e.Actor.Net as Server, e.Actor, e.Effect);
        }

        private static void SendEffectApplied(NetEndpoint endpoint, Actor actor, EntityEffectWrapper effect)
        {
            endpoint.BeginPacket(_pEffectApplied)
                .Write(actor.RefId)
                .Write(effect.Def)
                .Write(effect.Target)
                .Write(effect.Budget.HasValue)
                .Write(effect.Budget ?? 0)
                .Write(effect.TicksPerUnit);
        }

        private static void ReceiveEffectApplied(NetEndpoint endpoint, Packet packet)
        {
            var r = packet.PacketReader;
            var actor = endpoint.World.GetEntity<Actor>(r.ReadEntityRefId());
            var effectdef = r.ReadDef<EffectDef>();
            var targetdef = r.ReadDef();
            var hasBudget = r.ReadBoolean();
            var budget = r.ReadInt32();
            var ticksPerUnit = r.ReadInt32();
            var effect = new EntityEffectWrapper(effectdef, targetdef, hasBudget ? budget : null, ticksPerUnit);
            actor.Effects.Apply(effect);
        }

        private static void HandleActorEffectAborted(ActorEffectAbortedEvent e)
        {
            SendEffectAborted(e.Actor.Net as Server, e.Actor, e.Effect);
        }

        private static void SendEffectAborted(NetEndpoint endpoint, Actor actor, EntityEffectWrapper effect)
        {
            endpoint.BeginPacket(_pEffectAborted)
                .Write(actor.RefId)
                .Write(effect.Def)
                .Write(effect.Target);
        }

        private static void ReceiveEffectAborted(NetEndpoint endpoint, Packet packet)
        {
            var r = packet.PacketReader;
            var actor = endpoint.World.GetEntity<Actor>(r.ReadEntityRefId());
            var effectdef = r.ReadDef<EffectDef>();
            var targetdef = r.ReadDef();
            actor.Effects.Abort(effectdef, targetdef);
        }
    }
}

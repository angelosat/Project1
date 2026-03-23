using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Framework;

namespace Project1.Core.Animations
{
    [EnsureStaticCtorCall]
    internal static class AnimationPackets
    {
        internal static readonly PacketId _pBoneToggled = Registry.PacketHandlers.Register(OnBoneToggled);
        internal static readonly PacketId _pRestingFrameOverride = Registry.PacketHandlers.Register(OnRestingFrameOverride);
        static AnimationPackets()
        {
            Registry.MapEventHooksServer.Register<ActorBoneToggledEvent>(HandleActorBoneToggled);
            Registry.MapEventHooksServer.Register<ActorRestingFrameOverridenEvent>(HandleRestingFrameOverriden);
        }

        private static void HandleRestingFrameOverriden(ActorRestingFrameOverridenEvent e)
        {
            SendRestingFrameOverride(e.Actor.Net, e.Actor, e.Bone, e.KeyFrame);
        }

        private static void SendRestingFrameOverride(NetEndpoint endpoint, Actor actor, BoneDef def, Keyframe kf)
        {
            endpoint.BeginPacket(_pRestingFrameOverride)
                .Write(actor.RefId)
                .Write(def)
                .Write(kf);
        }

        private static void OnRestingFrameOverride(NetEndpoint endpoint, Packet packet)
        {
            var r = packet.PacketReader;
            var actor = endpoint.World.Get<Actor>(r.ReadEntityRefId());
            var bonedef = r.ReadDef<BoneDef>();
            var kf = Keyframe.Create(r);
            actor.SpriteComp.OverrideRestingFrame(bonedef, kf);
        }

        private static void HandleActorBoneToggled(ActorBoneToggledEvent e)
        {
            SendActorBoneToggled(e.Actor.Net, e.Actor, e.Bone, e.Toggle, e.Cascade);
        }

        private static void SendActorBoneToggled(NetEndpoint endpoint, Actor actor, BoneDef def, bool toggle, bool cascade)
        {
            endpoint.BeginPacket(_pBoneToggled)
                .Write(actor.RefId)
                .Write(def)
                .Write(toggle)
                .Write(cascade);
        }

        private static void OnBoneToggled(NetEndpoint endpoint, Packet packet)
        {
            var r = packet.PacketReader;
            var actor = endpoint.World.Get<Actor>(r.ReadEntityRefId());
            var bonedef = r.ReadDef<BoneDef>();
            var toggle = r.ReadBoolean();
            var cascade = r.ReadBoolean();
            actor.SpriteComp.ToggleBone(bonedef, toggle, cascade);
        }
    }
}

using Project1.Core.Entities.Actors;
using Project1.Framework.Events;

namespace Project1.Core.Animations
{
    internal record struct ActorBoneToggledEvent(Actor Actor, BoneDef Bone, bool Toggle, bool Cascade) : IEventPayload { }
    internal record struct ActorRestingFrameOverridenEvent(Actor Actor, BoneDef Bone, Keyframe KeyFrame) : IEventPayload { }
}

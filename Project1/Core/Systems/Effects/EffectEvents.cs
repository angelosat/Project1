using Project1.Core.Entities.Actors;
using Project1.Core.Events;
using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Framework.Events;
using Project1.Framework.Serialization;

namespace Project1.Core.Systems.Effects
{
    public record struct ActorEffectAppliedEvent(Actor Actor, EntityEffectWrapper Effect) : IEventPayload { }
    public record struct ActorEffectAbortedEvent(Actor Actor, EntityEffectWrapper Effect) : IEventPayload { }

    [NetworkReplicate]
    public record struct ActorEffectRemovedEvent(StaticWorld World,
                                                 EntityRefId ActorId,
                                                 EffectDef EffectDef)
        : IWorldEvent, ISerializableNewNew<ActorEffectRemovedEvent>
    {
        public static ActorEffectRemovedEvent Create(IDataReader r)
            => new()
            {
                ActorId = r.ReadEntityRefId(),
                EffectDef = r.ReadDef<EffectDef>()
            };

        public readonly IDataWriter Write(IDataWriter w)
            => w.Write(this.ActorId)
                .Write(this.EffectDef);
    }

    [NetworkReplicate]
    public record struct ActorEffectRemovedIndexedEvent(StaticWorld World,
                                                        EntityRefId ActorId,
                                                        int EffectId)
        : IWorldEvent, ISerializableNewNew<ActorEffectRemovedIndexedEvent>
    {
        public static ActorEffectRemovedIndexedEvent Create(IDataReader r)
            => new()
            {
                ActorId = r.ReadEntityRefId(),
                EffectId = r.ReadInt32()
            };

        public readonly IDataWriter Write(IDataWriter w)
            => w.Write(this.ActorId)
                .Write(this.EffectId);
    }

    [NetworkReplicate]
    public record struct ActorEffectAppliedNewEvent(StaticWorld World,
                                                    EntityRefId ActorId,
                                                    EffectDef EffectDef,
                                                    Def OptionalTarget = null)
       : IWorldEvent, ISerializableNewNew<ActorEffectAppliedNewEvent>
    {
        public static ActorEffectAppliedNewEvent Create(IDataReader r)
            => new()
            {
                ActorId = r.ReadEntityRefId(),
                EffectDef = r.ReadDef<EffectDef>(),
                OptionalTarget = r.ReadDef<Def>(),
            };

        public readonly IDataWriter Write(IDataWriter w)
            => w.Write(this.ActorId)
                .Write(this.EffectDef)
                .Write(this.OptionalTarget);
    }
}

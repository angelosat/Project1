using Project1.Core.Entities.Actors;
using Project1.Core.Events;
using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Framework.Events;
using Project1.Framework.Serialization;
using System;

namespace Project1.Core.Systems.Effects
{
    //public record struct ActorEffectAppliedEvent(Actor Actor, EntityEffectWrapper Effect) : IEventPayload { }
    public record struct ActorEffectAbortedEvent(Actor Actor, EntityEffectRuntime Effect) : IEventPayload { }

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

    //[NetworkReplicate]
    //public record struct ActorEffectAppliedNewNewEvent(StaticWorld World,
    //                                                EntityRefId ActorId,
    //                                                EffectData Data,
    //                                                EffectSchedule Schedule,
    //                                                EffectMetaData MetaData = default)
    //   : IWorldEvent, ISerializableNewNew<ActorEffectAppliedNewNewEvent>
    //{
    //    public static ActorEffectAppliedNewNewEvent Create(IDataReader r)
    //        => new()
    //        {
    //            ActorId = r.ReadEntityRefId(),
    //            Data = (EffectData)((EffectData)Activator.CreateInstance(r.ReadDef<EffectDef>().DataClass)).Read(r),
    //            Schedule = EffectSchedule.Create(r),
    //            MetaData = EffectMetaData.Create(r),
    //        };

    //    public readonly IDataWriter Write(IDataWriter w)
    //        => w.Write(this.ActorId)
    //            .Write(this.Data.Def)
    //            .Write(this.Data)
    //            .Write(this.Schedule)
    //            .Write(this.MetaData);
    //}
    [NetworkReplicate]
    public record struct ActorEffectAppliedNewNewEvent(StaticWorld World,
                                                    EntityRefId ActorId,
                                                    EffectConfig Config,
                                                    EffectSchedule Schedule,
                                                    EffectMetaData MetaData = default)
       : IWorldEvent, ISerializableNewNew<ActorEffectAppliedNewNewEvent>
    {
        public static ActorEffectAppliedNewNewEvent Create(IDataReader r)
            => new()
            {
                ActorId = r.ReadEntityRefId(),
                Config = EffectConfig.Create(r),
                Schedule = EffectSchedule.Create(r),
                MetaData = EffectMetaData.Create(r),
            };

        public readonly IDataWriter Write(IDataWriter w)
            => w.Write(this.ActorId)
                //.Write(this.Data.Def)
                //.Write(this.Data)
                .Write(this.Config)
                .Write(this.Schedule)
                .Write(this.MetaData);
    }
    //[NetworkReplicate]
    //public record struct ActorEffectAppliedNewNewΝεςEvent(StaticWorld World,
    //                                                EntityRefId ActorId,
    //                                                EffectConfig Config,
    //                                                EffectMetaData MetaData = default)
    //   : IWorldEvent, ISerializableNewNew<ActorEffectAppliedNewNewEvent>
    //{
    //    public static ActorEffectAppliedNewNewEvent Create(IDataReader r)
    //        => new()
    //        {
    //            ActorId = r.ReadEntityRefId(),
    //            Data = (EffectData)((EffectData)Activator.CreateInstance(r.ReadDef<EffectDef>().DataClass)).Read(r),
    //            Schedule = EffectSchedule.Create(r),
    //            MetaData = EffectMetaData.Create(r),
    //        };

    //    public readonly IDataWriter Write(IDataWriter w)
    //        => w.Write(this.ActorId)
    //            .Write(this.Data.Def)
    //            .Write(this.Data)
    //            .Write(this.Schedule)
    //            .Write(this.MetaData);
    //}
}

using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Core.Systems.Magic;
using Project1.Framework;
using Project1.Framework.Serialization;

#nullable enable

namespace Project1.Core.Systems.Effects;

[EnsureStaticCtorCall]
static class EffectsSystem
{
    static EffectsSystem()
    {
        //Registry.WorldEventHooksClient.Register<ActorEffectAppliedNewEvent>(OnActorEffectApplied);
        //Registry.WorldEventHooksServer.Register<ActorEffectAppliedNewEvent>(OnActorEffectApplied);

        Registry.WorldEventHooksClient.Register<ActorEffectAppliedNewNewEvent>(OnActorEffectAppliedNew);
        Registry.WorldEventHooksServer.Register<ActorEffectAppliedNewNewEvent>(OnActorEffectAppliedNew);

        Registry.WorldEventHooksClient.Register<ActorEffectRemovedEvent>(OnActorEffectRemoved);
        Registry.WorldEventHooksServer.Register<ActorEffectRemovedEvent>(OnActorEffectRemoved);

        Registry.WorldEventHooksClient.Register<ActorEffectRemovedIndexedEvent>(OnActorEffectIndexedRemoved);
        Registry.WorldEventHooksServer.Register<ActorEffectRemovedIndexedEvent>(OnActorEffectIndexedRemoved);
    }

    private static void OnActorEffectAppliedNew(ActorEffectAppliedNewNewEvent e)
    {
        var actor = e.World.Resolve<Actor>(e.ActorId);
        var data = e.Data;
        var schedule = e.Schedule;
        if (schedule.IsInstant)
        {
            data.Perform(actor);
            return;
        }

        var runtime = new EntityEffectRuntime
        {
            Dto = data,
            TickInterval = schedule.TickInterval,
            TicksRemaining = schedule.TickCount
        };

        actor.Effects.Apply(runtime);
    }

    private static void OnActorEffectIndexedRemoved(ActorEffectRemovedIndexedEvent e)
    {
        var world = e.World;
        var actor = world.Resolve<Actor>(e.ActorId);
        actor.Effects.RemoveAt(e.EffectId);
    }

    private static void OnActorEffectRemoved(ActorEffectRemovedEvent e)
    {
        var world = e.World;
        var actor = world.Resolve<Actor>(e.ActorId);
        var effectDef = e.EffectDef;
        actor.Effects.RemoveInternal(effectDef);
    }
}

public readonly record struct EffectSchedule : ISerializableNewNew<EffectSchedule>
{
    public bool IsInstant { get; init; }
    public SimulationDuration TickInterval { get; init; }
    public int TickCount { get; init; }

    public static EffectSchedule Instant => new()
    {
        IsInstant = true
    };

    public static EffectSchedule Timed(
        SimulationDuration tickInterval,
        int tickCount) => new()
        {
            TickInterval = tickInterval,
            TickCount = tickCount
        };

    public static EffectSchedule Permanent(
      SimulationDuration tickInterval,
      int tickCount) => new()
      {
          TickInterval = tickInterval,
          TickCount = tickCount
      };

    public IDataWriter Write(IDataWriter w)
        => w.Write(this.IsInstant)
            .Write(this.TickInterval)
            .Write(this.TickCount);

    public static EffectSchedule Create(IDataReader r)
        => new()
        {
            IsInstant = r.ReadBoolean(),
            TickInterval = r.ReadInt32(),
            TickCount = r.ReadInt32()
        };
}

public readonly record struct EffectMetaData : ISerializableNewNew<EffectMetaData>
{
    public SpellSchoolDef School { get; init; }

    public static EffectMetaData Create(IDataReader r)
        => new()
        {
            School = r.ReadDef<SpellSchoolDef>()
        };

    public IDataWriter Write(IDataWriter w)
        => w.Write(this.School);
}

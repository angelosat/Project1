using Project1.Core.Simulation;
using Project1.Framework.Serialization;

#nullable enable

namespace Project1.Core.Systems.Effects;

public readonly record struct EffectSchedule : ISerializableNewNew<EffectSchedule>
{
    public const int IndefiniteTickCount = -1;
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
        SimulationDuration tickInterval) => new()
        {
            TickInterval = tickInterval,
            TickCount = IndefiniteTickCount
        };

    public IDataWriter Write(IDataWriter w)
        => w.Write(IsInstant)
            .Write(TickInterval)
            .Write(TickCount);

    public static EffectSchedule Create(IDataReader r)
        => new()
        {
            IsInstant = r.ReadBoolean(),
            TickInterval = r.ReadInt32(),
            TickCount = r.ReadInt32()
        };
}

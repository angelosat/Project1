using System;

namespace Project1.Core.Simulation;

//public record struct SimulationTick(double Value)
//{
//    public static readonly SimulationTick Immediate = new() { Value = -1 };
//    public static implicit operator double(SimulationTick t) => t.Value;
//    public static implicit operator SimulationTick(double i) => new() { Value = i };
//}
public record struct SimulationTick(ulong Value)
{
    public static implicit operator ulong(SimulationTick t) => t.Value;
    public static implicit operator SimulationTick(ulong i) => new() { Value = i };

    public static SimulationDuration operator -(SimulationTick a, SimulationTick b)
    {
        if (a.Value < b.Value)
            //throw new ArgumentOutOfRangeException(nameof(b), "Cannot calculate a negative simulation duration.");
            ArgumentOutOfRangeException.ThrowIfNegative(a.Value - b.Value, "Cannot calculate a negative simulation duration.");

        return new(checked((int)(a.Value - b.Value)));
    }

    public static SimulationTick operator +(SimulationTick tick, SimulationDuration duration)
    {
        if (duration.Value < 0)
            ArgumentOutOfRangeException.ThrowIfNegative(duration.Value);
        return new(checked(tick.Value + (ulong)duration.Value));
    }
}

public record struct SimulationDuration(int Value)
{
    public static SimulationDuration Infinite => new(-1);
    public readonly bool IsInfinite => this.Value == -1;
    public static implicit operator int(SimulationDuration d) => d.Value;
    public static implicit operator SimulationDuration(int i) => new() { Value = i };
}

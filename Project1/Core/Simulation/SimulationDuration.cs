using System;
using System.Collections.Generic;

namespace Project1.Core.Simulation;

public record struct SimulationDuration(int Value)
{
    public static SimulationDuration Infinite => new(-1);
    public readonly bool IsInfinite => Value == -1;
    public static implicit operator int(SimulationDuration d) => d.Value;
    public static implicit operator SimulationDuration(int i) => new() { Value = i };

    public readonly TimeSpan ToTimespan()
        => TimeSpan.FromSeconds(Ticks.PerGameSecond * this.Value);

    public string Format()
    {
        List<string> final = new(3);
        var span = this.ToTimespan();

        //if (span.Days > 0) final.Add($"{span.Days} d");
        //if (span.Hours > 0) final.Add($"{span.Hours} h");
        //if (span.Minutes > 0) final.Add($"{span.Minutes} m");
        //if (span.Seconds > 0) final.Add($"{span.Seconds} s");

        if (span.Days > 0) final.Add($"{span.Hours / 24} d");
        if (span.Hours > 0) final.Add($"{span.Hours} m");
        if (span.Minutes > 0) final.Add($"{span.Minutes} s");
        //if (span.Seconds > 0) final.Add($"{span.Seconds} s");

        return string.Join(" ", final);
    }
}

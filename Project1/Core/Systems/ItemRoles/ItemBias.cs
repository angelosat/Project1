using Project1.Core.Entities;
using Project1.Core.Helpers;
using System;

namespace Project1.Core.Systems.ItemRoles;

internal class ItemBias(Entity entity, int value)
{
    public readonly int EntityID = entity.RefId;
    public int Value = value;
    readonly Accumulator Accumulator = new();
    public float DecayTicksPerUnit = 1 / Ticks.FromSeconds(10);

    public int Tick()
    {
        if (this.Value == 0)
            return 0;

        this.Accumulator.Add(this.DecayTicksPerUnit);
        var delta = this.Accumulator.Flush();

        if (Math.Abs(delta) >= Math.Abs(this.Value))
            this.Value = 0;
        else
            this.Value -= Math.Sign(this.Value) * delta;
        return this.Value;
    }
}

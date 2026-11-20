using Project1.Core.Entities;
using Project1.Framework.Events;
using System;

namespace Project1.Core.Systems.Stats;

internal sealed class TrackingStat(TrackingStatDef def, float initialValue = 0) : IUpdatable
{
    internal readonly TrackingStatDef Def = def;
    readonly ChangeNotifier Notifier = new();
    internal float Value
    {
        get;
        set
        {
            var oldValue = field;
            field = value;
            if (oldValue != value)
                this.Notifier.Notify();
        }
    } = initialValue;

    public IDisposable Subscribe(Action handler)
        => this.Notifier.Subscribe(handler);
}

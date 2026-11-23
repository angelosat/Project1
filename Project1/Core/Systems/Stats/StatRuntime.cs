using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Core.Stats;
using Project1.Framework;
using Project1.Framework.Events;
using System;

namespace Project1.Core.Systems.Stats;

public sealed class StatRuntime(StatDef def)
{
    public StatRuntime(StatDef def, float value) : this(def)
    {
        this.Value = value;
    }

    public readonly ChangeNotifier Notifier = new();

    public StatDef Def = def;

    public bool Dirty = true;

    public float Value
    {
        get;
        set
        {
            field = value;
            this.Notifier.Notify();
        }
    }

    public float Mod
    {
        get;
        set
        {
            field = value;
            //this.Notifier.Notify();
        }
    }

    public string Format()
        => StatSystem.Format(this.Def, this.Value);

    public override string ToString()
        => this.Format();

    internal void MarkDirty()
    {
        this.Dirty = true;
        this.Notifier.Notify();
    }
}

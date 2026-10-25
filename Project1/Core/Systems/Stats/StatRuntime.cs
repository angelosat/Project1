using Project1.Core.Entities.Actors;
using Project1.Core.Entities.Stats;
using Project1.Framework.Events;

namespace Project1.Core.Systems.Stats;

public sealed class StatRuntime(StatDef def)
{
    public StatRuntime(StatDef def, float value) : this(def)
    {
        this.Value = value;
    }
    public ChangeNotifier Notifier = new();
    public StatDef Def = def;
    public float Value;
    public float Bonus;
    public bool Dirty = true;

    public float Resolve(Actor actor)
    {
        if (this.Dirty)
        {
            this.Dirty = false;
            this.Value = this.Def.BaseValue + this.Def.Worker.CalculateActorStat(this.Def, actor);
        }
        return this.Value;
    }

    public string Format()
        => StatSystem.Format(this.Def, this.Value);

    public string ResolveAndFormat(Actor actor)
      => StatSystem.Format(this.Def, this.Resolve(actor));

    public override string ToString()
        => this.Format();

    internal void MarkDirty()
    {
        this.Dirty = true;
        this.Notifier.Notify();
    }
}

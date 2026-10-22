using Project1.Core.Entities.Stats;

namespace Project1.Core.Systems.Stats;

public sealed class StatRuntime(StatDef def)
{
    public StatRuntime(StatDef def, float value) : this(def)
    {
        this.Value = value;
    }
    public StatDef Def = def;
    public float Value;
    public float Bonus;
    public bool Dirty;
}

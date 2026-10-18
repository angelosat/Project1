using Project1.Core.Entities.Stats;

namespace Project1.Core.Systems.Stats;

internal class StatRuntime(StatDef def)
{
    public StatDef Def = def;
    public float Value;
    public bool Dirty;
}

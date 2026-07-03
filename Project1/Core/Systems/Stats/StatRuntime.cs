using Project1.Core.Entities.Stats;

namespace Project1.Core.Systems.Stats;

internal class StatRuntime
{
    public StatDef Def;
    public float Value;
    public bool Dirty;

    public StatRuntime(StatDef def)
    {
        this.Def = def;
    }
}

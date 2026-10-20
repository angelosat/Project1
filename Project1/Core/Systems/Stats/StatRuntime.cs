using Project1.Core.Entities.Stats;

namespace Project1.Core.Systems.Stats;

public class StatRuntime(StatDef def)
{
    public StatDef Def = def;
    public float Value;
    public float Bonus;
    public bool Dirty;
}

using Project1.Framework.Base;
using Project1.Framework.Needs;

namespace Start_a_Town_.Components
{
    public abstract class ConsumableEffect
    {
        EffectDef Def;
        Def Tag;
        float Delta;
        public abstract void Apply(GameObject actor);
    }
}

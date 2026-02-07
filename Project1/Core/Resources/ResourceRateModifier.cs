using Project1.Core.Entities;

namespace Project1.Core.Resources
{
    public class ResourceRateModifier
    {
        public ResourceRateModifierDef Def;
        int TicksRemaining;
        public ResourceRateModifier(ResourceRateModifierDef def)
        {
            this.Def = def;
            this.TicksRemaining = def.BaseDurationInTicks;
        }
        public bool Tick(GameObject parent)
        {
            if (this.Def.Type == ResourceRateModifierDef.Types.Permanent)
                return true;
            this.TicksRemaining--;
            if (this.TicksRemaining <= 0)
                return false;
            return true;
        }
    }
}

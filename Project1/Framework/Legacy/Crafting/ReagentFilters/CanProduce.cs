using Project1.Core;
using Project1.Core.Entities;

namespace Project1.Core.Legacy.Crafting.ReagentFilters
{
    class CanProduce : Reaction.Reagent.ReagentFilter
    {
        public override string Name => "Can Produce";
        Reaction.Product.Types Type;
        public CanProduce(Reaction.Product.Types type)
        {
            this.Type = type;
        }

        public override bool Condition(Entity obj)
        {
            return obj?.Def?.CanProcessInto.Contains(this.Type) ?? false;
        }

        public override string ToString()
        {
            return Name + ": " + this.Type.ToString();
        }
    }
}

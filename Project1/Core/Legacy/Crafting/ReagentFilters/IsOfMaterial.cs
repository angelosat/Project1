using Project1.Core;
using Project1.Core.Entities;
using Project1.Core.Systems.Materials;

namespace Project1.Core.Legacy.Crafting.ReagentFilters
{
    class IsOfMaterial : Reaction.Reagent.ReagentFilter
    {
        MaterialDef Material;
        public override string Name => "Is of material";
        
        public IsOfMaterial(MaterialDef material)
        {
            this.Material = material;
        }
        public override bool Condition(Entity obj)
        {
            return obj.Body.Material == this.Material;
        }
        public override string ToString()
        {
            return Name + ": " + this.Material.ToString();
        }
    }
}

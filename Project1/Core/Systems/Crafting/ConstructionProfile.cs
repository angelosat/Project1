using Project1.Core.Construction;
using Project1.Core.Systems.Materials;
using Project1.Framework;

namespace Project1.Core.Systems.Crafting
{
    public class ConstructionProfile(ConstructionCategoryDef category, MaterialRefinementDef[] refinements) : Inspectable
    {
        public readonly MaterialRefinementDef[] Refinements = refinements;
        public readonly ConstructionCategoryDef Category = category;
        public MaterialDef[] Materials { get; init; }
        public bool IsDeconstructible { get; init; } = true;
        public int Dimension = 1;
    }
}

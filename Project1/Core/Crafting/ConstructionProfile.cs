using Project1.Core.Materials;
using Project1.Framework;

namespace Project1.Core.Crafting
{
    public class ConstructionProfile(MaterialRefinementDef[] refinements) : Inspectable
    {
        public readonly MaterialRefinementDef[] Refinements = refinements;
        public MaterialDef[] Materials { get; init; }
        public bool IsDeconstructible { get; init; } = true;
        public int Dimension = 1;
    }
}

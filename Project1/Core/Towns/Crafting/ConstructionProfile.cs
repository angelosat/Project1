using Project1.Core.Base;
using Project1.Core.Materials;

namespace Project1.Core.Towns.Crafting
{
    public class ConstructionProfile(MaterialRefinementDef[] refinements) : Inspectable
    {
        public readonly MaterialRefinementDef[] Refinements = refinements;
        public int Dimension = 1;
    }
}

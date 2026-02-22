using Project1.Core.Materials;
using Project1.Framework;
using System.Collections.Generic;

namespace Project1.Core.Crafting
{
    public class ConstructionProfile(MaterialRefinementDef[] refinements) : Inspectable
    {
        public readonly MaterialRefinementDef[] Refinements = refinements;
        public MaterialDef[] Materials { get; init; }
        public int Dimension = 1;
    }
}

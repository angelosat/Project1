using Project1.Core.Crafting;
using System.Collections.Generic;

namespace Project1.Core
{
    public class WorkstationDef(string name, WorkstationCapabilityDef[] capabilities, int maxModules = 1) : Def(name)
    {
        //public readonly HashSet<MaterialRefinementDef> Refinements = [.. processesOffered];
        public int MaxModules = maxModules;
        public HashSet<WorkstationCapabilityDef> Capabilities = [.. capabilities];
    }
}

using System.Collections.Generic;

namespace Start_a_Town_
{
    public class WorkstationDef(string name, WorkstationCapabilityDef[] capabilities, int maxModules = 1) : Def(name)
    {
        //public readonly HashSet<MaterialRefinementDef> Refinements = [.. processesOffered];
        public int MaxModules = maxModules;
        public HashSet<WorkstationCapabilityDef> Capabilities = [.. capabilities];
    }
}

using System.Collections.Generic;

namespace Start_a_Town_
{
    public class WorkstationDef(string name, MaterialRefinementDef[] processesOffered) : Def(name)
    {
        public readonly HashSet<MaterialRefinementDef> Refinements = [.. processesOffered];
    }
}

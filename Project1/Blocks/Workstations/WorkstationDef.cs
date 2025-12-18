using System.Collections.Generic;

namespace Start_a_Town_
{
    public class WorkstationDef(string name, RawMaterialStateDef[] processesOffered) : Def(name)
    {
        public readonly HashSet<RawMaterialStateDef> Refinements = [.. processesOffered];
    }
}

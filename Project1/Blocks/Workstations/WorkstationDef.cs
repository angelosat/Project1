using System.Collections.Generic;

namespace Start_a_Town_
{
    public class WorkstationDef(string name, MaterialMappingDef[] processesOffered) : Def(name)
    {
        public readonly HashSet<MaterialMappingDef> Processes = [.. processesOffered];
    }
}

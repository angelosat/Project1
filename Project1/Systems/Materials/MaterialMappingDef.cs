namespace Start_a_Town_
{
    public class MaterialMappingDef(string name, MaterialTypeDef type, MaterialProcessDef process) : Def(name)
    {
        public readonly (MaterialTypeDef MaterialType, MaterialProcessDef Process) Mapping = (type, process);
    }
}

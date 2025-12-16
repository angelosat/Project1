namespace Start_a_Town_
{
    public class MaterialMappingDef(string name, MaterialTypeDef type, MaterialStageDef process, Sprite sprite = null) : Def(name)
    {
        public readonly (MaterialTypeDef MaterialType, MaterialStageDef Process) Mapping = (type, process);
        public readonly Sprite Sprite = sprite ?? Sprite.Default;
    }
}

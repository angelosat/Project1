namespace Start_a_Town_
{
    public class MaterialMappingDef(string name, MaterialTypeDef type, MaterialFormDef process, Sprite sprite = null) : Def(name)
    {
        public readonly (MaterialTypeDef MaterialType, MaterialFormDef Process) Mapping = (type, process);
        public readonly Sprite Sprite = sprite ?? Sprite.Default;

        public MaterialTypeDef MaterialType => this.Mapping.MaterialType;
        public MaterialFormDef Process => this.Mapping.Process;
    }
}

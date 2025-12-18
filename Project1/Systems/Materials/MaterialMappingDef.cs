namespace Start_a_Town_
{
    public class MaterialMappingDef(string name, MaterialTypeDef type, RefinementPathDef process, Sprite sprite = null) : Def(name)
    {
        public readonly (MaterialTypeDef MaterialType, RefinementPathDef Process) Mapping = (type, process);
        public readonly Sprite Sprite = sprite ?? Sprite.Default;

        public MaterialTypeDef MaterialType => this.Mapping.MaterialType;
        public RefinementPathDef Process => this.Mapping.Process;
    }
}

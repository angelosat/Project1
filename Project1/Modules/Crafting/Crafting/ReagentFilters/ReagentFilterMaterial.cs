namespace Start_a_Town_
{
    public class ReagentFilterMaterial
    {
        public MaterialDef SpecificMaterial;
        public MaterialTypeDef SpecificMaterialType;

        public ReagentFilterMaterial()
        {
        }

        public ReagentFilterMaterial(MaterialDef mat, MaterialTypeDef matType)
        {
            this.SpecificMaterial = mat;
            this.SpecificMaterialType = matType;
        }

        public bool Condition(MaterialDef def)
        {
            if (this.SpecificMaterial != null)
                return def == this.SpecificMaterial;
            else
                return this.SpecificMaterialType == null || def.Type == this.SpecificMaterialType;
        }
    }
}

namespace Start_a_Town_
{
    public class MaterialRefinementDef(string name, MaterialRefinementDef source, MaterialTypeDef materialType, Sprite sprite) : Def(name)
    {
        public readonly Sprite Sprite = sprite;
        public readonly MaterialTypeDef MaterialType = materialType;
        internal MaterialRefinementDef Source = source;
        public int FuelConsumption, FuelProduction;
    }
}

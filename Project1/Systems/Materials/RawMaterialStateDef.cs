namespace Start_a_Town_
{
    public class RawMaterialStateDef(string name, MaterialTypeDef materialType, Sprite sprite) : Def(name)
    {
        public readonly Sprite Sprite = sprite;
        public readonly MaterialTypeDef MaterialType = materialType;
    }
}

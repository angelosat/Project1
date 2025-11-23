namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class ConstructionCategoryDefOf
    {
        public static readonly ConstructionCategoryDef Walls = new("Walls",
            BuildToolDefOf.Single,
            BuildToolDefOf.Line,
            BuildToolDefOf.Floor,
            BuildToolDefOf.Wall,
            BuildToolDefOf.Enclosure, 
            BuildToolDefOf.Box,
            BuildToolDefOf.BoxFilled,
            BuildToolDefOf.Pyramid,
            BuildToolDefOf.Roof);
        public static readonly ConstructionCategoryDef Doors = new("Doors", BuildToolDefOf.SinglePreview);
        public static readonly ConstructionCategoryDef Production = new("Production", BuildToolDefOf.SinglePreview);
        public static readonly ConstructionCategoryDef Furniture = new("Furniture", BuildToolDefOf.SinglePreview);
        static ConstructionCategoryDefOf()
        {
            Def.Register(typeof(ConstructionCategoryDefOf));
        }
    }
}

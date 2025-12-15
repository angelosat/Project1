namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal class ItemFamilyDefOf
    {
        static public readonly ItemFamilyDef Tool = new ItemFamilyDef("Tool", typeof(ToolSystem));
        static public readonly ItemFamilyDef Plant = new ItemFamilyDef("Plant", typeof(PlantSystem));
        static public readonly ItemFamilyDef RawMaterial = new ItemFamilyDef("RawMaterial", typeof(RawMaterialSystem));
        static public readonly ItemFamilyDef Actor = new ItemFamilyDef("Actor", typeof(ActorSystem));
    }
}

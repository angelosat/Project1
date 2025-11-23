namespace Start_a_Town_
{
    static class RoomRoleDefOf
    {
        static public readonly RoomRoleDef Bedroom = new RoomRoleDef("Bedroom").AddFurniture(FurnitureDefOf.Bed);
        static public readonly RoomRoleDef Dining = new RoomRoleDef("Dining Room").AddFurniture(FurnitureDefOf.Table);
        static public readonly RoomRoleDef Tavern = new RoomRoleDef("Tavern").AddFurniture(FurnitureDefOf.Table);
        static public readonly RoomRoleDef Inn = new RoomRoleDef("Inn").AddFurniture(FurnitureDefOf.Counter);
        static public readonly RoomRoleDef Shop = new RoomRoleDef("Shop").AddFurniture(FurnitureDefOf.Counter);

        static RoomRoleDefOf()
        {
            Def.Register(Bedroom);
            Def.Register(Dining);
            Def.Register(Inn);
            Def.Register(Tavern);
            Def.Register(Shop);
        }
        static public readonly RoomRoleDef[] All = { Bedroom, Dining, Inn, Tavern, Shop };
    }
}

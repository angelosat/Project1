using Project1.Core.Base;
using Project1.Core.Helpers;
using Project1.Core.Rooms;

namespace Project1.Core.Rooms
{
    [EnsureStaticCtorCall]
    static class RoomRoleDefOf
    {
        static public readonly RoomRoleDef Bedroom = new RoomRoleDef("Bedroom").AddFurniture(FurnitureDefOf.Bed);
        static public readonly RoomRoleDef Dining = new RoomRoleDef("Dining Room").AddFurniture(FurnitureDefOf.Table);
        static public readonly RoomRoleDef Tavern = new RoomRoleDef("Tavern").AddFurniture(FurnitureDefOf.Table);
        static public readonly RoomRoleDef Inn = new RoomRoleDef("Inn").AddFurniture(FurnitureDefOf.Counter);
        static public readonly RoomRoleDef Shop = new RoomRoleDef("Shop").AddFurniture(FurnitureDefOf.Counter);

        static RoomRoleDefOf()
        {
            Def.Register(typeof(RoomRoleDefOf));
        }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Rooms
{
    internal static class RoomSystem
    {
        static public IEnumerable<RoomRoleDef> RolesByFurniture(FurnitureDef furn)
        {
            return Def.GetDefs<RoomRoleDef>().Where(r => r.Furniture.Contains(furn));
        }
    }
}

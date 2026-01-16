using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    internal static class RoomSystem
    {
        static public IEnumerable<RoomRoleDef> ByFurniture(FurnitureDef furn)
        {
            return Def.GetDefs<RoomRoleDef>().Where(r => r.Furniture.Contains(furn));
        }
    }
}

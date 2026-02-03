using Project1.Framework.Base;
using Start_a_Town_;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Framework.Rooms
{
    internal static class RoomSystem
    {
        static public IEnumerable<RoomRoleDef> ByFurniture(FurnitureDef furn)
        {
            return Def.GetDefs<RoomRoleDef>().Where(r => r.Furniture.Contains(furn));
        }
    }
}

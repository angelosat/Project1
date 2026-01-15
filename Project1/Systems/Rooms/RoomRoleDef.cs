using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public class RoomRoleDef : Def
    {
        public readonly HashSet<FurnitureDef> Furniture = new();
        public RoomRoleDef(string name) : base(name)
        {
        }
        public RoomRoleDef AddFurniture(params FurnitureDef[] furniture)
        {
            foreach (var f in furniture)
                this.Furniture.Add(f);
            return this;
        }
        static public IEnumerable<RoomRoleDef> ByFurniture(FurnitureDef furn)
        {
            return RoomRoleDefOf.All.Where(r => r.Furniture.Contains(furn));
        }
    }
}

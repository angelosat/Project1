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
        
    }
}

using Project1.Core.Base;
using System.Collections.Generic;

namespace Project1.Core.Rooms
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

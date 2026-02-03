using Project1.Framework.Base;
using Start_a_Town_;
using System.Collections.Generic;

namespace Project1.Framework.Rooms
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

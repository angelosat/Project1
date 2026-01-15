using Start_a_Town_.UI;
using System.Linq;

namespace Start_a_Town_
{
    internal class RoomGui : GroupBox, ISelectionBound
    {
        //Room Room;
        //public ISelectable CurrentSelection { get => this.Room; set => this.Room = value as Room; }
        public ISelectable CurrentSelection { get; set; }
        ComboBoxFinal<RoomRoleDef> CboxRole, CboxOwner, CboxWorkplace;
        public void OnBind(ISelectable selectable)
        {
            this.Build(selectable as Room);
        }
        void Build(Room room)
        {
            //var box = new GroupBox();
            //Room currentRoom = null;
            //IntVec3 center = default;
            this.AddControlsVertically(
                new ComboBoxFinal<RoomRoleDef>(128, "Role", r => r?.Label ?? "none", setRoomDef, () => room?.RoomRole, () => room.Furnitures.SelectMany(f => RoomRoleDef.ByFurniture(f)).Distinct().Prepend(null)),
                new ComboBoxFinal<Actor>(128, "Owner", a => a?.Name ?? "none", setOwner, () => room?.GetOwner(), () => room?.Map.Town.GetMembers().Prepend(null)),
                new ComboBoxFinal<Workplace>(128, "Workplace", w => w?.Name ?? "none", setWorkplace, () => room?.Workplace, () => room.Map.Town.ShopManager.GetShops().Where(sh => sh.IsValidRoom(room)).Prepend(null)),
                new Label(() => $"Interior: {room?.Interior.Count} cells"),
                new Label(() => $"Edges: {room?.Border.Count} cells"),
                new Label(() => $"Value: {room?.Value}"),
                new Button("Refresh", refresh)
                );
            //this.SetGetDataAction(o =>
            //{
            //    var oo = ((MapBase map, IntVec3 global))o;
            //    var map = oo.map;
            //    var global = oo.global;
            //    room = map.Town.RoomManager.GetRoomAt(global);
            //    center = global;
            //    box.Tag = room;
            //    box.GetWindow().SetTitle(room.Name);
            //});
            //box.ToWindow("Room settings");
            //return box;

            void setRoomDef(RoomRoleDef rdef) => PacketsRooms.SetRoomType(room.Map.Net, room.Map.Net.CurrentPlayer, room, rdef);
            void setOwner(Actor actor) => PacketsRooms.SetOwner(room.Map.Net, room.Map.Net.CurrentPlayer, room, actor);
            void setWorkplace(Workplace wplace) => PacketsRooms.SetWorkplace(room.Map.Net, room.Map.Net.CurrentPlayer, room, wplace);
            void refresh() => PacketsRooms.Refresh(room.Map.Net, room.Map.Net.GetPlayer(), room, IntVec3.Zero);
        }
    }
}

using Project1.Core.Entities.Actors;
using Project1.Core.Towns;
using Project1.Core.UI;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.UI;
using System.Linq;

namespace Project1.Core.Rooms
{
    class RoomGui : SelectionBoundControl// GroupBox, ISelectionBound
    {
        readonly ChangeNotifier Notifications = new();
        Room currentRoom;
        IntVec3 center;

        //public ISelectable CurrentSelection { get => this.currentRoom; set => this.currentRoom = value as Room; }

        public RoomGui()
        {
            this.AddControlsVertically(
                new ComboBoxFinal<RoomRoleDef>(128, "Role", r => r?.LabelReadable ?? "none", setRoomDef, () => currentRoom?.RoomRole, () => currentRoom.Furnitures.SelectMany(f => RoomSystem.RolesByFurniture(f)).Distinct().Prepend(null)),
                new ComboBoxFinal<Actor>(128, "Owner", a => a?.Name ?? "none", setOwner, () => currentRoom?.GetOwner(), () => currentRoom?.Map.Town.GetMembers().Prepend(null)),
                new ComboBoxFinal<Workplace>(128, "Workplace", w => w?.Name ?? "none", setWorkplace, () => currentRoom?.Workplace, () => currentRoom.Map.Town.ShopManager.GetShops().Where(sh => sh.IsValidRoom(currentRoom)).Prepend(null)),
                new LabelNew(() => $"Interior: {currentRoom?.Interior.Count} cells").Bind(this.Notifications),
                new LabelNew(() => $"Edges: {currentRoom?.Border.Count} cells").Bind(this.Notifications),
                new LabelNew(() => $"Value: {currentRoom?.Value}").Bind(this.Notifications),
                new Button("Refresh", refresh)
                );
        }

        protected override void OnBind(ISelectable selectable)
        {
            if (selectable is not Room room)
                return;
            if (room == this.currentRoom)
                return;
            var global = room.Global;
            currentRoom = room;// map.Town.RoomManager.GetRoomAt(global);
            center = global;
            this.Notifications.Notify();
        }

        void setRoomDef(RoomRoleDef rdef) => PacketsRooms.SetRoomType(currentRoom.Map.Net, currentRoom.Map.Net.CurrentPlayer, currentRoom, rdef);
        void setOwner(Actor actor) => PacketsRooms.SetOwner(currentRoom.Map.Net, currentRoom.Map.Net.CurrentPlayer, currentRoom, actor);
        void setWorkplace(Workplace wplace) => PacketsRooms.SetWorkplace(currentRoom.Map.Net, currentRoom.Map.Net.CurrentPlayer, currentRoom, wplace);
        void refresh() => PacketsRooms.Refresh(currentRoom.Map.Net, currentRoom.Map.Net.GetPlayer(), currentRoom, center);
    }
}

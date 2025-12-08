using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    public partial class Room
    {
        static class Packets
        {
            static readonly int PacketSetOwner, PacketSetRoomType, PacketSetWorkplace, PacketRefresh;
            static Packets()
            {
                PacketSetOwner = Registry.PacketHandlers.Register(SetOwner);
                PacketSetRoomType = Registry.PacketHandlers.Register(SetRoomType);
                PacketSetWorkplace = Registry.PacketHandlers.Register(SetWorkplace);
                PacketRefresh = Registry.PacketHandlers.Register(Refresh);
            }

            public static void SetRoomType(NetEndpoint net, PlayerData player, Room room, RoomRoleDef roomType)
            {
                if (net is Server)
                    room.RoomRole = roomType;
                var w = net.BeginPacket(PacketSetRoomType);//.Write(player.ID, room.ID, roomType?.Name ?? "");
                w.Write(player.ID);
                w.Write(room.ID);
                w.Write(roomType?.Name ?? "");
            }
            private static void SetRoomType(NetEndpoint net, Packet pck)
            {
                var r = pck.PacketReader;
                var player = net.GetPlayer(r.ReadInt32());
                var room = net.Map.Town.RoomManager.GetRoom(r.ReadInt32());
                var roomdef = r.ReadString() is string roomRoleName && !roomRoleName.IsNullEmptyOrWhiteSpace() ? Def.GetDef<RoomRoleDef>(roomRoleName) : null;
                if (net is Client)
                    room.RoomRole = roomdef;
                else
                    SetRoomType(net, player, room, roomdef);
            }

            public static void SetOwner(NetEndpoint net, PlayerData player, Room room, Actor owner)
            {
                if (net is Server)
                    room.ForceAddOwner(owner);
                //net.GetOutgoingStreamOrderedReliable().Write(PacketSetOwner, player.ID, room.ID, owner?.RefId ?? -1);
                var w = net.BeginPacket(PacketSetOwner);//.Write(player.ID, room.ID, owner?.RefId ?? -1);
                w.Write(player.ID);
                w.Write(room.ID);
                w.Write(owner?.RefId ?? -1);
            }
            private static void SetOwner(NetEndpoint net, Packet pck)
            {
                var r = pck.PacketReader;
                var player = net.GetPlayer(r.ReadInt32());
                var roomID = r.ReadInt32();
                var room = net.Map.Town.RoomManager.GetRoom(roomID);
                var owner = r.ReadInt32() is int id && id != -1 ? net.World.GetEntity<Actor>(id) : null;
                if (net is Server)
                    SetOwner(net, player, room, owner);
                else
                    room.ForceAddOwner(owner);
            }

            internal static void SetWorkplace(NetEndpoint net, PlayerData player, Room room, Workplace wplace)
            {
                if (net is Server)
                    room.SetWorkplace(wplace);
                //var w = net.GetOutgoingStreamOrderedReliable();
                //w.Write(PacketSetWorkplace);
                var w = net.BeginPacket(PacketSetWorkplace);
                w.Write(player.ID);
                w.Write(room.ID);
                w.Write(wplace?.ID ?? -1);
            }
            private static void SetWorkplace(NetEndpoint net, Packet pck)
            {
                var r = pck.PacketReader;
                var player = net.GetPlayer(r.ReadInt32());
                var roomID = r.ReadInt32();
                var room = net.Map.Town.RoomManager.GetRoom(roomID);
                var wplace = r.ReadInt32() is int id && id != -1 ? net.Map.Town.ShopManager.GetShop(id) : null;

                if (net is Server)
                    SetWorkplace(net, player, room, wplace);
                else
                    room.SetWorkplace(wplace);
            }

            internal static void Refresh(NetEndpoint net, PlayerData playerData, Room room, IntVec3 center)
            {
                if (net is Server)
                    room.Refresh(center);
                var w = net.BeginPacket(PacketRefresh);//.Write(playerData.ID, room.ID, center);
                w.Write(playerData.ID);
                w.Write(room.ID);
                w.Write(center);
            }
            private static void Refresh(NetEndpoint net, Packet pck)
            {
                var r = pck.PacketReader;
                var player = net.GetPlayer(r.ReadInt32());
                var room = net.Map.Town.RoomManager.GetRoom(r.ReadInt32());
                var center = r.ReadIntVec3();
                if (net is Client)
                    room.Refresh(center);
                else
                    Refresh(net, player, room, center);
            }
        }
    }
}

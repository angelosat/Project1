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
                PacketSetOwner = Network.RegisterPacketHandler(SetOwner);
                PacketSetRoomType = Network.RegisterPacketHandler(SetRoomType);
                PacketSetWorkplace = Network.RegisterPacketHandler(SetWorkplace);
                PacketRefresh = Network.RegisterPacketHandler(Refresh);
            }

            public static void SetRoomType(INetEndpoint net, PlayerData player, Room room, RoomRoleDef roomType)
            {
                if (net is Server)
                    room.RoomRole = roomType;
                //net.GetOutgoingStreamOrderedReliable().Write(PacketSetRoomType, player.ID, room.ID, roomType?.Name ?? "");
                net.BeginPacket(ReliabilityType.OrderedReliable, PacketSetRoomType).Write(player.ID, room.ID, roomType?.Name ?? "");
            }
            private static void SetRoomType(INetEndpoint net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var room = net.Map.Town.RoomManager.GetRoom(r.ReadInt32());
                var roomdef = r.ReadString() is string roomRoleName && !roomRoleName.IsNullEmptyOrWhiteSpace() ? Def.GetDef<RoomRoleDef>(roomRoleName) : null;
                if (net is Client)
                    room.RoomRole = roomdef;
                else
                    SetRoomType(net, player, room, roomdef);
            }

            public static void SetOwner(INetEndpoint net, PlayerData player, Room room, Actor owner)
            {
                if (net is Server)
                    room.ForceAddOwner(owner);
                //net.GetOutgoingStreamOrderedReliable().Write(PacketSetOwner, player.ID, room.ID, owner?.RefId ?? -1);
                net.BeginPacket(ReliabilityType.OrderedReliable, PacketSetOwner).Write(player.ID, room.ID, owner?.RefId ?? -1);
            }
            private static void SetOwner(INetEndpoint net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var roomID = r.ReadInt32();
                var room = net.Map.Town.RoomManager.GetRoom(roomID);
                var owner = r.ReadInt32() is int id && id != -1 ? net.GetNetworkObject<Actor>(id) : null;
                if (net is Server)
                    SetOwner(net, player, room, owner);
                else
                    room.ForceAddOwner(owner);
            }

            internal static void SetWorkplace(INetEndpoint net, PlayerData player, Room room, Workplace wplace)
            {
                if (net is Server)
                    room.SetWorkplace(wplace);
                //var w = net.GetOutgoingStreamOrderedReliable();
                //w.Write(PacketSetWorkplace);
                var w = net.BeginPacket(ReliabilityType.OrderedReliable, PacketSetWorkplace);
                w.Write(player.ID);
                w.Write(room.ID);
                w.Write(wplace?.ID ?? -1);
            }
            private static void SetWorkplace(INetEndpoint net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var roomID = r.ReadInt32();
                var room = net.Map.Town.RoomManager.GetRoom(roomID);
                var wplace = r.ReadInt32() is int id && id != -1 ? net.Map.Town.ShopManager.GetShop(id) : null;

                if (net is Server)
                    SetWorkplace(net, player, room, wplace);
                else
                    room.SetWorkplace(wplace);
            }

            internal static void Refresh(INetEndpoint net, PlayerData playerData, Room room, IntVec3 center)
            {
                if (net is Server)
                    room.Refresh(center);
                //net.GetOutgoingStreamOrderedReliable().Write(PacketRefresh, playerData.ID, room.ID, center);
                net.BeginPacket(ReliabilityType.OrderedReliable, PacketRefresh).Write(playerData.ID, room.ID, center);
            }
            private static void Refresh(INetEndpoint net, BinaryReader r)
            {
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

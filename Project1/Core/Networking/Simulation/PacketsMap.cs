using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Simulation;
using Project1.Framework;
using System;

namespace Project1.Core.Networking.Simulation
{
    class PacketsMap
    {
        static readonly int
            PacketSyncSetCellData;//,
            //PacketSpawn;
        static PacketsMap()
        {
            PacketSyncSetCellData = Registry.PacketHandlers.Register(SyncSetCellData);
            //PacketSpawn = Registry.PacketHandlers.Register(ReceiveSpawnEntity);
        }
        //public static void SendSpawnEntity(INetEndpoint net, GameObject entity, MapBase map, Vector3 global, Vector3 velocity)
        //{
        //    if (net is not Server server)
        //        return;
        //    var w = server.BeginPacket(PacketSpawn);
        //    w.Write(entity.RefId);
        //    w.Write(global);
        //    w.Write(velocity);
        //}
        //static void ReceiveSpawnEntity(NetEndpoint net, Packet pck)
        //{
        //    var r = pck.PacketReader;
        //    var client = net as Client;
        //    var actor = client.World.GetEntity(r.ReadInt32());
        //    var global = r.ReadVector3();
        //    var velocity = r.ReadVector3();
        //    var map = client.Map;
        //    map.SyncSpawn(actor, global, velocity);
        //}
        public static void SyncSetCellData(MapBase map, IntVec3 global, byte data)
        {
            var net = map.Net as Server;
            if (net is not Server server)
                throw new Exception();
            map.SetCellData(global, data);
            //net.WriteToStream(PacketSyncSetCellData, global, data);
            net.BeginPacket(PacketSyncSetCellData)
                .Write(global)
                .Write(data);
        }
        private static void SyncSetCellData(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var global = r.ReadIntVec3();
            var data = r.ReadByte();
            if (net is Client)
                net.Map.SetCellData(global, data);
            else
                SyncSetCellData(net.Map, global, data);
        }
    }
}

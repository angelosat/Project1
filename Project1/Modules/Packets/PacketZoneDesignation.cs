using System.IO;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketZoneDesignation
    {
        static readonly int PacketPlayerZoneDesignation;
        static PacketZoneDesignation()
        {
            PacketPlayerZoneDesignation = Network.RegisterPacketHandler(Receive);
        }
        static public void Send(INetEndpoint net, ZoneDef zoneDef, int zoneID, Vector3 begin, int w, int h, bool remove)
        {
            var stream = net.GetOutgoingStream();
            stream.Write(PacketPlayerZoneDesignation);
            stream.Write(zoneDef.Name);
            stream.Write(zoneID);
            stream.Write(begin);
            stream.Write(w);
            stream.Write(h);
            stream.Write(remove);
        }
        static public void Receive(INetEndpoint net, BinaryReader r)
        {
            //var zoneType = Type.GetType(r.ReadString());
            var zoneType = Def.GetDef<ZoneDef>(r.ReadString());
            var zoneID = r.ReadInt32();
            var begin = r.ReadVector3();
            var width = r.ReadInt32();
            var height = r.ReadInt32();
            var remove = r.ReadBoolean();
            net.Map.Town.ZoneManager.PlayerEdit(zoneID, zoneType, begin, width, height, remove);
            if (net is Server)
                Send(net, zoneType, zoneID, begin, width, height, remove);
        }
    }
}

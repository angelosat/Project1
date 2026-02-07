using Microsoft.Xna.Framework;
using Project1.Core.Base;
using Project1.Core.Net;
using Project1.Core.Helpers;
using Project1.Core.Net;

namespace Project1.Core.Towns.Zones
{
    [EnsureStaticCtorCall]
    static class PacketPlayerZoneDesignation
    {
        static readonly int _pPlayerZoneDesignation;
        static PacketPlayerZoneDesignation()
        {
            _pPlayerZoneDesignation = Registry.PacketHandlers.Register(Receive);
        }
        static public void Send(NetEndpoint net, ZoneDef zoneDef, int zoneID, Vector3 begin, int w, int h, bool remove)
        {
            var stream = net.BeginPacketImmediate(_pPlayerZoneDesignation);

            stream.Write(zoneDef.Name);
            stream.Write(zoneID);
            stream.Write(begin);
            stream.Write(w);
            stream.Write(h);
            stream.Write(remove);
        }
        static public void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
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

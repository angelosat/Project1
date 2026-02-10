using System;
using Project1.Framework;
using Project1.Core.Entities;
using Project1.Core.Net;
using Project1.Framework.Events;

namespace Project1.Core.Networking.Entities
{
    [EnsureStaticCtorCall]
    static class PacketEntitySync
    {
        static readonly int _packetTypeId;
        static PacketEntitySync()
        {
            _packetTypeId = Registry.PacketHandlers.Register(Receive);
        }
        static public void Send(NetEndpoint net, GameObject entity)
        {
            if (net is Client)
                throw new Exception();
            var w = net.BeginPacket(_packetTypeId);

            w.Write(entity.RefId);
            entity.SyncWrite(w);
        }
        static public void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            if (net is Server)
                throw new Exception();
            var entity = net.World.GetEntity(r.ReadInt32());
            entity.SyncRead(r);
        }
    }
}

using Microsoft.Xna.Framework;
using Project1.Core.Networking;
using Project1.Framework;
using System;

namespace Project1.Core.AI.Packets
{
    [EnsureStaticCtorCall]
    static class PacketPlayerInputDirection
    {
        static readonly int p;
        static PacketPlayerInputDirection()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }
        internal static void Send(NetEndpoint net, Vector2 direction)
        {
            if (net is Server)
                throw new NotImplementedException();
            var w = net.BeginPacket(p);
            w.Write(net.GetPlayer().ID);
            w.Write(direction);
        }
        private static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            if (net is not Server server)
                throw new NotImplementedException();
            var pl = net.GetPlayer(r.ReadInt32());
            var dir = r.ReadVector2();
            if (pl.ControllingEntity is null)
            {
                server.SyncReport("received direction packet but player controlling entity is null");
                return;
            }
            pl.ControllingEntity.Transform.Direction = dir;
        }
    }
}

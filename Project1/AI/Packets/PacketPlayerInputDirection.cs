using System;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketPlayerInputDirection
    {
        static readonly int p;
        static PacketPlayerInputDirection()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        internal static void Send(INetEndpoint net, Vector2 direction)
        {
            if (net is Server)
                throw new NotImplementedException();
            var w = net.GetOutgoingStreamOrderedReliable();
            w.Write(p);
            w.Write(net.GetPlayer().ID);
            w.Write(direction);
        }
        private static void Receive(INetEndpoint net, BinaryReader r)
        {
            if(net is Client)
                throw new NotImplementedException();
            var pl = net.GetPlayer(r.ReadInt32());
            var dir = r.ReadVector2();
            if (pl.ControllingEntity is null)
            {
                net.SyncReport("received direction packet but player controlling entity is null");
                return;
            }
            pl.ControllingEntity.Transform.Direction = dir;
        }
    }
}

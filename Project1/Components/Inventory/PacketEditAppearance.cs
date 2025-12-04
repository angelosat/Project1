using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketEditAppearance
    {
        static readonly int p;
        static PacketEditAppearance()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }
        public static void Send(Actor actor, CharacterColors colors)
        {
            var w = actor.Net.BeginPacketOld(p);
            w.Write(actor.RefId);
            colors.Write(w);
        }
        private static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var actorID = r.ReadInt32();
            var actor = net.World.GetEntity(actorID) as Actor;
            var colors = new CharacterColors(r);
            actor.Sprite.Customization = colors;
            if (net is Server)
                Send(actor, colors);
        }
    }
}

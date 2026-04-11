using Project1.Core.Entities.Actors;
using Project1.Core.Entities.ColorCustomization;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Framework;

namespace Project1.Core.Networking.Inventory
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
            var w = actor.Net.BeginPacket(p);
            w.Write(actor.RefId);
            colors.Write(w);
        }
        private static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var actorID = r.ReadEntityRefId();
            var actor = net.World.Get(actorID) as Actor;
            var colors = new CharacterColors(r);
            actor.Sprite.Customization = colors;
            if (net is Server)
                Send(actor, colors);
        }
    }
}

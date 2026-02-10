using Project1.Framework;
using Project1.Core.Net;
using Project1.Framework.Events;

namespace Project1.Core.Networking.Simulation
{
    [EnsureStaticCtorCall]
    internal class PacketSaving
    {
        static readonly int p;
        static PacketSaving()
        {
            p = Registry.PacketHandlers.Register(Receive);
        }
        public static void Send(Server server)
        {
            var w = server.BeginPacket(p);
            w.Write(server.IsSaving);
        }
        private static void Receive(NetEndpoint net, Packet packet)
        {
            ((Client)net).SetSaving(packet.Reader.ReadBoolean());
        }
    }
}

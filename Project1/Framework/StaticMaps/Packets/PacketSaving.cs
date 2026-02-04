using Project1.Framework.Base;
using Project1.Framework.Net;
using Start_a_Town_;


namespace Project1.Framework.StaticMaps.Packets
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

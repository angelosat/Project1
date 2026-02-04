using Project1.Framework.Base;
using Project1.Framework.Net;
using Start_a_Town_;

namespace Project1.Core.Networking
{
    [EnsureStaticCtorCall]
    internal class PacketsConstruction
    {
        static readonly int _pSync, _pReady, _pFinished;

        static PacketsConstruction()
        {
            _pSync = Registry.PacketHandlers.Register(OnSync);
            _pReady = Registry.PacketHandlers.Register(OnReady);
            _pFinished = Registry.PacketHandlers.Register(OnFinished);
            
            Registry.MapEventHooksServer.Register<ConstructionFinishedEvent>(SendFinished);
            Registry.MapEventHooksServer.Register<ConstructionReadyEvent>(SendReady);
            Registry.MapEventHooksServer.Register<ConstructionUpdatedEvent>(SendSync);

        }

        private static void SendFinished(ConstructionFinishedEvent e)
        {
            Server.Instance.BeginPacket(_pFinished)
                .Write(e.Source.Parent.OriginGlobal);
        }

        static void OnFinished(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var comp = client.Map.GetBlockEntityComp<BlockConstructionComp>(r.ReadIntVec3());
            comp.Map.Events.Post(new ConstructionFinishedEvent(comp));
        }
        public static void SendReady(ConstructionReadyEvent e)
        {
            Server.Instance.BeginPacket(_pReady)
                .Write(e.Source.Parent.OriginGlobal);
        }
        private static void OnReady(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var comp = client.Map.GetBlockEntityComp<BlockConstructionComp>(r.ReadIntVec3());
            comp.Map.Events.Post(new ConstructionReadyEvent(comp));
        }

        public static void SendSync(ConstructionUpdatedEvent e)
        {
            var w = Server.Instance.BeginPacket(_pSync)
                .Write(e.Source.Parent.OriginGlobal);
            e.Source.Write(w);
        }
        private static void OnSync(NetEndpoint endpoint, Packet packet)
        {
            var client = endpoint as Client;
            var r = packet.PacketReader;
            var comp = client.Map.GetBlockEntityComp<BlockConstructionComp>(r.ReadIntVec3());
            comp.Read(r);
        }
    }
}

using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Serialization;
using Project1.Framework;
using Project1.Framework.Events;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Input.Orders
{
    [EnsureStaticCtorCall]
    internal static class PacketsOrderCommands
    {
        static readonly PacketId _pPlayerIssuedOrderCommand;
        static PacketsOrderCommands()
        {
            _pPlayerIssuedOrderCommand = Registry.PacketHandlers.Register(ReceivePlayerIssuedOrderCommand);
            Registry.PlayerInputEventHooks.Register<PlayerIssuedOrderCommandEvent>(OnPlayerIssuedOrderCommand);
        }

        private static void ReceivePlayerIssuedOrderCommand(NetEndpoint endpoint, Packet packet)
        {
            var r = packet.PacketReader;
            var def = r.ReadDef<OrderCommandDef>();
            var targets = r.ReadListTargets(endpoint.Map);
            def.Worker.Execute(targets);
            if(endpoint is Server server)
                SendPlayerIssuedOrderCommand(server, def, targets);
        }
        private static void SendPlayerIssuedOrderCommand(NetEndpoint endpoint, OrderCommandDef def, List<TargetArgs> targets)
        {
            endpoint.BeginPacketImmediate(_pPlayerIssuedOrderCommand)
                .Write(def)
                .Write(targets);
        }

        private static void OnPlayerIssuedOrderCommand(PlayerIssuedOrderCommandEvent e)
        {
            //var net = Client.Instance;
            var net = e.Targets.First().Map.Net;
            if (net.IsClient)
                SendPlayerIssuedOrderCommand(net, e.Def, e.Targets);
            else
                e.Def.Worker.Execute(e.Targets);
        }
    }
}

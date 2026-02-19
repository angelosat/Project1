using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Core.Serialization;
using Project1.Core.UI.Hud;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.Serialization;

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
            var selection = r.Read<SelectionIntent>();
            var map = endpoint.Map; // temp
            def.Worker.Execute(map, selection);
            if (endpoint is Server server)
                SendPlayerIssuedOrderCommand(server, def, selection);
            else 
                SelectionManager.Instance.RefreshOrderButtons();
        }
        private static void SendPlayerIssuedOrderCommand(NetEndpoint endpoint, OrderCommandDef def, SelectionIntent selection)
        {
            endpoint.BeginPacketImmediate(_pPlayerIssuedOrderCommand)
                .Write(def)
                .Write(selection);
        }

        private static void OnPlayerIssuedOrderCommand(PlayerIssuedOrderCommandEvent e)
        {
            var map = Ingame.GetMap(); // temp
            var net = map.Net;
            if (net.IsClient)
                SendPlayerIssuedOrderCommand(net, e.Def, e.Selection);
            else
                e.Def.Worker.Execute(map, e.Selection);
        }
    }
}

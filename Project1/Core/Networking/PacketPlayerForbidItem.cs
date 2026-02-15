using Project1.Core.Helpers;
using Project1.Core.Input;
using Project1.Framework;
using Project1.Framework.Events;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Networking
{
    [EnsureStaticCtorCall]
    internal class PacketPlayerForbidItem
    {
        static readonly public PacketId _pPlayerForbidItem;
        static PacketPlayerForbidItem()
        {
            Registry.PlayerInputEventHooks.Register<PlayerForbidItemsEvent>(OnPlayerForbidItems);

            _pPlayerForbidItem = Registry.PacketHandlers.Register(ReceivePlayerForbidItems);
        }
        private static void OnPlayerForbidItems(PlayerForbidItemsEvent e)
        {
            var entities = e.Entities;
            var refIds = entities.Select(o => (EntityRefId)o.RefId).ToList();
            SendPlayerForbidItem(Client.Instance, refIds);
        }
        static void SendPlayerForbidItem(NetEndpoint endpoint, List<EntityRefId> ids)
        {
            endpoint.BeginPacketImmediate(_pPlayerForbidItem)
                .Write(ids);
        }
        private static void ReceivePlayerForbidItems(NetEndpoint endpoint, Packet packet)
        {
            var r = packet.PacketReader;
            var list = r.ReadListEntityRefId();
            foreach (var id in list)
                endpoint.World.GetEntity(id).ToggleForbidden();
            if (endpoint is Server server)
                SendPlayerForbidItem(server, list);
        }
    }
}

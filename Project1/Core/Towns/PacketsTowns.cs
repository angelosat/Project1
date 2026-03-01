using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Input;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns
{
    [EnsureStaticCtorCall]
    internal static class PacketsTowns
    {
        static readonly PacketId pToggleTownMemeber;
        static PacketsTowns()
        {
            pToggleTownMemeber = Registry.PacketHandlers.Register(ReceiveToggleTownMembers);
            Registry.PlayerInputEventHooks.Register<PlayerTogglingTownMembersEvent>(OnPlayerTogglingTownMembers);
        }
        private static void OnPlayerTogglingTownMembers(PlayerTogglingTownMembersEvent e)
        {
            if (Ingame.Net.IsServer)
                Ingame.CurrentMap.Town.ToggleMembers(e.Actors);
            else
                Send(Ingame.Net, e.Actors);
        }
        static void Send(NetEndpoint net, IReadOnlyCollection<Entity> entities)
        {
            net.BeginPacketImmediate(pToggleTownMemeber)
                .Write(entities.Select(e => e.RefId).ToArray());
        }
        private static void ReceiveToggleTownMembers(NetEndpoint endpoint, Packet packet)
        {
            var r = packet.PacketReader;
            var actors = endpoint.Map.World.GetEntities<Actor>(r.ReadListEntityRefId());
            endpoint.Map.Town.ToggleMembers(actors);
            if (endpoint is Server)
                Send(endpoint, [.. actors]);
        }
    }
}
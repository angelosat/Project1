using Project1.Core.Components;
using Project1.Core.Entities.Actors;
using Project1.Core.Input;
using Project1.Core.Networking;
using Project1.Core.Screens;
using Project1.Framework;
using Project1.Framework.Events;

namespace Project1.Core.AI.Packets
{
    [EnsureStaticCtorCall]
    static class PacketControlActor
    {
        static readonly PacketId pControlActor;
        static PacketControlActor()
        {
            pControlActor = Registry.PacketHandlers.Register(Receive);
            Registry.PlayerInputEventHooks.Register<PlayerControlActorRequestEvent>(OnPlayerControlActor);
        }

        private static void OnPlayerControlActor(PlayerControlActorRequestEvent e)
        {
            if (Ingame.Net.IsServer)
                Perform(Ingame.Net, Ingame.Net.GetPlayer(e.Player.ID), e.Actor);
            Send(Ingame.Net, e.Player.ID, e.Actor.RefId);
        }

        internal static void Send(NetEndpoint net, PlayerId playerid, EntityRefId entityid)
        {
            var w = net.BeginPacket(pControlActor);
            w.Write(playerid);
            w.Write(entityid);
        }
        private static void Receive(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var playerid = (PlayerId)r.ReadInt32();
            var player = net.GetPlayer(playerid);
            var entityid = (EntityRefId)r.ReadInt32();
            var nextEntity = entityid != -1 ? net.World.GetEntity(entityid) as Actor : null;
            Perform(net, player, nextEntity);
            if (net is Server)
            {
                //if (lastEntity is not null)
                //    lastEntity.AI.Enable();
                //if (nextEntity is not null)
                //    nextEntity.AI.Disable();
                Send(net, playerid, entityid);
            }
        }

        private static bool Perform(NetEndpoint net, PlayerData player, Actor nextEntity)
        {
            if (nextEntity?.IsPlayerControlled ?? false)
                return false;
            var lastEntity = player.ControllingEntity;
            player.ControllingEntity = nextEntity;
            //var net = Ingame.Net;

            //net.EventOccured((int)Message.Types.PlayerControlNpc, player, nextEntity, lastEntity);
            net.Map.Events.Post(new PlayerControlActorEvent(player, nextEntity, lastEntity));

            if (nextEntity is not null)
                net.Report($"{player.Name} is assuming direct control over {nextEntity.Name}");
            else
                net.Report($"{player.Name} no longer controlling {lastEntity.Name}");

            lastEntity?.AI.Enable();
            nextEntity?.AI.Disable();
            return true;
        }
    }
}

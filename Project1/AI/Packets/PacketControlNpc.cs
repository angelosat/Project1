using System;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketControlNpc
    {
        static readonly int PType;
        static PacketControlNpc()
        {
            PType = Network.RegisterPacketHandler(Receive);
        }
        internal static void Send(INetwork net, int entityid)
        {
            if (net is Server)
                throw new Exception();
            var plid = net.GetPlayer().ID;
            Send(net, plid, entityid);
        }
        internal static void Send(INetwork net, int playerid, int entityid)
        {
            var w = net.GetOutgoingStream();
            w.Write(PType);
            w.Write(playerid);
            w.Write(entityid);
        }
        private static void Receive(INetwork net, BinaryReader r)
        {
            var player = net.GetPlayer(r.ReadInt32());
            var entityid = r.ReadInt32();
            var nextEntity = entityid != -1 ? net.GetNetworkObject(entityid) as Actor : null;
            if (nextEntity?.IsPlayerControlled ?? false)
                return;
            var lastEntity = player.ControllingEntity;
            player.ControllingEntity = nextEntity;
            net.EventOccured(Components.Message.Types.PlayerControlNpc, player, nextEntity, lastEntity);

            if (nextEntity is not null)
                net.Write($"{player.Name} is controlling {nextEntity.Name}");
            else
                net.Write($"{player.Name} no longer controlling {lastEntity.Name}");

            if (net is Server)
            {
                if (lastEntity is not null)
                    lastEntity.AI.Enable();
                if (nextEntity is not null)
                    nextEntity.AI.Disable();
                Send(net, player.ID, entityid);
            }
        }
    }
}

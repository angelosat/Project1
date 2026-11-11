using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Framework;
using Project1.Framework.Serialization;
using System;

#nullable enable

namespace Project1.Core.Systems.ItemRoles;

[EnsureStaticCtorCall]
internal static class Packets_ItemPrefs
{
    internal static readonly PacketId
        _pSyncDeltas = Registry.PacketHandlers.Register(ReceiveSyncDeltas);

    static Packets_ItemPrefs()
    {
        Registry.WorldEventHooksServer.Register<ItemPrefsDeltaEvent>(SendItemPrefsDeltaEvent);
    }

    private static void SendItemPrefsDeltaEvent(ItemPrefsDeltaEvent e)
    {
        var actor = e.Actor;
        var deltas = e.Deltas;
        var w = Server.Instance.BeginPacket(_pSyncDeltas);
        w.Write(actor.RefId);
        w.Write(deltas.Length);
        for (int i = 0; i < deltas.Length; i++)
        {
            var (role, olditem, newitem, score) = deltas[i];
            w.Write(role);
            w.Write(olditem);
            w.Write(newitem);
            w.Write(score);
        }
    }

    private static void ReceiveSyncDeltas(NetEndpoint endpoint, Packet packet)
    {
        if (endpoint is Server)
            throw new Exception();
        var r = packet.PacketReader;
        var actorId = r.ReadEntityRefId();
        var actor = endpoint.World.Get<Actor>(actorId);
        var manager = actor.ItemPreferences;

        // read deltas
        var length = r.ReadInt32();
        for (int i = 0; i < length; i++)
        {
            var role = r.ReadDef<ItemRoleDef>();
            var olditemid = r.ReadEntityRefId();
            var newitemid = r.ReadEntityRefId();
            var olditem = olditemid > 0 ? actor.World.Get(olditemid) : null;
            var newitem = newitemid > 0 ? actor.World.Get(newitemid) : null;
            var score = r.ReadInt32();
            manager.UpdatePref(role, newitem, score);
        }
    }
}

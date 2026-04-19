using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Framework;
using Project1.Framework.Serialization;
using System;

#nullable enable

namespace Project1.Core.Systems.ItemRoles;

public partial class ItemPreferenceManager
{
    [EnsureStaticCtorCall]
    static class Packets_ItemPrefs
    {
        static readonly int pSyncPrefsAll;

        static Packets_ItemPrefs()
        {
            pSyncPrefsAll = Registry.PacketHandlers.Register(Receive);
        }

        private static void Receive(INetEndpoint net, Packet pck)
        {
            if (net is Server)
                throw new Exception();
            var r = pck.PacketReader;

            var actor = net.World.Get<Actor>(r.ReadInt32());
            var manager = actor.ItemPreferences;

            // read deltas
            var length = r.ReadInt32();
            for (int i = 0; i < length; i++)
            {
                var role = r.ReadDef<ItemRoleDef>();
                var olditemid = r.ReadEntityRefId();
                var newitemid = r.ReadEntityRefId();
                var olditem = olditemid > 0 ? actor.Map.World.Get(olditemid) : null;
                var newitem = newitemid > 0 ? actor.Map.World.Get(newitemid) : null;
                var score = r.ReadInt32();
                manager.UpdatePref(role, newitem, score);
            }
        }

        public static void SyncDeltas(Actor actor, (ItemRoleDef role, Entity oldItem, Entity newItem, int score)[] deltas)
        {
            var w = (actor.Net as Server).BeginPacket(pSyncPrefsAll);
            w.Write(actor.RefId);
            w.Write(deltas.Length);
            for (int i = 0; i < deltas.Length; i++)
            {
                var (role, olditem, newitem, score) = deltas[i];
                w.Write(role);
                w.Write(olditem?.RefId ?? -1);
                w.Write(newitem?.RefId ?? -1);
                w.Write(score);
            }
        }
    }
}

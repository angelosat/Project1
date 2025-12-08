using System.Collections.Generic;
using System.IO;

namespace Start_a_Town_.Net
{
    public class PlayerList
    {
        readonly NetEndpoint Net;
        readonly Dictionary<int, PlayerData> List = new();
        public IEnumerable<PlayerData> GetList()
        {
            return this.List.Values;
        }
        public PlayerList(NetEndpoint net)
        {
            this.Net = net;
        }
        public void Write(IDataWriter writer)
        {
            writer.Write(this.List.Count);
            foreach (var player in this.List.Values)
            {
                player.Write(writer);
            }
        }

        public static PlayerList Read(NetEndpoint net, IDataReader reader)
        {
            PlayerList list = new PlayerList(net);
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                list.Add(PlayerData.Read(reader));
            }

            return list;
        }

        public void Add(PlayerData player)
        {
            this.List.Add(player.ID, player);
            //this.Net.EventOccured((int)Components.Message.Types.PlayerConnected, player);
            this.Net.Events.Post(new PlayerConnectionEvent(player, true));
        }
        class PlayerConnectionEvent(PlayerData player, bool connected) : EventPayloadBase
        {
            readonly PlayerData Player = player;
            readonly bool Connected = connected;
        }
        public void Remove(PlayerData player)
        {
            this.List.Remove(player.ID);
            //this.Net.EventOccured((int)Components.Message.Types.PlayerDisconnected, player);
            this.Net.Events.Post(new PlayerConnectionEvent(player, false));
        }
        internal int GetLowestSpeed()
        {
            int speed = 4;
            foreach (var pl in this.List.Values)
            {
                speed = pl.SuggestedSpeed < speed ? pl.SuggestedSpeed : speed;
            }

            return speed;
        }

        internal PlayerData GetPlayer(int id)
        {
            return this.List.GetValueOrDefault(id);
        }
    }
}

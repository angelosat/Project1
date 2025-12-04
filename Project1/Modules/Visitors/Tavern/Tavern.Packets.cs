using System.Linq;
using Start_a_Town_.Net;
using System.IO;

namespace Start_a_Town_
{
    public partial class Tavern
    {
        static class Packets
        {
            static int PacketOrderAdd, PacketOrderRemove, PacketOrderSync, PacketOrderUpdateIngredients;
            static public void Init()
            {
                PacketOrderAdd = NetEndpoint.RegisterPacketHandler(HandleAddOrder);
                PacketOrderSync = NetEndpoint.RegisterPacketHandler(HandleSyncOrder);
                PacketOrderRemove = NetEndpoint.RegisterPacketHandler(HandleRemoveOrder);
                PacketOrderUpdateIngredients = NetEndpoint.RegisterPacketHandler(UpdateOrderIngredients);
            }
            private static void HandleRemoveOrder(NetEndpoint net, Packet pck)
            {
                var r = pck.PacketReader;
                var pl = net.GetPlayer(r.ReadInt32());
                var tavern = net.Map.Town.ShopManager.GetShop(r.ReadInt32()) as Tavern;
                var orderid = r.ReadInt32();
                var order = tavern.GetOrder(orderid);
                if (net is Client)
                    tavern.RemoveOrder(order);
                else
                    SendRemoveOrder(net, pl, tavern, order);
            }
            public static void SendRemoveOrder(INetEndpoint net, PlayerData player, Tavern tavern, CraftOrder order)
            {
                if (net is Server)
                    tavern.RemoveOrder(order);
                //net.GetOutgoingStreamOrderedReliable().Write(PacketOrderRemove, player.ID, tavern.ID, order.ID);
                net.BeginPacket(ReliabilityType.OrderedReliable, PacketOrderRemove).Write(player.ID, tavern.ID, order.ID);
            }
            private static void HandleAddOrder(NetEndpoint net, Packet pck)
            {
                var r = pck.PacketReader;
                var pl = net.GetPlayer(r.ReadInt32());
                var tavern = net.Map.Town.ShopManager.GetShop(r.ReadInt32()) as Tavern;
                var reaction = r.ReadDef<Reaction>();
                var id = r.ReadInt32();
                if (net is Client)
                    tavern.AddOrder(new CraftOrder(reaction) { ID = id });
                else
                    SendAddMenuItem(net, pl, tavern, reaction, id);
            }

            static public void SendAddMenuItem(INetEndpoint net, PlayerData player, Tavern tavern, Reaction reaction, int id = -1)
            {
                if (net is Server)
                {
                    id = tavern.MenuItemIDSequence++;
                    tavern.AddOrder(new CraftOrder(reaction) { ID = id });
                }
                //net.GetOutgoingStreamOrderedReliable().Write(PacketOrderAdd, player.ID, tavern.ID, reaction, id);
                net.BeginPacket(ReliabilityType.OrderedReliable, PacketOrderAdd).Write(player.ID, tavern.ID, reaction, id);
            }

            static public void SendOrderSync(INetEndpoint net, PlayerData player, Tavern tavern, CraftOrder order, bool enabled)
            {
                if (net is Server)
                    order.Enabled = enabled;
                //net.GetOutgoingStreamOrderedReliable().Write(PacketOrderSync, player.ID, tavern.ID, order.ID, enabled);
                net.BeginPacket(ReliabilityType.OrderedReliable, PacketOrderSync).Write(player.ID, tavern.ID, order.ID, enabled);
            }
            private static void HandleSyncOrder(NetEndpoint net, Packet pck)
            {
                var r = pck.PacketReader;
                var pl = net.GetPlayer(r.ReadInt32());
                var tavern = net.Map.Town.ShopManager.GetShop(r.ReadInt32()) as Tavern;
                var order = tavern.GetOrder(r.ReadInt32());
                var enabled = r.ReadBoolean();
                if (net is Client)
                    order.Enabled = enabled;
                else
                    //net.GetOutgoingStreamOrderedReliable().Write(PacketOrderSync, pl.ID, tavern.ID, order.ID, enabled);
                    net.BeginPacket(ReliabilityType.OrderedReliable, PacketOrderSync).Write(pl.ID, tavern.ID, order.ID, enabled);
            }

            public static void UpdateOrderIngredients(INetEndpoint net, PlayerData player, Tavern tavern, CraftOrder order, string reagent, ItemDef[] defs, MaterialDef[] mats, MaterialTypeDef[] matTypes)
            {
                if (net is Server)
                    order.ToggleReagentRestrictions(reagent, defs, mats, matTypes);
                //var w = net.GetOutgoingStreamOrderedReliable();
                //w.Write(PacketOrderUpdateIngredients);
                var w = net.BeginPacket(ReliabilityType.OrderedReliable, PacketOrderUpdateIngredients);

                w.Write(player.ID);
                w.Write(tavern.ID);
                w.Write(order.ID);
                w.Write(reagent);
                w.Write(defs?.Select(d => d.Name).ToArray());
                w.Write(mats?.Select(d => d.Name).ToArray());
                w.Write(matTypes?.Select(d => d.Name).ToArray());
            }
            private static void UpdateOrderIngredients(NetEndpoint net, Packet pck)
            {
                var r = pck.PacketReader;
                var player = net.GetPlayer(r.ReadInt32());
                var tavern = net.Map.Town.GetShop<Tavern>(r.ReadInt32());
                var order = tavern.GetOrder(r.ReadInt32());
                var reagent = r.ReadString();
                var defs = r.ReadStringArray().Select(Def.GetDef<ItemDef>).ToArray();
                var mats = r.ReadStringArray().Select(Def.GetDef<MaterialDef>).ToArray();
                var matTypes = r.ReadStringArray().Select(Def.GetDef<MaterialTypeDef>).ToArray();
                if (net is Client)
                    order.ToggleReagentRestrictions(reagent, defs, mats, matTypes);
                else
                    UpdateOrderIngredients(net, player, tavern, order, reagent, defs, mats, matTypes);
            }
        }
    }
}

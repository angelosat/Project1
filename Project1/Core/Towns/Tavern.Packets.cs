using System.Linq;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Materials;
using Project1.Core.Net;
using Project1.Core.Net;
using Project1.Core.Entities;
using Project1.Core.Helpers;
using Project1.Framework.Events;

namespace Project1.Core.Towns
{
    public partial class Tavern
    {
        static class Packets
        {
            static int PacketOrderAdd, PacketOrderRemove, PacketOrderSync, PacketOrderUpdateIngredients;
            static public void Init()
            {
                PacketOrderAdd = Registry.PacketHandlers.Register(HandleAddOrder);
                PacketOrderSync = Registry.PacketHandlers.Register(HandleSyncOrder);
                PacketOrderRemove = Registry.PacketHandlers.Register(HandleRemoveOrder);
                PacketOrderUpdateIngredients = Registry.PacketHandlers.Register(UpdateOrderIngredients);
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
            public static void SendRemoveOrder(NetEndpoint net, PlayerData player, Tavern tavern, CraftOrderOld order)
            {
                if (net is Server)
                    tavern.RemoveOrder(order);
                net.BeginPacket(PacketOrderRemove)
                    .Write(player.ID)
                    .Write(tavern.ID)
                    .Write(order.ID);
            }
            private static void HandleAddOrder(NetEndpoint net, Packet pck)
            {
                var r = pck.PacketReader;
                var pl = net.GetPlayer(r.ReadInt32());
                var tavern = net.Map.Town.ShopManager.GetShop(r.ReadInt32()) as Tavern;
                var reaction = r.ReadDef<Reaction>();
                var id = r.ReadInt32();
                if (net is Client)
                    tavern.AddOrder(new CraftOrderOld(reaction) { ID = id });
                else
                    SendAddMenuItem(net, pl, tavern, reaction, id);
            }

            static public void SendAddMenuItem(NetEndpoint net, PlayerData player, Tavern tavern, Reaction reaction, int id = -1)
            {
                if (net is Server)
                {
                    id = tavern.MenuItemIDSequence++;
                    tavern.AddOrder(new CraftOrderOld(reaction) { ID = id });
                }
                net.BeginPacket(PacketOrderAdd)
                    .Write(player.ID)
                    .Write(tavern.ID)
                    .Write(reaction)
                    .Write(id);
            }

            static public void SendOrderSync(NetEndpoint net, PlayerData player, Tavern tavern, CraftOrderOld order, bool enabled)
            {
                if (net is Server)
                    order.Enabled = enabled;
                net.BeginPacket(PacketOrderSync)
                    .Write(player.ID)
                    .Write(tavern.ID)
                    .Write(order.ID)
                    .Write(enabled);
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
                    net.BeginPacket(PacketOrderSync)
                        .Write(pl.ID)
                        .Write(tavern.ID)
                        .Write(order.ID)
                        .Write(enabled);
            }

            public static void UpdateOrderIngredients(NetEndpoint net, PlayerData player, Tavern tavern, CraftOrderOld order, string reagent, ItemDef[] defs, MaterialDef[] mats, MaterialTypeDef[] matTypes)
            {
                if (net is Server)
                    order.ToggleReagentRestrictions(reagent, defs, mats, matTypes);
                var w = net.BeginPacket(PacketOrderUpdateIngredients);

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

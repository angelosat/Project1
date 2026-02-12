using Project1.Core.Towns;
using Project1.Core.Components;
using Project1.Core.Helpers;
using Project1.Core.Legacy.Crafting.Packets;
using Project1.Core.Materials;
using Project1.Core.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Project1.Core.Entities;
using Project1.Framework.Serialization;
using Project1.Framework;
using Project1.Framework.Events;

namespace Project1.Core.Legacy.Crafting
{
    public class CraftingManagerOld : TownComponent
    {
        static readonly int pReaget, pPriority, pQuantity, pRestrictions;
        static CraftingManagerOld()
        {
            pReaget = Registry.PacketHandlers.Register(CraftingOrderToggleReagent);
            pPriority = Registry.PacketHandlers.Register(CraftingOrderModifyPriority);
            pQuantity = Registry.PacketHandlers.Register(CraftingOrderModifyQuantity);
            pRestrictions = Registry.PacketHandlers.Register(SetOrderRestrictions);
            PacketCraftOrderToggleHaul.Init();
            PacketCraftOrderChangeMode.Init();
        }

        internal IEnumerable<KeyValuePair<IntVec3, ICollection<CraftOrderOld>>> ByWorkstationNew()
        {
            return this.Map.GetBlockEntitiesCache()
                .Where(e => e.Value.HasComp<BlockEntityCompWorkstationOld>())
                .Select(r => new KeyValuePair<IntVec3, ICollection<CraftOrderOld>>(r.Key, r.Value.GetComp<BlockEntityCompWorkstationOld>().Orders));
        }
        internal BlockEntityCompWorkstationOld GetWorkstation(IntVec3 global)
        {
            return this.Map.GetBlockEntity(global)?.GetComp<BlockEntityCompWorkstationOld>();
        }

        public override string Name => "Crafting";
        int OrderSequence = 1;

        // TODO: add order priorities
        public CraftingManagerOld(Town town)
        {
            this.Town = town;
        }

        public static void SetOrderRestrictions(CraftOrderOld order, string reagent, ItemDef[] defs, MaterialDef[] mats, MaterialTypeDef[] matTypes)
        {
            var net = order.Map.Net;
            var w = net.BeginPacket(pRestrictions);
            w.Write(order.Workstation);
            w.Write(order.ID);
            w.Write(reagent);
            w.Write(defs?.Select(d => d.Name).ToArray());
            w.Write(mats?.Select(d => d.Name).ToArray());
            w.Write(matTypes?.Select(d => d.Name).ToArray());
        }
        private static void SetOrderRestrictions(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var benchEntity = net.Map.Town.CraftingManager.GetWorkstation(r.ReadIntVec3());
            var order = benchEntity.GetOrder(r.ReadInt32());
            var reagent = r.ReadString();
            var defs = r.ReadStringArray().Select(Def.GetDef<ItemDef>).ToArray();
            var mats = r.ReadStringArray().Select(Def.GetDef<MaterialDef>).ToArray();
            var matTypes = r.ReadStringArray().Select(Def.GetDef<MaterialTypeDef>).ToArray();
            order.ToggleReagentRestrictions(reagent, defs, mats, matTypes);
            if (net is Server)
            {
                SetOrderRestrictions(order, reagent, defs, mats, matTypes);
            }
        }


        static void CraftingOrderToggleReagent(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var global = r.ReadIntVec3();
            var orderID = r.ReadInt32();
            var benchEntity = net.Map.Town.CraftingManager.GetWorkstation(global);
            var order = benchEntity.GetOrder(orderID);

            var reagent = r.ReadString();
            var itemID = r.ReadInt32();
            var add = r.ReadBoolean();
            order.ToggleReagentRestriction(reagent, itemID, add);
            net.EventOccured((int)Message.Types.OrderParametersChanged, order);

            if (net is Server server)
                WriteOrderToggleReagent(server.OutgoingStreamOrderedReliable, order, reagent, itemID, add);
        }
        static void CraftingOrderModifyPriority(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var global = r.ReadIntVec3();
            var orderIndex = r.ReadInt32();
            var benchEntity = net.Map.Town.CraftingManager.GetWorkstation(global);
            var order = benchEntity.GetOrder(orderIndex);
            var increase = r.ReadBoolean();
            if (!benchEntity.Reorder(orderIndex, increase))
                return;
            if (net is Server server)
                WriteOrderModifyPriority(server.OutgoingStreamOrderedReliable, order, increase);
        }

        internal void RegisterOrder(CraftOrderOld ord)
        {
            this.RegistryOrders.Add(ord.ID, ord);
        }
        public CraftOrderOld GetOrder(int orderID)
        {
            return this.RegistryOrders[orderID];
        }
        static void CraftingOrderModifyQuantity(NetEndpoint net, Packet pck)
        {
            var r = pck.PacketReader;
            var global = r.ReadIntVec3();
            var orderid = r.ReadString();
            var benchEntity = net.Map.Town.CraftingManager.GetWorkstation(global);
            var order = benchEntity.GetOrder(orderid);

            var quantity = r.ReadInt32();
            var lastquantity = order.Quantity;
            order.Quantity += quantity;
            order.Quantity = Math.Max(order.Quantity, 0);
            if (order.Quantity == lastquantity)
                return;

            if (net is Server server)
                WriteOrderModifyQuantityParams(server.OutgoingStreamOrderedReliable, order, quantity);
        }

        public static void WriteOrderModifyQuantityParams(BinaryWriter w, CraftOrderOld order, int quantity)
        {
            w.Write(pQuantity);
            w.Write(order.Workstation);
            w.Write(order.GetUniqueLoadID());
            w.Write(quantity);
        }
        public static void WriteOrderToggleReagent(BinaryWriter w, CraftOrderOld order, string reagent, int item, bool add)
        {
            w.Write(pReaget);
            w.Write(order.Workstation);
            w.Write(order.ID);
            w.Write(reagent);
            w.Write(item);
            w.Write(add);
        }

        internal static void WriteOrderModifyPriority(BinaryWriter w, CraftOrderOld order, bool increase)
        {
            w.Write(pPriority);
            w.Write(order.Workstation);
            w.Write(order.ID);
            w.Write(increase);
        }
        readonly Dictionary<int, CraftOrderOld> RegistryOrders = new();

        internal CraftOrderOld RemoveOrder(IntVec3 station, int orderID)
        {
            var bench = this.GetWorkstation(station);
            this.RegistryOrders.Remove(orderID);
            return bench.RemoveOrder(orderID);
        }

        internal void AddOrder(IntVec3 station, Reaction reaction)// int reactionID)
        {
            var order = new CraftOrderOld(this.OrderSequence++, reaction, this.Town.Map, station);
            var benchEntity = this.Map.GetBlockEntity(station).GetComp<BlockEntityCompWorkstationOld>();
            benchEntity.Orders.Add(order);
            this.RegistryOrders.Add(order.ID, order);
        }
        internal bool OrderExists(CraftOrderOld craftOrderNew)
        {
            var orders = this.GetOrdersNew(craftOrderNew.Workstation);
            if (orders == null)
                return false;
            return orders.Contains(craftOrderNew);
        }

        public CraftOrderOld GetOrder(IntVec3 benchGlobal, int orderIndex)
        {
            return this.GetWorkstation(benchGlobal).GetOrder(orderIndex);
        }
        internal List<CraftOrderOld> GetOrdersNew(IntVec3 workstationGlobal)
        {
            var benchEntity = this.Map.GetBlockEntity(workstationGlobal).GetComp<BlockEntityCompWorkstationOld>();
            return benchEntity.Orders.ToList();
        }

        protected override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.OrderSequence.Save("IDSequence"));
        }
        public override void Load(SaveTag tag)
        {
            tag.TryGetTagValue<int>("IDSequence", v => this.OrderSequence = v);
        }

        public override void Write(IDataWriter w)
        {
            w.Write(this.OrderSequence);
        }
        public override void Read(IDataReader r)
        {
            this.OrderSequence = r.ReadInt32();
        }
    }
}

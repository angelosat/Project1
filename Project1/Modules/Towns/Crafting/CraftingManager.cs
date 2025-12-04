using Start_a_Town_.Components;
using Start_a_Town_.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Start_a_Town_
{
    public class CraftingManager : TownComponent
    {
        static readonly int pReaget, pPriority, pQuantity, pRestrictions;
        static CraftingManager()
        {
            pReaget = Start_a_Town_.Registry.PacketHandlers.Register(CraftingOrderToggleReagent);
            pPriority = Start_a_Town_.Registry.PacketHandlers.Register(CraftingOrderModifyPriority);
            pQuantity = Start_a_Town_.Registry.PacketHandlers.Register(CraftingOrderModifyQuantity);
            pRestrictions = Start_a_Town_.Registry.PacketHandlers.Register(SetOrderRestrictions);

            PacketOrderAdd.Init();
            //PacketOrderRemove.Init();
            //PacketCraftingOrderSync.Init();
            PacketCraftOrderToggleHaul.Init();
            PacketCraftOrderChangeMode.Init();
        }

        internal IEnumerable<KeyValuePair<IntVec3, ICollection<CraftOrder>>> ByWorkstationNew()
        {
            return this.Map.GetBlockEntitiesCache()
                .Where(e => e.Value.HasComp<BlockEntityCompWorkstation>())
                .Select(r => new KeyValuePair<IntVec3, ICollection<CraftOrder>>(r.Key, r.Value.GetComp<BlockEntityCompWorkstation>().Orders));
        }
        internal BlockEntityCompWorkstation GetWorkstation(IntVec3 global)
        {
            return this.Map.GetBlockEntity(global)?.GetComp<BlockEntityCompWorkstation>();
        }

        public override string Name => "Crafting";
        int OrderSequence = 1;

        // TODO: add order priorities

        public CraftingManager(Town town)
        {
            this.Town = town;
        }

        public static void SetOrderRestrictions(CraftOrder order, string reagent, ItemDef[] defs, MaterialDef[] mats, MaterialTypeDef[] matTypes)
        {
            var net = order.Map.Net;
            //var w = net.GetOutgoingStreamOrderedReliable();
            //w.Write(pRestrictions);
            var w = net.BeginPacket(ReliabilityType.OrderedReliable, pRestrictions);

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
            net.EventOccured(Message.Types.OrderParametersChanged, order);

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

        internal void RegisterOrder(CraftOrder ord)
        {
            this.Registry.Add(ord.ID, ord);
        }
        public CraftOrder GetOrder(int orderID)
        {
            return this.Registry[orderID];
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

        public static void WriteOrderModifyQuantityParams(BinaryWriter w, CraftOrder order, int quantity)
        {
            w.Write(pQuantity);
            w.Write(order.Workstation);
            w.Write(order.GetUniqueLoadID());
            w.Write(quantity);
        }
        public static void WriteOrderToggleReagent(BinaryWriter w, CraftOrder order, string reagent, int item, bool add)
        {
            w.Write(pReaget);
            w.Write(order.Workstation);
            w.Write(order.ID);
            w.Write(reagent);
            w.Write(item);
            w.Write(add);
        }

        internal static void WriteOrderModifyPriority(BinaryWriter w, CraftOrder order, bool increase)
        {
            w.Write(pPriority);
            w.Write(order.Workstation);
            w.Write(order.ID);
            w.Write(increase);
        }
        readonly Dictionary<int, CraftOrder> Registry = new();

        internal CraftOrder RemoveOrder(IntVec3 station, int orderID)
        {
            var bench = this.GetWorkstation(station);
            this.Registry.Remove(orderID);
            return bench.RemoveOrder(orderID);
        }

        internal void AddOrder(IntVec3 station, Reaction reaction)// int reactionID)
        {
            var order = new CraftOrder(this.OrderSequence++, reaction, this.Town.Map, station);
            var benchEntity = this.Map.GetBlockEntity(station).GetComp<BlockEntityCompWorkstation>();
            benchEntity.Orders.Add(order);
            this.Registry.Add(order.ID, order);
        }
        internal bool OrderExists(CraftOrder craftOrderNew)
        {
            var orders = this.GetOrdersNew(craftOrderNew.Workstation);
            if (orders == null)
                return false;
            return orders.Contains(craftOrderNew);
        }

        public CraftOrder GetOrder(IntVec3 benchGlobal, int orderIndex)
        {
            return this.GetWorkstation(benchGlobal).GetOrder(orderIndex);
        }
        internal List<CraftOrder> GetOrdersNew(IntVec3 workstationGlobal)
        {
            var benchEntity = this.Map.GetBlockEntity(workstationGlobal).GetComp<BlockEntityCompWorkstation>();
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

        public override void Write(BinaryWriter w)
        {
            w.Write(this.OrderSequence);
        }
        public override void Read(IDataReader r)
        {
            this.OrderSequence = r.ReadInt32();
        }
    }
}

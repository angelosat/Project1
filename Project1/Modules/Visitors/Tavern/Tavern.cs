using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;
using System.IO;

namespace Start_a_Town_
{
    public partial class Tavern : Workplace
    {
        bool NoonPassed;
        public override void Tick()
        {
            var time = this.Town.Map.World.Clock;
            if (!this.NoonPassed && time.Hours == 12)
            {
                //unrent rooms
                foreach (var bedroom in this.GetRooms().Where(r => r.RoomRole == RoomRoleDefOf.Bedroom))
                {
                    bedroom.Owner?.Possessions.Unclaim(bedroom);
                }
                this.NoonPassed = true;
            }
            else if (this.NoonPassed && time.Hours == 0)
            {
                this.NoonPassed = false;
            }
        }

        internal bool IsInnkeeperServicing()
        {
            var map = this.Town.Map;
            return this.WorkerProps.Values.Any(p =>
            {
                var actor = map.Net.GetNetworkObject<Actor>(p.ActorID);
                return
                    p.GetJob(JobInnKeeper).Enabled &&
                    actor.GetState().CurrentTaskBehavior is TaskBehaviorInnKeeper &&
                    actor.CellIfSpawned.Value == map.GetBehindOfBlock(this.Counter.Value);
            });
        }

        internal Room GetFreeBedroom(int maxValue)
        {
            return this.GetRooms().FirstOrDefault(r =>
                r.RoomRole == RoomRoleDefOf.Bedroom &&
                r.Owner is null &&
                r.Value <= maxValue && // TODO order by value
                !IsReserved(r));
        }

        private bool IsReserved(Room r)
        {
            return this.Customers.Any(c => c.Bedroom == r);
        }

        static public readonly JobDef JobWaiter = new("JobTavernWaiter", new TaskGiverTavernWaiter());
        static public readonly JobDef JobCook = new("JobTavernCook", new TaskGiverTavernCook());
        static public readonly JobDef JobInnKeeper = new("JobTavernInnKeeper", new TaskGiverInnKeeper());
        static public readonly JobDef[] RolesAll = { JobWaiter, JobCook, JobInnKeeper };

        static public readonly HashSet<RoomRoleDef> ValidRooms = new() { RoomRoleDefOf.Bedroom };
        static Tavern()
        {
            Packets.Init();

            Def.Register(JobWaiter);
            Def.Register(JobCook);
            Def.Register(JobInnKeeper);
        }
        public static void Init() { }
        readonly HashSet<IntVec3> Tables = new();
        readonly HashSet<IntVec3> Workstations = new();
        readonly public List<CustomerTavern> Customers = new();

        public bool CanOfferBed => this.Counter.HasValue; // TODO also check for bed availability

        public override bool IsValidRoom(Room room)
        {
            return ValidRooms.Contains(room.RoomRole);
        }
        
        public override IEnumerable<JobDef> GetRoleDefs()
        {
            foreach (var r in RolesAll)
                yield return r;
        }
        
        readonly List<CraftOrder> Orders = new();
        int MenuItemIDSequence = 1;
        public Tavern()
        {

        }
        public Tavern(TownComponent manager, int id) : base(manager, id)
        {
            this.Name = string.Format("Tavern{0}", this.ID);
        }
        public Tavern(TownComponent manager) : base(manager)
        {
        }
        
        public override CraftOrder GetOrder(int orderid)
        {
            return this.Orders.First(o => o.ID == orderid);
        }
        public void AddOrder(CraftOrder order)
        {
            this.Orders.Add(order);
            this.Town.Net.EventOccured(Components.Message.Types.TavernMenuChanged, this, new CraftOrder[] { order }, new CraftOrder[] { });
        }
        public void RemoveOrder(CraftOrder order)
        {
            this.Orders.Remove(order);
            this.Town.Net.EventOccured(Components.Message.Types.TavernMenuChanged, this, new CraftOrder[] {  }, new CraftOrder[] { order });
            this.Town.Net.EventOccured(Components.Message.Types.OrderDeleted, order);
        }
        public void RemoveOrder(int orderid)
        {
            var order = this.GetOrder(orderid);
            this.Orders.Remove(order);
            this.Town.Net.EventOccured(Components.Message.Types.TavernMenuChanged, this, new CraftOrder[] { }, new CraftOrder[] { order });
            this.Town.Net.EventOccured(Components.Message.Types.OrderDeleted, order);
        }
        public void AddTable(IntVec3 global)
        {
            this.Tables.Add(global);
        }
        public void AddWorkstation(IntVec3 global)
        {
            this.Workstations.Add(global);
        }
        public CustomerTavern AddCustomer(Actor customer, Room freeBedroom)
        {
            var customerProps = new CustomerTavern(this, customer.RefID, freeBedroom);
            this.Customers.Add(customerProps);
            return customerProps;
        }
        public CustomerTavern AddCustomer(Actor customer, IntVec3 table, VisitorCraftRequest request)
        {
            var customerProps = new CustomerTavern(this, customer.RefID, table, request);
            this.Customers.Add(customerProps);
            return customerProps;
        }
        
        internal void RemoveCustomer(int customerID)
        {
            this.Customers.RemoveAll(p => p.CustomerID == customerID);
        }
        public CustomerTavern GetCustomerProperties(Actor actor)
        {
            return this.Customers.First(c => c.CustomerID == actor.RefID);
        }
        public CustomerTavern GetCustomerProperties(int actorID)
        {
            return this.Customers.First(c => c.CustomerID == actorID);
        }
        public IntVec3? GetAvailableTable()
        {
            foreach (var table in this.Tables)
                if (!this.Customers.Any(c => c.Table == table))
                    return table;
            return null;
        }
        public bool TryGetAvailableTable(out IntVec3 table)
        {
            foreach (var t in this.Tables)
                if (!this.Customers.Any(c => c.Table == t))
                {
                    table = t;
                    return true;
                }
            table = default;
            return false;
        }
        public IEnumerable<CraftOrder> GetAvailableOrders()
        {
            foreach(var o in this.Orders)
                yield return o;
        }
        public IntVec3? GetAvailableKitchen()
        {
            return this.Workstations.FirstOrDefault(this.Town.ReservationManager.IsReserved);
        }
        public override bool IsAllowed(Block block)
        {
            return (block == BlockDefOf.Kitchen) || block is BlockStool || block is BlockShopCounter;
        }
        internal override void AddFacility(IntVec3 global)
        {
            var block = this.Town.Map.GetBlock(global);
            if (block is BlockStool)
                this.Tables.Add(global);
            else if (block == BlockDefOf.Kitchen)
                this.Workstations.Add(global);
            else if (block is BlockShopCounter)
                this.Counter = global;
            else
                throw new Exception();
        }
        
        public CustomerProperties GetCustomer(Actor customer)
        {
            return this.Customers.FirstOrDefault(c => c.CustomerID == customer.RefID);
        }
        public override IEnumerable<IntVec3> GetFacilities()
        {
            foreach (var v in this.Workstations)
                yield return v;
            foreach (var v in this.Tables)
                yield return v;
        }
        static GroupBox OrdersGUI;
        static GroupBox CreateOrdersGUI()
        {
            var ordersBox = new GroupBox() { Name = "Orders" };
            Tavern tav = null;
            var ordersTable = new TableScrollableCompact<CraftOrder>()
                .AddColumn(new(), "enabled", UIManager.TextureTickBox.Width, o => new CheckBoxNew() { TickedFunc = () => o.Enabled, LeftClickAction = () => Packets.SendOrderSync(tav.Town.Net, tav.Town.Net.GetPlayer(), tav, o, !o.Enabled) })
                .AddColumn(new(), "name", 130, o => new Label(o.Name, () => o.ShowDetailsUI((o, n, i, m, t) => Packets.UpdateOrderIngredients(tav.Town.Net, tav.Town.Net.GetPlayer(), tav, o, n, i, m, t))), 0)
                .AddColumn(new(), "price", 70 - Icon.Cross.SourceRect.Width, o => new Label("-"))
                .AddColumn(new(), "remove", Icon.Cross.SourceRect.Width, o => IconButton.CreateSmall(Icon.Cross, () => Packets.SendRemoveOrder(tav.Town.Net, tav.Town.Net.GetPlayer(), tav, o)));

            ordersBox.SetGetDataAction(t =>
            {
                tav = t as Tavern;
                ordersTable.ClearItems().AddItems(tav.Orders);
            });
            ordersTable.OnGameEventAction = e =>
            {
                switch (e.Type)
                {
                    case Components.Message.Types.TavernMenuChanged:
                        var t = e.Parameters[0] as Tavern;
                        if (tav != t)
                            break;
                        var ordersAdded = e.Parameters[1] as CraftOrder[];
                        ordersTable.AddItems(ordersAdded);
                        var ordersRemoved = e.Parameters[2] as CraftOrder[];
                        ordersTable.RemoveItems(ordersRemoved);
                        break;

                    default:
                        break;
                }
            };
            var recipeListGui = new ListBoxNoScroll<Reaction, Button>(r =>
                new Button(r.Name, () => Packets.SendAddMenuItem(tav.Town.Net, tav.Town.Net.GetPlayer(), tav, r))).HideOnAnyClick();
            var btnAdd = new Button("Add menu item", () => recipeListGui.Toggle());

            ordersBox.AddControlsVertically(ordersTable, btnAdd);
            return ordersBox;
        }
       
        protected override IEnumerable<GroupBox> GetUI()
        {
            yield return OrdersGUI ??= CreateOrdersGUI();
        }

        protected override void ResolveExtraReferences()
        {
            this.NoonPassed = this.Town.Map.Clock.Hours >= 12;
            this.Tables.RemoveWhere(g => this.Town.Map.GetBlock(g) is not BlockStool);
            this.Workstations.RemoveWhere(g => this.Town.Map.GetBlock(g) != BlockDefOf.Kitchen);
        }
        internal override void OnBlocksChanged(IEnumerable<IntVec3> positions)
        {
            foreach(var pos in positions)
            {
                var block = this.Map.GetBlock(pos);
                if (this.Tables.Contains(pos) && block is not BlockStool)
                    this.Tables.Remove(pos);
                else if (this.Workstations.Contains(pos) && block != BlockDefOf.Kitchen)
                    this.Workstations.Remove(pos);
            }
        }
        protected override void SaveExtra(SaveTag tag)
        {
            this.Tables.Save(tag, "Tables");
            this.Workstations.Save(tag, "Kitchens");
            this.Customers.SaveNewBEST(tag, "Customers");
            this.Orders.SaveNewBEST(tag, "Orders");
        }
        protected override void WriteExtra(BinaryWriter w)
        {
            w.Write(this.Tables);
            w.Write(this.Workstations);
            this.Customers.Write(w);
            this.Orders.Write(w);
        }
        protected override void LoadExtra(SaveTag tag)
        {
            this.Tables.Load(tag, "Tables");
            this.Workstations.Load(tag, "Kitchens");
            this.Customers.Load(tag, "Customers", this);
            this.Orders.TryLoad(tag, "Orders");
        }
        protected override void ReadExtra(BinaryReader r)
        {
            this.Tables.Read(r);
            this.Workstations.Read(r);
            this.Customers.InitializeNew(r, this);
            this.Orders.Read(r);
        }
    }
}

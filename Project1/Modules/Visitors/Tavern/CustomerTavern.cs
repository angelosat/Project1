using System.IO;

namespace Start_a_Town_
{
    public class CustomerTavern : CustomerProperties
    {
        public IntVec3 Table;
        public VisitorCraftRequest CraftRequest;
        public int TicksWaited;
        public Tavern Tavern;
        public bool IsSeated;
        public int DishID = -1;

        public int ServedByID = -1, OrderTakenByID = -1;
        public bool IsServed => this.ServedByID != -1;
        public bool IsOrderTaken => this.OrderTakenByID != -1;

        Actor CachedCustomer, CachedServedBy, CachedOrderTakenBy;
        Entity CachedDish;
        public Room Bedroom;

        public Actor Customer
        {
            get { return CachedCustomer ??= (CustomerID != -1 ? this.Tavern.Town.Net.GetNetworkEntity(this.CustomerID) as Actor : null); }
            set
            {
                this.CustomerID = value?.RefId ?? -1;
                this.CachedCustomer = value;
            }
        }
        public Actor ServedBy
        {
            get { return CachedServedBy ??= (ServedByID != -1 ? this.Tavern.Town.Net.GetNetworkEntity(this.ServedByID) as Actor : null); }
            set
            {
                this.ServedByID = value?.RefId ?? -1;
                this.CachedServedBy = value;
            }
        }
        public Actor OrderTakenBy
        {
            get
            {
                return CachedOrderTakenBy ??= (OrderTakenByID != -1 ? this.Tavern.Town.Net.GetNetworkEntity(this.OrderTakenByID) as Actor : null);
            }
            set
            {
                this.OrderTakenByID = value?.RefId ?? -1;
                this.CachedOrderTakenBy = value;
            }
        }
        public Entity Dish
        {
            get { return this.CachedDish ??= (DishID != -1 ? this.Tavern.Town.Net.GetNetworkEntity(this.DishID) as Entity : null); }
            set { this.DishID = value?.RefId ?? -1; }
        }


        public CustomerTavern(Tavern tavern)
        {
            this.Tavern = tavern;
        }
        public CustomerTavern(Tavern tavern, int actorID, IntVec3 table, VisitorCraftRequest request) : this(tavern)
        {
            CustomerID = actorID;
            Table = table;
            this.CraftRequest = request;
        }

        public CustomerTavern(Tavern tavern, int actorID, Room bedroom) : this(tavern)
        {
            this.CustomerID = actorID;
            this.Bedroom = bedroom;
        }
        protected override void SaveExtra(SaveTag save)
        {
            this.Table.Save(save, "Table");
            this.TicksWaited.Save(save, "TicksWaited");
            this.ServedByID.Save(save, "ServedBy");
            this.OrderTakenByID.Save(save, "OrderTakenBy");
        }
        protected override void LoadExtra(SaveTag save)
        {
            this.Table = save.LoadIntVec3("Table");
            this.TicksWaited = save.GetValue<int>("TicksWaited");
            this.ServedByID = save.GetValue<int>("ServedBy");
            this.OrderTakenByID = save.GetValue<int>("OrderTakenBy");
        }
        protected override void WriteExtra(BinaryWriter w)
        {
            w.Write(this.Table);
            w.Write(this.TicksWaited);
            w.Write(this.ServedByID);
            w.Write(this.OrderTakenByID);
        }
        protected override void ReadExtra(BinaryReader r)
        {
            this.Table = r.ReadIntVec3();
            this.TicksWaited = r.ReadInt32();
            this.ServedByID = r.ReadInt32();
            this.OrderTakenByID = r.ReadInt32();
        }
    }
}

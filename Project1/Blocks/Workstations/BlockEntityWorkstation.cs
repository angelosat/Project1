using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public abstract class BlockEntityWorkstation : BlockEntity
    {
        protected BlockEntityWorkstation(IntVec3 originGlobal) : base(originGlobal)
        {
        }
        //public override IEnumerable<IntVec3> InteractionSpots => BlockDefOf.Workbench.GetInteractionSpotsLocal(this.Map.GetCell(this.OriginGlobal).Orientation).Select(c=>this.OriginGlobal + c);
    }
    //public abstract class BlockEntityWorkstation : BlockEntity
    //{
    //    /// <summary>
    //    /// make this a property that returns first item from this.getorders(this.global) ?
    //    /// </summary>
    //    public GameObject CurrentWorker;
    //    public List<CraftOrder> Orders = new List<CraftOrder>();
    //    public bool ExecutingOrders;
    //    public abstract Container Input { get; }
    //    public abstract IsWorkstation.Types Type { get; }

    //    public BlockEntityWorkstation(IntVec3 originGlobal)
    //        : base(originGlobal)
    //    {

    //    }

    //    public override IEnumerable<IntVec3> InteractionSpots => BlockDefOf.Workbench.GetOperatingPositions(this.Map.GetCell(this.OriginGlobal).Orientation);

    //    internal CraftOrder GetOrder(string uniqueID)
    //    {
    //        return this.Orders.Find(o => o.GetUniqueLoadID() == uniqueID);
    //    }
    //    internal CraftOrder GetOrder(int uniqueID)
    //    {
    //        return this.Orders.Find(o => o.ID == uniqueID);
    //    }
    //    internal bool Reorder(int orderID, bool increasePriority)
    //    {
    //        var order = this.GetOrder(orderID);
    //        if (order == null)
    //            return false;
    //        var prevIndex = this.Orders.IndexOf(order);
    //        this.Orders.Remove(order);
    //        var newIndex = Math.Max(0, Math.Min(this.Orders.Count, prevIndex + (increasePriority ? -1 : 1)));
    //        this.Orders.Insert(newIndex, order);
    //        return true;
    //    }
    //    public override List<GameObjectSlot> GetChildren()
    //    {
    //        if (this.Input != null)
    //            return this.Input.Slots;
    //        return new List<GameObjectSlot>();
    //    }

    //    public bool Insert(Entity material)
    //    {
    //        return this.Input.Insert(material);
    //    }

    //    protected override void AddSaveData(SaveTag tag)
    //    {
    //        tag.TrySaveRefs(this.Orders, "Orders");
    //    }
    //    protected override void LoadExtra(SaveTag tag)
    //    {
    //        tag.TryLoadRefs("Orders", ref this.Orders);
    //    }
    //    protected override void WriteExtra(BinaryWriter w)
    //    {
    //        this.Orders.Write(w);
    //    }
    //    protected override void ReadExtra(BinaryReader r)
    //    {
    //        this.Orders.Read(r);
    //    }
    //}
}

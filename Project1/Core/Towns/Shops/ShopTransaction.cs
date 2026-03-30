using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.Serialization;

#nullable enable

namespace Project1.Core.Towns.Shops;
internal record struct ShopTransactionUpdatedEvent(MapBase Map, ShopTransaction Transaction) : IEventPayload { }
sealed class ShopTransaction : ISerializable// ISerializableNew<ShopTransaction>
{
    internal enum TransactionState
    {
        Queuing, WaitingForPayment, Paid, Processed, Complete, Disposed, Cancelled
    }
    internal TransactionState State;
    bool _cancelled;
    public EntityRefId Buyer { get; private set; }
    public EntityRefId Seller = EntityRefId.Null;
    public EntityRefId Item { get; private set; }
    public EntityRefId Money = EntityRefId.Null;
    public int Price;
    public IntVec3 Counter { get; private set; }
    double TicksRemaining = Ticks.FromHours(1);

    ShopTransaction() { }
    public ShopTransaction(Actor buyer, Entity item, int price, IntVec3 counter)
    {
        this.Buyer = buyer.RefId;
        this.Item = item.RefId;
        this.Price = price;
        this.Counter = counter;
    }

    internal bool IsCancelled => this._cancelled;
    internal bool IsDisposed => this.State == TransactionState.Disposed;
    internal bool IsComplete => this.State == TransactionState.Complete;
    internal bool IsProcessed => this.State == TransactionState.Processed;
    internal bool IsPaid => this.State == TransactionState.Paid;
    internal bool WaitingForPayment => this.State == TransactionState.WaitingForPayment;
    public bool TimedOut => this.TicksRemaining <= 0;

    internal void Cancel()
        => this._cancelled = true;
    internal void Tick()
    {
        if (this.TicksRemaining <= 0)
            return;
        this.TicksRemaining--;
    }
    internal void RefreshTimer()
        => this.TicksRemaining = Ticks.FromHours(1);
    internal void MarkPaid()
    {
        if (this.State != TransactionState.WaitingForPayment)
            throw new System.Exception();
        this.State = TransactionState.Paid;
    }
    internal void MarkProcessed()
    {
        if (this.State != TransactionState.Paid)
            throw new System.Exception();
        this.State = TransactionState.Processed;
    }
    internal void MarkComplete()
    {
        if (this.State != TransactionState.Processed)
            throw new System.Exception();
        this.State = TransactionState.Complete;
    }
    internal void RingUp()
    {
        if (this.State != TransactionState.Queuing)
            throw new System.Exception();
        this.State = TransactionState.WaitingForPayment;
    }
    internal void Dispose() => this.State = TransactionState.Disposed;

    public void Write(IDataWriter w)
    {
        w.Write(this.Buyer);
        w.Write(this.Seller);
        w.Write(this.Item);
        w.Write(this.Money);
        w.Write(this.Counter);
        w.Write(this.Price);
        w.Write((int)this.State);
    }

    public ISerializable Read(IDataReader r)
    {
        this.Buyer = r.ReadEntityRefId();
        this.Seller = r.ReadEntityRefId();
        this.Item = r.ReadEntityRefId();
        this.Money = r.ReadEntityRefId();
        this.Counter = r.ReadIntVec3();
        this.Price = r.ReadInt32();
        this.State = (TransactionState)r.ReadInt32();
        return this;
    }

    //public static ShopTransaction Create(IDataReader r)
    //{
    //    var buyer = r.ReadEntityRefId();
    //    var seller = r.ReadEntityRefId();
    //    var item = r.ReadEntityRefId();
    //    var money = r.ReadEntityRefId();
    //    var counter = r.ReadIntVec3();
    //    var price = r.ReadInt32();
    //    var state = (TransactionState)r.ReadInt32();

    //    return new ShopTransaction()
    //    {
    //        Buyer = buyer,
    //        Seller = seller,
    //        Item = item,
    //        Money = money,
    //        Counter = counter,
    //        Price = price,
    //        State = state
    //    };
    //}

    //public ShopTransaction Read(IDataReader r)
    //{

    //    this.Buyer = r.ReadEntityRefId();
    //    this.Seller = r.ReadEntityRefId();
    //    this.Item = r.ReadEntityRefId();
    //    this.Money = r.ReadEntityRefId();
    //    this.Counter = r.ReadIntVec3();
    //    this.Price = r.ReadInt32();
    //    this.State = (TransactionState)r.ReadInt32();
    //    return this;
    //}
}

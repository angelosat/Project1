using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Core.Towns.Services;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.Serialization;

#nullable enable

namespace Project1.Core.Towns.Shops;
public interface ITownServiceTransaction
{
    EntityRefId Buyer { get; }
    EntityRefId Seller { get; }
    TownServiceDef Service { get; }
    double TickStarted { get; }
    bool IsSucceeded { get; }
    bool IsFailed { get; }
    void Write(IDataWriter w);
    void Read(IDataReader r);
}
internal record struct ShopTransactionUpdatedEvent(MapBase Map, ITownServiceTransaction Transaction) : IEventPayload { }
internal record struct ShopTransactionFinishedEvent(MapBase Map, ITownServiceTransaction Transaction) : IEventPayload { }
sealed class ShopTransaction : ITownServiceTransaction
{
    internal enum TransactionState
    {
        Queuing, WaitingForPayment, Paid, Processed, Complete, Succeeded, Failed
    }
    internal TransactionState State;
    bool _cancelled;
    public EntityRefId Buyer { get; private set; }
    public EntityRefId Seller { get; set; } = EntityRefId.Null;
    public EntityRefId Item { get; private set; }
    public EntityRefId Money = EntityRefId.Null;
    public int Price;
    public IntVec3 Counter { get; private set; }
    double TicksRemaining = Ticks.FromHours(1);
    public double TickStarted { get; set; }
    public TownServiceDef Service => TownServiceDefOf.Selling;

    ShopTransaction() { }
    public ShopTransaction(double tickStarted, Actor buyer, Entity item, int price, IntVec3 counter)
    {
        this.Buyer = buyer.RefId;
        this.Item = item.RefId;
        this.Price = price;
        this.Counter = counter;
        this.TickStarted = tickStarted;
    }

    public bool IsFailed => this.State == TransactionState.Failed;
    public bool IsSucceeded => this.State == TransactionState.Succeeded;
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
    internal void MarkCancelled()
    {
        this.State = TransactionState.Failed;
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
    internal void Dispose() => this.State = TransactionState.Succeeded;

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

    public void Read(IDataReader r)
    {
        this.Buyer = r.ReadEntityRefId();
        this.Seller = r.ReadEntityRefId();
        this.Item = r.ReadEntityRefId();
        this.Money = r.ReadEntityRefId();
        this.Counter = r.ReadIntVec3();
        this.Price = r.ReadInt32();
        this.State = (TransactionState)r.ReadInt32();
    }
}

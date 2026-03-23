using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Framework;

#nullable enable

namespace Project1.Core.Towns.Shops;


sealed class ShopTransaction(Actor buyer, Entity item, IntVec3 counter)
{
    internal enum TransactionState
    {
        Unstarted, WaitingForPayment, Paid, Complete, Disposed, Cancelled
    }
    internal TransactionState State;
    bool _cancelled;
    public readonly EntityRefId Buyer = buyer.RefId;
    public EntityRefId Seller = EntityRefId.Null;
    public readonly EntityRefId Item = item.RefId;
    public EntityRefId Money = EntityRefId.Null;
    public readonly IntVec3 Counter = counter;
    double TicksRemaining = Ticks.FromHours(1);
    internal bool IsCancelled => this._cancelled;
    internal bool IsDisposed => this.State == TransactionState.Disposed;
    internal bool IsComplete => this.State == TransactionState.Complete;
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
    internal void MarkPaid() => this.State = TransactionState.Paid;
    internal void RingUp() => this.State = TransactionState.WaitingForPayment;
    //internal void RingUpFinish() => this.State = TransactionState.Complete;
    internal void Dispose() => this.State = TransactionState.Disposed;

}

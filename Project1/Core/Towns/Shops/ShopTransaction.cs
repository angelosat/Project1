using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Core.Towns.Services;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.Serialization;
using System;

#nullable enable

namespace Project1.Core.Towns.Shops;
internal record struct ShopTransactionUpdatedEvent(MapBase Map, TownServiceRequest Transaction) : IEventPayload { }
internal record struct TownServiceComplete(MapBase Map, TownServiceRequest Transaction) : IEventPayload { }
public sealed class ShopTransaction : TownServiceRequest
{
    internal enum TransactionState
    {
        Queuing, WaitingForPayment, Paid, Processed, Complete, Succeeded, Failed
    }
    internal TransactionState State;
    bool _cancelled;
    EntityRefId _buyerInt, _sellerInt = EntityRefId.Null;
    internal override EntityRefId Buyer => this._buyerInt;//{ get; }
    internal override EntityRefId Seller => this._sellerInt;//{ get; } = EntityRefId.Null;
    public EntityRefId Item { get; private set; }
    public EntityRefId Money = EntityRefId.Null;
    public int Price;
    public IntVec3 Counter { get; private set; }
    double TicksRemaining = Ticks.FromHours(1);
    //internal override SimulationTick TickStarted { get; set; }
    internal override TownServiceDef Service => TownServiceDefOf.Buying;
    //internal override int PatienceInitial { get; set; }
    public ShopTransaction() { }
    public ShopTransaction(SimulationTick tickStarted, int patienceSnapshot, Actor buyer, Entity item, int price, IntVec3 counter) : base(buyer)
    {
        this._buyerInt = buyer.RefId;
        this.Item = item.RefId;
        this.Price = price;
        this.Counter = counter;
        //this.TickStarted = tickStarted;
        //this.PatienceInitial = patienceSnapshot;
    }

    internal override bool IsFailed => this.State == TransactionState.Failed;
    internal override bool IsSucceeded => this.State == TransactionState.Succeeded;
    internal bool IsComplete => this.State == TransactionState.Complete;
    internal bool IsProcessed => this.State == TransactionState.Processed;
    internal bool IsPaid => this.State == TransactionState.Paid;
    internal bool WaitingForPayment => this.State == TransactionState.WaitingForPayment;
    public bool TimedOut => this.TicksRemaining <= 0;

    internal void SetSeller(Actor seller)
        => this._sellerInt = seller.RefId;
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

    protected override void WriteExtra(IDataWriter w)
    {
        //w.Write(this.TickStarted);
        //w.Write(this.PatienceInitial);
        w.Write(this.Buyer);
        w.Write(this.Seller);
        w.Write(this.Item);
        w.Write(this.Money);
        w.Write(this.Counter);
        w.Write(this.Price);
        w.Write((int)this.State);
    }

    protected override void ReadExtra(IDataReader r)
    {
        //this.TickStarted = (SimulationTick)r.ReadUInt64();
        //this.PatienceInitial = r.ReadInt32();
        this._buyerInt = r.ReadEntityRefId();
        this._sellerInt = r.ReadEntityRefId();
        this.Item = r.ReadEntityRefId();
        this.Money = r.ReadEntityRefId();
        this.Counter = r.ReadIntVec3();
        this.Price = r.ReadInt32();
        this.State = (TransactionState)r.ReadInt32();
    }

}

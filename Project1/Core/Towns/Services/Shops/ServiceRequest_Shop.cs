using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Events;
using System;

#nullable enable

namespace Project1.Core.Towns.Services.Shops;
internal record struct ShopTransactionUpdatedEvent(MapBase Map, ServiceRequest Transaction) : IEventPayload { }
internal record struct TownServiceCompleteEvent(MapBase Map, ServiceRequest Transaction) : IEventPayload { }
public sealed class ServiceRequest_Shop : ServiceRequest
{
    //internal enum TransactionState
    //{
    //    Queuing, WaitingForPayment, Paid, Processed, Complete, Succeeded, Failed
    //}
    //internal TransactionState State;
    //bool _cancelled;
    public EntityRefId Item { get; private set; }
    double TicksRemaining = Ticks.FromHours(1);
    internal override TownServiceDef Service => TownServiceDefOf.Buying;
    public ServiceRequest_Shop() { }
    public ServiceRequest_Shop(Actor buyer, Entity item, int price, IntVec3 counter) : base(buyer, price, counter)
    {
        this.Item = item.RefId;
    }

    //internal bool IsComplete => this.State == TransactionState.Complete;
    //internal bool IsProcessed => this.State == TransactionState.Processed;
    //internal bool IsPaid => this.State == TransactionState.Paid;
    //internal bool WaitingForPayment => this.State == TransactionState.WaitingForPayment;
    public bool TimedOut => this.TicksRemaining <= 0;

    //internal void SetSeller(Actor seller)
    //    => this.Vendor = seller.RefId;
    //internal void Cancel()
    //    => this._cancelled = true;
    internal void Tick()
    {
        if (this.TicksRemaining <= 0)
            return;
        this.TicksRemaining--;
    }
    internal void RefreshTimer()
        => this.TicksRemaining = Ticks.FromHours(1);
    //internal void MarkPaid()
    //{
    //    if (this.State != TransactionState.WaitingForPayment)
    //        throw new System.Exception();
    //    this.State = TransactionState.Paid;
    //}
    //internal void MarkCancelled()
    //{
    //    this.State = TransactionState.Failed;
    //}
    //internal void MarkProcessed()
    //{
    //    if (this.State != TransactionState.Paid)
    //        throw new System.Exception();
    //    this.State = TransactionState.Processed;
    //}
    //internal void MarkComplete()
    //{
    //    if (this.State != TransactionState.Processed)
    //        throw new System.Exception();
    //    this.State = TransactionState.Complete;
    //}
    //internal void RingUp()
    //{
    //    if (this.State != TransactionState.Queuing)
    //        throw new System.Exception();
    //    this.State = TransactionState.WaitingForPayment;
    //}
    //internal void Dispose() => this.State = TransactionState.Succeeded;

    //protected override void SaveExtra(SaveTag tag)
    //{
    //    tag.Save("Item", this.Item);
    //    tag.Save("State", (int)this.State);
    //}

    //protected override void LoadExtra(SaveTag tag)
    //{
    //    this.Item = tag.LoadEntityRefId("Item");
    //    this.Money = tag.LoadEntityRefId("Money");
    //    this.State = (TransactionState)tag.LoadInt("State");
    //}

    //protected override void WriteExtra(IDataWriter w)
    //{
    //    w.Write(this.Item);
    //    w.Write(this.Money);
    //    w.Write((int)this.State);
    //}

    //protected override void ReadExtra(IDataReader r)
    //{
    //    this.Item = r.ReadEntityRefId();
    //    this.Money = r.ReadEntityRefId();
    //    this.State = (TransactionState)r.ReadInt32();
    //}
}
//public sealed class ServiceRequest_Shop : ServiceRequest
//{
//    internal enum TransactionState
//    {
//        Queuing, WaitingForPayment, Paid, Processed, Complete, Succeeded, Failed
//    }
//    internal TransactionState State;
//    bool _cancelled;
//    public EntityRefId Item { get; private set; }
//    double TicksRemaining = Ticks.FromHours(1);
//    internal override TownServiceDef Service => TownServiceDefOf.Buying;
//    public ServiceRequest_Shop() { }
//    public ServiceRequest_Shop(Actor buyer, Entity item, int price, IntVec3 counter) : base(buyer, price, counter)
//    {
//        this.Item = item.RefId;
//    }

//    internal bool IsComplete => this.State == TransactionState.Complete;
//    internal bool IsProcessed => this.State == TransactionState.Processed;
//    internal bool IsPaid => this.State == TransactionState.Paid;
//    internal bool WaitingForPayment => this.State == TransactionState.WaitingForPayment;
//    public bool TimedOut => this.TicksRemaining <= 0;

//    internal void SetSeller(Actor seller)
//        => this.Vendor = seller.RefId;
//    internal void Cancel()
//        => this._cancelled = true;
//    internal void Tick()
//    {
//        if (this.TicksRemaining <= 0)
//            return;
//        this.TicksRemaining--;
//    }
//    internal void RefreshTimer()
//        => this.TicksRemaining = Ticks.FromHours(1);
//    internal void MarkPaid()
//    {
//        if (this.State != TransactionState.WaitingForPayment)
//            throw new System.Exception();
//        this.State = TransactionState.Paid;
//    }
//    internal void MarkCancelled()
//    {
//        this.State = TransactionState.Failed;
//    }
//    internal void MarkProcessed()
//    {
//        if (this.State != TransactionState.Paid)
//            throw new System.Exception();
//        this.State = TransactionState.Processed;
//    }
//    internal void MarkComplete()
//    {
//        if (this.State != TransactionState.Processed)
//            throw new System.Exception();
//        this.State = TransactionState.Complete;
//    }
//    internal void RingUp()
//    {
//        if (this.State != TransactionState.Queuing)
//            throw new System.Exception();
//        this.State = TransactionState.WaitingForPayment;
//    }
//    internal void Dispose() => this.State = TransactionState.Succeeded;

//    protected override void SaveExtra(SaveTag tag)
//    {
//        tag.Save("Item", this.Item);
//        tag.Save("State", (int)this.State);
//    }

//    protected override void LoadExtra(SaveTag tag)
//    {
//        this.Item = tag.LoadEntityRefId("Item");
//        this.Money = tag.LoadEntityRefId("Money");
//        this.State = (TransactionState)tag.LoadInt("State");
//    }

//    protected override void WriteExtra(IDataWriter w)
//    {
//        w.Write(this.Item);
//        w.Write(this.Money);
//        w.Write((int)this.State);
//    }

//    protected override void ReadExtra(IDataReader r)
//    {
//        this.Item = r.ReadEntityRefId();
//        this.Money = r.ReadEntityRefId();
//        this.State = (TransactionState)r.ReadInt32();
//    }
//}

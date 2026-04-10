using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Towns.Services;
using Project1.Framework;
using Project1.Framework.Serialization;
using System;

namespace Project1.Core.Towns.Services.Inns;

public sealed class ServiceRequest_Inn : ServiceRequest
{
    enum States { Queuing, AwaitingPayment, Paid, Processed, Succeeded, Failed }
    internal EntityRefId Money;
    internal override TownServiceDef Service => TownServiceDefOf.Lodging;

    public IntVec3 Desk { get; private set; }
    States State;

    public ServiceRequest_Inn(Actor guest, IntVec3 desk) : base(guest)
    {
        Desk = desk;
    }

    public ServiceRequest_Inn()
    {
    }

    public bool IsAwaitingPayment => this.State == States.AwaitingPayment;
    public bool IsPaid => this.State == States.Paid;
    public bool IsProcessed => this.State == States.Processed;
    internal override bool IsSucceeded => this.State == States.Succeeded;
    internal override bool IsFailed => this.State == States.Failed;

    internal void AssignClerk(Actor clerk)
    {
        if (this.State != States.Queuing)
            throw new Exception();
        this.Vendor = clerk.RefId;
        this.State = States.AwaitingPayment;
    }
    internal void MarkFailed()
        => this.State = States.Failed;
    internal void MarkFinished()
    {
        if (this.State != States.Processed)
            throw new Exception();
        this.State = States.Succeeded;
    }
    internal void MarkPaid(Entity money)
    {
        if (this.State != States.AwaitingPayment)
            throw new Exception();
        this.Money = money.RefId;
        this.State = States.Paid;
    }

    internal void MarkProcessed()
    {
        if (this.State != States.Paid)
            throw new Exception();
        this.State = States.Processed;
    }

    protected override void SaveExtra(SaveTag tag)
    {
        tag.Save("Money", this.Money);
        tag.Save("Counter", this.Desk);
        tag.Save("State", (int)this.State);
    }

    protected override void LoadExtra(SaveTag tag)
    {
        this.Money = tag.LoadEntityRefId("Money");
        this.Desk = tag.LoadIntVec3("Counter");
        this.State = (States)tag.LoadInt("State");
    }

    protected override void WriteExtra(IDataWriter w)
    {
        w.Write(this.Money);
        w.Write(this.Desk);
        w.Write((int)this.State);
    }

    protected override void ReadExtra(IDataReader r)
    {
        this.Money = r.ReadEntityRefId();
        this.Desk = r.ReadIntVec3();
        this.State = (States)r.ReadInt32();
    }
}

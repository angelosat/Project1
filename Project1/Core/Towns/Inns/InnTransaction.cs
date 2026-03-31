using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Towns.Services;
using Project1.Core.Towns.Shops;
using Project1.Framework;
using Project1.Framework.Serialization;
using System;

namespace Project1.Core.Towns.Inns;

public sealed class InnTransaction(double tickStarted, int patienceInitial, EntityRefId guest, IntVec3 desk) : ITownServiceTransaction
{
    enum States { Queuing, AwaitingPayment, Paid, Processed, Succeeded, Failed }
    internal EntityRefId Guest = guest;
    internal EntityRefId Clerk { get; private set; }
    internal EntityRefId Money;
    public EntityRefId Buyer => this.Guest;
    public EntityRefId Seller => this.Clerk;
    public TownServiceDef Service => TownServiceDefOf.Lodging;
    public double TickStarted { get; private set; } = tickStarted;
    public IntVec3 Desk { get; private set; } = desk;
    public int PatienceInitial { get; private set; } = patienceInitial;
    States State;

    public bool IsAwaitingPayment => this.State == States.AwaitingPayment;
    public bool IsPaid => this.State == States.Paid;
    public bool IsProcessed => this.State == States.Processed;
    public bool IsSucceeded => this.State == States.Succeeded;
    public bool IsFailed => this.State == States.Failed;

    internal void AssignClerk(Actor clerk)
    {
        if (this.State != States.Queuing)
            throw new Exception();
        this.Clerk = clerk.RefId;
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

    public void Write(IDataWriter w)
    {
        w.Write(this.TickStarted);
        w.Write(this.PatienceInitial);
        w.Write(this.Guest);
        w.Write(this.Clerk);
        w.Write(this.Money);
        w.Write(this.Desk);
        w.Write((int)this.State);
    }

    public void Read(IDataReader r)
    {
        this.TickStarted = r.ReadDouble();
        this.PatienceInitial = r.ReadInt32();
        this.Guest = r.ReadEntityRefId();
        this.Clerk = r.ReadEntityRefId();
        this.Money = r.ReadEntityRefId();
        this.Desk = r.ReadIntVec3();
        this.State = (States)r.ReadInt32();
    }
}

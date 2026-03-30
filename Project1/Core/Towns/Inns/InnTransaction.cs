using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Framework;
using System;

namespace Project1.Core.Towns.Inns;

public sealed class InnTransaction(EntityRefId guest, IntVec3 desk)
{
    enum States { Queuing, AwaitingPayment, Paid, Processed, Finished, Disposed }
    readonly EntityRefId Guest = guest;
    internal EntityRefId Clerk { get; private set; }
    internal EntityRefId Money;// { get; private set; }
    public IntVec3 Desk { get; init; } = desk;
    States State;

    public bool IsAwaitingPayment => this.State == States.AwaitingPayment;
    public bool IsPaid => this.State == States.Paid;
    public bool IsProcessed => this.State == States.Processed;
    public bool IsFinished => this.State == States.Finished;
    public bool IsDisposed => this.State == States.Disposed;

    internal void AssignClerk(Actor clerk)
    {
        if (this.State != States.Queuing)
            throw new Exception();
        this.Clerk = clerk.RefId;
        this.State = States.AwaitingPayment;
    }

    internal void MarkFinished()
    {
        if (this.State != States.Processed)
            throw new Exception();
        //this.State = States.Finished;
        this.State = States.Disposed;
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

    internal void Dispose()
        => this.State = States.Disposed;
}

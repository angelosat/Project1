using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Framework;
using System;

namespace Project1.Core.Towns.Services.Repairing;

internal sealed class ServiceRequest_Repair : ServiceRequest
{
    enum States { Pending, Queuing, Success, Failure }
    States State;
    internal EntityRefId Item;
    internal int Price;
    //internal IntVec3 Counter;
    internal IntVec3? RepairBench;

    internal override TownServiceDef Service => TownServiceDefOf.Repairing;

    internal override bool IsSucceeded => this.State == States.Success;

    internal override bool IsFailed => this.State == States.Failure;

    public ServiceRequest_Repair(Actor customer, Entity item, int price, IntVec3 counter) : base(customer, counter)
    {
        this.Item = item.RefId;
        this.Price = price;
        //this.Counter = counter;
    }

    internal void MarkSuccess() => this.State = States.Success;
}

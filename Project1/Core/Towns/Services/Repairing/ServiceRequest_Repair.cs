using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Framework;
using System;

namespace Project1.Core.Towns.Services.Repairing;

internal sealed class ServiceRequest_Repair : ServiceRequest
{
    internal EntityRefId Item;
    internal int Price;
    internal IntVec3 Counter;

    internal override TownServiceDef Service => throw new NotImplementedException();

    internal override bool IsSucceeded => throw new NotImplementedException();

    internal override bool IsFailed => throw new NotImplementedException();

    public ServiceRequest_Repair(Actor customer, Entity item, int price, IntVec3 counter) : base(customer)
    {
        this.Item = item.RefId;
        this.Price = price;
        this.Counter = counter;
    }
}

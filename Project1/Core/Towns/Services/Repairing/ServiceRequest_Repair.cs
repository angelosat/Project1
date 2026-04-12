using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Framework;
using Project1.Framework.Serialization;

namespace Project1.Core.Towns.Services.Repairing;

internal sealed class ServiceRequest_Repair : ServiceRequest
{
    enum States { Pending, VendorWaitingItem, VendorWorking, VendorWaitingPay, Success, Failure }
    States State;
    internal IntVec3? RepairBench;

    public ServiceRequest_Repair(Actor customer, Entity item, int price, IntVec3 counter) : base(customer, item, price, counter)
    {
    }

    internal override TownServiceDef Service => TownServiceDefOf.Repairing;

    //public ServiceRequest_Repair(Actor customer, Entity item, int price, IntVec3 counter) : base(customer, price, counter)
    //{
    //    this.Item = item.RefId;
    //}

    internal void MarkSuccess() => this.State = States.Success;

    protected override void SaveExtra(SaveTag tag)
    {
        if (this.RepairBench.HasValue)
            tag.Save("RepairBench", this.RepairBench.Value);
    }

    protected override void LoadExtra(SaveTag tag)
    {
        if (tag.TryLoadIntVec3("RepairBench", out var value)) 
            this.RepairBench = value;
    }
}

using Project1.Core.Entities.Actors;
using Project1.Framework;

namespace Project1.Core.Towns.Services.Inns;

public sealed class ServiceRequest_Inn : ServiceRequest
{
    internal override TownServiceDef Service => TownServiceDefOf.Lodging;

    public ServiceRequest_Inn(Actor guest, int price, IntVec3 desk) : base(guest, price, desk)
    {
    }

    public ServiceRequest_Inn()
    {
    }
}

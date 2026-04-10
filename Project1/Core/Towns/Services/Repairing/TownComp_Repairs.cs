using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Towns.Services.Shops;
using Project1.Framework;
using System.Collections.Generic;

namespace Project1.Core.Towns.Services.Repairing;

public sealed class TownComp_Repairs : TownComp
{
    public override string Name => "Repairs";

    private readonly Dictionary<IntVec3, Queue<Actor>> QueuesPerServicePoint = [];
    private readonly Dictionary<EntityRefId, ServiceRequest_Repair> _requests = [];

    public TownComp_Repairs(Town town) : base(town)
    {
    }

    internal void Begin(Actor customer, Entity item, int price, IntVec3 counter)
    {
        var req = new ServiceRequest_Repair(customer, item, price, counter);
        this.Town.ServiceRequests.Register(req);
        this.AddInt(req);
    }
    internal bool TryGet(Actor customer, out ServiceRequest_Repair req)
        => this._requests.TryGetValue(customer.RefId, out req);

        private void AddInt(ServiceRequest_Repair req)
    {
        this._requests.Add(req.Customer, req);
    }

    internal IEnumerable<IntVec3> Counters => this.QueuesPerServicePoint.Keys;

    internal override void Scan(BlockEntity entity)
    {
        if (!entity.HasComp<BlockShopComp>())
            return;
        this.QueuesPerServicePoint.Add(entity.OriginGlobal, new());
    }
}

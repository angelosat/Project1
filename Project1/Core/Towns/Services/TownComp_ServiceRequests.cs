using Project1.Core.Blocks;
using Project1.Core.Entities.Actors;
using Project1.Core.Towns.Services.Shops;
using Project1.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns.Services;

public readonly record struct TownServiceRequestId(ulong Value)
{
    public static readonly TownServiceRequestId Null = new(0);
    public static implicit operator TownServiceRequestId(ulong v) => new(v);
    public static implicit operator ulong(TownServiceRequestId v) => (ulong)v.Value;
}

public class TownComp_ServiceRequests : TownComp
{
    readonly Dictionary<TownServiceRequestId, ServiceRequest> _openRequests = [];
    readonly Dictionary<EntityRefId, ServiceRequest> _openRequestsByCustomer = [];
    readonly Dictionary<EntityRefId, ServiceRequest> _openRequestsByVendor = [];
    public IReadOnlyCollection<ServiceRequest> GetAllRequests() => this._openRequests.Values;
    public IEnumerable<T> GetAllRequests<T>() where T : ServiceRequest => this._openRequests.Values.OfType<T>();
    HashSet<IntVec3> CountersAll = [];
    Dictionary<TownServiceDef, HashSet<IntVec3>> CountersByService = [];
    Dictionary<IntVec3, Queue<Actor>> QueuesByCounter = [];

    public TownComp_ServiceRequests(Town town) : base(town)
    {
        foreach (var def in Def.Get<TownServiceDef>())
            this.CountersByService.Add(def, []);
    }

    public override string Name => "Services";

    TownServiceRequestId NextId => ++field;

    internal TownServiceRequestId Register(ServiceRequest request)
    {
        var id = this.NextId;
        request.Id = id;
        this._openRequests.Add(id, request);
        this._openRequestsByCustomer.Add(request.Customer, request);
        return id;
    }

    internal void Remove(TownServiceRequestId id)
    {
        var req = this._openRequests[id];
        this._openRequests.Remove(id);
        this._openRequestsByCustomer.Remove(req.Customer);
        this._openRequestsByVendor.Remove(req.Vendor);
    }

    internal ServiceRequest Get(TownServiceRequestId id)
        => this._openRequests[id];

    internal ServiceRequest GetByCustomer(EntityRefId customerId)
        => this._openRequestsByCustomer[customerId];

    internal ServiceRequest GetByVendor(EntityRefId vendorId)
     => this._openRequestsByVendor[vendorId];

    internal IEnumerable<Actor> Peek(TownServiceDef service)
        //=> this.CountersByService[service].Select(c => this.QueuesByCounter[c].Peek());
        => this.CountersByService[service].Where(c => this.QueuesByCounter[c].Count > 0).Select(c => this.QueuesByCounter[c].Peek());

    internal IEnumerable<ServiceRequest> GetAllPendingRequests(TownServiceDef service)
    {
        foreach (var customer in this.Peek(service))
            if (this._openRequestsByCustomer.TryGetValue(customer.RefId, out var request))
                if(request.Vendor == EntityRefId.Null)
                    yield return request;
    }
    internal bool TryGetByVendor(Actor actor, out ServiceRequest req)
        => this._openRequestsByVendor.TryGetValue(actor.RefId, out req);
    internal bool TryGetByVendor<T>(Actor actor, out T req) where T : ServiceRequest
    {
        if(this._openRequestsByVendor.TryGetValue(actor.RefId, out var found))
        {
            req = (T)found;
            return true;
        }
        req = null;
        return false;
    }
    internal override void Scan(BlockEntity entity)
    {
        if (!entity.HasComp<BlockShopComp>())
            return;
        RegisterCounterInt(entity.OriginGlobal);
    }

    private void RegisterCounterInt(IntVec3 cell)
    {
        this.CountersAll.Add(cell);
        this.QueuesByCounter.Add(cell, new());
        this.CountersByService[TownServiceDefOf.Repairing].Add(cell); // HACK
    }

    internal IEnumerable<IntVec3> GetCounters(TownServiceDef service)
    //=> this.CountersByService[service];
    {
        if (service == TownServiceDefOf.Repairing)
            return this.Town.Repairs.Counters;
        return [];
    }

    internal void Enqueue(Actor actor, IntVec3 counter)
        => this.QueuesByCounter[counter].Enqueue(actor);

    internal void Enqueue(Actor actor)
       => this.QueuesByCounter[this._openRequestsByCustomer[actor.RefId].Counter.Value].Enqueue(actor);

    internal void AssignVendor(ServiceRequest req, Actor vendor)
    {
        this._openRequestsByVendor.Add(vendor.RefId, req);
    }

    
}

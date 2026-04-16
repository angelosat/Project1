using Project1.Core.AI;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Towns.AI.Behaviors;
using Project1.Framework;
using System;
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
    readonly Dictionary<IntVec3, ServiceRequest> _openRequestsByCounter = [];
    readonly HashSet<IntVec3> CountersAll = [];
    readonly Dictionary<TownServiceDef, HashSet<IntVec3>> CountersByService = [];
    readonly Dictionary<IntVec3, Queue<Actor>> QueuesByCounter = [];

    public IReadOnlyCollection<ServiceRequest> GetAllRequests() => this._openRequests.Values;
    public IEnumerable<T> GetAllRequests<T>() where T : ServiceRequest => this._openRequests.Values.OfType<T>();

    public TownComp_ServiceRequests(Town town) : base(town)
    {
        foreach (var def in Def.Get<TownServiceDef>())
            this.CountersByService.Add(def, []);
        town.Map.Events.ListenTo<CounterServiceChangedEvent>(HandleCounterTownServiceChanged);
        town.Map.Events.ListenTo<BlockEntityRemovedEvent>(HandleBlockEntityRemoved);
        town.Map.Events.ListenTo<EntityDespawnedEvent>(HandleEntityDespawned);
        town.Map.Events.ListenTo<VisitorDepartingEvent>(HandleVisitorDeparting);
    }

    private void HandleVisitorDeparting(VisitorDepartingEvent e)
    {
        this.AbortVisitorRequest(e.Actor);
    }

    private void AbortVisitorRequest(Actor visitor)
    {
        if (this._openRequestsByCustomer.TryGetValue(visitor.RefId, out var req)
                    || this._openRequestsByVendor.TryGetValue(visitor.RefId, out req))
            req.MarkFailed();
    }

    private void HandleEntityDespawned(EntityDespawnedEvent e)
    {
        //if (this._openRequestsByCustomer.TryGetValue(e.Entity.RefId, out var req)
        //    || this._openRequestsByVendor.TryGetValue(e.Entity.RefId, out req))
        //    req.MarkFailed();
        if (e.Entity is not Actor actor)
            return;
        this.AbortVisitorRequest(actor);
    }

    private void HandleBlockEntityRemoved(BlockEntityRemovedEvent e)
    {
        var cell = e.Entity.OriginGlobal;
        if (this._openRequestsByCounter.TryGetValue(cell, out var req))
            req.MarkFailed();
        this.CountersAll.Remove(cell);
        if (e.Entity.TryGetComp<BlockShopComp>(out var comp))
            if (comp.Service is TownServiceDef service)
                this.CountersByService[service].Remove(cell);
        this.QueuesByCounter.Remove(cell);
    }

    private void HandleCounterTownServiceChanged(CounterServiceChangedEvent e)
    {
        var cell = e.Comp.Parent.OriginGlobal;
        if (e.OldService is TownServiceDef defold)
        {
            //if (this.QueuesByCounter[cell].TryPeek(out var affectedCustomer))
            //    if (this._openRequestsByCustomer.TryGetValue(affectedCustomer.RefId, out var affectedReq))
            //        affectedReq.MarkFailed();
            if(this._openRequestsByCounter.TryGetValue(cell, out var affectedReq))
                //if (this._openRequestsByCustomer.TryGetValue(affectedCustomer.RefId, out var affectedReq))
                    affectedReq.MarkFailed();
            this.CountersByService[defold].Remove(cell);
        }
        if (e.Comp.Service is TownServiceDef defnew)
            this.CountersByService[defnew].Add(cell);
    }

    public override string Name => "Services";

    TownServiceRequestId NextId => ++field;

    internal TownServiceRequestId Register(ServiceRequest request)
    {
        var id = this.NextId;
        request.Id = id;
        this._openRequests.Add(id, request);
        this._openRequestsByCustomer.Add(request.Customer, request);
        if (request.Counter.HasValue)
            this._openRequestsByCounter.Add(request.Counter.Value, request);
        return id;
    }

    internal void Remove(TownServiceRequestId id)
    {
        var req = this._openRequests[id];
        this._openRequests.Remove(id);
        this._openRequestsByCustomer.Remove(req.Customer);
        this._openRequestsByVendor.Remove(req.Vendor);
        if (req.Counter.HasValue)
            this._openRequestsByCounter.Remove(req.Counter.Value);

        if(this.World.Get<Actor>(req.Customer) is Actor customer)
            if (customer.CurrentPlan is Plan planCustomer && planCustomer.ServiceRequest == req)
                planCustomer.Cancel();
        if (this.World.Get<Actor>(req.Vendor) is Actor vendor)
            if (vendor.CurrentPlan is Plan planVendor && planVendor.ServiceRequest == req)
                planVendor.Cancel();

        var counter = req.Counter;
        //if(counter.HasValue)
        //{
        //    if (!this.QueuesByCounter[counter.Value].TryDequeue(out var customer) || customer.RefId != req.Customer)
        //    {
        //        //throw new InvalidOperationException();
        //        $"warning: service request removed before customer enqueued".ToConsole();
        //    }
        //}
        if (counter.HasValue)
        {
            if (this.QueuesByCounter.TryGetValue(counter.Value, out var queue))
                if(!queue.TryDequeue(out customer) || customer.RefId != req.Customer)
            {
                //throw new InvalidOperationException();
                $"warning: service request removed before customer enqueued".ToConsole();
            }
        }
    }

    internal ServiceRequest Get(TownServiceRequestId id)
        => this._openRequests[id];

    internal ServiceRequest GetByCustomer(EntityRefId customerId)
        => this._openRequestsByCustomer[customerId];

    internal ServiceRequest GetByVendor(EntityRefId vendorId)
     => this._openRequestsByVendor[vendorId];

    internal IEnumerable<Actor> Peek(TownServiceDef service)
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
    internal bool TryGetByCustomer(Actor actor, out ServiceRequest req)
     => this._openRequestsByCustomer.TryGetValue(actor.RefId, out req);
    internal bool TryGetByVendor<T>(Actor actor, out T req) where T : ServiceRequest
    {
        if(this._openRequestsByVendor.TryGetValue(actor.RefId, out var found))
        {
            req = found as T;
            return req is not null;
        }
        req = null;
        return false;
    }
    internal override void Scan(BlockEntity entity)
    {
        if (!entity.TryGetComp<BlockShopComp>(out var shopcomp))
            return;
        RegisterCounterInt(shopcomp);
    }

    private void RegisterCounterInt(BlockShopComp comp)
    {
        var cell = comp.Parent.OriginGlobal;
        this.CountersAll.Add(cell);
        this.QueuesByCounter.Add(cell, new());
        if(comp.Service is not null)
            this.CountersByService[comp.Service].Add(cell);
    }

    internal IReadOnlySet<IntVec3> GetCounters(TownServiceDef service)
    
        => this.CountersByService[service];

    internal void Enqueue(Actor actor, IntVec3 counter)
        => this.QueuesByCounter[counter].Enqueue(actor);

    internal void Enqueue(Actor actor)
       => this.QueuesByCounter[this._openRequestsByCustomer[actor.RefId].Counter.Value].Enqueue(actor);

    internal void AssignVendor(ServiceRequest req, Actor vendor)
    {
        req.AssignVendor(vendor);
        this._openRequestsByVendor.Add(vendor.RefId, req);
    }
}

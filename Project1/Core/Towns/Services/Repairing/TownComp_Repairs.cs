using Project1.Core.Blocks;
using Project1.Core.Crafting;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Inventory;
using Project1.Core.Towns.Services.Shops;
using Project1.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Project1.Core.Towns.Services.Repairing;

public sealed class TownComp_Repairs : TownComp
{
    public override string Name => "Repairs";

    private readonly Dictionary<IntVec3, Queue<Actor>> QueuesPerServicePoint = [];
    private readonly HashSet<IntVec3> _repairStationsAll = [];
    private readonly HashSet<IntVec3> _repairStationsAvailable = [];
    private readonly Dictionary<EntityRefId, ServiceRequest_Repair> _requests = [];
    private readonly Dictionary<IntVec3, ServiceRequest_Repair> _requestsByRepairBench = [];
    private readonly Dictionary<EntityRefId, ServiceRequest_Repair> _requestsByItem = [];
    internal IReadOnlySet<IntVec3> RepairStationsAvailable => this._repairStationsAvailable;

    public TownComp_Repairs(Town town) : base(town)
    {
        //town.Map.Events.ListenTo<ActorHaulingNewItemEvent>(HandleActorHaulingNewItem);
    }

    //private void HandleActorHaulingNewItem(ActorHaulingNewItemEvent e)
    //{
    //    var item = e.Actor.Hauled;
    //    if (!this._requestsByItem.TryGetValue(item.RefId, out var request))
    //        return;
    //    if (request.ItemSubmitted)
    //        return;
    //    if (e.Actor.RefId != request.Vendor)
    //    {
    //        if (e.Actor.RefId != request.Customer)
    //            Debug.Fail("Unexpected item holder for repair request");
    //        return;
    //    }
    //    request.ItemSubmitted = true;
    //}

    internal override void Tick()
    {
        foreach(var req in this._requests.Values.ToArray())
        {
            if (!(req.IsFailed || req.IsSucceeded))
                continue;
            this.Map.Events.Post(new TownServiceCompleteEvent(this.Map, req));
            this.RemoveInt(req);
        }
    }

    internal void Begin(Actor customer, Entity item, int price, IntVec3 counter)
    {
        var req = new ServiceRequest_Repair(customer, item, price, counter);
        this.Town.ServiceRequests.Register(req);
        this.AddInt(req);
    }
    internal bool TryGetByCustomer(Actor customer, out ServiceRequest_Repair req)
        => this._requests.TryGetValue(customer.RefId, out req);

    private void AddInt(ServiceRequest_Repair req)
    {
        this._requests.Add(req.Customer, req);
        this._requestsByItem.Add(req.Item, req);
    }
    private void RemoveInt(ServiceRequest_Repair req)
    {
        this._requests.Remove(req.Customer);
        this._requestsByItem.Remove(req.Item);
        if (req.RepairBench.HasValue)
        {
            this._requestsByRepairBench.Remove(req.RepairBench.Value);
            this._repairStationsAvailable.Add(req.RepairBench.Value);
        }
        this.Town.ServiceRequests.Remove(req.Id);
    }

    internal void AssignVendor(ServiceRequest_Repair req, Actor vendor)
    {
        req.Vendor = vendor.RefId;
        this.Town.ServiceRequests.AssignVendor(req, vendor);
    }
    internal void AssignRepairBench(ServiceRequest_Repair req, IntVec3 bench)
    {
        req.RepairBench = bench;
        this._repairStationsAvailable.Remove(bench);
        this._requestsByRepairBench.Add(bench, req);
    }
    internal IEnumerable<IntVec3> Counters => this.QueuesPerServicePoint.Keys;

    internal override void Scan(BlockEntity entity)
    {
        var cell = entity.OriginGlobal;
        if (entity.HasComp<BlockShopComp>())
            this.QueuesPerServicePoint.Add(cell, new());
        if (entity.TryGetComp<BlockWorkstationComp>(out var workstation) && workstation.WorkstationType.Capabilities.Contains(WorkstationCapabilityDefOf.Repairing))
        {
            this._repairStationsAll.Add(cell);
            this._repairStationsAvailable.Add(cell);
        }
    }
}

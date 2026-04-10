using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Core.Towns.Services.Shops;
using Project1.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns.Services.Inns;
record struct InnGuestProfile(EntityRefId ActorId, IntVec3 AssignedBed, SimulationTick TimeCheckedIn) { }
public sealed class TownComp_Inns : TownComp
{

    public override string Name => "Inns";
    private readonly HashSet<IntVec3> AllBeds = [];
    private readonly Dictionary<IntVec3, Queue<Actor>> QueuesPerServicePoint = [];
    private readonly Dictionary<EntityRefId, InnGuestProfile> RegistryByGuest = [];
    private readonly Dictionary<IntVec3, InnGuestProfile> RegistryByBed = [];
    private readonly HashSet<Actor> GuestsQueuing = [];
    private readonly Dictionary<EntityRefId, ServiceRequest_Inn> OpenTransactionsByGuest = [];
    private readonly Dictionary<IntVec3, ServiceRequest_Inn> OpenTransactionsByDesk = [];
    private readonly Dictionary<EntityRefId, ServiceRequest_Inn> OpenTransactionsByClerk = [];
    public IEnumerable<IntVec3> AvailableBeds => this.AllBeds.Where(b => !this.RegistryByBed.ContainsKey(b));
    public ServiceRequest_Inn GetTransactionByGuest(Actor actor)
    {
        if (this.OpenTransactionsByGuest.TryGetValue(actor.RefId, out var foundByGuest))
            return foundByGuest;
        return null;
    }
    public ServiceRequest_Inn GetTransactionByClerk(Actor actor)
    {
        if (this.OpenTransactionsByClerk.TryGetValue(actor.RefId, out var foundByGuest))
            return foundByGuest;
        return null;
    }
    public ServiceRequest_Inn GetTransaction(IntVec3 desk)
    {
        if (this.OpenTransactionsByDesk.TryGetValue(desk, out var found))
            return found;
        return null;
    }
    public bool TryGetTransaction(Actor guest, out ServiceRequest_Inn transaction)
        => this.OpenTransactionsByGuest.TryGetValue(guest.RefId, out transaction);
    public override void Tick()
    {
        foreach(var (id, t) in this.OpenTransactionsByGuest.ToArray())
        {
            if (t.IsSucceeded || t.IsFailed)
            {
                this.OpenTransactionsByGuest.Remove(id);
                this.OpenTransactionsByDesk.Remove(t.Desk);
                this.OpenTransactionsByClerk.Remove(t.Vendor);
                this.Town.OpenTransactions.Remove(id);
                this.Map.Events.Post(new TownServiceComplete(this.Map, t));
            }
        }
    }
    // TODO: track how long each guest was waited and return the one who has waited the longest
    public Actor PeekNextGuestInQueue(IntVec3 servicePoint)
    {
        var queue = this.QueuesPerServicePoint[servicePoint];
        while (queue.Count > 0)
        {
            var guest = queue.Peek();
            if (this.GuestsQueuing.Contains(guest))
                return guest;
            queue.Dequeue();
        }
        return null;
    }
    public bool TryEnqueue(Actor guest, IntVec3 servicePoint)
    {
        if (this.GuestsQueuing.Contains(guest))
            return false;
        if (!this.QueuesPerServicePoint.TryGetValue(servicePoint, out var queue))
            throw new System.Exception();
        this.GuestsQueuing.Add(guest);
        this.QueuesPerServicePoint[servicePoint].Enqueue(guest);
        var transaction = new ServiceRequest_Inn(guest, servicePoint);
        AddInt(transaction);
        this.Town.OpenTransactions.Add(guest.RefId, transaction);
        //this.OpenTransactionsByGuest.Add(guest.RefId, transaction);
        //this.OpenTransactionsByDesk.Add(servicePoint, transaction);
        //this.Town.OpenTransactions.Add(guest.RefId, transaction);
        return true;
    }

    private void AddInt(ServiceRequest_Inn transaction)
    {
        this.OpenTransactionsByGuest.Add(transaction.Customer, transaction);
        this.OpenTransactionsByDesk.Add(transaction.Desk, transaction);
        if (transaction.Vendor != EntityRefId.Null)
            this.OpenTransactionsByClerk.Add(transaction.Vendor, transaction);
    }

    internal void AbortQueuing(Actor actor)

    {
        this.GuestsQueuing.Remove(actor);
        var t = this.OpenTransactionsByGuest[actor.RefId];
        t.MarkFailed();
        this.Map.Events.Post(new ShopTransactionUpdatedEvent(this.Map, t));

    }
    public TownComp_Inns(Town town) : base(town)
    {
        town.Map.Events.ListenTo<BlockEntityRemovedEvent>(HandleBlockEntityRemoved);
        town.Map.Events.ListenTo<BlocksChangedEvent>(HandleBlocksChanged);
    }
    public IEnumerable<IntVec3> GetServicePoints()
        => this.QueuesPerServicePoint.Keys;
    public IEnumerable<IntVec3> GetServicePointsWithQueue()
        => this.QueuesPerServicePoint.Where(kv => kv.Value.Count > 0).Select(kv => kv.Key);
    public Queue<Actor> GetQueue(IntVec3 desk)
        => this.QueuesPerServicePoint[desk];
    public bool TryFindBedFrom(IntVec3 servicePoint, out IntVec3 foundBed)
    {
        var nextGuest = this.QueuesPerServicePoint[servicePoint].Peek();
        foreach(var bed in this.AvailableBeds)
        {
            if(nextGuest.CanReach(bed))
            {
                foundBed = bed;
                return true;
            }
        }
        foundBed = default;
        return false;
    }
    public bool HasProfile(Actor actor) => this.RegistryByGuest.ContainsKey(actor.RefId);
    internal bool RegisterGuest(IntVec3 servicePoint)
    {
        var queue = this.QueuesPerServicePoint[servicePoint];
        var guest = queue.Peek();
        IntVec3? foundBed = default;
        foreach(var potentialBed in this.AvailableBeds)
        {
            if (guest.CanReach(potentialBed))
            {
                foundBed = potentialBed;
                break;
            }
        }
        if (foundBed is null)
            return false;
        var bed = foundBed.Value;
        queue.Dequeue();
        this.GuestsQueuing.Remove(guest);
        var entry = new InnGuestProfile(guest.RefId, bed, this.Map.World.CurrentTick);
        var transaction = this.OpenTransactionsByGuest[guest.RefId];
        transaction.MarkFinished();
        //this.ActiveTransactions.Remove(guest.RefId);
        this.RegistryByGuest.Add(guest.RefId, entry);
        this.RegistryByBed.Add(bed, entry);
        this.Town.Ownership.Assign(bed, guest);
        this.Map.Events.Post(new ShopTransactionUpdatedEvent(this.Map, transaction));

        return true;
    }
    internal bool Checkout(Actor guest)
    {
        if (!this.RegistryByGuest.TryGetValue(guest.RefId, out var entry))
            return false;
        this.RegistryByGuest.Remove(guest.RefId);
        this.RegistryByBed.Remove(entry.AssignedBed);
        this.Town.Ownership.Assign(entry.AssignedBed, null);
        return true;
    }
    private void HandleBlockEntityRemoved(BlockEntityRemovedEvent e)
    {
        this.AllBeds.Remove(e.Entity.OriginGlobal);
    }
    private void HandleBlocksChanged(BlocksChangedEvent e)
    {
        foreach (var pos in e.Changes)
        {
            if (pos.Block.BlockDef == BlockDefOf.ReceptionDesk)
                this.QueuesPerServicePoint.Add(pos.Global, []);
            else
                this.QueuesPerServicePoint.Remove(pos.Global);
        }
    }
    public void ToggleBed(IntVec3 global)
    {
        if (!this.AllBeds.Remove(global))
            this.AllBeds.Add(global);
        $"[{this.Map.Net}] bed: {global} toggled for inn services".ToConsole();
    }
    internal override void ResolveReferences()
    {
        foreach (var (chunk, cell, id) in this.Map.GetAllCellsWithIndex())
        {
            if (cell.Block.BlockDef != BlockDefOf.ReceptionDesk)
                continue;
            var global = id.GetGlobal(chunk);
            this.QueuesPerServicePoint.Add(global, []);
        }

        foreach (var req in this.Town.ServiceRequests.GetAllRequests<ServiceRequest_Inn>())
            this.AddInt(req);
    }

    internal bool IsQueuing(Actor actor)
        => this.GuestsQueuing.Contains(actor);

    internal void AssignClerk(IntVec3 desk, Actor actor)
    {
        var req = this.OpenTransactionsByDesk[desk];
        req.AssignClerk(actor);
        this.OpenTransactionsByClerk.Add(actor.RefId, req);
        //this.Map.Events.Post(new ShopTransactionUpdatedEvent(this.Map, transaction));
        this.Map.Events.Post(new TownServiceRequestUpdatedEvent(this.Map, req));

    }

    internal void MarkPaid(Actor actor, Entity hauled)
    {
        var req = this.GetTransactionByGuest(actor);
        req.MarkPaid(hauled);
        //this.Map.Events.Post(new ShopTransactionUpdatedEvent(this.Map, transaction));
        this.Map.Events.Post(new TownServiceRequestUpdatedEvent(this.Map, req));

    }

    internal void MarkProcessed(EntityRefId buyer)
    {
        var req = this.OpenTransactionsByGuest[buyer];
        req.MarkProcessed();
        //this.Map.Events.Post(new ShopTransactionUpdatedEvent(this.Map, transaction));
        this.Map.Events.Post(new TownServiceRequestUpdatedEvent(this.Map, req));

    }

}

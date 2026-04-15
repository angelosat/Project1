using Project1.Core.Blocks;
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
    //private readonly Dictionary<IntVec3, Queue<Actor>> QueuesPerServicePoint = [];
    private readonly Dictionary<EntityRefId, InnGuestProfile> RegistryByGuest = [];
    private readonly Dictionary<IntVec3, InnGuestProfile> RegistryByBed = [];
    private readonly HashSet<Actor> GuestsQueuing = [];
    private readonly Dictionary<EntityRefId, ServiceRequest_Inn> OpenTransactionsByGuest = [];
    private readonly Dictionary<IntVec3, ServiceRequest_Inn> OpenTransactionsByDesk = [];
    private readonly Dictionary<EntityRefId, ServiceRequest_Inn> OpenTransactionsByClerk = [];
    int Price = 100;
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

    internal override void Tick()
    {
        foreach(var (id, t) in this.OpenTransactionsByGuest.ToArray())
        {
            if (t.IsSucceeded || t.IsFailed)
            {
                this.OpenTransactionsByGuest.Remove(id);
                this.OpenTransactionsByDesk.Remove(t.Counter.Value);
                this.OpenTransactionsByClerk.Remove(t.Vendor);
                this.Town.OpenTransactions.Remove(id);
                this.Map.Events.Post(new TownServiceCompleteEvent(this.Map, t));
            }
        }
    }

    internal ServiceRequest Begin(Actor guest, IntVec3 servicePoint)
    {
        var transaction = new ServiceRequest_Inn(guest, this.Price, servicePoint);
        AddInt(transaction);
        this.Town.ServiceRequests.Register(transaction);
        return transaction;
    }

    private void AddInt(ServiceRequest_Inn transaction)
    {
        this.OpenTransactionsByGuest.Add(transaction.Customer, transaction);
        this.OpenTransactionsByDesk.Add(transaction.Counter.Value, transaction);
        if (transaction.Vendor != EntityRefId.Null)
            this.OpenTransactionsByClerk.Add(transaction.Vendor, transaction);
    }

   
    public TownComp_Inns(Town town) : base(town)
    {
        town.Map.Events.ListenTo<BlockEntityRemovedEvent>(HandleBlockEntityRemoved);
        //town.Map.Events.ListenTo<BlocksChangedEvent>(HandleBlocksChanged);
    }
    
    public bool HasProfile(Actor actor) => this.RegistryByGuest.ContainsKey(actor.RefId);
    
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
    //private void HandleBlocksChanged(BlocksChangedEvent e)
    //{
    //    foreach (var pos in e.Changes)
    //    {
    //        if (pos.Block.BlockDef == BlockDefOf.ReceptionDesk)
    //            this.QueuesPerServicePoint.Add(pos.Global, []);
    //        else
    //            this.QueuesPerServicePoint.Remove(pos.Global);
    //    }
    //}
    public void ToggleBed(IntVec3 global)
    {
        if (!this.AllBeds.Remove(global))
            this.AllBeds.Add(global);
        $"[{this.Map.Net}] bed: {global} toggled for inn services".ToConsole();
    }
    internal override void ResolveReferences()
    {
        //foreach (var (chunk, cell, id) in this.Map.GetAllCellsWithIndex())
        //{
        //    if (cell.Block.BlockDef != BlockDefOf.ReceptionDesk)
        //        continue;
        //    var global = id.GetGlobal(chunk);
        //    this.QueuesPerServicePoint.Add(global, []);
        //}

        foreach (var req in this.Town.ServiceRequests.GetAllRequests<ServiceRequest_Inn>())
            this.AddInt(req);
    }

    internal override void Scan(BlockEntity entity)
    {
        if (!entity.HasComp<BlockBedComp>())
            return;
        this.AllBeds.Add(entity.OriginGlobal);
    }

    internal bool RegisterGuest(ServiceRequest_Inn req)
    {
        var guestid = req.Customer;
        var guest = this.World.Get<Actor>(guestid);
        IntVec3? foundBed = default;
        foreach (var potentialBed in this.AvailableBeds)
        {
            if (guest.CanReach(potentialBed))
            {
                foundBed = potentialBed;
                break;
            }
        }
        if (foundBed is null)
            throw new System.Exception();
        var bed = foundBed.Value;
        var entry = new InnGuestProfile(guestid, bed, this.Map.World.CurrentTick);
        req.MarkSucceeded();
        this.RegistryByGuest.Add(guestid, entry);
        this.RegistryByBed.Add(bed, entry);
        this.Town.Ownership.Assign(bed, guest);
        return true;
    }
    
    internal void AbortQueuing(Actor actor)

    {
        this.GuestsQueuing.Remove(actor);
        var t = this.OpenTransactionsByGuest[actor.RefId];
        t.MarkFailed();
    }

    internal void AssignClerk(IntVec3 desk, Actor actor)
    {
        var req = this.OpenTransactionsByDesk[desk];
        req.AssignVendor(actor);
        this.OpenTransactionsByClerk.Add(actor.RefId, req);
    }
}

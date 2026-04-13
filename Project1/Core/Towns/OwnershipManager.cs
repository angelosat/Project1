using Project1.Core.Blocks;
using Project1.Core.Entities.Actors;
using Project1.Framework;
using System.Collections.Generic;

namespace Project1.Core.Towns;

record struct BedAssignment(Actor Actor, BlockBedComp Bed) { }
public sealed class OwnershipManager : TownComp
{
    public override string Name => "Ownership";

    readonly Dictionary<EntityRefId, HashSet<IntVec3>> _actorPossesions = [];
    readonly Dictionary<Actor, BlockBedComp> _actorBeds = [];
    public OwnershipManager(Town town) : base(town)
    {
    }
    internal bool TryGetAssignedBed(Actor actor, out BlockBedComp comp)
        => this._actorBeds.TryGetValue(actor, out comp);
    public IEnumerable<IntVec3> GetOwnedBlocks(Actor actor)
    {
        if (this._actorPossesions.TryGetValue(actor.RefId, out var list))
            return list;
        return [];
    }

    void Add(EntityRefId actor, IntVec3 be)
    {
        if (!this._actorPossesions.TryGetValue(actor, out var list))
            this._actorPossesions[actor] = list = [];
        list.Add(be);
    }

    void Remove(EntityRefId actor, IntVec3 be)
    {
        if (!this._actorPossesions.TryGetValue(actor, out var list))
            throw new System.Exception();
        list.Remove(be);
        if (list.Count == 0)
            this._actorPossesions.Remove(actor);
    }

    void Remove(IntVec3 be)
    {
        foreach (var list in this._actorPossesions.Values)
            list.Remove(be);
        foreach (var actorid in this._actorPossesions.Keys)
            if (this._actorPossesions[actorid].Count == 0)
                this._actorPossesions.Remove(actorid);
    }

    internal override void ResolveReferences()
    {
        this.Map.Events.ListenTo<BlockOwnerChangedEvent>(HandleBlockOwnerChanged);
        this.Map.Events.ListenTo<BlockEntityRemovedEvent>(HandleBlockEntityRemoved);

        //foreach(var be in this.Map.BlockEntities)
        //{
        //    if (!be.TryGetComp<BlockOwnershipComp>(out var comp))
        //        continue;
        //    var owner = comp.Owner;
        //    if (owner == EntityRefId.Null)
        //        continue;
        //}
    }

    

    private void HandleBlockEntityRemoved(BlockEntityRemovedEvent e)
    {
        if (!e.Entity.TryGetComp<BlockOwnershipComp>(out var comp))
            return;

        var ownerId = comp.Owner;
        if (ownerId == EntityRefId.Null)
            return;

        if (this.Map.World.Get<Actor>(ownerId) is not Actor owner)
            return;

        if (this._actorBeds.TryGetValue(owner, out var bed) && e.Entity == bed.Parent)
        {
            this._actorBeds.Remove(owner);

            // Optional: notify other systems of ownership loss
            //comp.Map.Events.Post(new BlockOwnerChangedEvent(comp.Parent, null, owner));
        }
    }

    private void HandleBlockOwnerChanged(BlockOwnerChangedEvent e)
    {
        this.Assign(e.Entity, e.NewOwner);
        return;
        var be = e.Entity;
        var newOwner = e.NewOwner;
        var prevOwnerId = e.PreviousOwner;

        var bed = be.GetComp<BlockBedComp>();
        if (bed == null)
            return;


        // Remove old mapping
        if (prevOwnerId != EntityRefId.Null && this.Map.World.Get<Actor>(prevOwnerId) is Actor prevOwner &&
            this._actorBeds.TryGetValue(prevOwner, out var prevBed) &&
            prevBed == bed)
        {
            this._actorBeds.Remove(prevOwner);
        }

        // Assign new mapping
        if (newOwner != null)
        {
            if (this._actorBeds.TryGetValue(newOwner, out var existing) && existing != bed)
            {
                existing.Parent.GetComp<BlockOwnershipComp>().SetOwner(null);
            }

            this._actorBeds[newOwner] = bed;
        }
    }
    public bool Assign(IntVec3 bed, Actor newOwner)
        => this.Assign(this.Map.GetBlockEntity(bed), newOwner);
    
    public bool Assign(BlockEntity be, Actor newOwner)
    {
        var bed = be.GetComp<BlockBedComp>();
        if (bed == null)
            return false;

        var comp = bed.Parent.GetComp<BlockOwnershipComp>();
        var prevOwnerId = comp.Owner;
        if (prevOwnerId == newOwner.RefId)
            return false;

        // Remove old mapping
        if (prevOwnerId != EntityRefId.Null && this.Map.World.Get<Actor>(prevOwnerId) is Actor prevOwner &&
            this._actorBeds.TryGetValue(prevOwner, out var prevBed) &&
            prevBed == bed)
        {
            this._actorBeds.Remove(prevOwner);
        }

        // Assign new mapping
        if (newOwner != null)
        {
            if (this._actorBeds.TryGetValue(newOwner, out var existing) && existing != bed)
            {
                if (bed != existing)
                    throw new System.Exception();
                existing.Parent.GetComp<BlockOwnershipComp>().SetOwner(null);
            }
            this._actorBeds[newOwner] = bed;
        }

        return true;
    }
}

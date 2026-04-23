using Project1.Core.AI;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Simulation;
using Project1.Core.Systems.Recipes;
using Project1.Core.Towns;
using Project1.Framework;
using Project1.Framework.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Project1.Core.Systems.Crafting;

//public interface IStructId
//{
//    int Value { get; }
//}
public interface IStructIdInt<T>
{
    int Value { get; }
    static abstract T Create(int value);
}
public readonly record struct CraftingOrderId(int Value) : IStructIdInt<CraftingOrderId>
{
    public static readonly CraftingOrderId Null = new(0);

    public static CraftingOrderId Create(int value) => new(value);

    public static implicit operator CraftingOrderId(int v) => new(v);
    public static implicit operator int(CraftingOrderId v) => (int)v.Value;
}
//public readonly record struct CraftingOrderId(int Value)
//{
//    public static readonly CraftingOrderId Null = new(0);
//    public static implicit operator CraftingOrderId(int v) => new(v);
//    public static implicit operator int(CraftingOrderId v) => (int)v.Value;
//}
public sealed class CraftingManager : TownComp
{
    private CraftingOrderId NextOrderId = 1;
    public override string Name => "CraftingManager";
    readonly Dictionary<IntVec3, BlockWorkstationComp> _workstationsByPosition = [];
    readonly Dictionary<WorkstationDef, HashSet<BlockWorkstationComp>> _byType = [];
    readonly Dictionary<CraftingOrderId, CraftingOrder> _ordersById = [];
    readonly Dictionary<BlockEntity, CraftingOrder> _ordersByWorkstation = [];

    readonly Dictionary<BlockWorkstationComp, Contract> _contractsByWorkstation = [];
    readonly Dictionary<Actor, Contract> _contractsByActor = [];

    readonly HashSet<Entity> _unfinishedItems = [];
    readonly Dictionary<Actor, HashSet<Entity>> _unfinishedByActor = [];

    public IReadOnlySet<Entity> UnfinishedItems => this._unfinishedItems;
    public IEnumerable<BlockWorkstationComp> AllWorkstations => this._byType.SelectMany(d => d.Value);
    public IEnumerable<IGrouping<BlockEntity, List<CraftingOrder>>> OrdersByWorkstation => AllWorkstations.GroupBy(i => i.Parent, i => i.Orders);
    public IEnumerable<BlockWorkstationComp> AllWorkstationModules => this._workstationsByPosition.Values;

    List<ICraftingPlugin> Plugins = [new RecipeMasterySystem(), new CraftingPlugin_Skill()];

    public CraftingManager(Town town) : base(town)
    {
        var workstationDefs = Def.Get<WorkstationDef>();
        foreach (var def in workstationDefs)
            this._byType.Add(def, []);
    }

    internal override void ResolveReferences()
    {
        this.Town.Map.Events.ListenTo<CellsInvalidatedEvent>(OnBlocksUpdated);
        this.Town.Map.Events.ListenTo<BlockEntityRemovedEvent>(OnBlockEntityRemoved);
        this.Town.Map.Events.ListenTo<BlockEntityAddedEvent>(OnBlockEntityAdded);
        this.ScanWorkstations();
        this.ScanOrders();
        this.Town.Map.Events.ListenTo<ActorPlanAssignedEvent>(OnActorPlanAssigned);
        this.Town.Map.Events.ListenTo<EntitySpawnedEvent>(OnEntitySpawned);
        this.Town.Map.Events.ListenTo<EntityDisposedEvent>(OnEntityDisposed);
    }

    private void OnEntityDisposed(EntityDisposedEvent e)
    {
        var entity = e.Entity;
        if (entity.Def != ItemDefOf.UnfinishedItem)
            return;
        this._unfinishedItems.Remove(entity);
        var comp = entity.GetComponent<UnfinishedItemComp>();
        //var order = this._ordersById[comp.OrderId];
        if(this.TryGetOrder(comp.OrderId, out var order))
            order.UnfinishedItem = null;
        //var actor = comp.Author;
        var actor = entity.Author;
        if (!this._unfinishedByActor.TryGetValue(actor, out var list))
            return;
        list.Remove(entity);
        if (list.Count == 0)
            this._unfinishedByActor.Remove(actor);
    }

    private void OnEntitySpawned(EntitySpawnedEvent e)
    {
        var entity = e.Entity;
        if (entity.Def != ItemDefOf.UnfinishedItem)
            return;
        this._unfinishedItems.Add(entity);
        //var actor = entity.GetComponent<UnfinishedItemComp>().Author;
        var actor = entity.Author;
        if (!this._unfinishedByActor.TryGetValue(actor, out var list))
            this._unfinishedByActor[actor] = list = [];
        list.Add(entity);
    }

    private void OnActorPlanAssigned(ActorPlanAssignedEvent e)
    {
        if (!this._contractsByActor.TryGetValue(e.Actor, out var contract))
            return;
        //if (e.Behavior is null)
            this.ClearContract(contract);
    }

    private void ScanWorkstations()
    {
        foreach (var comp in this.Town.Map.BlockEntities
                        .Select(e =>
                        {
                            e.Comps.TryGetComp<BlockWorkstationComp>(out var c);
                            return c;
                        })
                        .Where(c => c is not null))
        {
            foreach (var cell in comp.Parent.CellsOccupied)
                this.RegisterWorkstation(cell, comp);
        }
    }
    void ScanOrders()
    {
        foreach (var workstation in this._byType.SelectMany(c => c.Value))
            foreach (var order in workstation.Orders)
            {
                this._ordersById.Add(order.Id, order);
                this.NextOrderId = Math.Max(this.NextOrderId, order.Id + 1);
            }
    }
    Dictionary<CraftingOrder, Actor> commitmentsByOrder = [];
    Dictionary<Actor, CraftingCommitment> commitments = [];
    internal void Commit(CraftingOrder order, Actor actor)
    {
        if(this.commitments.TryGetValue(actor, out var existing))
        {
            if (existing.Order != order)
                throw new Exception();
            return;
        }    
        this.commitments[actor] = new(actor, order);
        this.commitmentsByOrder[order] = actor;
    }
    internal void Uncommit(Actor actor)
    {
        if (!this.commitments.TryGetValue(actor, out var commitment))
            return;
        this.commitments.Remove(actor);
        this.commitmentsByOrder.Remove(commitment.Order);
    }
    internal bool TryGetCommitedOrder(Actor actor, out CraftingOrder order)
    {
        if (!this.commitments.TryGetValue(actor, out var commitment))
        {
            order = null;
            return false;
        }
        order = commitment.Order;
        return true;
    }
    internal bool CanCommit(Actor actor, CraftingOrder order)
    {
        if (this.commitmentsByOrder.TryGetValue(order, out var worker))
            return worker == actor;
        return true;
    }
    internal void MarkCompleted(CraftingOrder order, Actor actor, Entity product)
    {
        order.CompletedBy(actor);
        var commitment = this.commitments[actor];
        commitment.Product = product;
        //this.World.Events.Post(new ActorFinishedCraftingEvent(actor, order, product));
        foreach (var plugin in this.Plugins)
            plugin.Handle(actor, order, product);
    }

    private void AwardRecipeMastery(Actor actor, CraftingOrder order, Entity product)
    {
        actor.GetComponent<RecipesComp>().Add(product.Profile);
    }

    internal Entity? ProductToMove(Actor actor)
    {
        if (this.commitments.TryGetValue(actor, out var com))
            return com.Product;
        return null;
    }
    internal Contract Commit(Actor actor, BlockWorkstationComp workstation, CraftingOrder order, IEnumerable<Entity> ingredients)
    {
        var contract = new Contract(actor, workstation, order, ingredients);
        this._contractsByActor.Add(actor, contract);
        this._contractsByWorkstation.Add(workstation, contract);
        return contract;
    }
    internal void ClearContract(Contract contract)
    {
        this._contractsByActor.Remove(contract.Author);
        this._contractsByWorkstation.Remove(contract.Workstation);
    }
    internal Contract GetContract(Actor actor) => this._contractsByActor[actor];
    internal IReadOnlySet<Entity> GetUnfinishedItems(Actor actor)
    {
        if (!this._unfinishedByActor.TryGetValue(actor, out var list))
            return null;
        return list;
    }
    internal bool IsUnfinished(Entity entity) => this._unfinishedItems.Contains(entity);
    internal IEnumerable<(Entity item, BlockWorkstationComp workstation)> GetUnfinishedItemsOnWorkstations(Actor actor)
    {
        var items = this.GetUnfinishedItems(actor);
        if (items is null)
            yield break;
        foreach(var i in items)
            if(this._workstationsByPosition.TryGetValue(i.Cell.Below, out var workstation))
                yield return (i, workstation);
    }
    void RegisterOrder(CraftingOrder order)
    {
        this._ordersById.Add(order.Id, order);
        this._ordersByWorkstation.Add(order.Workstation.Parent, order);
    }
    public IEnumerable<CraftingOrder> GetAllOrdersUnsorted()
    {
        foreach (var order in this._ordersById.Values)
            yield return order;
    }
    private void OnBlocksUpdated(CellsInvalidatedEvent changed)
    {
        
    }
    private void OnBlockEntityAdded(BlockEntityAddedEvent e)
    {
        if (e.Entity.Def.Block is not BlockWorkstation)
            return;
        this.RegisterWorkstation(e.Entity.GetComp<BlockWorkstationComp>());
    }
    private void OnBlockEntityRemoved(BlockEntityRemovedEvent e)
    {
        if (e.Entity.Def.Block is not BlockWorkstation)
            return;
        this.UnregisterWorkstation(e.Entity.GetComp<BlockWorkstationComp>());
    }

    private bool UnregisterWorkstation(BlockWorkstationComp comp)
    {
        this._byType[comp.WorkstationType].Remove(comp);
        foreach (var pos in comp.Parent.CellsOccupied)
            this._workstationsByPosition.Remove(pos);
        foreach (var order in comp.Orders)
        {
            this._ordersById.Remove(order.Id);
            order.Dispose();
        }
        return true;
    }

    private void RegisterWorkstation(IntVec3 pos, BlockWorkstationComp workstation)
    {
        if (this._workstationsByPosition.TryGetValue(pos, out var existing))
            this._byType[existing.WorkstationType].Remove(existing);
        this._workstationsByPosition[pos] = workstation;
        this._byType[workstation.WorkstationType].Add(workstation);
    }
    private void RegisterWorkstation(BlockWorkstationComp workstation)
    {
        var entity = workstation.Parent;
        foreach(var cell in entity.CellsOccupied)
            //this._byPosition.Add(cell, workstation);
            this._workstationsByPosition[cell] = workstation;

        this._byType[workstation.WorkstationType].Add(workstation);
    }
    internal CraftingOrder CreateOrderNewInt(IntVec3 workstationPosition, AddOrderRequest req)
    {
        //var workstation = this.Map.GetBlockEntity(workstationPosition) ?? throw new ArgumentException($"Block entity doesn't exist at {workstationPosition}");
        //var comp = workstation.GetComp<BlockWorkstationComp>() ?? throw new ArgumentException($"{workstation} doesn't own a {nameof(BlockWorkstationComp)}");
        var product = req.ProductDef;
        var capability = req.WorkstationCapability;
        //var order = CreateOrderNew(workstationPosition, recipe, capability);
        var workstation = this.Map.GetBlockEntity(workstationPosition) ?? throw new ArgumentException($"Block entity doesn't exist at {workstationPosition}");
        var comp = workstation.GetComp<BlockWorkstationComp>() ?? throw new ArgumentException($"{workstation} doesn't own a {nameof(BlockWorkstationComp)}");

        //var reqs = CraftingSystem.GetValidIngredientsPerSlot(recipe);
        //var reqs = capability.Worker.GetValidIngredientsPerSlot(recipe);
        var reqs = capability.Worker.GetCraftingRulesStruct(product);
        if (reqs.Count() > workstation.CellsOccupied.Count)
        {
            Log.Error($"Not enough workstation modules to craft {product.LabelReadable}");
            return null;
        }
        var order = new CraftingOrder(this.NextOrderId++, comp, product, capability) { Source = req };
        comp.Orders.Add(order);
        this._ordersById.Add(order.Id, order);

        this.Map.Events.Post(new CraftOrderAddedEvent(comp, order));
        return order;
    }
   
    internal CraftingOrder DeleteOrder(int id)
    {
        if (!this._ordersById.TryGetValue(id, out var order)) throw new ArgumentException($"Order with id: {id} didn't exist");
        this._ordersById.Remove(id);
        var comp = this.Map.GetBlockEntity(order.Workstation.Global).GetComp<BlockWorkstationComp>();
        comp.Orders.Remove(order);
        this.Map.Events.Post(new CraftOrderRemovedEvent(comp, order));
        order.Dispose();
        return order;
    }

    static List<QualityDef> qualityTiers => field ??= Def.Get<QualityDef>().Where(d=>d.Threshold.HasValue).ToList();
    internal QualityDef GetCrafingQuality(Actor actor, CraftingOrder order)
    //=> this.Plugins.Sum(p => p.GetQualityBonus(actor, order));
    {
        var q = this.Plugins.Sum(p => p.GetQualityBonus(actor, order));
        var roll = RandomHelper.NextGaussian(q, 10);
        //var maxweight = qualityTiers.Sum(d => d.ProbabilityTableWeight);
        var table = qualityTiers.Select(d => (d.Threshold, d)).ToArray();
        for (int i = 0; i < table.Length; i++)
        {
            var o = table[i];
            if (roll <= o.Threshold)
                return o.d;
        }
        return table[^1].d;
    }

    internal bool TryGetOrder(int orderId, out CraftingOrder order)
        => this._ordersById.TryGetValue(orderId, out order);
    internal float GetProgressFor(Actor actor)
    {
        throw new NotImplementedException();
    }

    internal bool CanContinueItem(Actor actor, UnfinishedItemComp comp)
        => this.TryGetOrder(comp.OrderId, out var order) && order.Pending;

    internal CraftingOrder Get(CraftingOrderId id)
        => this._ordersById[id];
}

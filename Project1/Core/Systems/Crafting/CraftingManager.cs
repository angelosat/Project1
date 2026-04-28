using Project1.Core.AI;
using Project1.Core.Animations;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Legacy.Crafting;
using Project1.Core.Simulation;
using Project1.Core.Systems.Recipes;
using Project1.Core.Towns;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using SharpDX.Direct2D1.Effects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.Crafting;
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

    readonly Dictionary<CraftingOrderId, Actor> commitersByOrder = [];
    readonly Dictionary<Actor, CraftingCommitment> commitmentsByActor = [];
    readonly Dictionary<EntityRefId, CraftingCommitment> commitmentsByIngredients = [];

    readonly Dictionary<EntityRefId, CraftingOrder> ordersByUnfinishedItem = [];

    public IReadOnlySet<Entity> UnfinishedItems => this._unfinishedItems;
    public IEnumerable<BlockWorkstationComp> AllWorkstations => this._byType.SelectMany(d => d.Value);
    public IEnumerable<IGrouping<BlockEntity, List<CraftingOrder>>> OrdersByWorkstation => AllWorkstations.GroupBy(i => i.Parent, i => i.Orders);
    public IEnumerable<BlockWorkstationComp> AllWorkstationModules => this._workstationsByPosition.Values;

    static readonly List<ICraftingPlugin> Plugins = [new RecipeMasterySystem(), new CraftingPlugin_Skill()];

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
        this.World.Events.ListenTo<EntityDisposedEvent>(OnEntityDisposed);
    }

    private void OnEntityDisposed(EntityDisposedEvent e)
    {
        var entity = e.Entity;
        if (entity.Def != ItemDefOf.UnfinishedItem)
            return;
        this._unfinishedItems.Remove(entity);
        var comp = entity.GetComponent<UnfinishedItemComp>();
        //if(this.TryGetOrder(comp.OrderId, out var order))
        //    order.UnfinishedItem = null;
        if(this.ordersByUnfinishedItem.Remove(entity.RefId, out var order))
            order.UnfinishedItem = null;
        var actor = entity.Author;
        if (!this._unfinishedByActor.TryGetValue(actor, out var list))
            return;
        list.Remove(entity);
        if (list.Count == 0)
            this._unfinishedByActor.Remove(actor);
    }

    private void OnEntitySpawned(EntitySpawnedEvent e)
    {
        //var entity = e.Entity;
        //if (entity.Def != ItemDefOf.UnfinishedItem)
        //    return;
        //this._unfinishedItems.Add(entity);
        ////var actor = entity.GetComponent<UnfinishedItemComp>().Author;
        //var actor = entity.Author;
        //if (!this._unfinishedByActor.TryGetValue(actor, out var list))
        //    this._unfinishedByActor[actor] = list = [];
        //list.Add(entity);
    }
    internal void BindUnfinishedItem(Actor actor, CraftingOrder order, Entity entity)
    {
        if (entity.Def != ItemDefOf.UnfinishedItem)
            throw new Exception();
        this._unfinishedItems.Add(entity);
        if (!this._unfinishedByActor.TryGetValue(actor, out var list))
            this._unfinishedByActor[actor] = list = [];
        list.Add(entity);
        order.UnfinishedItem = entity;
        this.ordersByUnfinishedItem.Add(entity.RefId, order);
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
    internal void Commit(CraftingOrder order, Actor actor, List<CraftingOrder.OrderFeasibilityResult.Allocation> allocations)
    {
        var ingredientsPerBone = allocations.Select(a => (a.Bone, a.Entity));
    }
    internal override bool IsClaimedBySystem(Entity item)
        => this.commitmentsByIngredients.ContainsKey(item.RefId);
    internal void BindIngredient(Actor actor, CraftingOrder order, Entity targetStack, BoneDef bone)
    {
        var commitment = this.commitmentsByActor[actor];
        commitment.Bind(bone, targetStack);
        this.commitmentsByIngredients.Add(targetStack.RefId, commitment);
    }

    // Creates a new commitment with no ingredients bound
    internal void Commit(CraftingOrder order, Actor actor)
    {
        if(this.commitmentsByActor.TryGetValue(actor, out var existing))
        {
            if (existing.Order != order.Id)
                throw new Exception();
            return;
        }    
        this.commitmentsByActor[actor] = new(actor.RefId, order.Id, this.World.CurrentTick, order.GetBoneLayout());
        this.commitersByOrder[order.Id] = actor;
        order.CurrentWorker = actor.RefId;
    }
    internal void Uncommit(Actor actor)
    {
        if (!this.commitmentsByActor.TryGetValue(actor, out var commitment))
            return;
        this.commitmentsByActor.Remove(actor);
        this.commitersByOrder.Remove(commitment.Order);
        this.Get(commitment.Order).CurrentWorker = EntityRefId.Null;
        foreach (var ingredient in commitment.Ingredients.Values)
            this.commitmentsByIngredients.Remove(ingredient.Item);
    }
    internal bool TryGetCommitedOrder(Actor actor, out CraftingOrder order)
    {
        if (!this.commitmentsByActor.TryGetValue(actor, out var commitment))
        {
            order = null;
            return false;
        }
        order = this.Get(commitment.Order);
        return true;
    }
    internal bool CanCommit(Actor actor, CraftingOrder order)
    {
        if (this.commitersByOrder.TryGetValue(order.Id, out var worker))
            return worker == actor;
        return true;
    }
    internal void MarkCompleted(CraftingOrder order, Actor actor, Entity product)
    {
        order.CompletedBy(actor);
        var commitment = this.commitmentsByActor[actor];
        commitment.Product = product.RefId;
        this.World.Events.Post(new ActorFinishedCraftingEvent(actor.RefId, order.Id, product.RefId));
        foreach (var plugin in Plugins)
            plugin.Handle(actor, order, product);
    }

    private void AwardRecipeMastery(Actor actor, CraftingOrder order, Entity product)
    {
        actor.GetComponent<RecipesComp>().Add(product.Profile);
    }

    internal Entity? ProductToMove(Actor actor)
    {
        if (this.commitmentsByActor.TryGetValue(actor, out var com))
            return com.Product.HasValue ? this.World.Get(com.Product.Value) : null; // maybe check if the order actually involves a product? (eg. repairing)
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

            if (this.commitersByOrder.Remove(order.Id, out var actor))
                this.commitmentsByActor.Remove(actor);
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
    internal static QualityDef GetCrafingQuality(Actor actor, CraftingOrder order)
    //=> this.Plugins.Sum(p => p.GetQualityBonus(actor, order));
    {
        var q = Plugins.Sum(p => p.GetQualityBonus(actor, order));
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
    
    internal CraftingOrder Get(CraftingOrderId id)
        => this._ordersById[id];

    protected override void SaveExtra(SaveTag tag)
    {
        tag.Save("Commitments", this.commitmentsByActor.Values);
    }

    public override void Load(SaveTag tag)
    {
        //var commitments = tag.LoadList<CraftingCommitment>("Commitments");
        if(tag.TryLoadList<CraftingCommitment>("Commitments", out var commitments))
        foreach (var c in commitments)
            this.RegisterCommitmentInt(c);
    }

    void RegisterCommitmentInt(CraftingCommitment commitment)
    {
        var actor = this.World.Get<Actor>(commitment.Actor);
        this.commitersByOrder[commitment.Order] = actor;
        this.commitmentsByActor[actor] = commitment;
        foreach (var i in commitment.Ingredients.Values)
            this.commitmentsByIngredients[i.Item] = commitment;
    }

    //internal Entity CreateProductFromOrder(Actor actor, CraftingOrder order, IEnumerable<Entity> ingredients)
    //{
    //    var quality = this.GetCrafingQuality(actor, order);
    //    var product = order.WorkstationCapability.Worker.CreateProduct(actor, order, ingredients, quality);
    //    return product;
    //}
}

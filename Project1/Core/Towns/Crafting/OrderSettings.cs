using Project1.Core.Entities;
using Project1.Core.UI;
using Project1.Core.Base;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Interfaces;
using Project1.Core.Materials;
using Project1.Core.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using static Project1.Core.Towns.Crafting.OrderSettings.OrderFeasibilityResult;
using Project1.Core.Animations;

namespace Project1.Core.Towns.Crafting
{
    public class OrderSettings : IListable, ISaveableNewNew<OrderSettings>, ISerializableNew<OrderSettings>
    {
        public enum CraftMode
        {
            FixedAmount,       // Craft X times
            StockpileLimit,    // Craft until stockpile has at least X
            Infinite           // Craft forever
        }
        static public CraftMode[] AllModes = [CraftMode.FixedAmount, CraftMode.StockpileLimit, CraftMode.Infinite];
        public CraftMode Mode;
        int _amount = 1;
        public int Amount//; // X for FixedAmount or StockpileLimit, ignored for Infinite
        {
            get => this._amount;
            set => this._amount = Math.Max(value, 0);
        }
        public bool Enabled;
        public bool Pending => this.Mode == CraftMode.Infinite || this.Mode == CraftMode.FixedAmount && this.Amount > 0;
        public EntityCreationRequest Target { get; init; }

        // Explicit actor restriction
        public HashSet<int> AllowedActors = [];

        // Minimum skill requirement
        public int SkillFilter;

        public int Id { get; private set; }
        public SkillDef Skill { get; init; }
        public MaterialRefinementDef Refinement { get; init; }
        public Def ProductDef { get; internal set; }
        public WorkstationCapabilityDef WorkstationCapability { get; internal set;}
        public BlockWorkstationComp Workstation { get; internal set; }
        public string Label => this.ProductDef.Label;
        public Dictionary<BoneDef, HashSet<MaterialDef>> Filters = [];
        public Dictionary<BoneDef, IngredientRequirementNew> FiltersNew = [];
        public Dictionary<BoneDef, CraftingRule> Rules = [];
        Dictionary<BoneDef, MaterialDef[]> AcceptableMaterials = [];
        public bool IsAllowed(BoneDef bone, MaterialDef mat) => !this.Filters[bone].Contains(mat);
        public bool IsAllowed(BoneDef bone, MaterialRefinementDef form) => RawMaterialSystem.MaterialsByType[form.MaterialType].All(mat => !this.Filters[bone].Contains(mat));
        internal void Toggle(BoneDef bone, MaterialRefinementDef form, MaterialDef material)
        {
            var filters = this.Filters[bone];
            if(material is not null)
            {
                if (filters.Contains(material))
                    filters.Remove(material);
                else
                    filters.Add(material);
            }
            else
            {
                var allMats = RawMaterialSystem.MaterialsByType[form.MaterialType];
                if (allMats.Any(filters.Contains))
                    foreach (var mat in allMats)
                        filters.Remove(mat);
                else
                    foreach (var mat in allMats)
                        filters.Add(mat);
            }
            this.CacheAcceptableMaterials(bone);
            this.Workstation.Map.Events.Post(new CraftOrderUpdatedEvent(this));
        }
        public IEnumerable<BoneDef> GetSlotMapping() => CraftingSystem.GetSlotMapping(this.ProductDef);
        OrderSettings(Def recipe)
        {
            this.ProductDef = recipe;
            this.CreateFilters();
        }
        public OrderSettings(int id, BlockWorkstationComp owner, Def recipe) : this(recipe)
        {
            this.Id = id;
            this.Skill = CraftingSystem.GetCraftingSkill(recipe);
            this.Workstation = owner;
        }
        public OrderSettings(int id, BlockWorkstationComp owner, MaterialRefinementDef refinement)
        {
            this.Id = id;
            this.Skill = refinement.MaterialType.SkillToRefine;
            this.Refinement = refinement;
            this.Workstation = owner;
        }
        void CreateFilters()
        {
            foreach (var rule in CraftingSystem.GetCraftingRulesStruct(this.ProductDef))
            {
                this.Rules.Add(rule.Bone, rule);
                this.Filters.Add(rule.Bone, []);
                CacheAcceptableMaterials(rule.Bone);
            }
        }

        private void CacheAcceptableMaterials(BoneDef bone)
        {
            this.AcceptableMaterials[bone] =
                                this.Rules[bone].Forms
                                    .SelectMany(f => RawMaterialSystem.MaterialsByType[f.MaterialType])
                                    .Where(m => !Filters[bone].Contains(m))
                                    .ToArray();
        }

        //void CreateFiltersNew()
        //{
        //    var n = 0;
        //    var slots = this.Workstation.Parent.CellsOccupied.ToArray();
        //    foreach (var (bone, validRefinements, quantity) in CraftingSystem.GetCraftingRules(this.ProductDef))
        //    {
        //        var slot = slots[n++].Above;
        //        this.FiltersNew.Add(bone, new([.. validRefinements], quantity, slot, [.. this.Workstation.Map.GetEntitiesAt(slot)]));
        //    }
        //}
        public IEnumerable<IngredientRequirement> GetIngredientRequirementsOld()
        {
            yield return new(ItemDefOf.Ingredient, this.Refinement.Source, 1, this.Workstation.Global.Above, RawMaterialSystem.MaterialsByType[this.Refinement.MaterialType]);
        }
        public IEnumerable<IngredientRequirementNew> GetIngredientRequirements()
        {
            var n = 0;
            var slots = this.Workstation.Parent.CellsOccupied.ToArray();
            foreach (var (validRefinements, quantity) in CraftingSystem.GetValidIngredientsPerSlot(this.ProductDef))
            {
                var slot = slots[n++].Above;
                yield return new([.. validRefinements], quantity, slot, [.. this.Workstation.Map.GetEntitiesAt(slot)]);
            }
        }
        (IntVec3 cell, IEnumerable<Entity> cellEntities) GetEntitiesAtWorkstationSlot(BoneDef bone)
        {
            var slots = this.Workstation.Parent.CellsOccupied.ToArray();
            var rules = CraftingSystem.GetCraftingRulesStruct(this.ProductDef).ToList();
            var slotId = rules.FindIndex(r => r.Bone == bone);
            var slot = slots[slotId].Above;
            return (slot, this.Workstation.Map.GetEntitiesAt(slot));
        }
        public bool Matches(Entity item)
        {
            var rules = CraftingSystem.GetCraftingRulesStruct(this.ProductDef);
            foreach(var rule in rules)
            {
                if (!rule.Matches(item, out _))
                    return false;
                if (this.Filters[rule.Bone].Contains(item.PrimaryMaterial))
                    return false;
            }
            return true;
        }
        public bool MatchesPartial(Entity item, out int demand)
        {
            if (item.Def == ItemDefOf.Ingredient)
            {
                var rules = CraftingSystem.GetCraftingRulesStruct(this.ProductDef);
                foreach (var rule in rules)
                {
                    if (!rule.Matches(item, out _) && this.Filters[rule.Bone].Contains(item.PrimaryMaterial))
                        continue;
                    demand = rule.Quantity;
                    var slotEntities = this.GetEntitiesAtWorkstationSlot(rule.Bone);
                    foreach (var inSlot in slotEntities.cellEntities)
                        if (rule.Matches(inSlot, out var alreadyCovered))
                            demand -= alreadyCovered;
                    if (demand > 0)
                        return true;
                }
            }
            demand = -1;
            return false;
        }
        public IEnumerable<Entity> AlreadyBoundInSlots()
        {
            foreach (var (bone, rule) in this.Rules)
            {
                var items = this.GetEntitiesAtWorkstationSlot(bone);
                foreach (var item in items.cellEntities)
                    if (rule.Matches(item, out _))
                        yield return item;
            }
        }
        public bool IsReadyToCraft(out List<Entity> handled)
        {
            handled = new();
            foreach (var (bone, rule) in this.Rules)
            {
                var items = this.GetEntitiesAtWorkstationSlot(bone);
                handled.AddRange(items.cellEntities);
                if (!items.cellEntities.Any(item => rule.Matches(item, out var missing) && missing == 0))
                    return false;
            }
            return true;
        }
        public bool CheckFuelReq()
        {
            if (this.ProductDef is not MaterialRefinementDef refinement)
                return true;
            var fuelReq = refinement.FuelConsumption;
            var workstation = this.Workstation.Parent;
            var fuelcomp = workstation.GetComp<BlockFuelComp>();
            return fuelReq <= fuelcomp.FuelAvailable;
        }
        public int GetFuelReq()
        {
            if (this.ProductDef is not MaterialRefinementDef refinement)
                return 0;
            return refinement.FuelConsumption;
        }
        public bool TryConsumeFuel()
        {
            var fuel = this.GetFuelReq();
            var workstation = this.Workstation.Parent;
            var fuelcomp = workstation.GetComp<BlockFuelComp>();
            return fuelcomp.TryConsumeFuel(fuel);
        }
        //public bool IsFeasibleNew(IReadOnlyList<Entity> items, out (Entity bestEntity, int bestAmount) allocation)
        //{
        //    allocation = default;
        //    Dictionary<MaterialDef, MaterialPoolEntry> pool = [];
        //    Dictionary<Entity, IntVec3> entityToSlot = [];
        //    foreach (var i in items)
        //    {
        //        if(!pool.TryGetValue(i.PrimaryMaterial, out var entry))
        //        {
        //            entry = new();
        //            pool[i.PrimaryMaterial] = entry;
        //        }
        //        entry.Available += i.StackSize;
        //        entry.Candidates.Add(i);
        //    }
        //    // sort rules by more restrictive first
        //    var sortedRules = this.Rules.Values
        //        .OrderBy(rule => rule.Forms.SelectMany(f => RawMaterialSystem.MaterialsByType[f.MaterialType]).Count(m => !this.Filters[rule.Bone].Contains(m)));
        //    foreach (var rule in sortedRules)
        //    {
        //        var disallowed = this.Filters[rule.Bone];

        //        var matchedMaterial = rule.Forms
        //            .SelectMany(f => RawMaterialSystem.MaterialsByType[f.MaterialType])
        //            .Where(m => !this.Filters[rule.Bone].Contains(m))
        //            .FirstOrDefault(m => pool.TryGetValue(m, out var c) && c.Available >= rule.Quantity);

        //        if (matchedMaterial is null)
        //            return false; // slot cannot be satisfied

        //        var entry = pool[matchedMaterial];
        //        entry.Available -= rule.Quantity; // consume
        //        if (allocation.bestEntity is null)
        //        {
        //            var nextItem = entry.Candidates.First();
        //            allocation = (nextItem, Math.Min(rule.Quantity, nextItem.StackSize));
        //        }
        //    }
        //    return true;
        //}
        public OrderFeasibilityResult IsFeasibleNew(
            IReadOnlyList<Entity> candidates, 
            HashSet<IntVec3> excludedSlots,
            Entity preferredEntity // usually the carried item, or null
        )
        {
            var result = new OrderFeasibilityResult();

            // 1. Build pool by material
            var pool = new Dictionary<(MaterialDef, Def), MaterialPoolEntry>();
            foreach (var e in candidates)
            {
                if (!pool.TryGetValue((e.PrimaryMaterial, e.Profile), out var entry))
                {
                    entry = new MaterialPoolEntry();
                    pool[(e.PrimaryMaterial, e.Profile)] = entry;
                }
                entry.Available += e.StackSize;
                entry.Candidates.Add(e);
            }


            // 2. Sort rules by restrictiveness (fewest valid materials first)
            var sortedRules = this.Rules.Values
                .OrderBy(rule => GetAcceptableMaterials(rule.Bone).Length);

            // 3. Allocate each slot
            foreach (var rule in sortedRules)
            {
                var acceptableMaterials = GetAcceptableMaterials(rule.Bone);

                // 3a. Check if slot already satisfied by in-slot entity
                var (slotCell, slotEntities) = GetEntitiesAtWorkstationSlot(rule.Bone);
                if (slotEntities.FirstOrDefault(e => rule.Matches(e, out _)) is Entity inSlot)
                {
                    //result.FilledSlots.Add(slotCell);
                    result.ArmedSlots.Add((slotCell, inSlot));
                    continue;
                }

                // Skip this slot if it’s excluded (already being deposited into by another actor)
                if (excludedSlots.Contains(slotCell))
                    continue;

                if (preferredEntity != null &&
                    rule.Matches(preferredEntity, out _) &&
                    pool.TryGetValue((preferredEntity.PrimaryMaterial, preferredEntity.Profile), out var entry) &&
                    entry.Available > 0)
                {
                    entry.Available -= rule.Quantity;

                    // Record allocation
                    result.Allocations.Add(new Allocation
                    {
                        Entity = preferredEntity,
                        Slot = slotCell,
                        Quantity = Math.Min(rule.Quantity, preferredEntity.StackSize)
                    });
                    continue;
                }

                // 3b. Find a material that can satisfy this slot
                //var matchedMaterial = acceptableMaterials
                //    .FirstOrDefault(m => pool.TryGetValue((m, out var p) && p.Available >= rule.Quantity);

                //if (matchedMaterial == null)
                //{
                //    // This slot cannot be satisfied; order is infeasible
                //    result.State = CraftingOrderState.NotEnoughItems;
                //    return result;
                //}

                //// 3c. Consume from pool
                //var matEntry = pool[matchedMaterial];
                var matchedMaterialForm = GetAcceptableMaterialForms(rule.Bone)
                    .FirstOrDefault(mf => pool.TryGetValue(mf, out var entry) && entry.Available >= rule.Quantity);

                if (matchedMaterialForm == default)
                {
                    result.State = CraftingOrderState.NotEnoughItems;
                    return result;
                }

                var matEntry = pool[matchedMaterialForm];
                matEntry.Available -= rule.Quantity;

                // 3d. Pick candidate entity to satisfy this slot
                var nextEntity = matEntry.Candidates.First();
                var allocatedQuantity = Math.Min(rule.Quantity, nextEntity.StackSize);

                result.Allocations.Add(new OrderFeasibilityResult.Allocation
                {
                    Entity = nextEntity,
                    Slot = slotCell,
                    Quantity = allocatedQuantity
                });
            }

            // 4. Determine overall state
            //result.State = result.FilledSlots.Count == this.Rules.Count
            //    ? CraftingOrderState.ReadyToCraft
            //    : CraftingOrderState.NeedsTransfer;
            result.State =
                result.ArmedSlots.Count == Rules.Count
                    ? CraftingOrderState.ReadyToCraft
                    : CraftingOrderState.NeedsTransfer;

            return result;
        }

        public OrderFeasibilityResult IsFeasibleNewPrevious(IReadOnlyList<Entity> candidates)
        {
            var result = new OrderFeasibilityResult();

            // 1. Build pool by material
            var pool = new Dictionary<MaterialDef, MaterialPoolEntry>();
            foreach (var e in candidates)
            {
                if (!pool.TryGetValue(e.PrimaryMaterial, out var entry))
                {
                    entry = new MaterialPoolEntry();
                    pool[e.PrimaryMaterial] = entry;
                }
                entry.Available += e.StackSize;
                entry.Candidates.Add(e);
            }

            // 2. Sort rules by restrictiveness (fewest valid materials first)
            var sortedRules = this.Rules.Values
                .OrderBy(rule => GetAcceptableMaterials(rule.Bone).Length);

            // 3. Allocate each slot
            foreach (var rule in sortedRules)
            {
                var acceptableMaterials = GetAcceptableMaterials(rule.Bone);

                // 3a. Check if slot already satisfied by in-slot entity
                var (slotCell, slotEntities) = GetEntitiesAtWorkstationSlot(rule.Bone);
                if (slotEntities.FirstOrDefault(e => rule.Matches(e, out _)) is Entity inSlot)
                {
                    result.FilledSlots.Add(slotCell);
                    continue;
                }

                // 3b. Find a material that can satisfy this slot
                var matchedMaterial = acceptableMaterials
                    .FirstOrDefault(m => pool.TryGetValue(m, out var p) && p.Available >= rule.Quantity);

                if (matchedMaterial == null)
                {
                    result.State = CraftingOrderState.NotEnoughItems;
                    return result; // slot impossible → order impossible
                }

                // 3c. Consume from pool
                var matEntry = pool[matchedMaterial];
                matEntry.Available -= rule.Quantity;

                // 3d. Pick candidate entity to satisfy this slot
                var nextEntity = matEntry.Candidates.First();
                var allocatedQuantity = Math.Min(rule.Quantity, nextEntity.StackSize);

                result.Allocations.Add(new OrderFeasibilityResult.Allocation
                {
                    Entity = nextEntity,
                    Slot = slotCell,
                    Quantity = allocatedQuantity
                });
            }

            // 4. Determine overall state
            result.State = result.FilledSlots.Count == this.Rules.Count
                ? CraftingOrderState.ReadyToCraft
                : CraftingOrderState.NeedsTransfer;

            return result;
        }

        // Helper to get cached acceptable materials (dynamic filters)
        private MaterialDef[] GetAcceptableMaterials(BoneDef bone)
        {
            if (!AcceptableMaterials.TryGetValue(bone, out var mats))
            {
                var rule = this.Rules[bone];
                mats = rule.Forms
                    .SelectMany(f => RawMaterialSystem.MaterialsByType[f.MaterialType])
                    .Where(m => !Filters[bone].Contains(m))
                    .ToArray();
                AcceptableMaterials[bone] = mats;
            }
            return mats;
        }
        // Helper to get cached acceptable materials (dynamic filters)
        public (MaterialDef Material, MaterialRefinementDef Form)[] GetAcceptableMaterialForms(BoneDef bone)
        {
            return this.Rules[bone].Forms
                .SelectMany(f => RawMaterialSystem.MaterialsByType[f.MaterialType]
                    .Where(m => !Filters[bone].Contains(m))
                    .Select(m => (m, f))) // pair material + form
                .ToArray();
        }


        public enum CraftingOrderState
        {
            NotEnoughItems,      // No ingredients available at all
            NeedsTransfer,       // Ingredients exist on the map but not in slots
            ReadyToCraft         // All required ingredients are already in slots
        }
        sealed class MaterialPoolEntry
        {
            public int Available;
            public List<Entity> Candidates = new();
            public Dictionary<Entity, int> Remaining = new();
            public MaterialRefinementDef Form;
        }

        /// <summary>
        /// Result returned to planner
        /// </summary>
        public class OrderFeasibilityResult
        {
            public CraftingOrderState State;
            public List<IntVec3> FilledSlots = new(); // slots already satisfied
                                                      // Slots that already contain valid ingredients
            public List<(IntVec3 Slot, Entity Entity)> ArmedSlots = new();
            public struct Allocation
            {
                public Entity Entity;
                public IntVec3 Slot;
                public int Quantity;
            }

            public List<Allocation> Allocations = new();
        }


        public bool IsFeasible(IReadOnlyList<Entity> items)
        {
            Dictionary<MaterialDef, int> pool = [];
            foreach (var i in items)
                pool[i.PrimaryMaterial] += i.StackSize;

            // sort rules by more restrictive first
            var sortedRules = this.Rules.Values
                .OrderBy(rule => rule.Forms.SelectMany(f => RawMaterialSystem.MaterialsByType[f.MaterialType]).Count(m => !this.Filters[rule.Bone].Contains(m)));
            foreach (var rule in sortedRules)
            {
                var disallowed = this.Filters[rule.Bone];

                var matchedMaterial = rule.Forms
                    .SelectMany(f => RawMaterialSystem.MaterialsByType[f.MaterialType])
                    .Where(m => !this.Filters[rule.Bone].Contains(m))
                    .FirstOrDefault(m => pool.TryGetValue(m, out var c) && c >= rule.Quantity);

                if (matchedMaterial is null)
                    return false; // slot cannot be satisfied

                pool[matchedMaterial] -= rule.Quantity; // consume
            }
            return true;
        }
        public bool CanActorPerform(Actor actor)
        {
            if (!this.Enabled) return false;
            if (this!.AllowedActors.Contains(actor.RefId)) return false;
            if (actor.Skills.GetSkill(this.Skill).Level < SkillFilter) return false;
            return true;
        }
        public bool ShouldQueue(int currentStock)
        {
            return Mode switch
            {
                CraftMode.FixedAmount => Amount > 0,
                CraftMode.StockpileLimit => currentStock < Amount,
                CraftMode.Infinite => true,
                _ => false
            };
        }

        public Control GetListControlGui()
        {
            return new OrderSettingsGui(this);
        }
        public void ChangePriority(int priorityDelta)
        {
            if (priorityDelta > 0)
                this.Workstation.MoveDown(this);
            else if (priorityDelta < 0)
                this.Workstation.MoveUp(this);
        }

        internal EntityCreationRequest GetCreationRequest()
        {
            return new EntityCreationRequest(this.ProductDef, null, stackSize: 1);
        }

        internal void CompletedBy(Actor actor)
        {
            if(this.Mode == CraftMode.FixedAmount) this.Amount--;
            this.Workstation.Map.Events.Post(new CraftOrderUpdatedEvent(this));
            this.Workstation.Map.Events.Post(new CraftOrderCompletedEvent(this, actor));
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.Id.Save(tag, "Id");
            ((int)this.Mode).Save(tag, "Mode");
            this.Amount.Save(tag, "Amount");
            this.ProductDef.Save(tag, "Product");

            return tag;
        }

        public static OrderSettings Create(SaveTag tag)
        {
            var order = new OrderSettings(tag.LoadDef<Def>("Product"));
            if (tag.TryLoadInt("Id", out var id)) order.Id = id;
            if (tag.TryLoadInt("Mode", out var mode)) order.Mode = (CraftMode)mode;
            if (tag.TryLoadInt("Amount", out var amount)) order.Amount = amount;
            return order;
        }

        public OrderSettings Read(IDataReader r)
        {
            this.Id = r.ReadInt32();
            this.Mode = (CraftMode)r.ReadInt32();
            this.Amount = r.ReadInt32();
            return this;
        }

        public void Write(IDataWriter w)
        {
            w.Write(this.ProductDef);
            w.Write(this.Id);
            w.Write((int)this.Mode);
            w.Write(this.Amount);
        }

        public static OrderSettings Create(IDataReader r)
        {
            var product = r.ReadDef();
            return new OrderSettings(product).Read(r);
        }

        
    }
    public record IngredientRequirementNew(HashSet<MaterialRefinementDef> Refinements, int Quantity, IntVec3 Slot, List<Entity> InSlot)
    {
        public readonly HashSet<MaterialDef> FilteredMaterials = [];
        internal bool Matches(Entity e)
        {
            return e.Def == ItemDefOf.Ingredient && this.Refinements.Contains(e.Profile) && !this.FilteredMaterials.Contains(e.Body.Material);
        }
        internal bool MatchesPartial(Entity e, out int missing)
        {
            if (e.Def == ItemDefOf.Ingredient && this.Refinements.Contains(e.Profile) && !this.FilteredMaterials.Contains(e.Body.Material))
            {
                missing = this.Quantity - e.StackSize;
                return true;
            }
            missing = -1;
            return false;
        }
        public IngredientRequirementNew ToggleMaterial(MaterialDef mat)
        {
            if (this.FilteredMaterials.Contains(mat))
                this.FilteredMaterials.Remove(mat);
            else
                this.FilteredMaterials.Add(mat);
            return this;
        }
    }

    public class IngredientRequirement(ItemDef itemType, Def context, int quantity, IntVec3 workstationSlot, HashSet<MaterialDef> materials)
    {
        public readonly ItemDef ItemType = itemType;
        public readonly Def Context = context;
        public readonly int Quantity = quantity;
        public readonly IntVec3 Slot = workstationSlot;

        public readonly HashSet<MaterialDef> FilteredMaterials = materials;

        internal bool Matches(Entity e)
        {
            return e.Def == this.ItemType && e.Profile == this.Context && this.FilteredMaterials.Contains(e.Body.Material);
        }
        internal bool MatchesPartial(Entity e, out int missing)
        {
            if (e.Def == this.ItemType && e.Profile == this.Context && !this.FilteredMaterials.Contains(e.Body.Material))
            {
                missing = this.Quantity - e.StackSize;
                return true;
            }
            missing = -1;
            return false;
        }
        //public IngredientRequirement ToggleMaterial(MaterialDef mat)
        //{
        //    if (this.Materials.Contains(mat))
        //        this.Materials.Remove(mat);
        //    else
        //        this.Materials.Add(mat);
        //    return this;
        //}
    }
}

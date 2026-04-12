using Project1.Core.Animations;
using Project1.Core.Blocks;
using Project1.Core.Blocks.Comps;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Skills;
using Project1.Core.Systems.Materials;
using Project1.Core.UI;
using Project1.Framework;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Crafting
{
    public class CraftingOrder : IListable, ISaveableNewNew<CraftingOrder>, ISerializableNew<CraftingOrder>
    {
        public enum CraftMode
        {
            FixedAmount,       // Craft X times
            StockpileLimit,    // Craft until stockpile has at least X
            Infinite           // Craft forever
        }
        static public readonly CraftMode[] AllModes = [CraftMode.FixedAmount, CraftMode.StockpileLimit, CraftMode.Infinite];
        public CraftMode Mode;
        int _amount = 1;
        public int Amount//; // X for FixedAmount or StockpileLimit, ignored for Infinite
        {
            get => this._amount;
            set => this._amount = Math.Max(value, 0);
        }
        public bool Enabled;
        public bool IsDisposed { get; private set; }
        public bool Pending => !this.IsDisposed && (this.Mode == CraftMode.Infinite || this.Mode == CraftMode.FixedAmount && this.Amount > 0);
        public IEnumerable<BoneDef> GetSlotMapping() => this.WorkstationCapability.Worker.GetBoneLayout();

        public HashSet<int> AllowedActors = [];

        public int SkillFilter;

        public EntityRefId CurrentWorker;
        public int Id { get; private set; }
        public SkillDef Skill { get; init; }
        public MaterialRefinementDef Refinement { get; init; }
        public Def ProductDef { get; internal set; }
        public WorkstationCapabilityDef WorkstationCapability { get; internal set;}
        public BlockWorkstationComp Workstation { get; internal set; }
        public string LabelReadable
            => this.ProductDef is Def def ?
            $"{this.WorkstationCapability.LabelReadable}: {def.LabelReadable}" :
            $"{this.WorkstationCapability.LabelReadable}";
        public Dictionary<BoneDef, HashSet<MaterialDef>> Filters = [];
        public Dictionary<BoneDef, IngredientRequirement> FiltersNew = [];
        public Dictionary<BoneDef, CraftingRule> Rules = [];
        readonly Dictionary<BoneDef, MaterialDef[]> AcceptableMaterials = [];
        internal Entity UnfinishedItem;
        internal void Dispose() => this.IsDisposed = true;

        public bool IsAllowed(BoneDef bone, MaterialDef mat) => !this.Filters[bone].Contains(mat);
        public bool IsAllowed(BoneDef bone, MaterialTypeDef form) => MaterialSystem.MaterialsByType[form].All(mat => !this.Filters[bone].Contains(mat));
        internal void Toggle(BoneDef bone, MaterialTypeDef form, MaterialDef material)
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
                var allMats = MaterialSystem.MaterialsByType[form];
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
        CraftingOrder(Def recipe, WorkstationCapabilityDef capability)
        {
            this.WorkstationCapability = capability;
            this.ProductDef = recipe;
            this.CreateFilters();
        }
        public CraftingOrder(int id, BlockWorkstationComp owner, Def recipe, WorkstationCapabilityDef capability) : this(recipe, capability)
        {
            this.Id = id;
            this.Skill = capability.Worker.CraftingSkill;
            this.Workstation = owner;
        }

        public CraftingOrder(int id, BlockWorkstationComp owner, MaterialRefinementDef refinement)
        {
            this.Id = id;
            this.Skill = refinement.MaterialType.SkillToRefine;
            this.Refinement = refinement;
            this.Workstation = owner;
        }

        void CreateFilters()
        {
            foreach (var rule in this.WorkstationCapability.Worker.GetCraftingRulesStruct(this.ProductDef))
            {
                this.Rules.Add(rule.Bone, rule);
                this.Filters.Add(rule.Bone, []);
                CacheAcceptableMaterials(rule.Bone);
            }
        }

        private void CacheAcceptableMaterials(BoneDef bone)
        {
            this.AcceptableMaterials[bone] =
                                [.. this.Rules[bone].MaterialTypes.SelectMany(mt=>
                                MaterialSystem.MaterialsByType[mt]
                                    .Where(m => !Filters[bone].Contains(m)))];
        }
       
        (IntVec3 cell, IEnumerable<Entity> cellEntities) GetEntitiesAtWorkstationSlot(BoneDef bone)
        {
            var slots = this.Workstation.Parent.CellsOccupied.ToArray();
            var rules = this.WorkstationCapability.Worker.GetCraftingRulesStruct(this.ProductDef).ToList();
            var slotId = rules.FindIndex(r => r.Bone == bone);
            var slot = slots[slotId].Above;
            return (slot, this.Workstation.Map.GetEntitiesAt(slot));
        }

        public bool CheckFuelReq()
        {
            var (resource, value) = this.WorkstationCapability.Worker.ResourceConsumption;
            if (resource == null)
                return true;
            var workstation = this.Workstation.Parent;
            var currentResource = workstation.GetComp<BlockResourcesComp>().GetValue(resource);
            return value <= currentResource;
        }

        public bool TryConsumeFuel()
        {
            var (resource, value) = this.WorkstationCapability.Worker.ResourceConsumption;
            if (resource == null)
                return true;
            var workstation = this.Workstation.Parent;
            var comp = workstation.GetComp<BlockResourcesComp>();
            if (comp.GetValue(resource) < value)
                return false;
            comp.ApplyDelta(resource, -value);
            return true;
        }

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
                    result.Allocations.Add(new OrderFeasibilityResult.Allocation
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
                mats =
                    //rule.Profiles
                    //.SelectMany(f => RawMaterialSystem.MaterialsByType[f.MaterialType])
                    //[.. RawMaterialSystem.MaterialsByType[rule.MaterialType].Where(m => !Filters[bone].Contains(m))];
                    [.. rule.MaterialTypes.SelectMany(t=>MaterialSystem.MaterialsByType[t].Where(m => !Filters[bone].Contains(m)))];
                AcceptableMaterials[bone] = mats;
            }
            return mats;
        }
        //// Helper to get cached acceptable materials (dynamic filters)
        //public (MaterialDef Material, MaterialRefinementDef Form)[] GetAcceptableMaterialForms(BoneDef bone)
        //{
        //    return 
        //        this.Rules[bone].Profiles
        //        .SelectMany(f => RawMaterialSystem.MaterialsByType[f.MaterialType]
        //            .Where(m => !Filters[bone].Contains(m))
        //            .Select(m => (m, f))) // pair material + form
        //        .ToArray();
        //}

        // Helper to get cached acceptable materials (dynamic filters)
        public (MaterialDef Material, Def Form)[] GetAcceptableMaterialForms(BoneDef bone)
        {
            var rule = this.Rules[bone];
            //var mats = RawMaterialSystem.MaterialsByType[rule.MaterialTypes].Where(m => !Filters[bone].Contains(m));
            //var array = rule.MaterialTypes
            //    //.Profiles
            //    .SelectMany(mt => RawMaterialSystem.MaterialsByType[mt]
            //        .Select(m => 
            //        (m, rule.Profiles))) // pair material + form
            //    .ToArray();
            var array = rule.MaterialTypes
                .SelectMany(mt => MaterialSystem.MaterialsByType[mt])
                .SelectMany(p => rule.Profiles, (m, p) => (m, p)).ToArray();
            return array;
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
            //return new EntityCreationRequest(this.ProductDef, null, stackSize: 1);
            return new EntityCreationRequest(this.ProductDef, null, stackSize: this.WorkstationCapability.Worker.GetOutputStackSize(this.ProductDef));
        }

        internal void CompletedBy(Actor actor)
        {
            this.CurrentWorker = EntityRefId.Null;
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
            this.WorkstationCapability.Save(tag, "Domain");

            return tag;
        }

        public static CraftingOrder Create(SaveTag tag)
        {
            var product = tag.LoadDef<Def>("Product");
            var domain = tag.LoadDef<WorkstationCapabilityDef>("Domain");
            var order = new CraftingOrder(product, domain);
            if (tag.TryLoadInt("Id", out var id)) order.Id = id;
            if (tag.TryLoadInt("Mode", out var mode)) order.Mode = (CraftMode)mode;
            if (tag.TryLoadInt("Amount", out var amount)) order.Amount = amount;
            return order;
        }

        public CraftingOrder Read(IDataReader r)
        {
            this.Id = r.ReadInt32();
            this.Mode = (CraftMode)r.ReadInt32();
            this.Amount = r.ReadInt32();
            return this;
        }

        public void Write(IDataWriter w)
        {
            w.Write(this.ProductDef);
            w.Write(this.WorkstationCapability);
            w.Write(this.Id);
            w.Write((int)this.Mode);
            w.Write(this.Amount);
        }
        public static CraftingOrder Create(IDataReader r)
        {
            var product = r.ReadDef();
            var domain = r.ReadDef<WorkstationCapabilityDef>();
            return new CraftingOrder(product, domain).Read(r);
        }

    }
}
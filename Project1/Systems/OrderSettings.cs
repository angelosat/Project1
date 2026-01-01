using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
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
        public bool Pending => this.Amount > 0;
        public EntityCreationRequest Target { get; init; }

        // Explicit actor restriction
        public HashSet<int> AllowedActors = [];

        // Minimum skill requirement
        public int SkillFilter;

        public int Id { get; private set; }
        public SkillDef Skill { get; init; }
        public MaterialRefinementDef Refinement { get; init; }
        public Def ProductDef { get; internal set; }
        public BlockWorkstationComp Workstation { get; internal set; }
        public string Label => this.ProductDef.Label;

        public IEnumerable<BoneDef> GetSlotMapping() => CraftingSystem.GetSlotMapping(this.ProductDef);
        OrderSettings()
        {
            
        }
        public OrderSettings(int id, BlockWorkstationComp owner, Def recipe)
        {
            this.Id = id;
            this.Skill = CraftingSystem.GetCraftingSkill(recipe);
            this.ProductDef = recipe;
            this.Workstation = owner;
        }
        public OrderSettings(int id, BlockWorkstationComp owner, MaterialRefinementDef refinement)
        {
            this.Id = id;
            this.Skill = refinement.MaterialType.SkillToRefine;
            this.Refinement = refinement;
            this.Workstation = owner;
        }
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
            this.Amount--;
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
            var order = new OrderSettings();
            if (tag.TryLoadInt("Id", out var id)) order.Id = id;
            if (tag.TryLoadInt("Mode", out var mode)) order.Mode = (CraftMode)mode;
            if (tag.TryLoadInt("Amount", out var amount)) order.Amount = amount;
            if (tag.TryLoadDefOut<Def>("Product", out var def)) order.ProductDef = def;
            return order;
        }

        public OrderSettings Read(IDataReader r)
        {
            this.Id = r.ReadInt32();
            this.Mode = (CraftMode)r.ReadInt32();
            this.Amount = r.ReadInt32();
            this.ProductDef = r.ReadDef();
            return this;
        }

        public void Write(IDataWriter w)
        {
            w.Write(this.Id);
            w.Write((int)this.Mode);
            w.Write(this.Amount);
            w.Write(this.ProductDef);
        }

        public static OrderSettings Create(IDataReader r)
        {
            return new OrderSettings().Read(r);
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

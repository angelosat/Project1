using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public partial class Ingredient : Inspectable
    {
        public ItemDef ItemDef;
        public MaterialDef Material;
        public MaterialTypeDef MaterialType;
        public int Amount = 1;
        /// <summary>
        /// The max value of 1 means that the amount of material required should be enough to completely fill the volume of a block.
        /// Used for calculation of actual ingredient amounts depending on ingredient dimensions.
        /// </summary>
        public string Name;
        readonly List<Modifier> Modifiers = new();
        readonly HashSet<ItemCategory> SpecifiedCategories = new();
        readonly HashSet<MaterialDef> SpecifiedMaterials = new();
        readonly HashSet<ItemDef> SpecifiedItemDefs = new();
        public IngredientRestrictions DefaultRestrictions = new();
        readonly List<Func<Entity, bool>> SpecialFilters = new();

        HashSet<MaterialDef> _resolvedMaterials;
        HashSet<ItemDef> _resolvedItemDefs;
        public HashSet<MaterialDef> ResolvedMaterials
        { 
            get
            {
                this.TryResolve();
                return this._resolvedMaterials;
            }
        }
        public HashSet<ItemDef> ResolvedItemDefs
        {
            get
            {
                this.TryResolve();
                return this._resolvedItemDefs;
            }
        }
        bool Resolved;
        public bool IsPreserved;

        public Ingredient(string name)
        {
            this.Name = name;
        }
        public Ingredient()
        {

        }
        public Ingredient(ItemDef item = null, MaterialDef material = null, MaterialTypeDef materialType = null, int amount = 1)
        {
            ItemDef = item;
            Material = material;
            MaterialType = materialType;
            Amount = amount;
            if (item is not null)
                this.SetAllow(item, true);
            if (material is not null)
                this.SetAllow(material, true);
            if (materialType is not null)
                this.SetAllow(materialType, true);
        }
        public IEnumerable<MaterialDef> GetAllValidMaterials()
        {
            this.TryResolve();
            foreach (var i in _resolvedMaterials)
                yield return i;
        }

        internal IEnumerable<ItemMaterialAmount> GetItemMaterialAmounts(Block block)
        {
            this.TryResolve();
            foreach (var item in this._resolvedItemDefs)
            {
                foreach (var mat in this._resolvedMaterials)
                {
                    if (mat.Type == item.DefaultMaterialType)
                        yield return new ItemMaterialAmount(item, mat, block.Size.Volume * item.StackCapacity / block.BuildProperties.Dimension);// this.GetFinalIngredientAmount(item));
                }
            }
        }

        public IEnumerable<ItemDef> GetAllValidItemDefs()
        {
            this.TryResolve();
            foreach (var d in this._resolvedItemDefs)
                yield return d;
        }
        public bool Evaluate(Entity item)
        {
            this.TryResolve();

            return this._resolvedItemDefs.Contains(item.Def)
                && this._resolvedMaterials.Contains(item.PrimaryMaterial)
                && this.SpecialFilters.All(f => f(item));
        }
        public Ingredient IsBuildingMaterial()
        {
            this.Modifiers.Add(new Modifier("Any building material", def => def.CraftingProperties?.IsBuildingMaterial ?? false));
            return this;
        }

        public Ingredient SetAllow(IEnumerable<MaterialTypeDef> types, bool allow)
        {
            foreach (var t in types)
                this.SetAllow(t, allow);
            return this;
        }
        public Ingredient SetAllow(ItemCategory category, bool allow)
        {
            if (allow)
                this.SpecifiedCategories.Add(category);
            else
                this.SpecifiedCategories.Remove(category);
            return this;
        }
        public Ingredient SetAllow(MaterialDef mat, bool allow)
        {
            if (mat is null)
                throw new Exception();
            if (allow)
                this.SpecifiedMaterials.Add(mat);
            else
                this.SpecifiedMaterials.Remove(mat);
            return this;
        }
        public Ingredient SetAllow(MaterialTypeDef matType, bool allow)
        {
            if (matType is null)
                throw new Exception();
            foreach (var m in matType.SubTypes)
                this.SetAllow(m, allow);
            return this;
        }
        public Ingredient SetAllow(ItemDef def, bool allow)
        {
            if (def is null)
                throw new Exception();
            if (allow)
                this.SpecifiedItemDefs.Add(def);
            else
                this.SpecifiedItemDefs.Remove(def);
            return this;
        }
        private void TryResolve()
        {
            if (this.Resolved)
                return;
            ResolveAllowedItems();
            //if (!this.SpecifiedMaterials.Any())
            ResolveAllowedMaterials();
            this.Resolved = true;
        }

        private void ResolveAllowedItems()
        {
            var finalDefs = Def.GetDefs<ItemDef>();

            if (this.SpecifiedItemDefs.Any())
                this._resolvedItemDefs = this.SpecifiedItemDefs;
            else
            {
                if (this.Modifiers.Any())
                    finalDefs = finalDefs.Where(d => this.Modifiers.All(m => m.Evaluate(d)));
                if (this.SpecifiedMaterials.Any())
                    finalDefs = finalDefs.Where(d => d.ValidMaterialTypes.Any(t => this.SpecifiedMaterials.Any(m => m.Type == t)));
                if(this.SpecifiedCategories.Any())
                    finalDefs = finalDefs.Where(d => this.SpecifiedCategories.Contains(d.Category));
                this._resolvedItemDefs = new(finalDefs);
            }
        }

        private void ResolveAllowedMaterials()
        {
            this._resolvedMaterials = new();
            if (!this.SpecifiedMaterials.Any())
                foreach (var m in this._resolvedItemDefs.SelectMany(i => i.GetValidMaterials()))
                    this._resolvedMaterials.Add(m);
            else
                this._resolvedMaterials = this.SpecifiedMaterials;
        }

        internal string GetLabel()
        {
            return $"{this.Amount}x {this.Material?.Name ?? ""} {this.ItemDef?.Label ?? ""} {this.Modifiers.FirstOrDefault()?.Label ?? ""}";
        }

        public Ingredient AddResourceFilter(ResourceDef resDef)
        {
            this.SpecialFilters.Add(e => e.GetResource(resDef)?.Percentage < 1);
            return this;
        }
        public Ingredient Preserve()
        {
            this.IsPreserved = true;
            return this;
        }
        public override string Label => $"{this.Amount}x material";//$"{typeof(Ingredient).Name}:{this.Name}";
       
        public Control GetGui()
        {
            return new Label($"{this.Amount}x material");
        }
    }
}

using Project1.Core.Plants;
using Project1.Core.Graphics;
using Project1.Core.Legacy;
using Project1.Core.Legacy.Properties;
using Project1.Core.Legacy.Storage;
using Project1.Core.Legacy.Storage.New;
using Project1.Core.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using Project1.Core.Simulation;
using Project1.Core.Simulation.Physics;
using Project1.Core.Gear;
using Project1.Framework;

namespace Project1.Core.Entities
{
    public class ItemDef : Def
    {
        public int StackCapacity = 1;
        public int StackDimension = 1;
        public ItemCategory Category;
        public Sprite DefaultSprite;
        public MaterialDef DefaultMaterial;
        public bool MadeFromMaterials;
        public List<Reaction.Product.Types> CanProcessInto = new();
        public int BaseValue = 1;
        public MaterialTypeDef DefaultMaterialType;
        public List<MaterialDef> CanBeMadeFrom = new();
        public List<MaterialTypeDef> ValidMaterialTypes = new();
        public PlantSpeciesDef PlantProperties;
        public ItemToolDef ToolProperties;
        public ApparelDef ApparelProperties;
        public CraftingProperties CraftingProperties;
        public RecipeProperties RecipeProperties;
        public GearTypeDef GearType;
        public Func<ItemDef, GameObject> Randomizer;
        public List<MaterialToken> MadeFrom = new();
        public bool QualityLevels;
        internal IEnumerable<IItemDefVariator> StorageFilterVariations;
        public Func<Entity, string> NameGetter;
        public Func<Entity, Def> VariationGetter;
        public ObjectSize Size;
        public string Prefix, Suffix;
        public bool ReplaceName;
        internal Type VariantType;
        internal Type[] CompTypes = [];
        public readonly List<EntityComp.Spec> Specs = [];// [new SpriteComp.Props()];
        public Bone Body;
        public readonly Type ItemClass;
        public float Weight = 1;
        public float Height = 1;
        public bool IsHaulable = true;
        public string Description;

        public ItemDef(string name, Type itemClass) : base(name)//, itemClass)
        {
            this.ItemClass = itemClass;
        }

        internal GameObject CreateRandom()
        {
            return this.Randomizer?.Invoke(this);
        }
        internal virtual Entity Create(Def profile = null, int amount = -1)
        {
            var entity = (Entity)Activator.CreateInstance(this.ItemClass, this, amount);
            //entity.Def = this;
            entity.Profile = profile;
            entity.InitComps(this);
            return entity;
        }

        //[Obsolete]
        //internal Entity CreateBase(ItemVariantDef def)
        //{
        //    var obj = Activator.CreateInstance(this.ItemClass) as Entity;
        //    obj.Def = def;
        //    obj.InitComps(def);
        //    return obj;
        //}
        internal Entity CreateFrom(MaterialDef mat)
        {
            var item = ItemFactory.CreateFrom(this, mat);
            return item;
        }
        public IEnumerable<ItemMaterialAmount> GenerateVariants(int amount = 1)
        {
            if (this.DefaultMaterialType == null)
                yield break;
            foreach (var m in this.DefaultMaterialType.SubTypes)
                yield return new ItemMaterialAmount(this, m, amount);
        }

        public MaterialTypeDef MaterialType
        {
            get
            {
                return this.DefaultMaterial?.Type ?? this.DefaultMaterialType;
            }
        }
        
        public ItemDef SetMadeFrom(params MaterialToken[] tokens)
        {
            this.MadeFrom.AddRange(tokens);
            return this;
        }
        public ItemDef SetMadeFrom(params MaterialTypeDef[] types)
        {
            this.ValidMaterialTypes.AddRange(types);
            return this;
        }
        public ItemDef AddSpec(EntityComp.Spec prop)
        {
            this.Specs.Add(prop);
            return this;
        }
        public IEnumerable<MaterialDef> GetValidMaterials()
        {
            IEnumerable<MaterialDef> validMats = this.ValidMaterialTypes.SelectMany(t => t.SubTypes);
            foreach (var m in validMats)
                yield return m;
        }
        public IEnumerable<Entity> CreateFromAllMAterials()
        {
            foreach (var m in this.ValidMaterialTypes.SelectMany(t => t.SubTypes))
                yield return ItemFactory.CreateFrom(this, m);
        }
        
        internal Reaction CreateRecipe()
        {
            return this.RecipeProperties.CreateRecipe(this);
        }

        public StorageFilterCategoryNewNew GetSpecialFilter()
        {
            if (this.StorageFilterVariations is null)
                return null;
            var filter = new StorageFilterCategoryNewNew(this);
            filter.AddLeafs(this.StorageFilterVariations.Select(v => v.GetFilter()));
            return filter;
        }


        /// <summary>
        /// Different than map.isstandablein because this takes into account actor's height and returns true only if the actor can stand upright (no crouching)
        /// </summary>
        /// <param name="cell">the lower cell occupied by the entity</param>
        /// <returns></returns>
        public bool CanFitIn(MapBase map, IntVec3 cell)
        {
            return
                map.GetBlock(cell.Below).Solid
                && Enumerable.Range(0, (int)Math.Ceiling(this.Height))
                    .All(z => !map.GetBlock(cell + new IntVec3(0, 0, z)).Solid);
        }
        public IEnumerable<IntVec3> OccupyingCellsStanding()
        {
            return Enumerable.Range(0, (int)Math.Ceiling(this.Height)).Select(z => new IntVec3(0, 0, z));
        }
        public IEnumerable<IntVec3> OccupyingCellsStanding(IntVec3 pos)
        {
            return Enumerable.Range(0, (int)Math.Ceiling(this.Height)).Select(z => pos + new IntVec3(0, 0, z));
        }
        public IEnumerable<IntVec3> OccupyingCellsStandingWithBase(IntVec3 pos)
        {
            return this.OccupyingCellsStanding(pos).Prepend(pos);
        }
    }
}

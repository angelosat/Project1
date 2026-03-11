using System;
using System.Collections.Generic;
using System.Linq;
using Project1.Core.Entities;
using Project1.Core.Towns.Constructions;
using Project1.Core.Blocks;
using Project1.Core.Helpers;
using Project1.Core.Legacy;
using Project1.Core.Legacy.Crafting.Blocks;
using Project1.Core.Simulation;
using Project1.Framework.Serialization;
using Project1.Framework.Helpers;
using Project1.Framework.UI;
using Project1.Framework;
using Project1.Core.Systems.Materials;

namespace Project1.Core.Blocks
{
    class BlockConstructionEntity : BlockEntity, IConstructible
    {
        public ProductMaterialPair Product;
        public List<ItemMaterialAmount> Container = new();

        public Progress BuildProgress { get; set; }
        public List<IntVec3> Children { get; set; } = new List<IntVec3>();

        public BlockConstructionEntity(BlockDef def, IntVec3 originGlobal)
                : base(def, originGlobal)
        {

        }
        public BlockConstructionEntity(ProductMaterialPair product, BlockDef def, IntVec3 origin, GameObject initialMaterial, int amount)
            : this(def, origin)
        {
            this.Product = product;
            if (amount > initialMaterial.StackSize)
                throw new Exception();
            this.Container.Add(new ItemMaterialAmount(initialMaterial.Def, initialMaterial.PrimaryMaterial, amount));
            this.BuildProgress = new Progress(0, getWorkAmount(), 0);

            int getWorkAmount() => this.Container.Sum(m => m.Material.Density * m.Amount) * this.Product.Block.BuildComplexity;
        }
        
        public override void GetTooltip(Control tooltip)
        {
            var product = this.Product;
            var req = product.Requirement;
            var block = product.Block;
            var ing = block.Ingredient;
            tooltip.AddControlsBottomLeft(new Label()
            {
                TextFunc = GetIngredientText
            });
        }
        internal override void GetSelectionInfo(IUISelection info, MapBase map, IntVec3 vector3)
        {
            var product = this.Product;
            info.AddInfo(
                Label.ParseWrap(product, " ", new Func<string>(this.Container.First().Amount.ToString), " / ", product.Requirement.Amount)
            );
            info.AddInfo(this.BuildProgress.GetGui("Construction"));
        }
        string GetIngredientText()
        {
            var product = this.Product;
            return $"{product.Requirement.Material.LabelReadable} {product.Requirement.Item.LabelReadable} {this.Container.First().Amount} / {product.Requirement.Amount}";
        }

        internal void HandleDepositedItem(GameObject dropped, int amount)
        {
            if (dropped.Def != this.Product.Requirement.Item)
                throw new Exception();
            if (dropped.PrimaryMaterial != this.Product.Requirement.Material)
                throw new Exception();
            var req = this.Container.FirstOrDefault(r => r.Item == dropped.Def);
            req.Amount += amount;
            dropped.Consume(amount);
            if (req.Amount > this.Product.Requirement.Amount)
                throw new Exception();
        }

        public bool IsReadyToBuild(out ItemDef def, out MaterialDef mat, out int amount)
        {
            var req = this.Container.First();
            if (req.Amount == this.Product.Requirement.Amount)
            {
                def = null;
                mat = null;
                amount = 0;
                return true;
            }
            def = this.Product.Requirement.Item;
            mat = this.Product.Requirement.Material;
            amount = this.Product.Requirement.Amount - req.Amount;
            return false;
        }

        public int GetMissingAmount(ItemDef def)
        {
            return this.GetReq(def).Amount - this.GetContainedMaterialAmount(def);
        }
        ItemMaterialAmount GetReq(ItemDef objid)
        {
            var req = this.Product.Requirement;
            if (objid != req.Item)
                throw new Exception();
            return req;
        }
        public bool IsValidHaulDestination(ItemDef objectID)
        {
            return this.Product.Requirement.Item == objectID;
        }
        private int GetContainedMaterialAmount(ItemDef def)
        {
            return this.Container.FirstOrDefault(o => o.Item == def).Amount;
        }

        protected override void AddSaveData(SaveTag tag)
        {
            this.Product.Save(tag, "Product");
            tag.Add(this.Children.Save("Children"));
            tag.Add(this.Container.SaveNewBEST("Container"));
            tag.Add(this.BuildProgress.Save("BuildProgress"));
        }
        protected override void LoadExtra(SaveTag tag)
        {
            tag.TryGetTag("Product", t => this.Product = new ProductMaterialPair(t));
            tag.TryGetTagValue<List<SaveTag>>("Children", t => this.Children.Load(t));
            this.Container.TryLoadMutable(tag, "Container");
            tag.TryGetTag("BuildProgress", v => this.BuildProgress = new Progress(v));
        }

        protected override void WriteExtra(IDataWriter w)
        {
            this.Product.Write(w);
            this.BuildProgress.Write(w);
            w.Write(this.Children);
            this.Container.Write(w);
        }
        protected override void ReadExtra(IDataReader r)
        {
            this.Product = new ProductMaterialPair(r);
            this.BuildProgress = new Progress(r);
            this.Children = r.ReadListIntVec3();
            this.Container.ReadMutableNew(r);
        }
    }
}
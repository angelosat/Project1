using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_
{
    partial class BlockDesignation
    {
        public class BlockDesignationEntity : BlockEntity, IConstructible
        {
            ProductMaterialPair _product;
            public ProductMaterialPair Product
            {
                get => this._product;
                set
                {
                    if (this._product is null)
                        this.BuildProgress = new Progress(0, value.Block.BuildComplexity, 0);
                    this._product = value;
                }
            }
            public Progress BuildProgress { get; set; }
            public List<IntVec3> Children { get; set; } = new List<IntVec3>();
            public BlockDesignationEntity(IntVec3 origin)
                : base(origin)
            {

            }
            public BlockDesignationEntity(ProductMaterialPair product, IntVec3 origin)
                : this(origin)
            {
                this.OriginGlobal = origin;
                this.Product = product;
            }

            public bool IsValidHaulDestination(ItemDef def)
            {
                var valid = this.Product.Requirement.Item == def;
                if (!valid)
                {

                }
                return valid;
            }
           
            public bool IsReadyToBuild(out ItemDef def, out MaterialDef mat, out int amount)
            {
                var product = this.Product;
                def = product.Requirement.Item;
                amount = product.Requirement.Amount;
                mat = product.Requirement.Material;
                return false;
            }
            
            public override void GetTooltip(Control tooltip)
            {
                var product = this.Product;
                var req = product.Requirement;
                tooltip.AddControlsBottomLeft(new Label()
                {
                    TextFunc = () => $"{product.Requirement.Material.Name} {product.Requirement.Item.Label} {0} / {product.Requirement.Amount}"
                });
            }
            internal override void GetSelectionInfo(IUISelection info, MapBase map, IntVec3 vector3)
            {
                info.AddInfo(new Label(this.Product));// this.Product.GetGui());
                //var product = this.Product;
                //var req = product.Requirement;
                //info.AddInfo(new Label() { TextFunc = () => $"{req.Material.Label} {req.Item.Label} {0} / {req.Amount}" });
            }
            protected override void OnDrawUI(SpriteBatch sb, Camera cam, IntVec3 global)
            {
                if (ToolManager.Instance.ActiveTool != null)
                    if (ToolManager.Instance.ActiveTool.Target != null)
                        if (ToolManager.Instance.ActiveTool.Target.Type == TargetType.Position && (IntVec3)ToolManager.Instance.ActiveTool.Target.Global == global)
                            Bar.Draw(sb, cam, global.Above, "", this.BuildProgress.Percentage, cam.Zoom * .2f);
            }

            protected override void AddSaveData(SaveTag tag)
            {
                this.Product.Save(tag, "Product");
                tag.Add(this.Children.Save("Children"));
                tag.Add(this.BuildProgress.Save("BuildProgress"));
            }
            protected override void LoadExtra(SaveTag tag)
            {
                tag.TryGetTag("Product", t => this.Product = new ProductMaterialPair(t));
                tag.TryGetTagValue<List<SaveTag>>("Children", t => this.Children.Load(t));
                tag.TryGetTag("BuildProgress", v => this.BuildProgress = new Progress(v));
            }

            protected override void WriteExtra(BinaryWriter w)
            {
                this.Product.Write(w);
                this.BuildProgress.Write(w);
                w.Write(this.Children);

            }
            protected override void ReadExtra(BinaryReader r)
            {
                this.Product = new ProductMaterialPair(r);
                this.BuildProgress = new Progress(r);
                this.Children = r.ReadListIntVec3();
            }
            public int GetMissingAmount(ItemDef def)
            {
                return this.Product.Requirement.Amount;// .GetMaterialRequirement(def);
            }
        }
    }
}

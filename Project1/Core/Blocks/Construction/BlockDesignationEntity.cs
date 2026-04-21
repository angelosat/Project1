using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Input;
using Project1.Core.Towns.Constructions;
using Project1.Core.Legacy.Crafting.Blocks;
using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Core.Entities;
using Project1.Framework.Serialization;
using Project1.Framework.Helpers;
using Project1.Framework.UI;
using Project1.Framework;
using Project1.Core.Systems.Materials;
using Project1.Framework.UI.Primitives;

namespace Project1.Core.Blocks
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
                        this.BuildProgress = new ProgressFloat(0, value.Block.BuildComplexity, 0);
                    this._product = value;
                }
            }
            public ProgressFloat BuildProgress { get; set; }
            public List<IntVec3> Children { get; set; } = new List<IntVec3>();
            public BlockDesignationEntity(BlockDef def, IntVec3 originGlobal)
                : base(def, originGlobal)
            {

            }
            public BlockDesignationEntity(ProductMaterialPair product, BlockDef def, IntVec3 originGlobal)
                : base(def, originGlobal)
            {
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
                    TextFunc = () => $"{product.Requirement.Material.Name} {product.Requirement.Item.LabelReadable} {0} / {product.Requirement.Amount}"
                });
            }
            internal override void GetSelectionInfo(IUISelection info, MapBase map, IntVec3 vector3)
            {
                info.AddInfo(new Label(this.Product));
            }
            protected override void OnDrawUI(SpriteBatch sb, Camera cam, IntVec3 global)
            {
                if (ToolManager.Instance.ActiveTool != null)
                    if (ToolManager.Instance.ActiveTool.Target != null)
                        if (ToolManager.Instance.ActiveTool.Target.Type == TargetType.Cell && (IntVec3)ToolManager.Instance.ActiveTool.Target.Global == global)
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
                tag.TryGetTag("BuildProgress", v => this.BuildProgress = new ProgressFloat(v));
            }

            protected override void WriteExtra(IDataWriter w)
            {
                this.Product.Write(w);
                this.BuildProgress.Write(w);
                w.Write(this.Children);

            }
            protected override void ReadExtra(IDataReader r)
            {
                this.Product = new ProductMaterialPair(r);
                this.BuildProgress = new ProgressFloat(r);
                this.Children = r.ReadListIntVec3();
            }
            public int GetMissingAmount(ItemDef def)
            {
                return this.Product.Requirement.Amount;
            }
        }
    }
}
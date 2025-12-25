using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Towns.Constructions;
using Start_a_Town_.UI;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.Core
{
    class BlockBrowserConstruction : GroupBox
    {
        readonly Dictionary<Block, ProductMaterialPair> LastSelectedVariant = new();
        ConstructionCategoryDef SelectedCategory;
        readonly Panel Panel_Blocks;
        readonly UIToolsBox ToolBox;
        Block CurrentSelected;
        readonly Dictionary<ConstructionCategoryDef, ButtonGridIcons<Block>> Categories = new();

        public BlockBrowserConstruction()
        {
            this.Panel_Blocks = new Panel() { AutoSize = true };
            this.ToolBox = new UIToolsBox(this.OnToolSelectedNew);
            var categories = Block.Registry.Values.Where(b => b.BuildProperties.Category is not null).GroupBy(b => b.BuildProperties.Category); // blocks without ingredients are built immediately (sleeping spots)
            foreach (var cat in categories)
            {
                var list = cat;
                var grid = new ButtonGridIcons<Block>(4, 6, list, (slot, block) =>
                {
                    slot.Tag = block;
                    slot.IsToggledFunc = () => ToolManager.Instance.ActiveTool is ToolBlockBuild drawing && drawing.Block == block;
                    slot.PaintAction = () => block.PaintIcon(slot.Width, slot.Height, 0, this.GetLastSelectedVariantOrDefault(block).Requirement?.Material);
                    slot.LeftClickAction = () => StartPainting(block);
                    slot.RightClickAction = () => UIBlockVariationPickerNew.Refresh(block, this.OnVariationSelected);
                    slot.HoverFunc = () => $"{block.Name}\n{this.GetLastSelectedVariantOrDefault(block).Requirement}\nTool necessity: {block.BuildProperties.ToolSensitivity:##0%}\nRight click to select variation";
                })
                { Location = this.Panel_Blocks.Controls.BottomLeft };
                this.Categories[cat.Key] = grid;
            }
            this.SelectedCategory = this.Categories.First().Key;
            this.Panel_Blocks.Controls.Add(this.Categories[this.SelectedCategory]);

            var cbox = new ComboBoxNew<ConstructionCategoryDef>(
                        new ButtonGridGenericNew<ConstructionCategoryDef>(
                            Def.GetDefs<ConstructionCategoryDef>(),
                            (c, b) =>
                            {
                                b.LeftClickAction = () =>
                                {
                                    this.Panel_Blocks.ClearControls();
                                    this.Panel_Blocks.AddControls(this.Categories[c]);
                                    this.SelectedCategory = c;
                                };
                            }),
                        this.SelectedCategory.Label,
                        this.Categories[this.SelectedCategory].Width);

            this.AddControlsVertically(
                cbox.ToPanel(),
                this.Panel_Blocks
                );
        }

        private void StartPainting(Block block)
        {
            this.CurrentSelected = block;
            var product = this.GetLastSelectedVariantOrDefault(block);
            this.ToolBox.SetProduct(product);
            this.OnToolSelectedNew(this.ToolBox.LastSelectedTool);
            var win = this.ToolBox.GetWindow();
            if (win is null)
            {
                win = this.ToolBox.ToWidget("Brushes");
                win.HideAction = () => ToolManager.SetTool(null);
            }
            //if (win.Show())
            //    win.Location = this.GetWindow().BottomLeft;
            if (!win.IsOpen)
            {
                win.Location = this.GetWindow().BottomLeft;
                win.Show();
            }
        }

        void OnToolSelectedNew(BuildToolDef toolDef)
        {
            //var tool = this.SelectedCategory.GetTool(toolDef, this.GetLastSelectedVariantOrDefault(this.CurrentSelected));
            var tool = this.SelectedCategory.GetTool(toolDef, this.GetLastSelectedVariantOrDefaultNew(this.CurrentSelected));
            this.ToolBox.LastSelectedTool = toolDef;
            ToolManager.SetTool(tool);
        }
        private ConstructionDesignationArgs GetLastSelectedVariantOrDefaultNew(Block block)
        {
            var profile = block.ConstructionProfile;
            //var validMats = Def.GetDefs<MaterialDef>().Where(m => profile.Refinements.Any(r => r.MaterialType == m.Type)).GroupBy(m=>m.Type).ToList();
            var refinement = profile.Refinements.First();
            var validMats = Def.GetDefs<MaterialDef>().Where(m => refinement.MaterialType == m.Type);
            var defaultMat = validMats.First();
            return new ConstructionDesignationArgs(block, refinement, defaultMat, 0);
        }
        private ProductMaterialPair GetLastSelectedVariantOrDefault(Block block)
        {
            if (!this.LastSelectedVariant.TryGetValue(block, out var lastVariant))
            {
                lastVariant = new ProductMaterialPair(block, block.GetAllValidConstructionMaterialsNew().FirstOrDefault()); // building might have no construction materials (sleeping spots)
                this.LastSelectedVariant[block] = lastVariant;
            }
            return lastVariant;
        }
        private void OnVariationSelected(ProductMaterialPair product)
        {
            if (product is null)
                return;
            this.LastSelectedVariant[product.Block] = product;
            this.CurrentSelected = product.Block;
            if (this.ToolBox.LastSelectedTool != null)
            {
                var tool = this.SelectedCategory.GetTool(this.ToolBox.LastSelectedTool, product);
                this.ToolBox.LastSelectedTool = tool.ToolDef;
                ToolManager.SetTool(tool);
            }
            this.Categories[this.SelectedCategory].FindChild(c => c.Tag == product.Block).Invalidate();
        }
        public override bool Hide()
        {
            this.CurrentSelected = null;
            if (this.ToolBox.GetWindow() != null)
                this.ToolBox.GetWindow().Hide();
            ToolManager.SetTool(null);
            return base.Hide();
        }
    }
}

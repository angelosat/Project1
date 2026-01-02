using Start_a_Town_.Towns.Constructions;
using Start_a_Town_.UI;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.Core
{
    class BlockBrowserConstruction : GroupBox
    {
        readonly Dictionary<BlockDef, ConstructionDesignationArgs> LastSelectedVariant = new();
        ConstructionCategoryDef SelectedCategory;
        readonly Panel Panel_Blocks;
        readonly UIToolsBox ToolBox;
        BlockDef CurrentSelected;
        readonly Dictionary<ConstructionCategoryDef, ButtonGridIcons<BlockDef>> Categories = new();
        UIBlockVariationPicker Picker;
        public BlockBrowserConstruction()
        {
            this.Picker = new();
            this.Panel_Blocks = new Panel() { AutoSize = true };
            this.ToolBox = new UIToolsBox(this.OnToolSelectedNew);
            //var categories = Block.Registry.Values.Where(b => b.BuildProperties.Category is not null).GroupBy(b => b.BuildProperties.Category); // blocks without ingredients are built immediately (sleeping spots)
            var categories = Def.GetDefs<BlockDef>()/*.Select(b => b.Worker)*/.Where(b => b.Worker.BuildProperties.Category is not null).GroupBy(b => b.Worker.BuildProperties.Category); // blocks without ingredients are built immediately (sleeping spots)
            foreach (var cat in categories)
            {
                var list = cat.Where(b => b.ConstructionProfile is not null);
                var grid = new ButtonGridIcons<BlockDef>(4, 6, list, (slot, block) =>
                {
                    slot.Tag = block;
                    slot.IsToggledFunc = () => ToolManager.Instance.ActiveTool is ToolBlockBuild drawing && drawing.Block == block.Worker;
                    slot.PaintAction = () => block.Worker.PaintIcon(slot.Width, slot.Height, 0, this.GetLastSelectedVariantOrDefaultNew(block).Material);
                    slot.LeftClickAction = () => StartPainting(block);
                    //slot.RightClickAction = () => UIBlockVariationPickerOld.Refresh(block, this.OnVariationSelected);
                    slot.RightClickAction = () => this.Picker.Refresh(block, this.OnVariationSelectedNew);
                    //slot.HoverFunc = () => $"{block.Name}\nTool necessity: {block.BuildProperties.ToolSensitivity:##0%}\nRight click to select variation";
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

        private void StartPainting(BlockDef block)
        {
            this.CurrentSelected = block;
            //var variant = this.GetLastSelectedVariantOrDefaultNew(block);
            this.ToolBox.SetProduct(block.Worker);
            this.OnToolSelectedNew(this.ToolBox.LastSelectedTool);
            var win = this.ToolBox.GetWindow();
            if (win is null)
            {
                win = this.ToolBox.ToWidget("Brushes");
                win.HideAction = () => ToolManager.SetTool(null);
            }
            if (!win.IsOpen)
            {
                win.Location = this.GetWindow().BottomLeft;
                win.Show();
            }
        }

        void OnToolSelectedNew(BuildToolDef toolDef)
        {
            var tool = this.SelectedCategory.GetTool(toolDef, this.GetLastSelectedVariantOrDefaultNew(this.CurrentSelected));
            this.ToolBox.LastSelectedTool = toolDef;
            ToolManager.SetTool(tool);
        }
        private ConstructionDesignationArgs GetLastSelectedVariantOrDefaultNew(BlockDef block)
        {
            if (this.LastSelectedVariant.TryGetValue(block, out var lastVariant))
            {
                return lastVariant;// new ConstructionDesignationArgs(block, lastVariant.refinement, lastVariant.material, 0);
            }
            var profile = block.ConstructionProfile;
            var refinement = profile.Refinements.First();
            var validMats = Def.GetDefs<MaterialDef>().Where(m => refinement.MaterialType == m.Type);
            var defaultMat = validMats.First();
            return new ConstructionDesignationArgs(block, refinement, defaultMat, 0);
        }
        //private ProductMaterialPair GetLastSelectedVariantOrDefault(Block block)
        //{
        //    if (!this.LastSelectedVariant.TryGetValue(block, out var lastVariant))
        //    {
        //        lastVariant = new ProductMaterialPair(block, block.GetAllValidConstructionMaterialsNew().FirstOrDefault()); // building might have no construction materials (sleeping spots)
        //        this.LastSelectedVariant[block] = lastVariant;
        //    }
        //    return lastVariant;
        //}
        void OnVariationSelectedNew(ConstructionDesignationArgs args)
        {
            var block = args.Block;
            this.LastSelectedVariant[block] = args;
            this.CurrentSelected = block;
            if (this.ToolBox.LastSelectedTool != null)
            {
                //var args = new ConstructionDesignationArgs(block, variant.refinement, variant.material, 0);
                var tool = this.SelectedCategory.GetTool(this.ToolBox.LastSelectedTool, args);
                this.ToolBox.LastSelectedTool = tool.ToolDef;
                ToolManager.SetTool(tool);
            }
            this.Categories[this.SelectedCategory].FindChild(c => c.Tag == block).Invalidate();
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

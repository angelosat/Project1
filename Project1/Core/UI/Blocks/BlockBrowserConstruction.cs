using System.Collections.Generic;
using System.Linq;
using Project1.Framework.UI;
using Project1.Core.Construction.Tools;
using Project1.Core.Towns.Constructions.Categories;
using Project1.Core.Blocks;
using Project1.Core.Input;
using Project1.Core.Input.Tools.Building;
using Project1.Core.Materials;
using Project1.Core.UI;

namespace Project1.Core.UI.Blocks
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
            var categories = Def.GetDefs<BlockDef>().Where(b => b.Worker.BuildProperties.Category is not null).GroupBy(b => b.Worker.BuildProperties.Category); // blocks without ingredients are built immediately (sleeping spots)
            foreach (var cat in categories)
            {
                var list = cat.Where(b => b.ConstructionProfile is not null);
                var grid = new ButtonGridIcons<BlockDef>(4, 6, list, (slot, block) =>
                {
                    slot.Tag = block;
                    slot.IsToggledFunc = () => ToolManager.Instance.ActiveTool is ToolBlockBuild drawing && drawing.Block == block.Worker;
                    slot.PaintAction = () => block.Worker.PaintIcon(slot.Width, slot.Height, 0, this.GetLastSelectedVariantOrDefaultNew(block).Material);
                    slot.LeftClickAction = () => StartPainting(block);
                    slot.RightClickAction = () => this.Picker.Refresh(block, this.OnVariationSelectedNew);
                    slot.HoverText = block.LabelReadable;
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
                        this.SelectedCategory.LabelReadable,
                        this.Categories[this.SelectedCategory].Width);

            this.AddControlsVertically(
                cbox.ToPanel(),
                this.Panel_Blocks
                );
        }
        private void StartPainting(BlockDef block)
        {
            this.CurrentSelected = block;
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
                return lastVariant;
            var profile = block.ConstructionProfile;
            var refinement = profile.Refinements.First();
            var validMats = Def.GetDefs<MaterialDef>().Where(m => refinement.MaterialType == m.Type);
            var defaultMat = validMats.First();
            return new ConstructionDesignationArgs(block, refinement, defaultMat, 0);
        }
        void OnVariationSelectedNew(ConstructionDesignationArgs args)
        {
            var block = args.Block;
            this.LastSelectedVariant[block] = args;
            this.CurrentSelected = block;
            if (this.ToolBox.LastSelectedTool != null)
            {
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

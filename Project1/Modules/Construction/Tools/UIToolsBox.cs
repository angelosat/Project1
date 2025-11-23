using Start_a_Town_.Components.Crafting;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public class UIToolsBox : GroupBox
    {
        readonly Panel PanelButtons;
        public BuildToolDef LastSelectedTool;
        ConstructionCategoryDef CurrentCategory;
        readonly Action<BuildToolDef> OnToolSelectedCallback;

        public UIToolsBox(Action<BuildToolDef> onToolSelected)
        {
            this.OnToolSelectedCallback = onToolSelected;
            this.Name = "Brushes";
            this.PanelButtons = new Panel()
            {
                AutoSize = true
            };
            this.AddControls(
                this.PanelButtons);
        }
        public void SetProduct(ProductMaterialPair product)
        {
            if (product is not null)
            {
                var cat = product.Block.ConstructionCategory;
                if (cat != this.CurrentCategory)
                {
                    this.Refresh(cat.Tools);
                }
                this.CurrentCategory = cat;
            }
            else
                this.CurrentCategory = null;
        }

        public void Refresh(IEnumerable<BuildToolDef> tools)
        {
            this.ClearControls();

            var grid = new GroupBox().AddControlsHorizontally(tools.Select(t =>
            {
                var btn = ButtonNew.CreateMedium(t.Icon, () => selectTool(t));
                btn.IsToggledFunc = () => ToolManager.Instance.ActiveTool is ToolBlockBuild buildTool && buildTool.ToolDef == t;
                btn.HoverText = t.Label;
                return btn;
            }));

            this.LastSelectedTool = tools.First();

            this.AddControlsVertically(grid, new Label(() => (ToolManager.Instance.ActiveTool as ToolBlockBuild)?.Status ?? ""));

            void selectTool(BuildToolDef t)
            {
                this.LastSelectedTool = t;
                this.OnToolSelectedCallback(t);
            }
        }
    }
}

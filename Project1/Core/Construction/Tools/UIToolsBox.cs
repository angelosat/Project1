using Project1.Core.Towns.Constructions.Categories;
using Project1.Core.Blocks;
using Project1.Core.Input;
using Project1.Core.UI;
using Project1.Core.Components.Crafting;
using Project1.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using Project1.Framework.UI;
using Project1.Core.Input.Building;

namespace Project1.Core.Construction.Tools
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
        public void SetProduct(Block block)
        {
            if (block is not null)
            {
                var cat = block.ConstructionCategory;
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
                btn.HoverText = t.LabelReadable;
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
using Project1.Framework.UI;
using System.Collections.Generic;

namespace Project1.Core.Systems.Recipes;

sealed class RecipesComp_Gui : GroupBox
{
    readonly RecipesComp Comp;
    readonly Table<RecipeKnowledge> Table = new Table<RecipeKnowledge>()
            .AddColumn("name", 64, r => new LabelNew(() => r.Recipe.LabelReadable))
            .AddColumn("value", 128, r => new LabelNew(() => $"{r.TimesCrafted} times crafted").InvalidateOn(r.Update));
    public RecipesComp_Gui(RecipesComp comp)
    {
        this.Controls.Add(this.Table);
        this.Table.AddItems(comp.All);
        comp.Updated += Comp_Updated;
    }
    internal override void OnDetached()
    {
        this.Comp.Updated -= Comp_Updated;
    }
    private void Comp_Updated((IEnumerable<RecipeKnowledge> added, IEnumerable<RecipeKnowledge> removed) e)
    {
        this.Table.AddItems(e.added);
    }

    public override void OnLayout(int availableWidth, int availableHeight)
    {
        this.AutoSize = false;
        this.Width = availableHeight;
        this.Height = availableHeight;
    }
}

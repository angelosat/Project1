using Project1.Framework.UI;
using Project1.Framework.UI.Primitives;
using System.Collections.Generic;

namespace Project1.Core.Systems.Recipes;

sealed class RecipesComp_Gui : Table<RecipeKnowledge>
{
    readonly RecipesComp Comp;
    //readonly Table<RecipeKnowledge> Table = new Table<RecipeKnowledge>()
    //        //.AddColumn("name", 50, r => new LabelNew(() => r.Recipe.LabelReadable))
    //        ////.AddColumn("value", 128, r => new LabelNew(() => $"{r.TimesCrafted} times crafted").InvalidateOn(r.Update));
    //        //.AddColumn("xp", 100, r => new Bar(r, 100, () => $"{r.CurrentXp} / {r.NextXp} xp").InvalidateOn(r.Update))
    //        //.AddColumn("level", 50, r => new LabelNew(() => $"Lvl: {r.Level}").InvalidateOn(r.Update))
    //    ;
  
    public RecipesComp_Gui(RecipesComp comp)
    {
        //this.Controls.Add(this.Table);
        //this.Table.AddItems(comp.All);
        var lvlWidth = (int)UIManager.Font.MeasureString("Lvl: ###").X + 4;
        this
            .AddColumn("name", 100-lvlWidth, r => new LabelNew(() => r.Recipe.LabelReadable))
            //.AddColumn("value", 128, r => new LabelNew(() => $"{r.TimesCrafted} times crafted").InvalidateOn(r.Update));
            .AddColumn("xp", 100, r => new Bar(r, 100, () => $"{r.CurrentXp} / {r.NextXp} xp").InvalidateOn(r.Update))
            .AddColumn("level", lvlWidth, r => new LabelNew(() => $"Lvl: {r.Level}").InvalidateOn(r.Update));
        this.AddItems(comp.All);
        comp.Updated += Comp_Updated;
    }
    protected override void OnDetached()
    {
        this.Comp.Updated -= Comp_Updated;
    }
    private void Comp_Updated((IEnumerable<RecipeKnowledge> added, IEnumerable<RecipeKnowledge> removed) e)
    {
        //this.Table.AddItems(e.added);
        this.AddItems(e.added);
    }

    public override void OnLayout(int availableWidth, int availableHeight)
    {
        this.AutoSize = false;
        this.Width = availableHeight;
        this.Height = availableHeight;
    }
}

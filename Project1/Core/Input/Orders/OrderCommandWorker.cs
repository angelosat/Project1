using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Simulation;
using Project1.Core.UI;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Project1.Core.Input.Orders;

internal class OrderCommandButton : QuickButton
{
    OrderCommandDef Def;
    Func<SelectionFinal> SelectionGetter;
    //PictureBoxNew icon;
    public OrderCommandButton(OrderCommandDef orderdef, Func<SelectionFinal> selectionGetter) 
        : base(new Icon(orderdef.Sprite), null, orderdef.LabelReadable)
    {
        this.Def = orderdef;
        this.SelectionGetter = selectionGetter;
        LeftClickAction = () => this.Def.Worker.Issue(this.SelectionGetter());
        HoverText = orderdef.LabelReadable;
        //this.icon = new PictureBoxNew(UIManager.IconX);
        //var manager = this.Selection.Targets.First().Map.Town.Shops;
        //this.InvalidateOn(manager.Notifier);
    }

    public override void Draw(SpriteBatch sb, Rectangle viewport)
    {
        base.Draw(sb, viewport);

        //if (this.Def is not null && 
            if(this.Def.Worker is OrderCommandWorkerTogglable worker)
        {
            var icon = this.SelectionGetter().Targets.Any(worker.IsToggled) ? Icon.Cross : Icon.Replace;
            icon.Draw(sb, this.ScreenLocation, Vector2.Zero);
        }
    }
}
internal abstract class OrderCommandWorkerTogglable : OrderCommandWorker
{
    internal abstract bool IsToggled(ISelectable target);
}
internal abstract class OrderCommandWorker
{
    internal abstract void Issue(SelectionFinal selection);
    internal abstract bool CanIssue(ISelectable target);
    internal bool CanIssue(ValidSelectedCount validCount, IReadOnlyCollection<ISelectable> targets)
        => validCount switch
        {
            ValidSelectedCount.Any => targets.Any(this.CanIssue),
            ValidSelectedCount.Single => targets.Count == 1 && this.CanIssue(targets.First()),
            _ => throw new UnreachableException()
        };
    [Obsolete]
    protected virtual void Execute(MapBase map, IEnumerable<ISelectable> targets) { }
    [Obsolete]
    internal void Execute(MapBase map, SelectionIntent selection) => this.Execute(map, selection.Resolve(map));

}

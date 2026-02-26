namespace Project1.Core.Input.Orders
{
    internal sealed record OrderCommandRuntime(OrderCommandDef Def)
    {
        internal void Issue(SelectionIntent selection)
        {
            //this.Def.Worker.Issue(this, selection);
            //Ingame.Instance.Events.Post(new PlayerIssuedOrderCommandEvent(this.Def, selection));
        }
        internal void Issue(SelectionFinal selection) => this.Def.Worker.Issue(this, selection);

        //internal bool CanIssue(IReadOnlyCollection<ISelectable> targets)
        //    => this.Def.ValidCount switch
        //    {
        //        ValidSelectedCount.Any => targets.Any(this.Def.Worker.CanIssue),
        //        ValidSelectedCount.Single => targets.Count == 1 && this.Def.Worker.CanIssue(targets.First()),
        //        _ => throw new UnreachableException()
        //    };
    }
}

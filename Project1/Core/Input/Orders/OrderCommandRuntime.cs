using Project1.Core.Screens;

namespace Project1.Core.Input.Orders
{
    internal record OrderCommandRuntime(OrderCommandDef Def)
    {
        internal void Issue(SelectionIntent selection)
        {
            Ingame.Instance.Events.Post(new PlayerIssuedOrderCommandEvent(this.Def, selection));
        }
    }
}

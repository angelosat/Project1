using Project1.Core.Screens;
using System.Collections.Generic;

namespace Project1.Core.Input.Orders
{
    internal record OrderCommandRuntime(OrderCommandDef Def, List<TargetArgs> Targets)
    {
        //internal OrderCommandDef Def;
        //internal List<TargetArgs> Targets = [];
        internal void Issue()
        {
            Ingame.Instance.Events.Post(new PlayerIssuedOrderCommandEvent(this.Def, this.Targets));
        }
    }
}

using Project1.Core.Input;
using Project1.Core.Simulation;

namespace Project1.Core.Interactions
{
    class InteractionDeconstructLogic : InteractionLogic
    {
        internal override void OnFinish(Interaction i)
        {
            WorldMutations.BreakBlock(new CellSelection(i.Actor.Map, i.Target.Global));
        }
    }
}

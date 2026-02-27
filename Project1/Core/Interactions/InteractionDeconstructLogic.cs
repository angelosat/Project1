using Project1.Core.Simulation;

namespace Project1.Core.Interactions
{
    class InteractionDeconstructLogic : InteractionLogic
    {
        internal override void OnFinish(Interaction i)
        {
            WorldMutations.DeconstructBlock(i.Actor.Map, i.Target.Global);
        }
    }
}

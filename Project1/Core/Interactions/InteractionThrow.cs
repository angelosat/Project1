using Microsoft.Xna.Framework;

namespace Project1.Core.Interactions
{
    internal sealed class InteractionThrowLogic : InteractionLogic
    {
        internal override void OnFinish(Interaction i)
        {
            var actor = i.Context.Actor;
            if (actor.Net.IsClient) return;
            var target = i.Context.Target;
            var velocity = new Vector3(target.Direction, 0) * 0.1f + actor.Velocity;
            // TODO use this.All to throw the whole item stack vs only one
            actor.Inventory.Throw(velocity, amount: -1);
        }
    }
}

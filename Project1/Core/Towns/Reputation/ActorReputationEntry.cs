using Project1.Core.Entities.Actors;
using Project1.Framework.Helpers;
using Project1.Framework.UI;

namespace Project1.Core.Towns.Reputation;

sealed class ActorReputationEntry(Actor actor, ulong tick) : IGuiNew
{
    internal EntityRefId ActorId = actor.RefId;
    readonly ProgressIntSigned Reputation = new(-100, 100, 0);
    internal ulong TickDiscovered = tick;

    public Control CreateControl()
        => new BarSigned(this.Reputation);

    internal void ApplyDelta(int v)
    {
        this.Reputation.ApplyDelta(v);
    }
}

using Project1.Core.Resources;
using Project1.Core.World.Encounters;
using Project1.Framework;
using Project1.Framework.Helpers;

namespace Project1.Core.World.Actions;

internal class WorldActionWorker_AvoidInjury : WorldActionWorker
{
    internal override void Resolve(WorldActionData data)
    {
        var actor = data.Actor;
        if(actor.World.Random.CoinFlip())
        {
            $"{actor} avoided injury!".ToConsole();
            return;
        }
        var dmgBase = data.Frontier.Def.Tier * 2;
        var dmg = dmgBase + actor.World.Random.Next(-dmgBase, dmgBase) / 2;
        //dmg += 50;
        actor.Resources.ApplyDelta(ResourceDefOf.Health, -dmg);
        actor.World.Events.Post(new ActorOffmapHealthEvent(actor, dmg));
        $"{actor} received {dmg} damage".ToConsole();
    }
}

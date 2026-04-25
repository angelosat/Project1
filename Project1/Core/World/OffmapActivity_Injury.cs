using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.World.WorldAreas;

namespace Project1.Core.World;

internal sealed class OffmapActivity_Injury : OffmapActivity
{
    internal override void Tick(FrontierWrapper frontier, Actor actor)
    {
        var dmgBase = frontier.Def.Tier * 2;
        var dmg = dmgBase + actor.World.Random.Next(-dmgBase, dmgBase) / 2;
        dmg += 50;
        actor.Resources.ApplyDelta(ResourceDefOf.Health, -dmg);
    }
    //internal override void Tick(FrontierWrapper frontier, Actor actor)
    //{
    //    var dmgBase = frontier.Def.Tier * 2;
    //    var dmg = dmgBase + actor.World.Random.Next(-dmgBase, dmgBase) / 2;
    //    dmg += 50;
    //    actor.Resources.ApplyDelta(ResourceDefOf.Health, -dmg);
    //    //actor.AI.State.Log.Write($"I was injured! ({delta} hp)");
    //    //actor.AI.State.Log.Write($"[Lost {dmg} health,{Color.Red}]");// while exploring {this.Name}");
    //    actor.AI.State.Log.Write($"Lost {dmg} health");// while exploring {this.Name}");
    //}
}

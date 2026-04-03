using Microsoft.Xna.Framework;
using Project1.Core.AI.MetaRoles.Adventurer;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Systems.Materials;
using Project1.Core.World.WorldAreas;

namespace Project1.Core.World;

internal abstract class OffmapActivity
{
    internal abstract void Tick(FrontierWrapper frontier, Actor actor);
}
internal sealed class OffmapActivity_FindLoot : OffmapActivity
{
    internal override void Tick(FrontierWrapper frontier, Actor actor)
    {
        var desiredLoot = actor.AI.GetMeta<RoleAdventurerData>().NextDesiredLoot;
        if (!desiredLoot.HasValue)
            return;
        if (frontier.Def.Tier != desiredLoot.Value.matdef.Tier)
            //throw new System.Exception();
            return;
        var loot = MaterialSystem.Create(desiredLoot.Value.refdef, desiredLoot.Value.matdef, 1);
        actor.World.Register(loot);
        var foundCount = loot.StackSize;
        actor.Inventory.Insert(loot, out var newTotal);
        actor.AI.State.Log.Write($"I found {loot.LabelReadable} x{foundCount} ({newTotal})");
    }
}
internal sealed class OffmapActivity_Injury : OffmapActivity
{
    internal override void Tick(FrontierWrapper frontier, Actor actor)
    {
        var dmg = 1;
        actor.Resources.ApplyDelta(ResourceDefOf.Health, -dmg);
        //actor.AI.State.Log.Write($"I was injured! ({delta} hp)");
        //actor.AI.State.Log.Write($"[Lost {dmg} health,{Color.Red}]");// while exploring {this.Name}");
        actor.AI.State.Log.Write($"Lost {dmg} health");// while exploring {this.Name}");
    }
}
internal sealed class OffmapActivity_ResourceGather : OffmapActivity
{
    internal override void Tick(FrontierWrapper frontier, Actor actor)
    {
        //throw new NotImplementedException();
    }
}
internal sealed class OffmapActivity_Quest : OffmapActivity
{
    internal override void Tick(FrontierWrapper frontier, Actor actor)
    {
        //var meta = actor.AI.Meta as RoleAdventurerData;
        //var activequest = actor.Net.Map.Town.QuestManagerNew.GetQuest(meta.ActiveQuest);
        //var questResolver = activequest.Def.Resolver;
    }
}

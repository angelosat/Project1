using Project1.Core.AI.MetaRoles.Adventurer;
using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Materials;
using Project1.Core.World.WorldAreas;

namespace Project1.Core.World;
internal sealed class OffmapActivity_FarmLoot : OffmapActivity
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
        actor.Skills.ApplyXp(desiredLoot.Value.matdef.Type.GatheringSkill, 10);
        actor.AI.State.Log.Write($"I found {loot.LabelReadable} x{foundCount} ({newTotal})");

    }
}

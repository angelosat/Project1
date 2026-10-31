using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Materials;
using Project1.Core.World.Frontiers;
using Project1.Framework.Helpers;
using System.Collections.Generic;

namespace Project1.Core.World.Actions;

internal sealed class WorldActionWorker_HarvestResource : WorldActionWorker
{
    static readonly List<MaterialRefinementDef> AvailableLoot = [MaterialRefinementDefOf.Logs, MaterialRefinementDefOf.Ore];
    static WorldActionData_HarvestResource Typed(WorldActionData data) => (WorldActionData_HarvestResource)data;
    internal override void Resolve(WorldActionData data)
    {
        var typed = Typed(data);
        var materialType = typed.MaterialType;
        var actor = data.Actor;
        var frontier = data.Frontier;
        var tier = frontier.Def.Tier;
        var random = actor.World.Random;
        var selectedRef = AvailableLoot.SelectRandom(random);
        var selectedMat = MaterialSystem.ByTierAndType(tier, selectedRef);
        var skill = selectedMat.Type.GatheringSkill;
        var item = MaterialSystem.Create(selectedRef, selectedMat, 1);
        actor.Inventory.Insert(item);
        actor.Skills.ApplyXp(skill, 10);
    }
}

internal sealed class WorldActionData_HarvestResource(FrontierRuntime frontier, Actor actor) : WorldActionData(frontier, actor)
{
    internal required MaterialTypeDef MaterialType;
}

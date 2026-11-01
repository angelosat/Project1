using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Systems.Materials;
using Project1.Core.World.Frontiers;
using Project1.Framework.Serialization;

namespace Project1.Core.World.Actions;

internal sealed class WorldActionWorker_HarvestResource : WorldActionWorker
{
    static WorldActionData_HarvestResource Typed(WorldActionData data) => (WorldActionData_HarvestResource)data;
    internal override void Resolve(WorldActionData data)
    {
        var typed = Typed(data);
        var actor = data.Actor;
        var frontier = data.Frontier;
        var tier = frontier.Def.Tier;
        var selectedRef = typed.Refinement;
        //var selectedMat = MaterialSystem.ByTierAndType(tier, selectedRef);
        var selectedMat = typed.Material;
        var skill = selectedMat.Type.GatheringSkill;
        var item = MaterialSystem.Create(selectedRef, selectedMat, 1);
        actor.Inventory.Insert(item);
        actor.Skills.ApplyXp(skill, 10);
    }
    internal override string GetStatusText(WorldActionRuntime runtime)
    {
        var typed = Typed(runtime.Data);
        return $"Harvesting {typed.Material.LabelReadable} {typed.Refinement.LabelReadable}";
    }
}

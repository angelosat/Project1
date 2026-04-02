using Project1.Core.AI.MetaRoles.Adventurer;
using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Materials;

namespace Project1.Core.World;

internal abstract class OffmapActivity
{
    internal abstract void Tick(Actor actor);
}
internal sealed class OffmapActivity_FindLoot : OffmapActivity
{
    internal override void Tick(Actor actor)
    {
        var desiredLoot = actor.AI.GetMeta<RoleAdventurerData>().NextDesiredLoot;
        if (!desiredLoot.HasValue)
            return;
        var loot = RawMaterialSystem.Create(desiredLoot.Value.refdef, desiredLoot.Value.matdef, 1);
        actor.World.Register(loot);
        actor.Inventory.Insert(loot);
        actor.AI.State.Log.Write($"I found {loot.LabelReadable}");
    }
}
internal sealed class OffmapActivity_ResourceGather : OffmapActivity
{
    internal override void Tick(Actor actor)
    {
        //throw new NotImplementedException();
    }
}
internal sealed class OffmapActivity_Quest : OffmapActivity
{
    internal override void Tick(Actor actor)
    {
        //var meta = actor.AI.Meta as RoleAdventurerData;
        //var activequest = actor.Net.Map.Town.QuestManagerNew.GetQuest(meta.ActiveQuest);
        //var questResolver = activequest.Def.Resolver;
    }
}

using Project1.Core.Systems.Materials;
using Project1.Core.Systems.Quests;

#nullable enable

namespace Project1.Core.AI.MetaRoles.Adventurer;

internal class RoleAdventurerData : RoleMetaWrapper
{
    internal QuestId ActiveQuest;
    internal (MaterialRefinementDef refdef, MaterialDef matdef)? NextDesiredLoot;
}

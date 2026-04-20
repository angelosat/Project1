using Project1.Core.Entities;
using Project1.Core.Systems.Magic;
using Project1.Core.Systems.Materials;
using System.Collections.Generic;

namespace Project1.Core.Systems.Consumables.Scrolls;

internal static class ScrollSystem
{
    internal static IEnumerable<Entity> GenerateTemplates()
    {
        List<SpellDef> spells = [SpellDefOf.Teleporting];
        foreach(var spell in spells)
        {
            var scroll = ConsumableSystem.CreateScroll(spell, MaterialDefOf.ShrubStem);
            yield return scroll;
        }
    }
}

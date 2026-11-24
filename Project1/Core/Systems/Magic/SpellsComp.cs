using Project1.Core.Entities;
using System.Collections.Generic;

namespace Project1.Core.Systems.Magic;

public class SpellsComp : EntityComp
{
    public override EntityCompDef CompDef => EntityCompDefOf.Abilities;

    public override string Name => "Spellbook";

    readonly HashSet<SpellDef> Spellbook = [];

    public IReadOnlySet<SpellDef> Spells => this.Spellbook;

    internal void Add(SpellDef spell)
    {
        this.Spellbook.Add(spell);
    }
}

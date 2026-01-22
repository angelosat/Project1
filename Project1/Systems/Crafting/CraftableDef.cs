using System;

namespace Start_a_Town_
{
    public class CraftableDef : Def
    {
        public readonly Type ProfileCategory;
        public Def[] Specific = [];

        public CraftableDef(string name, Type type) : base(name)
        {
            this.ProfileCategory = type;
        }
    }
    [EnsureStaticCtorCall]
    internal static class CraftableDefOf
    {
        static public readonly CraftableDef Smelting = new("Smelting", typeof(MaterialRefinementDef)) { Specific = [MaterialRefinementDefOf.Ingots] };
        static public readonly CraftableDef ToolMaking = new("ToolMaking", typeof(ToolProfileDef));
        static CraftableDefOf()
        {
            Def.Register(typeof(CraftableDefOf));
        }
    }
}

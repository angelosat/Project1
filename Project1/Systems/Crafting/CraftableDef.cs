using System;

namespace Start_a_Town_
{
    public class CraftableDef : Def
    {
        public readonly Type CraftableDefType;

        public CraftableDef(string name, Type type) : base(name)
        {
            this.CraftableDefType = type;
        }
    }
    [EnsureStaticCtorCall]
    internal static class CraftableDefOf
    {
        static public readonly CraftableDef Smelting = new("Smelting", typeof(MaterialRefinementDef));
        static public readonly CraftableDef ToolMaking = new("ToolMaking", typeof(ToolProfileDef));
        //static public readonly CraftableDef Cooking = new(typeof(ToolProfileDef));
        static CraftableDefOf()
        {
            Def.Register(typeof(CraftableDefOf));
        }
    }
}

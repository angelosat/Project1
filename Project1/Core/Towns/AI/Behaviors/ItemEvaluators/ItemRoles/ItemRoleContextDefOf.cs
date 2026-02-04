using Project1.Framework.Base;
using Project1.Framework.Needs;
using Project1.Framework.Skills;
using Start_a_Town_;

namespace Project1.Core.Towns.AI.Behaviors.ItemEvaluators.ItemRoles
{
    [EnsureStaticCtorCall]
    internal class ItemRoleContextDefOf
    {
        public static readonly ItemRoleContextDef Tool = new("Tool", typeof(ToolUseDef), typeof(ItemRoleToolWorker));
        public static readonly ItemRoleContextDef Need = new("Need", typeof(NeedDef), typeof(ItemRoleNeedWorker));

        static ItemRoleContextDefOf()
        {
            Def.Register(typeof(ItemRoleContextDefOf));
        }
    }
}

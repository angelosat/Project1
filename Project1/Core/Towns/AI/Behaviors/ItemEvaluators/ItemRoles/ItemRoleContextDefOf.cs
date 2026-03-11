using Project1.Framework;
using Project1.Core.Needs;
using Project1.Core.Systems.Tools;

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

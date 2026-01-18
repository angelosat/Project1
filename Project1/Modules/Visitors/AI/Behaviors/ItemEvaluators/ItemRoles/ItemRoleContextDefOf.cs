namespace Start_a_Town_
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

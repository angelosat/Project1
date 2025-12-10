namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    internal class ItemRoleContextDefOf
    {
        public static readonly ItemRoleContextDef Tool = new("Tool", typeof(ToolUseDef), typeof(ItemRoleToolWorker));

        static ItemRoleContextDefOf()
        {
            Def.Register(typeof(ItemRoleContextDefOf));
        }
    }
}

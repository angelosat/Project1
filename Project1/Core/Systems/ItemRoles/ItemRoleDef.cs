namespace Project1.Core.Systems.ItemRoles;

public sealed class ItemRoleDef : Def
{
    public required ItemRoleWorker Worker;
    internal readonly ItemRoleContextDef Context;
    public ItemRoleDef(string name, ItemRoleContextDef context)
        : base(name)
    {
        Context = context;
    }
}

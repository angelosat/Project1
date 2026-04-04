using System;
namespace Project1.Core.Systems.ItemRoles
{
    public class ItemRoleContextDef(string name, Type contextType, Type workerType) : Def(name)
    {
        internal Type Context = contextType;
        internal Type WorkerType = workerType;
    }
}

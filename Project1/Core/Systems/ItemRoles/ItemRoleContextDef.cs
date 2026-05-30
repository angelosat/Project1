using Project1.Framework.Helpers;
using System;
namespace Project1.Core.Systems.ItemRoles
{
    public class ItemRoleContextDef(string name, Type targetType, Type workerType) : Def(name)
    {
        [Obsolete]
        internal Type TargetType = targetType;
        internal Type WorkerType = workerType;
        internal ItemRoleWorker Worker = ActivatorSafe<ItemRoleWorker>.CreateInstance(workerType);
    }
}

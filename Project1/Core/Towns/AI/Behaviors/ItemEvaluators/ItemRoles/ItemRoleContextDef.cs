using Project1.Core.Base;
using System;
namespace Project1.Core.Towns.AI.Behaviors.ItemEvaluators.ItemRoles
{
    public class ItemRoleContextDef(string name, Type contextType, Type workerType) : Def(name)
    {
        internal Type Context = contextType;
        internal Type WorkerType = workerType;
    }
}

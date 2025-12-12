using System;
namespace Start_a_Town_
{
    public class ItemRoleContextDef(string name, Type contextType, Type workerType) : Def(name)
    {
        internal Type Context = contextType;
        internal Type WorkerType = workerType;
    }
}

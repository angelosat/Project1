using Project1.Core.Needs;
using Project1.Core.Entities.Actors;
using System;
using Project1.Framework.Helpers;

namespace Project1.Core.AI.MetaRoles
{
    public class RoleMetaDef(string name, Type runtimeType, Type workerType, NeedDef[] needs) : Def(name)
    {
        public readonly Type RuntimeType = runtimeType;
        public readonly RoleMetaWorker Worker = ActivatorSafe<RoleMetaWorker>.CreateInstance(workerType);
        public readonly NeedDef[] Needs = needs;

        public RoleMetaWrapper CreateWrapper()
        {
            var roleWrapper = ActivatorSafe<RoleMetaWrapper>.CreateInstance(this.RuntimeType);
            roleWrapper.Def = this;
            return roleWrapper;
        }
        public void AssignTo(Actor actor)
        {
            var roleWrapper = ActivatorSafe<RoleMetaWrapper>.CreateInstance(this.RuntimeType);
            roleWrapper.Def = this;
            roleWrapper.AssignTo(actor);
        }
    }
}

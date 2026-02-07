using Project1.Core.Needs;
using Project1.Core.Base;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using System;
namespace Project1.Core.World.MetaRoles
{
    public class RoleMetaDef(string name, Type wrapperType, Type workerType, NeedDef[] needs) : Def(name)
    {
        public readonly Type WrapperType = wrapperType;
        public readonly RoleMetaWorker Worker = ActivatorSafe<RoleMetaWorker>.CreateInstance(workerType);
        //public readonly Planner[] Planners = [.. planners.Select(ActivatorSafe<Planner>.CreateInstance)];
        public readonly NeedDef[] Needs = needs;

        public RoleMetaWrapper CreateWrapper()
        {
            var roleWrapper = ActivatorSafe<RoleMetaWrapper>.CreateInstance(this.WrapperType);
            roleWrapper.Def = this;
            return roleWrapper;
        }
        public void AssignTo(Actor actor)
        {
            var roleWrapper = ActivatorSafe<RoleMetaWrapper>.CreateInstance(this.WrapperType);
            roleWrapper.Def = this;
            roleWrapper.AssignTo(actor);
        }
    }
}

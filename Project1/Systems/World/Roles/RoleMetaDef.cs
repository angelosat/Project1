using System;
namespace Start_a_Town_
{
    public class RoleMetaDef(string name, Type wrapperType, Type workerType, NeedDef[] needs) : Def(name)
    {
        public readonly Type WrapperType = wrapperType;
        public readonly RoleMetaWorker Worker = ActivatorSafe<RoleMetaWorker>.CreateInstance(workerType);
        //public readonly Planner[] Planners = [.. planners.Select(ActivatorSafe<Planner>.CreateInstance)];
        public readonly NeedDef[] Needs = needs;

        public RoleMetaWrapper Create()
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

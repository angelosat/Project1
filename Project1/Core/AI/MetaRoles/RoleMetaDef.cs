using Project1.Core.AI.Thought;
using Project1.Core.Entities.Actors;
using Project1.Core.Needs;
using Project1.Framework.Helpers;
using System;
using System.Linq;

namespace Project1.Core.AI.MetaRoles
{
    public class RoleMetaDef(string name, Type runtimeType, Type workerType, NeedDef[] needs, Type[] thoughtProcessTypes) : Def(name)
    {
        public readonly Type RuntimeType = runtimeType;
        public readonly RoleMetaWorker Worker = ActivatorSafe<RoleMetaWorker>.CreateInstance(workerType);
        public readonly NeedDef[] Needs = needs;
        public readonly ThoughtProcess[] Thoughts = [.. thoughtProcessTypes.Select(testc => ActivatorSafe<ThoughtProcess>.CreateInstance(testc))];
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

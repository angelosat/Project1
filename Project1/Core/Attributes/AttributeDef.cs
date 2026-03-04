using Project1.Framework.Helpers;
using System;

namespace Project1.Core.Attributes
{
    public sealed class AttributeDef(string name, Type workerClass, string description = "") : Def(name)
    {
        readonly Type WorkerClass = workerClass;
        
        public string Description = description;

        public AttributeWorker Worker => field ??= ActivatorSafe<AttributeWorker>.CreateInstance(this.WorkerClass);
    }
}

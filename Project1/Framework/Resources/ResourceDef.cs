using Project1.Framework.Base;
using System;

namespace Project1.Framework.Resources
{
    public sealed class ResourceDef : Def
    {

        public Type WorkerClass;
       
        public readonly int BaseMax = 100;

        public ResourceDef(string name, Type workerClass) : base(name)
        {
            this.WorkerClass = workerClass;
        }

        ResourceWorker workerCached;
        public ResourceWorker Worker => workerCached ??= (ResourceWorker)Activator.CreateInstance(this.WorkerClass, this);

        public string Format => "";

        public string Description => this.Worker.Description;
    }
}

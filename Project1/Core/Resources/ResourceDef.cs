using System;

namespace Project1.Core.Resources
{
    public sealed class ResourceDef(string name, Type workerClass, float baseRegenRate = 0) : Def(name)
    {
        public float BaseRegenRate = baseRegenRate;

        public Type WorkerClass = workerClass;
       
        public readonly int BaseMax = 100;
        ResourceWorker workerCached;

        public ResourceWorker Worker => workerCached ??= (ResourceWorker)Activator.CreateInstance(this.WorkerClass, this);

        public string Format => "";

        public string Description => this.Worker.Description;
    }
}

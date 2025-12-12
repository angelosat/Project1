using System;

namespace Start_a_Town_
{
    public sealed class ResourceDef : Def
    {
        static public Progress Recovery => new(0, Ticks.PerGameMinute, Ticks.PerGameMinute); 

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

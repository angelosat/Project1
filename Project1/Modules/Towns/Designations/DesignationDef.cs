using Start_a_Town_.UI;
using System;

namespace Start_a_Town_
{
    public sealed class DesignationDef : Def
    {
        public QuickButton IconAdd;

        public QuickButton IconRemove;

        readonly Type WorkerClass;
       
        public DesignationDef(string name, Type workerClass, QuickButton icon) : base(name)
        {
            this.WorkerClass = workerClass;
            this.IconAdd = icon;
            this.IconRemove = icon != null ? new QuickButton(icon.Icon, null, "Cancel") { HoverText = $"Cancel {name}" }.AddOverlay(Icon.X) as QuickButton: null;
        }

        DesignationWorker _cachedWorker;
        DesignationWorker Worker => _cachedWorker ??= (DesignationWorker)Activator.CreateInstance(this.WorkerClass);

        public bool IsValid(MapBase map, IntVec3 global) => this.Worker.IsValid(map, global);
    }
}

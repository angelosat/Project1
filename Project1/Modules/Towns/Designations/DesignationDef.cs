using Start_a_Town_.UI;
using System;

namespace Start_a_Town_
{
    public sealed class DesignationDef : Def
    {
        readonly public QuickButton IconAdd;
        readonly public QuickButton IconRemove;

        readonly public Sprite SpriteAdd, SpriteRemove;
        readonly public char Symbol;
        readonly public string Verb;

        readonly public bool AffectsBlocks;

        readonly Type WorkerClass;
       
        public DesignationDef(string name, Type workerClass, Sprite sprite, string verb, string hoverText, bool affectsBlocks) : base(name)
        {
            this.AffectsBlocks = affectsBlocks;
            this.WorkerClass = workerClass;
            this.IconAdd = new QuickButton(new Icon(sprite), null, verb)
            {
                HoverText = hoverText
            };
            this.IconRemove = this.IconAdd != null ? new QuickButton(this.IconAdd.Icon, null, "Cancel") { HoverText = $"Cancel {name}" }.AddOverlay(Icon.X) as QuickButton : null;
        }
        public DesignationDef(string name, Type workerClass, char symbol, string verb, string hoverText, bool affectsBlocks) : base(name)
        {
            this.AffectsBlocks = affectsBlocks;
            this.WorkerClass = workerClass;
            this.IconAdd = new QuickButton(symbol, null, verb)
            {
                HoverText = hoverText
            };
            this.IconRemove = this.IconAdd != null ? new QuickButton(this.IconAdd.Icon, null, "Cancel") { HoverText = $"Cancel {name}" }.AddOverlay(Icon.X) as QuickButton : null;
        }
        DesignationWorker _cachedWorker;
        DesignationWorker Worker => _cachedWorker ??= (DesignationWorker)Activator.CreateInstance(this.WorkerClass);

        public bool IsValid(MapBase map, IntVec3 global) => this.IsValid(new TargetArgs(map, global));// this.Worker.IsValid(new TargetArgs(map, global));
        public bool IsValid(TargetArgs target) => this.Worker.IsValid(target);
    }
}

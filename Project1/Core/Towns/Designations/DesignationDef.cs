using Project1.Core.Graphics;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.UI;
using System;

namespace Project1.Core.Towns.Designations
{
    public sealed class DesignationDef : Def
    {
        //readonly public QuickButton IconAdd;
        //readonly public QuickButton IconRemove;

        readonly public Sprite SpriteAdd, SpriteRemove;
        public Icon Icon { get; init; }
        readonly public char Symbol;
        readonly public string Verb;

        //readonly public bool AffectsBlocks;

        readonly public TargetType TargetType;

        readonly Type WorkerClass;

        public bool IsManual = true;
       
        public DesignationDef(string name, Type workerClass, Sprite sprite, string verb, string hoverText, TargetType targetType) : base(name)
        {
            //this.AffectsBlocks = affectsBlocks;
            this.TargetType = targetType;// affectsBlocks ? TargetType.Cell : TargetType.Entity;
            this.WorkerClass = workerClass;
            this.Icon = new Icon(sprite);
            //this.IconAdd = new QuickButton(this.Icon, null, verb)
            //{
            //    HoverText = hoverText
            //};
            //this.IconRemove = this.IconAdd != null ? new QuickButton(this.IconAdd.Icon, null, "Cancel") { HoverText = $"Cancel {name}" }.AddOverlay(Icon.X) as QuickButton : null;
        }
        public DesignationDef(string name, Type workerClass, char symbol, string verb, string hoverText, TargetType targetType) : base(name)
        {
            //this.AffectsBlocks = affectsBlocks;
            this.TargetType = targetType;// affectsBlocks ? TargetType.Cell : TargetType.Entity;
            this.WorkerClass = workerClass;
            //this.IconAdd = new QuickButton(symbol, null, verb)
            //{
            //    HoverText = hoverText
            //};
            //this.IconRemove = this.IconAdd != null ? new QuickButton(this.IconAdd.Icon, null, "Cancel") { HoverText = $"Cancel {name}" }.AddOverlay(Icon.X) as QuickButton : null;
        }
        DesignationWorker _cachedWorker;
        public DesignationWorker Worker => _cachedWorker ??= (DesignationWorker)Activator.CreateInstance(this.WorkerClass);

        public bool IsValid(MapBase map, IntVec3 global) => this.IsValid(new InteractionTarget(map, global));// this.Worker.IsValid(new TargetArgs(map, global));
        public bool IsValid(InteractionTarget target) => this.Worker.IsValid(target);
    }
}

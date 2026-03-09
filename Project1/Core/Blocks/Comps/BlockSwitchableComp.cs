using Project1.Core.Resources;
using Project1.Core.Simulation;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;

#nullable enable

namespace Project1.Core.Blocks.Comps
{
    class BlockSwitchableComp : BlockComp
    {
        public new class Spec(ResourceDef? resource = null) : BlockComp.Spec
        {
            readonly ResourceDef? Resource = resource;

            public override Type CompType => typeof(BlockSwitchableComp);

            public override BlockSwitchableComp CreateComp() => new() { Resource = this.Resource };
        }
        ResourceDef? Resource { get; init; }
        readonly Scheduler Scheduler = new(Ticks.FromMinutes(10));
        public override BlockCompDef CompDef => BlockCompDefOf.Switchable;
        public bool IsOn { get; private set; } = true;
        public override void Tick()
        {
            if (this.Map.Net.IsClient)
                return;
            if (this.Scheduler.OnSchedule(this.Map.World.CurrentTick))
                this.ConsumeFuel();
        }
        void ConsumeFuel()
        {
            if (this.Resource is null)
                return;

            if (!this.IsOn)
                return;

            if (this.Parent.GetComp<BlockResourcesComp>() is not BlockResourcesComp resourcesComp)
                return;

            if (!resourcesComp.TryApplyDelta(this.Resource, -1))
                return;

            if (resourcesComp.GetValue(this.Resource) == 0)
                this.Switch(false);
        }
        public void Toggle()
            => this.Switch(!this.IsOn);
        public void Switch(bool on)
        {
            this.IsOn = on;
            foreach (var comp in this.Parent.Comps.Inner)
                comp.OnSwitched(this.IsOn);
            this.Map.Events.Post(new CellsInvalidatedEvent(this.Map, [this.Parent.OriginGlobal]));
        }
        internal bool IsSwitchable()
            => !this.IsOn && (this.Resource is ResourceDef res && this.Parent.GetComp<BlockResourcesComp>()?.GetValueOrDefault(res, 0) > 0);
            
        protected override void SaveExtra(SaveTag tag)
        {
            tag.Add(this.IsOn.Save("SwitchedOn"));
        }
        public override void Load(SaveTag tag)
        {
            tag.TryGetTagValue<bool>("SwitchedOn", v => this.IsOn = v);
        }
        public override void Write(IDataWriter w)
        {
            w.Write(this.IsOn);
        }
        public override ISerializable Read(IDataReader r)
        {
            this.IsOn = r.ReadBoolean();
            return this;
        }
        internal override IEnumerable<Control> GetInspectorControls()
        {
            yield return new Label(() => $"Switched: {(this.IsOn ? "on" : "off")}");
        }
    }
}

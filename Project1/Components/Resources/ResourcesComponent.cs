using Start_a_Town_.Components;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public class ResourcesComponent : EntityComponent
    {
        public Resource[] Resources;

        public override string Name { get; } = "Resources";
        
        public ResourcesComponent(Actor actor, ItemDef def)
        {
            var defs = def.ActorProperties.Resources;
            var count = defs.Length;
            this.Resources = new Resource[count];
            for (int i = 0; i < count; i++)
                this.Resources[i] = new Resource(defs[i]);
        }
        public ResourcesComponent()
        {
        }
        public ResourcesComponent(params Resource[] resources)
        {
            var count = resources.Length;
            this.Resources = new Resource[count];
            for (int i = 0; i < count; i++)
                this.Resources[i] = resources[i].Clone();
        }
        public ResourcesComponent(params ResourceDef[] defs)
        {
            var count = defs.Length;
            this.Resources = new Resource[count];
            for (int i = 0; i < count; i++)
                this.Resources[i] = new Resource(defs[i]);
        }
        internal override void Initialize(ComponentProps componentProps)
        {
            var props = componentProps as Props;
            var count = props.Defs.Length;
            this.Resources = new Resource[count];
            for (int i = 0; i < count; i++)
                this.Resources[i] = new Resource(props.Defs[i]);
        }
        public override void Tick()
        {
            foreach (var item in this.Resources)
                item.Tick();// this.Parent);
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {

            foreach (var item in this.Resources)
                item.HandleMessage(parent, e);

            switch (e.Type)
            {
                default:
                    break;
            }
            return false;
        }

        internal override void HandleRemoteCall(GameObject parent, ObjectEventArgs e)
        {
            foreach (var item in this.Resources)
                item.HandleRemoteCall(parent, e);
        }

        public override void OnNameplateCreated(GameObject parent, UI.Nameplate plate)
        {
            foreach (var res in this.Resources)
                res.OnNameplateCreated(parent, plate);
        }
        public override void OnHealthBarCreated(GameObject parent, UI.Nameplate plate)
        {
            foreach (var res in this.Resources)
                res.OnHealthBarCreated(parent, plate);
        }
        public override object Clone()
        {
            return new ResourcesComponent(this.Resources);
        }
        public override void OnObjectCreated(GameObject parent)
        {
            foreach (var r in this.Resources)
                r.Parent = parent as Actor;
        }

        public override string ToString()
        {
            string text = "";
            foreach (var item in this.Resources)
                text += item.ToString() + "\n";
            return text.TrimEnd('\n');
        }

        internal override void SaveExtra(SaveTag tag)
        {
            this.Resources.SaveImmutable(tag, "Resources");

        }
        internal override void Load(GameObject parent, SaveTag tag)
        {
            this.Resources.TryLoadImmutable(tag, "Resources");
        }
        public override void Write(IDataWriter writer)
        {
            this.Resources.Write(writer);
        }
        public override void Read(IDataReader reader)
        {
            this.Resources.Read(reader);
        }

        internal Resource GetResource(ResourceDef def)
        {
            return this.Resources.FirstOrDefault(r => r.ResourceDef == def);
        }
        [InspectorHidden]
        public Resource this[ResourceDef def] => this.GetResource(def);
        GroupBox _cachedGui;
        GroupBox CachedGui
        {
            get
            {
                if (this._cachedGui is null)
                {
                    this._cachedGui = new GroupBox();
                    for (int i = 0; i < this.Resources.Length; i++)
                        this._cachedGui.AddControlsBottomLeft(this.Resources[i].GetControl());
                }
                return this._cachedGui;
            }
        }
        internal override void GetSelectionInfo(IUISelection info, GameObject parent)
        {
            info.AddInfo(this.CachedGui);
        }
        internal override void GetSelectionInfo(SelectionManager info, GameObject parent)
        {
            info.AddInfo(this.CachedGui);
        }
        internal void AddModifier(ResourceRateModifier resourceRateModifier)
        {
            var resource = this.GetResource(resourceRateModifier.Def.Source);
            resource.AddModifier(resourceRateModifier);
        }
        internal override void Initialize(Entity parent, Dictionary<string, MaterialDef> materials)
        {
            for (int i = 0; i < this.Resources.Length; i++)
            {
                this.Resources[i].InitMaterials(parent, materials);
            }
        }
        public override void OnTooltipCreated(GameObject parent, Control tooltip)
        {
            foreach (var r in this.Resources)
                tooltip.AddControlsBottomLeft(r.GetControl());
        }
        public class Props : ComponentProps
        {
            public override Type CompClass => typeof(ResourcesComponent);
            public ResourceDef[] Defs;
            public Props(params ResourceDef[] defs)
            {
                this.Defs = defs;
            }
        }
        public override void OnObjectSynced(GameObject parent)
        {
            base.OnObjectSynced(parent);
            foreach (var r in this.Resources)
                r.Resolve(this.Parent as Entity);
        }
        public override void OnSpawn(MapBase newMap)
        {
            foreach (var r in this.Resources)
                r.Resolve(this.Parent as Entity);
        }
        public override void OnDespawnExtra(MapBase oldMap)
        {
            foreach (var r in this.Resources)
                r.OnDespawn(this.Parent as Entity);
        }

        internal void AdjustAndSync(ResourceDef def, float v)
        {
            this.Adjust(def, v);
            Resource.Packets.SendAdjust(this.Parent as Actor, def, v);
        }

        internal void Adjust(ResourceDef def, float v)
        {
            var res = this[def];
            res.Adjust(v);
        }
    }
}

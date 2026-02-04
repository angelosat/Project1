using Project1.Framework.Base;
using Project1.Framework.Entities;
using Project1.Framework.Interfaces;
using Project1.Framework.Materials;
using Project1.Framework.UI;
using Start_a_Town_;
using Start_a_Town_.Components;
using Start_a_Town_.UI;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Framework.Resources
{
    public class ResourcesComponent : EntityComp
    {
        //public Resource[] Resources = [];
        public Dictionary<ResourceDef, Resource> Resources = [];
        public override string Name { get; } = "Resources";

        internal override void CopyFrom(EntityComp comp)
        {
         
            var source = (ResourcesComponent)comp;
            
            foreach (var r in source.Resources.Values)
                this.Add(r.Def);
        }
        public void Add(ResourceDef def)
        {
            this.Resources[def] = new(def) { Owner = this.Owner as Entity };
            return;
            this.Resources.Add(def, new(def) { Owner = this.Owner as Entity });
        }
        public ResourcesComponent()
        {
        }
        //public ResourcesComponent(params Resource[] resources)
        //{
        //    var count = resources.Length;
        //    this.Resources = new Resource[count];
        //    for (int i = 0; i < count; i++)
        //        this.Resources[i] = resources[i].Clone();
        //}
        public ResourcesComponent(params ResourceDef[] defs)
        {
            throw new System.Exception();
            //var count = defs.Length;
            //this.Resources = new Resource[count];
            //for (int i = 0; i < count; i++)
            //    this.Resources[i] = new Resource(defs[i]);
        }
        
        public override void Tick()
        {
            foreach (var item in this.Resources.Values)
                item.Tick();// this.Parent);
        }

        //public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        //{

        //    foreach (var item in this.Resources)
        //        item.HandleMessage(parent, e);

        //    switch (e.Type)
        //    {
        //        default:
        //            break;
        //    }
        //    return false;
        //}

        //internal override void HandleRemoteCall(GameObject parent, ObjectEventArgs e)
        //{
        //    foreach (var item in this.Resources)
        //        item.HandleRemoteCall(parent, e);
        //}

        public override void OnNameplateCreated(GameObject parent, Nameplate plate)
        {
            foreach (var res in this.Resources.Values)
                res.OnNameplateCreated(parent, plate);
        }
        public override void OnHealthBarCreated(GameObject parent, Nameplate plate)
        {
            foreach (var res in this.Resources.Values)
                res.OnHealthBarCreated(parent, plate);
        }
        //public override object Clone()
        //{
        //    return new ResourcesComponent(this.Resources);
        //}
        //internal override void Resolve()
        //{
        //    foreach (var r in this.Resources)
        //        r.Owner = this.Owner as Actor;
        //}

        public override string ToString()
        {
            string text = "";
            foreach (var item in this.Resources)
                text += item.ToString() + "\n";
            return text.TrimEnd('\n');
        }

        internal override void SaveExtra(SaveTag tag)
        {
            //this.Resources.SaveImmutable(tag, "Resources");
            tag.SaveDefWrappers("Resources", this.Resources);
        }
        internal override void Load(GameObject parent, SaveTag tag)
        {
            //this.Resources.TryLoadImmutable(tag, "Resources");
            tag.LoadDefWrappers("Resources", this.Resources);
            this.Resolve();

        }
        public override void Write(IDataWriter writer)
        {
            //this.Resources.Values.Write(writer);
            writer.WriteValues(this.Resources);
        }
        public override void Read(IDataReader reader)
        {
            //this.Resources.Read(reader);
            reader.ReadDefWrappers(this.Resources);
            this.Resolve();
        }

        internal Resource GetResource(ResourceDef def)
        {
            return this.Resources[def];
            //return this.Resources.FirstOrDefault(r => r.ResourceDef == def);
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
                    foreach(var r in this.Resources.Values)
                        this._cachedGui.AddControlsBottomLeft(r.GetControlBar());
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
        internal override void ApplyMaterials(Entity parent, Dictionary<string, MaterialDef> materials)
        {
            //for (int i = 0; i < this.Resources.Length; i++)
            //{
            //    this.Resources[i].InitMaterials(parent, materials);
            //}
            foreach(var r in this.Resources.Values)
                r.InitMaterials(parent, materials);
        }
        public override void OnTooltipCreated(GameObject parent, Control tooltip)
        {
            foreach (var r in this.Resources.Values)
                //tooltip.AddControlsBottomLeft(r.GetControlBar());
                tooltip.AddControlsBottomLeft(r.GetControlLabel());
        }
        internal override void Resolve()
        {
            foreach (var r in this.Resources.Values)
            {
                r.Owner = this.Owner;
                //r.Revalidate();
            }
        }
        
        //public override void OnObjectSynced(GameObject parent)
        //{
        //    base.OnObjectSynced(parent);
        //    //foreach (var r in this.Resources.Values)
        //    //    r.Resolve(this.Owner as Entity);
        //}
        //public override void OnSpawn(MapBase newMap)
        //{
        //    foreach (var r in this.Resources)
        //        r.Resolve(this.Owner as Entity);
        //}
        //public override void OnDespawnExtra(MapBase oldMap)
        //{
        //    foreach (var r in this.Resources)
        //        r.OnDespawn(this.Owner as Entity);
        //}

        //internal void AdjustAndSync(ResourceDef def, float v)
        //{
        //    this.Adjust(def, v);
        //    Resource.Packets.SendAdjust(this.Owner as Actor, def, v);
        //}
        //void Revalidate()
        //{
        //    foreach (var r in this.Resources.Values)
        //        r.Revalidate();
        //}

        internal void ApplyDelta(ResourceDef def, float v)
        {
            var res = this[def];
            res.ApplyDelta(v);
        }
        public void SetValue(ResourceDef def, float value)
        {
            this[def].SetValue(value);
        }
        public new class Spec : Spec<ResourcesComponent> 
        {
            public ResourceDef[] Defs;
            public Spec(ResourceDef[] defs)
            {
                this.Defs = defs;
            }
            protected override void ApplyDefaultsTo(ResourcesComponent comp)
            {
                foreach (var def in this.Defs)
                    comp.Add(def);
            }
        }
    }
}

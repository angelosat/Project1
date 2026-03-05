using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Graphics;
using Project1.Core.Interactions;
using Project1.Core.Inventory;
using Project1.Core.Materials;
using Project1.Core.Rendering;
using Project1.Core.Simulation;
using Project1.Core.UI.Hud;
using Project1.Core.UI.NamePlates;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;

namespace Project1.Core.Entities
{
    public abstract class EntityComp<TConfig> : EntityComp
    where TConfig : EntityComp.Spec
    {
        public new TConfig Defaults => (TConfig)base.Defaults;
    }
    public abstract class EntityComp : Inspectable
    {
        public abstract EntityCompDef CompDef { get; }
        public int RuntimeIndex;
        public override string LabelReadable => CompDef.LabelReadable;
        public abstract string Name { get; }
        public override string ToString() => this.LabelReadable;
        internal Spec Defaults { get; private set; }
        public virtual void OnNameplateCreated(GameObject parent, Nameplate plate) { }
        public virtual void OnHealthBarCreated(GameObject parent, Nameplate plate) { }
        public Entity Owner;
        public EntityComp() { }
        public virtual void Tick() { }
        public virtual void Initialize(GameObject parent) { }
        public virtual void Randomize(GameObject parent, RandomThreaded random) { this.Initialize(parent); }
        public virtual void OnSpawn(MapBase newMap) { }
        public void OnDespawn(MapBase oldmap) 
        {
            oldmap.Events.Unsubscribe(this);
            this.OnDespawnExtra(oldmap);
        }
        public virtual void OnDespawnExtra(MapBase oldmap) { }
        public virtual void OnDispose() { }
        internal virtual void ResolveReferences() { }
        internal virtual void Resolve() { }
        internal virtual void InitializeOnce() { }
        public virtual void OnObjectLoaded(GameObject parent) { }
        public virtual void OnObjectSynced(GameObject parent) { }
        public virtual void SetMaterial(MaterialDef mat) { }
        internal virtual void ApplyMaterials(Entity parent, Dictionary<string, MaterialDef> materials) { }
        internal virtual void ApplyQuality(Entity parent, QualityDef quality) { }
        public virtual void Draw(MySpriteBatch sb, DrawObjectArgs e) { }
        public virtual void Draw(MySpriteBatch sb, GameObject parent, Camera camera) { }
        public virtual void DrawMouseover(MySpriteBatch sb, Camera camera, GameObject parent) { }
        public virtual void DrawUI(SpriteBatch sb, Camera camera, GameObject parent) { }
        public virtual void DrawAfter(MySpriteBatch sb, Camera cam) { }
        public virtual IEnumerable<Entity> GetChildren() { yield break; }
        public virtual void GetChildren(List<GameObjectSlot> list) { }
        public virtual void GetContainers(List<Container> list) { }
        public virtual void OnTooltipCreated(GameObject parent, Control tooltip) { }
        public virtual void GetInventoryTooltip(GameObject parent, Control tooltip) { this.OnTooltipCreated(parent, tooltip); }
        internal virtual ContextAction GetContextRB(GameObject parent, GameObject player) => null;
        internal virtual ContextAction GetContextActivate(GameObject parent, GameObject player) => null;
        public virtual void GetClientActions(GameObject parent, List<ContextAction> actions) { }
        public virtual void GetInteractions(GameObject parent, List<Interaction> actions) { }
        internal virtual void GetEquippedActionsWithTarget(GameObject parent, GameObject actor, TargetArgs t, List<Interaction> list) { }
        internal SaveTag SaveAs(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.SaveExtra(tag);
            return tag.Value != null ? tag : null;
        }
        internal virtual List<SaveTag> Save() => null;
        internal virtual void SaveExtra(SaveTag tag)
        {
            var list = this.Save();
            if (list != null)
                foreach (var t in list)
                    tag.Add(t);
        }
        internal virtual void Load(GameObject parent, SaveTag tag) => this.LoadExtra(tag);
        internal virtual void LoadExtra(SaveTag tag) { }
        public virtual Control GetParametrizer() => null;
        public virtual void Write(IDataWriter w) { }
        public virtual void Read(IDataReader r) { }
        public virtual GroupBox GetGUI() { return null; }
        internal virtual void GetInterface(GameObject parent, Control box) { }
        [Obsolete]
        internal virtual void GetManagementInterface(GameObject gameObject, Control box) { }
        internal virtual GroupBox GetDetailedGui() => null;
        internal virtual void OnMapLoaded(GameObject parent) { }
        internal virtual void GetQuickButtons(SelectionManager info, GameObject parent) { }
        internal virtual IEnumerable<Button> GetTabs() { yield break; }
        [Obsolete]
        internal virtual void GetSelectionInfo(IUISelection info, GameObject parent) { }
        internal virtual IEnumerable<Control> GetSelectionInfo() { yield break; }
        internal virtual void SyncWrite(IDataWriter w) { }
        internal virtual void SyncRead(GameObject parent, IDataReader r) { }
        internal virtual void CopyFrom(EntityComp source) { }
        internal virtual IEnumerable<GameObjectSlot> GetSlots() { yield break; }
        internal virtual void OnKill() { }
        internal virtual void ResolveReferencesNew() { }
        public abstract class Spec
        {
            public abstract Type CompClass { get; }
            internal abstract void ApplyDefaults(EntityComp props);
            internal abstract EntityComp CreateComp();
        }
        public abstract class Spec<T> : Spec where T : EntityComp, new()
        {
            public override Type CompClass => typeof(T);
            internal sealed override T CreateComp() => new();
            internal sealed override void ApplyDefaults(EntityComp comp)
            {
                comp.Defaults = this;
                this.ApplyDefaultsTo((T)comp);
            }
            protected virtual void ApplyDefaultsTo(T comp) { }
        }
    }
}
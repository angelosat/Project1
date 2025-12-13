using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Start_a_Town_.Components
{
    public abstract class EntityComp<TConfig> : EntityComp
    where TConfig : EntityComp.Spec
    {
        public new TConfig Defaults => (TConfig)base.Defaults;
    }

    public abstract class EntityComp : Inspectable//, ICloneable
    {
        public override string Label => this.Name;
        public abstract string Name { get; }
        public override string ToString()
        {
            return this.Label;
        }
        internal Spec Defaults { get; private set; }
        public virtual void OnNameplateCreated(GameObject parent, Nameplate plate) { }
        public virtual void OnHealthBarCreated(GameObject parent, Nameplate plate) { }

        public GameObject Owner;

        public EntityComp()
        {
        }
        public EntityComp(GameObject parent)
            : this()
        { }

        [Obsolete("Use Props-based creation instead.")]
        public virtual object Clone()
        {
            var t = this.GetType(); // concrete component type
            var props = Owner.Def.CompProps.First(p =>
            {
                var typeArg = p.GetType().BaseType.GetGenericArguments()[0];
                return t.IsAssignableFrom(typeArg);
            });

            var newComp = props.CreateComp();
            props.Apply(newComp);
            return newComp;
        }

        public virtual bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            return false;
        }
        internal virtual void HandleRemoteCall(GameObject gameObject, ObjectEventArgs e) { }
        internal virtual void HandleRemoteCall(GameObject gameObject, Message.Types type, BinaryReader r) { }

        public virtual void Tick() { }

        public void Tick(MapBase map, IBlockEntityCompContainer entity, Vector3 global)
        {
            throw new NotImplementedException();
        }

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

        public virtual void Resolve() { }
        public virtual void OnObjectLoaded(GameObject parent) { }
        public virtual void OnObjectSynced(GameObject parent) { }
        public virtual void SetMaterial(MaterialDef mat) { }

        internal virtual void Initialize(Entity parent, Dictionary<string, MaterialDef> materials) { }
        internal virtual void Initialize(Entity parent, Quality quality) { }

        //public virtual void MakeChildOf(GameObject parent) { this.Owner = parent; }

        public virtual void Draw(MySpriteBatch sb, DrawObjectArgs e) { }
        public virtual void Draw(MySpriteBatch sb, GameObject parent, Camera camera) { }

        public virtual void DrawMouseover(MySpriteBatch sb, Camera camera, GameObject parent) { }
        public virtual void DrawUI(SpriteBatch sb, Camera camera, GameObject parent) { }
        public virtual void DrawAfter(MySpriteBatch sb, Camera cam, GameObject parent) { }
        public virtual IEnumerable<GameObject> GetChildren() { yield break; }
        public virtual void GetChildren(List<GameObjectSlot> list) { }
        public virtual void GetContainers(List<Container> list) { }
        public virtual void OnTooltipCreated(GameObject parent, Control tooltip) { }
        public virtual void GetInventoryTooltip(GameObject parent, Control tooltip) { this.OnTooltipCreated(parent, tooltip); }
        internal virtual ContextAction GetContextRB(GameObject parent, GameObject player)
        {
            return null;
        }
        internal virtual ContextAction GetContextActivate(GameObject parent, GameObject player)
        {
            return null;
        }
        public virtual void GetClientActions(GameObject parent, List<ContextAction> actions)
        {
        }
        public virtual void GetInteractions(GameObject parent, List<Interaction> actions) { }
        public virtual void GetRightClickActions(GameObject parent, List<ContextAction> actions) { }
        internal virtual void GetEquippedActionsWithTarget(GameObject parent, GameObject actor, TargetArgs t, List<Interaction> list)
        {
        }
        public virtual void GetHauledActions(GameObject parent, TargetArgs target, List<Interaction> actions) { }

        internal SaveTag SaveAs(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.SaveExtra(tag);
            return tag.Value != null ? tag : null;
        }
        internal virtual List<SaveTag> Save()
        {
            return null;
        }
        internal virtual void SaveExtra(SaveTag tag)
        {
            var list = this.Save();
            if (list != null)
                foreach (var t in list)
                    tag.Add(t);
        }
        internal virtual void Load(GameObject parent, SaveTag tag)
        {
            this.LoadExtra(tag);
        }
        internal virtual void LoadExtra(SaveTag tag)
        {

        }
        public virtual Control GetParametrizer() => null;
        public virtual void Write(IDataWriter w) { }
        public virtual void Read(IDataReader r) { }
        internal virtual void GetAvailableTasks(GameObject parent, List<Interaction> list)
        {

        }

        public virtual GroupBox GetGUI() { return null; }
        internal virtual void GetInterface(GameObject parent, UI.Control box) { }
        [Obsolete]
        internal virtual void GetManagementInterface(GameObject gameObject, UI.Control box) { }
        internal virtual GroupBox GetDetailedGui() => null;
        internal virtual void OnMapLoaded(GameObject parent)
        {
        }
        internal virtual void GetQuickButtons(SelectionManager info, GameObject parent) { }
        internal virtual IEnumerable<Button> GetTabs() { yield break; }
        [Obsolete]
        internal virtual void GetSelectionInfo(IUISelection info, GameObject parent) { }
        internal virtual void GetSelectionInfo(SelectionManager info, GameObject parent) { }

        internal virtual void OnGameEvent(GameObject gameObject, GameEvent e)
        {
        }

        internal virtual void SyncWrite(IDataWriter w)
        {
        }
        internal virtual void SyncRead(GameObject parent, IDataReader r)
        {
        }

        internal virtual void ResolveReferences()
        {
        }
        public abstract class Spec 
        {
            internal abstract void Apply(EntityComp props);
            internal abstract EntityComp CreateComp();
        }
        public abstract class Spec<T> : Spec where T : EntityComp, new()
        {
            Type CompClass => typeof(T);
            internal sealed override T CreateComp() => new T();
            internal sealed override void Apply(EntityComp comp)
            {
                comp.Defaults = this;
                this.ApplyTo((T)comp);
            }
            protected virtual void ApplyTo(T comp) { }
        }
    }
}

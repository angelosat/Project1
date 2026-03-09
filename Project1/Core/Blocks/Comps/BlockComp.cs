using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Blocks.Comps;
using Project1.Core.Entities;
using Project1.Core.Graphics;
using Project1.Core.Legacy;
using Project1.Core.Simulation;
using Project1.Core.UI.Hud;
using Project1.Framework;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Project1.Core.Blocks
{

    public abstract class BlockComp : Inspectable, IBlockEntityComp, ISerializable
    {
        public abstract class Spec 
        {
            public abstract Type CompType { get; }
            public abstract BlockComp CreateComp();
        }
        public abstract BlockCompDef CompDef { get; }
        public int RuntimeIndex;
        protected event Action Updated;
        protected void NotifyUpdated() => this.Updated?.Invoke();

        public BlockEntity Parent;
        public MapBase Map => this.Parent.Map;
        public IntVec3 Global => this.Parent.OriginGlobal;
        public override string LabelReadable => this.CompDef.LabelReadable;
        public ObservableCollection<string> Errors => this.Parent.Errors;
        internal virtual void OnSpawned(BlockEntity entity, MapBase map) { }
        internal virtual void OnDespawned(BlockEntity entity, MapBase map) { }
        public virtual void Draw(Camera camera, MapBase map, IntVec3 global) { }
        public virtual void DrawUI(SpriteBatch sb, Camera camera) { }
        public virtual void Load(SaveTag tag)
        {
        }
        public SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.SaveExtra(tag);
            return tag;
        }
        protected virtual void SaveExtra(SaveTag tag)
        {
        }

        public virtual void Tick() { }

        internal virtual void DrawSelected(MySpriteBatch sb, Camera cam, MapBase map, IntVec3 global)
        {
           
        }

        internal virtual void OnDrop(GameObject actor, GameObject item, TargetArgs target, int quantity) { }
        internal virtual void OnRemoved(MapBase map, IntVec3 global, BlockEntity parent) { }
        internal virtual void OnNeighborChanged(MapBase map, IntVec3 source) { }
        [Obsolete]
        internal virtual void GetSelectionInfo(IUISelection info, MapBase map, IntVec3 vector3)
        {
           
        }
        internal virtual void GetSelectionInfo(SelectionManager info, MapBase map, IntVec3 vector3) { }
        internal virtual IEnumerable<(string label, Type type)> GetSelectionTabs() { yield break; }

        public virtual void Write(IDataWriter w)
        {
        }

        public virtual ISerializable Read(IDataReader r)
        {
            return this;
        }

        internal virtual void OnBlockBelowChanged(MapBase map, IntVec3 global)
        {
        }

        internal virtual void ResolveReferences(MapBase map, IntVec3 global)
        {
        }
        internal virtual void ResolveReferences()
        {
        }
        internal virtual void IsMadeFrom(ItemMaterialAmount[] itemDefMaterialAmounts)
        {
        }

        internal virtual void Deconstruct(GameObject actor, IntVec3 global)
        {
        }

        internal virtual void GetQuickButtons(Action<string, Type> register, MapBase map, IntVec3 vector3) { }

        internal virtual void Initialize() { }

        //internal virtual void GetSelectionInfo(Control container) { }
        internal virtual IEnumerable<Control> GetInspectorControls() { yield break; }

        internal virtual bool TryConsume(Entity item) => false;

        internal virtual void OnSwitched(bool on) { }
    }
}

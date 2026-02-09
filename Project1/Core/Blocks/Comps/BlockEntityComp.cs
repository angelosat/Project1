using System;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Base;
using Project1.Core.Interfaces;
using Project1.Core.Legacy;
using Project1.Core.Rendering;
using Project1.Core.Simulation;
using Project1.Core.Entities;
using Project1.Core.UI.Hud;
using Project1.Framework.UI;
using Project1.Framework.Serialization;
using Project1.Framework;

namespace Project1.Core
{

    public abstract class BlockEntityComp : Inspectable, IBlockEntityComp, ISerializable
    {
        public abstract class Spec 
        {
            public abstract Type CompType { get; }
            public abstract BlockEntityComp CreateComp();
        }
        public int RuntimeIndex;
        protected event Action Updated;
        protected void NotifyUpdated() => this.Updated?.Invoke();

        public BlockEntity Parent;
        public MapBase Map => this.Parent.Map;
        public IntVec3 Global => this.Parent.OriginGlobal;
        public override string LabelReadable => this.Name;
        public ObservableCollection<string> Errors => this.Parent.Errors;
        public abstract string Name { get; }
        public virtual void OnSpawned(BlockEntity entity, MapBase map) { }
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
        internal virtual void GetSelectionInfo(IUISelection info, MapBase map, IntVec3 vector3)
        {
           
        }
        internal virtual void GetSelectionInfo(SelectionManager info, MapBase map, IntVec3 vector3) { }

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

        internal virtual void IsMadeFrom(ItemMaterialAmount[] itemDefMaterialAmounts)
        {
        }

        internal virtual void Deconstruct(GameObject actor, IntVec3 global)
        {
        }

        internal virtual void GetQuickButtons(Action<string, Type> register, MapBase map, IntVec3 vector3) { }

        internal virtual void Initialize() { }

        internal virtual void GetSelectionInfo(Control container) { }

        internal virtual bool TryConsume(Entity item) => false;
    }
}

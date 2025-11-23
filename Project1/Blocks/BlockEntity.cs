using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Start_a_Town_
{
    public abstract class BlockEntity : Inspectable, IDisposable, IEntityCompContainer<BlockEntityComp>//, IHasChildren
    {
        public HashSet<IntVec3> CellsOccupied = new();
        public MapBase Map;
        public bool Exists => this.Map is not null;

        //public virtual IEnumerable<IntVec3> InteractionSpots { get { yield break; } }
        public IEnumerable<IntVec3> InteractionSpots => this.Map.GetCell(this.OriginGlobal).GetInteractionSpots(this.OriginGlobal);
        public IEnumerable<IntVec3> ReservedInteractionCells => this.InteractionSpots.SelectMany(ActorDefOf.Npc.OccupyingCellsStanding);

        public IntVec3 OriginGlobal;
        public readonly BlockEntityCompCollectionNew Comps = new();
        public ObservableCollection<string> Errors = new();

        public BlockEntity(IntVec3 originGlobal)
        {
            this.OriginGlobal = originGlobal;
        }

        public virtual void Tick(MapBase map, IntVec3 global)
        {
            foreach (var comp in this.Comps)
                comp.Tick();
        }
        public virtual void GetTooltip(Control tooltip) { }

        /// <summary>
        /// Dipose any children GameObjects here.
        /// </summary>
        public virtual void Dispose() { } // maybe make this abstract so i don't forget it?
        public virtual void OnRemoved(MapBase map, IntVec3 global)
        {
            foreach (var c in this.Comps)
                c.Remove(map, global, this);
        }
        public virtual void Break(MapBase map, IntVec3 global) { }
        public virtual void OnSpawned(MapBase map, IntVec3 global)
        {
            foreach (var comp in this.Comps)
                comp.OnSpawned(this, map, global);
        }

        public virtual GameObjectSlot GetChild(string containerName, int slotID)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Convert to void return and accept the list as an argument so derived objects can add their children and then call the base method so the base class can add its own?
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<GameObject> GetChildren() { yield break; }

        public bool HasComp<T>() where T : class, IBlockEntityComp
        {
            return this.GetComp<T>() != null;
        }
        public BlockEntity AddComp(BlockEntityComp comp)
        {
            comp.Parent = this;
            this.Comps.Add(comp);
            return this;
        }

        internal void OnDrop(GameObject actor, GameObject item, TargetArgs target, int quantity)
        {
            foreach (var comp in this.Comps)
                comp.OnDrop(actor, item, target, quantity);
        }

        internal void IsMadeFrom(ItemMaterialAmount[] itemDefMaterialAmounts)
        {
            foreach (var c in this.Comps)
                c.IsMadeFrom(itemDefMaterialAmounts);
        }

        internal Control GetErrorsGui()
        {
            return new ListBoxObservable<string, Label>(this.Errors, e => new UI.Label(e) { TextColor = Color.OrangeRed, Font = UIManager.FontBold });
        }

        internal virtual void Deconstruct(GameObject actor, IntVec3 global)
        {
            foreach (var c in this.Comps)
                c.Deconstruct(actor, global);
        }

        protected virtual void AddSaveData(SaveTag tag)
        {
        }
        public SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.CellsOccupied.Save(tag, "CellsOccupied");
            this.OriginGlobal.Save(tag, "OriginGlobal");
            tag.Add(this.Comps.Save("Components"));
            this.AddSaveData(tag);
            return tag;
        }
        public void Load(SaveTag tag)
        {
            tag.TryGetTag("Components", this.Comps.Load);
            tag.TryGetTagValue<Vector3>("OriginGlobal", v => this.OriginGlobal = v);
            this.CellsOccupied.Load(tag, "CellsOccupied");
            this.LoadExtra(tag);
        }
        protected virtual void LoadExtra(SaveTag tag) { }
        public T GetComp<T>() where T : class, IBlockEntityComp
        {
            return this.Comps.FirstOrDefault(c => c is T) as T;
        }
        public void Write(BinaryWriter w)
        {
            w.Write(this.OriginGlobal);
            this.CellsOccupied.Write(w);

            foreach (var c in this.Comps)
                c.Write(w);
            this.WriteExtra(w);
        }
        public void Read(BinaryReader r)
        {
            this.OriginGlobal = r.ReadIntVec3();
            this.CellsOccupied.Read(r);
            foreach (var c in this.Comps)
                c.Read(r);
            this.ReadExtra(r);
        }
        protected virtual void WriteExtra(BinaryWriter w) { }
        protected virtual void ReadExtra(BinaryReader r) { }
        internal virtual void HandleRemoteCall(MapBase map, Vector3 vector3, ObjectEventArgs e) { }

        public void Draw(Camera camera, MapBase map, IntVec3 global)
        {
            foreach (var comp in this.Comps)
                comp.Draw(camera, map, global);
        }
        public void DrawUI(SpriteBatch sb, Camera cam, IntVec3 global)
        {
            foreach (var comp in this.Comps)
                comp.DrawUI(sb, cam);
            if (this.Errors.Any())
                Icon.Cross.DrawFloating(sb, cam, this.OriginGlobal);
            this.OnDrawUI(sb, cam, global);
        }
        protected virtual void OnDrawUI(SpriteBatch sb, Camera cam, IntVec3 global) { }
        internal virtual void GetQuickButtons(SelectionManager uISelectedInfo, MapBase map, IntVec3 vector3)
        {
            foreach (var c in this.Comps)
                c.GetQuickButtons(uISelectedInfo, map, vector3);
        }
        internal virtual void GetQuickButtons(SelectionManagerNew uISelectedInfo, MapBase map, IntVec3 vector3)
        {
            foreach (var c in this.Comps)
                c.GetQuickButtons(uISelectedInfo, map, vector3);
        }
        internal virtual void GetSelectionInfo(IUISelection info, MapBase map, IntVec3 vector3)
        {
            foreach (var c in this.Comps)
                c.GetSelectionInfo(info, map, vector3);
        }
        internal virtual void GetSelectionInfo(SelectionManagerNew info, MapBase map, IntVec3 vector3)
        {
            foreach (var c in this.Comps)
                c.GetSelectionInfo(info, map, vector3);
        }
        internal void OnBlockBelowChanged(MapBase map, IntVec3 global)
        {
            foreach (var c in this.Comps)
                c.OnBlockBelowChanged(map, global);
        }

        internal void ResolveReferences(MapBase map, IntVec3 global)
        {
            foreach (var c in this.Comps)
                c.ResolveReferences(map, global);
            this.OnMapLoaded(map, global);
        }

        protected virtual void OnMapLoaded(MapBase map, IntVec3 global)
        {
        }

        internal void DrawSelected(MySpriteBatch sb, Camera cam, MapBase map, IntVec3 global)
        {
            foreach (var c in this.Comps)
                c.DrawSelected(sb, cam, map, global);
        }

        internal void NeighborChanged()
        {
            foreach (var comp in this.Comps)
                comp.NeighborChanged();
        }
        
    }
}

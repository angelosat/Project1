using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Core.Blocks;
using Project1.Core.Base;
using Project1.Core.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Project1.Core.Rendering;
using Project1.Core.Entities.Actors;
using Project1.Core.Interfaces;
using Project1.Core.UI;
using Project1.Core.Legacy;
using Project1.Core.Helpers;
using Project1.Core.Simulation;
using Project1.Core.Entities;

namespace Project1.Core
{
    public class BlockEntity : Inspectable, IDisposable, ISerializableNew<BlockEntity>, ISaveableNewNew<BlockEntity>//, IEntityCompContainer<BlockEntityComp>//, IHasChildren
    {
        public string Name = nameof(BlockEntity);
        public HashSet<IntVec3> CellsOccupied = [];
        public MapBase Map;
        public BlockDef Def { get; private set; }
        public bool Exists => this.Map is not null;

        //public virtual IEnumerable<IntVec3> InteractionSpots { get { yield break; } }
        public IEnumerable<IntVec3> InteractionSpots => this.Map.GetCell(this.OriginGlobal).GetInteractionSpots(this.Map, this.OriginGlobal);
        public IEnumerable<IntVec3> ReservedInteractionCells => this.InteractionSpots.SelectMany(ActorDefOf.Npc.OccupyingCellsStanding);

        public IntVec3 OriginGlobal;
        //public readonly BlockEntityCompCollectionNew Comps = new();
        public readonly BlockEntityCompCollection Comps;// = new();
        public ObservableCollection<string> Errors = new();
    
        public BlockEntity(BlockDef def, IntVec3 originGlobal)
        {
            this.Comps = new(this);
            this.OriginGlobal = originGlobal;
            this.CellsOccupied.Add(originGlobal);
            this.Def = def;
        }
        public BlockEntity(BlockDef def)
        {
            this.Comps = new(this);
            this.Def = def;
        }
        public BlockEntity SetFootprint(IEnumerable<IntVec3> cells)
        {
            foreach (var cell in cells)
                this.CellsOccupied.Add(cell);
            this.OriginGlobal = cells.First();
            return this;
        }
        public virtual void Tick(MapBase map, IntVec3 global)
        {
            foreach (var c in this.Comps.Values)
                c.Tick();
        }
        public virtual void GetTooltip(Control tooltip) { }

        /// <summary>
        /// Dipose any children GameObjects here.
        /// </summary>
        public virtual void Dispose() { } // maybe make this abstract so i don't forget it?
        public virtual void OnRemoved(MapBase map, IntVec3 global)
        {
            foreach (var c in this.Comps.Values)
                c.OnRemoved(map, global, this);
        }
        public virtual void Break(MapBase map, IntVec3 global) { }
        public virtual void OnSpawned(MapBase map, IntVec3 global)
        {
            foreach (var comp in this.Comps.Values)
                comp.OnSpawned(this, map);
        }
        public virtual void OnSpawned(MapBase map)
        {
            foreach (var comp in this.Comps.Values)
                comp.OnSpawned(this, map);
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

        public bool HasComp<T>() where T : BlockEntityComp// class, IBlockEntityComp
        {
            //return this.Comps.GetComp<T>() != null;
            return this.Comps.TryGetComp<T>(out _);
        }
        public BlockEntity AddComp(BlockEntityComp comp)
        {
            comp.Parent = this;
            this.Comps.AddComp(comp);
            return this;
        }
        public T GetComp<T>() where T : BlockEntityComp// class, IBlockEntityComp
        {
            return this.Comps.GetComp<T>();//.FirstOrDefault(c => c is T) as T;
        }
        internal void OnDrop(GameObject actor, GameObject item, TargetArgs target, int quantity)
        {
            foreach (var comp in this.Comps.Values)
                comp.OnDrop(actor, item, target, quantity);
        }

        internal void IsMadeFrom(ItemMaterialAmount[] itemDefMaterialAmounts)
        {
            foreach (var c in this.Comps.Values)
                c.IsMadeFrom(itemDefMaterialAmounts);
        }

        internal Control GetErrorsGui()
        {
            return new ListBoxObservable<string, Label>(this.Errors, e => new Label(e) { TextColor = Color.OrangeRed, Font = UIManager.FontBold });
        }

        internal virtual void Deconstruct(GameObject actor, IntVec3 global)
        {
            foreach (var c in this.Comps.Values)
                c.Deconstruct(actor, global);
        }
        
        protected virtual void AddSaveData(SaveTag tag)
        {
        }
        public SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Save("Def", this.Def);
            tag.Save("CellsOccupied", this.CellsOccupied);
            tag.Save("OriginGlobal", this.OriginGlobal);
            tag.Add(this.Comps.Save("Components"));
            this.AddSaveData(tag);
            return tag;
        }
        public void Load(SaveTag tag)
        {
            tag.TryGetTag("Components", this.Comps.Load);
            //tag.TryGetTagValue<IntVec3>("OriginGlobal", v => this.OriginGlobal = v);
            this.CellsOccupied.Load(tag, "CellsOccupied");
            this.LoadExtra(tag);
        }
        public static BlockEntity Create(SaveTag tag)
        {
            var def = tag.LoadDef<BlockDef>("Def");
            var global = tag.LoadIntVec3("OriginGlobal");
            var entity = def.CreateEntity(global);
            entity.Load(tag);
            return entity;
        }
        protected virtual void LoadExtra(SaveTag tag) { }
        
        public void Write(IDataWriter w)
        {
            w.Write(this.Def);
            w.Write(this.OriginGlobal);
            this.CellsOccupied.Write(w);

            foreach (var c in this.Comps.Values)
                c.Write(w);
            this.WriteExtra(w);
        }
        public BlockEntity Read(IDataReader r)
        {
            //this.Def = r.ReadDef<BlockDef>();
            //this.OriginGlobal = r.ReadIntVec3();
            this.CellsOccupied.Read(r);
            foreach (var c in this.Comps.Values)
                c.Read(r);
            this.ReadExtra(r);
            return this;
        }

        public static BlockEntity Create(IDataReader r)
        {
            var def = r.ReadDef<BlockDef>();
            var global = r.ReadIntVec3();
            var entity = def.CreateEntity(global);
            entity.Read(r);
            return entity;
        }
        protected virtual void WriteExtra(IDataWriter w) { }
        protected virtual void ReadExtra(IDataReader r) { }
        internal virtual void HandleRemoteCall(MapBase map, Vector3 vector3, ObjectEventArgs e) { }

        public void Draw(Camera camera, MapBase map, IntVec3 global)
        {
            foreach (var comp in this.Comps.Values)
                comp.Draw(camera, map, global);
        }
        public void DrawUI(SpriteBatch sb, Camera cam, IntVec3 global)
        {
            foreach (var comp in this.Comps.Values)
                comp.DrawUI(sb, cam);
            if (this.Errors.Any())
                Icon.Cross.DrawFloating(sb, cam, this.OriginGlobal);
            this.OnDrawUI(sb, cam, global);
        }
        protected virtual void OnDrawUI(SpriteBatch sb, Camera cam, IntVec3 global) { }
       
        internal virtual void GetQuickButtons(Action<string, Type> register, MapBase map, IntVec3 vector3)
        {
            foreach (var c in this.Comps.Values)
                c.GetQuickButtons(register, map, vector3);
        }
        internal virtual void GetSelectionInfo(Control container)
        {
            foreach (var c in this.Comps.Values)
                c.GetSelectionInfo(container);
        }
        internal virtual void GetSelectionInfo(IUISelection info, MapBase map, IntVec3 vector3)
        {
            foreach (var c in this.Comps.Values)
                c.GetSelectionInfo(info, map, vector3);
        }
        internal virtual void GetSelectionInfo(SelectionManager info, MapBase map, IntVec3 vector3)
        {
            foreach (var c in this.Comps.Values)
                c.GetSelectionInfo(info, map, vector3);
        }
        internal void OnBlockBelowChanged(MapBase map, IntVec3 global)
        {
            foreach (var c in this.Comps.Values)
                c.OnBlockBelowChanged(map, global);
        }

        internal void ResolveReferences(MapBase map, IntVec3 global)
        {
            foreach (var c in this.Comps.Values)
                c.ResolveReferences(map, global);
            this.OnMapLoaded(map, global);
        }

        protected virtual void OnMapLoaded(MapBase map, IntVec3 global)
        {
        }

        internal void DrawSelected(MySpriteBatch sb, Camera cam, MapBase map, IntVec3 global)
        {
            foreach (var c in this.Comps.Values)
                c.DrawSelected(sb, cam, map, global);
        }

        internal void OnNeighborChanged(MapBase map, IntVec3 source)
        {
            foreach (var comp in this.Comps.Values)
                comp.OnNeighborChanged(map, source);
        }
        bool _initialized;
        internal void Initialize()
        {
            if (this._initialized)
                throw new Exception();
            foreach (var c in this.Comps.Values)
                c.Initialize();
        }

        internal bool TryConsume(Entity item)
        {
            bool consumed = false;
            foreach (var c in this.Comps.Values)
                consumed |= c.TryConsume(item);
            return consumed;
                
        }

        internal void Attach(IntVec3 global)
        {
            //this.CellsOccupied.Add(global);
            this.Map.AttachCellToEntity(global, this);
        }
        
    }
}

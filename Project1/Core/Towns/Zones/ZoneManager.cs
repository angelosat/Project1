using System;
using System.Collections.Generic;
using System.Linq;
using Project1.Core.Base;
using Project1.Core.Rendering;
using Project1.Core.Input.Tools;
using Project1.Core.WorldGen;
using Project1.Core.UI;
using Project1.Core.Screens;
using Project1.Core.Helpers;
using Project1.Core.Helpers.Collections;
using Project1.Core.Helpers.Structs;
using Project1.Core.Simulation;
using Project1.Core.Simulation.Physics;
using Project1.Core.Entities;
using Project1.Framework.UI;
using Project1.Core.Input;
using Project1.Framework.IO;
using Project1.Framework.Math;

namespace Project1.Core.Towns.Zones
{
    [EnsureStaticCtorCall]
    public class ZoneManager : TownComponent
    {
        public override string Name => "ZoneManager";
        int _zoneIDSequence = 1;
        public int GetNextID() => _zoneIDSequence++;
        readonly public ObservableDictionary<int, Zone> Zones = [];
        public IEnumerable<Zone> AllZones => this.Zones.Values;
        readonly Dictionary<IntVec3, Zone> _cellsToZones = [];
        public IReadOnlyDictionary<IntVec3, Zone> CellsToZones => this._cellsToZones;

        static ZoneManager()
        {
            Hotkey = HotkeyManager.RegisterHotkey(ToolManagement.HotkeyContextManagement, "Zones", ToggleGui, System.Windows.Forms.Keys.Y);
        }
        public ZoneManager(Town town)
        {
            this.Town = town;
            var map = town.Map;

            map.Events.ListenTo<EntitySpawnedEvent>(OnEntitySpawned);
            map.Events.ListenTo<EntityDespawnedEvent>(OnEntityDespawned);
            map.Events.ListenTo<EntityAtRestEvent>(OnEntityAtRest);

            map.Events.ListenTo<CellsInvalidatedEvent>(OnCellsInvalidated);
        }

       

        private void OnEntitySpawned(EntitySpawnedEvent e)
        {
            var entity = e.Entity;
            var supportCell = entity.Cell.Below;
            if (!this._cellsToZones.TryGetValue(supportCell, out var zone))
                return;
            //if (!stockpile.Accepts(e.Entity))
            //    return;
            this.AddItem(zone, entity);

        }
        private void OnEntityDespawned(EntityDespawnedEvent e)
        {
            var supportCell = e.Entity.Cell.Below;
            if (!this._cellsToZones.TryGetValue(supportCell, out var zone))
                return;
            this.RemoveItem(zone, e.Entity);
        }
        private void OnEntityAtRest(EntityAtRestEvent e)
        {
            var cell = e.Entity.Cell;
            if (this._cellsToZones.TryGetValue(cell, out var zone))
            {
                if (e.AtRest)
                    this.AddItem(zone, e.Entity);
                else
                    this.RemoveItem(zone, e.Entity);
            }
        }
        internal Zone RegisterNewZone(ZoneDef zoneType, IEnumerable<IntVec3> allpositions)
        {
            var finalPositions = allpositions.Where(
                po => this.Town.GetZoneAt(po) == null &&
                zoneType.Worker.IsValidLocation(this.Town.Map, po));
            if (!finalPositions.Any())
                return null;
            if (!finalPositions.IsConnectedNew())
                return null;
            var zone = zoneType.Create(this, finalPositions);
            this.AddZone(zone);
            return zone;
        }
        internal void Delete(Zone zone)
        {
            this.DeleteZone(zone.ID);
        }
        internal void DeleteZone(int zoneID)
        {
            if (!this.Zones.TryGetValue(zoneID, out var zone))
                throw new Exception();
            foreach (var position in zone.Cells)
                this._cellsToZones.Remove(position);
            this.Zones.Remove(zoneID);
            FloatingText.Create(this.Map, zone.Average(), $"{zone.GetType()} deleted", ft => ft.Font = UIManager.FontBold);
            this.Map.Events.Post(new ZoneDeletedEvent(zone));
        }
        void AddZone(Zone zone)
        {
            if (zone.ID == 0)
                zone.ID = this.GetNextID();
            this.Zones.Add(zone.ID, zone);
            foreach (var position in zone.Cells)
                this._cellsToZones[position] = zone;
            zone.Manager = this;
            zone.Name = zone.UniqueName;
            
            FloatingText.Create(this.Town.Map, zone.Average(), $"{zone.GetType()} created", ft => ft.Font = UIManager.FontBold);
            this.Map.Events.Post(new ZoneCreatedEvent(zone));
        }
        internal override void ResolveReferences()
        {
            foreach (var z in this.AllZones)
                foreach (var cell in z.Cells)
                    this._cellsToZones[cell] = z;

            foreach (var entity in this.Map.Entities)
            {
                if (!this.CellsToZones.TryGetValue(entity.Cell.Below, out var zone))
                    continue;
                //this.AddItem(zone, entity);
                zone.CacheNew.Add(entity);
            }
        }
        internal void AddItem(Zone zone, Entity entity)
        {
            zone.CacheNew.Add(entity);
            this.Map.Events.Post(new EntityEnteredZoneEvent(entity, zone));
        }
        internal void RemoveItem(Zone zone, Entity entity)
        {
            zone.CacheNew.Remove(entity);
            this.Map.Events.Post(new EntityExitedZoneEvent(entity, zone));
        }
        internal T GetZone<T>(ZoneId zoneID) where T : Zone
        {
            if (zoneID == ZoneId.Null)
                return null;
            return this.Zones[zoneID] as T;
        }

        public Zone GetZoneAt(IntVec3 global)
        {
            //return this.Zones.Values.FirstOrDefault(z => z.Contains(global));
            if (this.CellsToZones.TryGetValue(global, out var zone))
                return zone;
            return null;
        }
        public T GetZoneAt<T>(IntVec3 global) where T : Zone
        {
            //return this.Zones.Values.FirstOrDefault(z => z.Contains(global)) as T;
            return this.GetZoneAt(global) as T;
        }
        public IEnumerable<T> GetZones<T>() where T : Zone
        {
            return this.Zones.Values.OfType<T>();
        }
        public IEnumerable<Zone> GetZones()
        {
            foreach (var z in this.Zones.Values)
                yield return z;
        }
        internal override void OnBlocksChanged(IEnumerable<IntVec3> positions)
        {
            for (int i = this.Zones.Count - 1; i >= 0; i--)
            {
                var item = this.Zones.ElementAt(i);
                foreach (var pos in positions)
                {
                    item.Value.OnBlockChangedNew(pos);
                    item.Value.OnBlockChangedNew(pos.Below);
                }
            }
        }
        private void OnCellsInvalidated(CellsInvalidatedEvent e)
        {
            foreach(var cell in e.Positions)
            {
                if (this.GetZoneAt(cell) is Zone zone)
                    zone.OnBlockChangedNew(cell);
                // for handling the case of an empty zone cell being now obstructed by a solid cell
                else if (this.GetZoneAt(cell.Below) is Zone zoneBelow)
                    zoneBelow.OnBlockChangedNew(cell.Below);
            }
        }
        internal Zone PlayerEdit(int zoneID, ZoneDef zoneType, IntVec3 a, int w, int h, bool remove)
        {
            if (remove)
                foreach (var zone in this.Zones.Values.ToList())
                    zone.Edit(a, a + new IntVec3(w - 1, h - 1, 0), remove);
            else
                if (zoneID == 0)
                    return RegisterNewZone(zoneType, a.GetBoxLazy(a + new IntVec3(w - 1, h - 1, 0)));
            else
                this.Zones[zoneID].Edit(a, a + new IntVec3(w - 1, h - 1, 0), remove);
            return null;
        }
        static readonly ZoneDef[] ZoneDefs = { ZoneDefOf.Stockpile, ZoneDefOf.Growing };

        internal override IEnumerable<Tuple<Func<string>, Action>> OnQuickMenuCreated()
        {
            yield return new Tuple<Func<string>, Action>(() => $"Zones [{Hotkey.GetLabel()}]", ToggleGui);
        }
        static Window _gui;
        static Lazy<Control> _guiNew = new(() => ContextMenuManager.CreateContextSubMenu("Zones", GetContextSubmenuItems()));
        private static readonly IHotkey Hotkey;

        public static void ToggleGui()
        {
            _guiNew.Value.Toggle();
        }
        static IEnumerable<(string, Action)> GetContextSubmenuItems()
        {
            foreach (var def in ZoneDefs)
                yield return (def.LabelReadable, () => Zone.Edit(Ingame.CurrentMap.Town, def));
        }
        public override ISelectable QuerySelectable(TargetArgs target)
        {
            var global = target.Global;
            return this.GetZoneAt(global);
        }
        public override void DrawBeforeWorld(MySpriteBatch sb, MapBase map, Camera cam)
        {
            if (!cam.DrawZones)
                return;
            foreach (var s in this.Zones.Values)
                s.DrawBeforeWorld(sb, map, cam);
        }
        internal override void OnCameraRotated(Camera camera)
        {
            foreach (var z in this.Zones.Values)
                z.OnCameraRotated(camera);
        }
        protected override void AddSaveData(SaveTag tag)
        {
            this._zoneIDSequence.Save(tag, "IDSequence");
            this.Zones.Values.SaveAbstract(tag, "Zones");
        }
        public override void Load(SaveTag tag)
        {
            tag.TryGetTagValue("IDSequence", ref this._zoneIDSequence);
            this.Zones.TryLoadByValueAbstractTypes(tag, "Zones", zone => zone.ID, this);
        }
        public override void Write(IDataWriter w)
        {
            w.Write(this._zoneIDSequence);
            this.Zones.Values.WriteAbstract(w);
        }
        public override void Read(IDataReader r)
        {
            this._zoneIDSequence = r.ReadInt32();
            this.Zones.ReadByValueAbstractTypes(r, zone => zone.ID, this);
        }
    }
}

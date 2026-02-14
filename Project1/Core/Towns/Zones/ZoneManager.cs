using Project1.Core.Entities;
using Project1.Core.Graphics;
using Project1.Core.Helpers.Structs;
using Project1.Core.Input;
using Project1.Core.Screens;
using Project1.Core.Simulation;
using Project1.Core.Simulation.Physics;
using Project1.Core.UI;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Input;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Towns.Zones
{
    [EnsureStaticCtorCall]
    public class ZoneManager : TownComponent
    {
        static readonly ZoneDef[] ZoneDefs;// = [ZoneDefOf.Stockpile, ZoneDefOf.Growing];
        public override string Name => "ZoneManager";
        int _zoneIDSequence = 1;
        public int GetNextID() => _zoneIDSequence++;
        readonly public ObservableDictionary<int, Zone> ZonesById = [];
        public IEnumerable<Zone> AllZones => this.ZonesById.Values;
        readonly Dictionary<IntVec3, Zone> _cellsToZones = [];
        public IReadOnlyDictionary<IntVec3, Zone> CellsToZones => this._cellsToZones;
        static ZoneManager()
        {
            Hotkey = HotkeyManager.RegisterHotkey(ToolManagement.HotkeyContextManagement, "Zones", ToggleGui, System.Windows.Forms.Keys.Y);
            ZoneDefs = [.. Def.GetDefs<ZoneDef>()];
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
            this.AddItem(zone, entity);
            zone.MarkDirty();
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
        internal void DeleteZone(int zoneID)
        {
            if (!this.ZonesById.TryGetValue(zoneID, out var zone))
                throw new Exception();
            foreach (var position in zone.Cells)
                this._cellsToZones.Remove(position);
            this.ZonesById.Remove(zoneID);
            this.Map.Events.Post(new ZoneDeletedEvent(zone));
        }
        void AddZone(Zone zone)
        {
            if (zone.ID == 0)
                zone.ID = this.GetNextID();
            this.ZonesById.Add(zone.ID, zone);
            foreach (var position in zone.Cells)
                this._cellsToZones[position] = zone;
            zone.Manager = this;
            zone.Name = zone.UniqueName;
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
            return this.ZonesById[zoneID] as T;
        }
        public Zone GetZoneAt(IntVec3 global)
        {
            if (this.CellsToZones.TryGetValue(global, out var zone))
                return zone;
            return null;
        }
        public T GetZoneAt<T>(IntVec3 global) where T : Zone
        {
            return this.GetZoneAt(global) as T;
        }
        public IEnumerable<T> GetZones<T>() where T : Zone
        {
            return this.ZonesById.Values.OfType<T>();
        }
        internal override void OnBlocksChanged(IEnumerable<IntVec3> positions)
        {
            for (int i = this.ZonesById.Count - 1; i >= 0; i--)
            {
                var item = this.ZonesById.ElementAt(i);
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
                foreach (var zone in this.ZonesById.Values.ToList())
                    this.EditZone(a, w, h, remove, zone);
            else if (zoneID == 0)
                    return RegisterNewZone(zoneType, a.GetBoxLazy(a + new IntVec3(w - 1, h - 1, 0)));
            else
                this.EditZone(a, a + new IntVec3(w - 1, h - 1, 0), remove, this.ZonesById[zoneID]);
            return null;
        }
        private void EditZone(IntVec3 a, int w, int h, bool remove, Zone zone)
        {
            EditZone(a, a + new IntVec3(w - 1, h - 1, 0), remove, zone);
        }
        private void EditZone(IntVec3 a, IntVec3 b, bool remove, Zone zone)
        {
            zone.Edit(a, b, remove);
            if (zone.IsEmpty)
                this.DeleteZone(zone.ID);
        }
        internal override IEnumerable<Tuple<Func<string>, Action>> OnQuickMenuCreated()
        {
            yield return new Tuple<Func<string>, Action>(() => $"Zones [{Hotkey.GetLabel()}]", ToggleGui);
        }
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
            foreach (var s in this.ZonesById.Values)
                s.DrawBeforeWorld(sb, map, cam);
        }
        internal override void OnCameraRotated(Camera camera)
        {
            foreach (var z in this.ZonesById.Values)
                z.OnCameraRotated(camera);
        }
        protected override void AddSaveData(SaveTag tag)
        {
            this._zoneIDSequence.Save(tag, "IDSequence");
            this.ZonesById.Values.SaveNewBEST(tag, "Zones");
        }
        public override void Load(SaveTag tag)
        {
            tag.TryGetTagValue("IDSequence", ref this._zoneIDSequence);
            var savedZones = tag.LoadList<Zone>("Zones").ToDictionary(z => z.ID, z => z);
            foreach (var (id, z) in savedZones)
            {
                z.Manager = this;
                this.ZonesById.Add(id, z);
            }
        }
        public override void Write(IDataWriter w)
        {
            w.Write(this._zoneIDSequence);
            w.Write(this.ZonesById.Values);
        }
        public override void Read(IDataReader r)
        {
            this._zoneIDSequence = r.ReadInt32();
            var zoneList = r.ReadList<Zone>();
            foreach (var zone in zoneList)
            {
                zone.Manager = this;
                this.ZonesById.Add(zone.ID, zone);
            }
        }
    }
}
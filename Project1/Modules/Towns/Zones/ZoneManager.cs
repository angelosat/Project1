using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    public class ZoneManager : TownComponent
    {
        public override string Name => "ZoneManager";
        int _zoneIDSequence = 1;
        public int GetNextID() => _zoneIDSequence++;
        readonly public ObservableDictionary<int, Zone> Zones = [];
        public IEnumerable<Zone> AllZones => this.Zones.Values;
        static ZoneManager()
        {
            Hotkey = HotkeyManager.RegisterHotkey(ToolManagement.HotkeyContextManagement, "Zones", ToggleGui, System.Windows.Forms.Keys.Y);
        }
        public ZoneManager(Town town)
        {
            this.Town = town;
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
            this.RegisterZone(zone);
            return zone;
        }
        internal void Delete(Zone zone)
        {
            this.Delete(zone.ID);
        }
        internal void Delete(int zoneID)
        {
            if (!this.Zones.TryGetValue(zoneID, out var zone))
                throw new Exception();
            this.Zones.Remove(zoneID);
            FloatingText.Create(this.Map, zone.Average(), $"{zone.GetType()} deleted", ft => ft.Font = UIManager.FontBold);
        }
        void RegisterZone(Zone zone)
        {
            if (zone.ID == 0)
                zone.ID = this.GetNextID();
            this.Zones.Add(zone.ID, zone);
            zone.Manager = this;
            zone.Name = zone.UniqueName;
            FloatingText.Create(this.Town.Map, zone.Average(), $"{zone.GetType()} created", ft => ft.Font = UIManager.FontBold);
        }

        internal T GetZone<T>(int zoneID) where T : Zone
        {
            return this.Zones[zoneID] as T;
        }

        public Zone GetZoneAt(IntVec3 global)
        {
            return this.Zones.Values.FirstOrDefault(z => z.Contains(global));
        }
        public T GetZoneAt<T>(IntVec3 global) where T : Zone
        {
            return this.Zones.Values.FirstOrDefault(z => z.Contains(global)) as T;
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
                yield return (def.Label, () => Zone.Edit(Ingame.CurrentMap.Town, def));
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
            tag.TryGetTagValueNew("IDSequence", ref this._zoneIDSequence);
            this.Zones.TryLoadByValueAbstractTypes(tag, "Zones", zone => zone.ID, this);
        }
        public override void Write(BinaryWriter w)
        {
            w.Write(this._zoneIDSequence);
            this.Zones.Values.WriteAbstract(w);
        }
        public override void Read(BinaryReader r)
        {
            this._zoneIDSequence = r.ReadInt32();
            this.Zones.ReadByValueAbstractTypes(r, zone => zone.ID, this);
        }
    }
}

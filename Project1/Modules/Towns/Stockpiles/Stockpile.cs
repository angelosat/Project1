using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Start_a_Town_
{
    public partial class Stockpile : Zone, IStorageNew, IContextable
    {
        public override ZoneDef ZoneDef => ZoneDefOf.Stockpile;
        public StorageSettings Settings { get; } = new();
        public int Priority => (int)this.Settings.Priority;
        static StorageFilterCategoryNewNew FiltersView = InitFilters();
        readonly StockpileSettings SettingsNew;
        
        public override string UniqueName => $"Zone_Stockpile_{this.ID}";
        readonly List<GameObject> Cache = [];
        public readonly ObservableCollection<GameObject> CacheObservable = [];
        public HashSet<Entity> AcceptedItems = [];
        public Stockpile(ZoneManager manager) : base(manager)
        {
            this.SettingsNew = new(this);
            this.Settings.Initialize(FiltersView);
        }
        public Stockpile(ZoneManager manager, Action<Stockpile> preset) : this(manager)
        {
            preset(this);
        }
        public void CacheContents()
        {
            this.CacheContentsNew();
        }
        public void CacheContentsNew()
        {
            this.Cache.Clear();
            foreach (var pos in this.Cells)
                this.Cache.AddRange(this.Map.GetObjects(pos.Above).Where(i => i.IsStockpilable()));

            foreach(var item in this.CacheObservable.ToArray())
                if (!item.IsSpawned || this.Cells.Contains(item.Cell.Below))
                    this.CacheObservable.Remove(item);
            foreach (var pos in this.Cells)
                foreach (var item in this.Map.GetObjects(pos.Above).Where(i => i.IsStockpilable()))
                    this.CacheObservable.Add(item);
        }
        public IEnumerable<GameObject> Contents => this.Cache;

        public IEnumerable<GameObject> GetContentsNew()
        {
            foreach (var i in this.Cache)
                yield return i;
        }
        public List<GameObject> GetContents()
        {
            var contents = new List<GameObject>();
            foreach (var pos in this.Cells)
                contents.AddRange(this.Town.Map.GetObjects(pos + IntVec3.UnitZ));
            return contents;
        }


        public override bool Accepts(Entity obj, IntVec3 pos)
        {
            if (!this.Cells.Contains(pos))
                return false;
            return this.Accepts(obj);
        }
       
        internal bool Accepts(Entity item) => this.SettingsNew.Accepts(item);
        //{
        //    return obj is Entity item && this.Settings.Accepts(item);
        //}
        //public bool CanAccept(GameObject item)
        //{
        //    return this.Accepts(item) && this.GetAvailableCells().Any();
        //}

        public IEnumerable<IntVec3> GetAvailableCells()
        {
            var emptyCells =
                this.Cells
                .Where(p => this.Town.ReservationManager.CanReserve(p.Above))
                .Where(p => !this.Town.Map.GetObjects(p.Above).Any())
                .Select(p => p.Above);
            return emptyCells;
        }

        public IEnumerable<TargetArgs> DistributeToStorageSpotsNewLazy(GameObject obj)
        {
            var emptyCells = new List<Vector3>();
            foreach (var pos in this.Cells)
            {
                var above = pos.Above;
                var itemsInCell = this.Map.GetObjects(above);
                if (!itemsInCell.Any())
                {
                    emptyCells.Add(above);
                    continue;
                }
                foreach (var existing in itemsInCell)
                {
                    if (!existing.IsHaulable) // not really necessary?
                        continue;
                    if (!this.SettingsNew.Accepts(existing as Entity))
                        continue;
                    if (!existing.CanAbsorb(obj))
                        continue;
                    /// dont haul to this existing item, because it might be reserved by another actor to be completely picked up
                    /// maybe if unreservedamount > 1? so that we ensure that the stack will continue existing even after being partially picked up by another actor?
                    if (existing.GetUnreservedAmount() == 0)
                        continue; 
                    yield return new TargetArgs(existing);
                }
            }
            foreach (var cell in emptyCells)
            {
                yield return new TargetArgs(this.Map, cell);
            }
        }
        public IEnumerable<IntVec3> DistributeToStorageSpotsCellsOnly(Entity item)
        {
            var emptyCells = new List<Vector3>();
            foreach (var pos in this.Cells)
            {
                var above = pos.Above;
                var itemsInCell = this.Map.GetEntitiesAt(above);
                if (!itemsInCell.Any())
                {
                    emptyCells.Add(above);
                    continue;
                }
                foreach (var existing in itemsInCell)
                {
                    if (!existing.IsHaulable) // not really necessary?
                        continue;
                    if (!this.SettingsNew.Accepts(existing as Entity))
                        continue;
                    if (!existing.CanAbsorb(item))
                        continue;
                    yield return pos;
                }
            }
            foreach (var cell in emptyCells)
                yield return cell;
        }
        public TargetArgs FindPlaceFor(Entity item)
        {
            if (!this.Accepts(item))
                return null;
            return this.DistributeToStorageSpotsNewLazy(item).FirstOrDefault();
        }
        public IEnumerable<IntVec3> FindPlacesFor(Entity item)
        {
            if (!this.Accepts(item))
                return null;
            return this.DistributeToStorageSpotsCellsOnly(item);
        }
        internal int AvailableCapacityFor(Entity item)
        {
            if (!this.Accepts(item))
                return 0;
            var availableCapacity = 0;
            foreach(var cell in this.Cells)
            {
                var cellEntities = this.Map.GetEntitiesAt(cell);
                if (!cellEntities.Any())
                    return item.StackMax;
                foreach (var entity in cellEntities)
                    if (entity.CanAbsorb(item))
                    {
                        availableCapacity += entity.StackMax - entity.StackSize;
                        if (availableCapacity >= entity.StackMax)
                            return entity.StackMax;
                    }
            }
            return availableCapacity;
        }

        internal override void OnBlockChanged(IntVec3 global)
        {
            var below = global.Below;

            if (this.Cells.Contains(global))
            {
                if (!Block.IsBlockSolid(this.Town.Map, global))
                {
                    this.RemovePosition(global);
                    return;
                }
            }
            else if (this.Cells.Contains(below))
            {
                if (!this.Map.IsAir(global))
                {
                    this.RemovePosition(below);
                    return;
                }
            }
        }
      
        public void GetContextActions(GameObject playerEntity, ContextArgs a)
        {
        }

        public bool IsValidStorage(Entity item, TargetArgs target, int amount)
        {
            if (!this.Accepts(item))
                return false;
            var itemsOnCell = this.Town.Map.GetObjects(target.Global);

            switch (target.Type)
            {
                case TargetType.Position:

                    break;

                case TargetType.Entity:
                    if (!itemsOnCell.Contains(target.Object))
                        throw new Exception();
                    if (target.Object.StackSize + amount > target.Object.StackMax)
                        throw new Exception();
                    break;

                default:
                    break;
            }

            return true;
        }

        public IEnumerable<Vector3> GetPositionsLazy()
        {
            foreach (var pos in this.Cells)
                yield return pos;
        }
        public override IEnumerable<(string name, Action action)> GetInfoTabs()
        {
            yield return ("Filters", () =>
            {
                var win = new StockpileFiltersGui(this).ToWindow("Filters");
                win.ToggleSmart();
            }
            );
        }
        public override void GetSelectionInfo(IUISelection panel)
        {
            panel.AddTabAction("Stockpile", this.ToggleFiltersUI);

        }
        static Window WindowFilters;
        static Control FiltersGui;
        private void ToggleFiltersUI()
        {
            FiltersView.SetOwner(this);

            // TODO: update controls when selecting another stockpile
            if (WindowFilters is not null && WindowFilters.Tag != this && WindowFilters.IsOpen)
            {
                WindowFilters.Client.ClearControls();
                WindowFilters.Client.AddControls(GetGUI());
                WindowFilters.SetTitle($"{this.UniqueName} settings");
                WindowFilters.SetTag(this);
                return;
            }
            if (WindowFilters is null)
            {
                WindowFilters =
                    GetGUI()
                    .ToWindow("Stockpile settings");
            }
            WindowFilters.SetTitle($"{this.UniqueName} settings");
            WindowFilters.SetTag(this);
            WindowFilters.Toggle();

            WindowFilters.SetOnSelectedTargetChangedAction(t =>
            {
                if (this.Town.ZoneManager.GetZoneAt<Stockpile>(t.Global) is Stockpile newStockpile)
                {
                    WindowFilters.SetTitle($"{newStockpile.UniqueName} settings");
                    WindowFilters.SetTag(newStockpile);
                    FiltersView.SetOwner(newStockpile);
                }
            });
        }
        static Control GetGUI()
        {
            if (FiltersGui is not null)
                return FiltersGui;

            var box = new GroupBox();
            box.AddControlsVertically(
                new GroupBox()
                    .AddControlsHorizontally(new ComboBoxNewNew<StoragePriority>(Enum.GetValues(typeof(StoragePriority)).Cast<StoragePriority>(), 128, p => $"Priority: {p}", p => $"{p}", syncPriority, () => FiltersView.Owner.Settings.Priority)),
                FiltersView
                    .GetGui()
                    .ToPanelLabeled("Fitlers"));
            
            FiltersGui = box;
            return box;

            void syncPriority(StoragePriority p)
            {
                Packets.SyncPriority(FiltersView.Owner, p);
            }
        }
        
        static StorageFilterCategoryNewNew InitFilters()
        {
            var cats = Def.Database.Values.OfType<ItemDef>().GroupBy(d => d.Category);

            var all = new StorageFilterCategoryNewNew("All");// { Owner = this };
            foreach (var cat in cats)
            {
                if (cat.Key == null)
                    continue;
                var c = new StorageFilterCategoryNewNew(cat.Key.Label) { Category = cat.Key };
                all.AddChildren(c);
                foreach (var def in cat)
                {
                    var record = new ItemFilter(def);
                    //this.Allowed.Add(def, record);
                    if (def.DefaultMaterialType != null)
                        c.AddChildren(new StorageFilterCategoryNewNew(def.Label) { Item = def }.AddLeafs(def.DefaultMaterialType.SubTypes.Select(m => new StorageFilterNewNew(def, m))));
                    //else if(def.GetSpecialFilters() is IEnumerable<StorageFilterNewNew> filters)
                    //    c.AddLeafs(filters);
                    else if(def.GetSpecialFilter() is StorageFilterCategoryNewNew filter)
                        c.AddChildren(filter);
                    else
                        c.AddLeafs(new StorageFilterNewNew(def));
                }
            }
            FiltersView = all;
            return all;
        }
        [Obsolete]
        public void ToggleItemFiltersCategories(int[] categoryIndices)
        {
            throw new Exception();
        }
        [Obsolete]
        public void ToggleItemFilters(int[] gameObjects)
        {
            throw new Exception();
        }
        protected override void SaveExtra(SaveTag tag)
        {
            tag.Add(this.Settings.Save("Settings"));
        }
        protected override void LoadExtra(SaveTag tag)
        {
            tag.TryGetTag("Settings", t => this.Settings.Load(t));
        }
        protected override void WriteExtra(IDataWriter w)
        {
            this.Settings.Write(w);
        }
        protected override void ReadExtra(IDataReader r)
        {
            this.Settings.Read(r);
        }

        public void FiltersGuiCallback(ItemDef item, MaterialDef material)
        {
            PacketStorageFiltersNew.Send(this, item, material);
        }
        public void FiltersGuiCallback(ItemDef item, Def variation)
        {
            PacketStorageFiltersNew.Send(this, item, variation);
        }
        public void FiltersGuiCallback(ItemCategory category)
        {
            PacketStorageFiltersNew.Send(this, category);
        }
        public void Toggle(ItemDef item, Def profile, MaterialDef material) => this.SettingsNew.Toggle(item, profile, material);
        public bool IsAllowed(ItemDef itemDef) => this.SettingsNew.IsAllowed(itemDef);
        public bool IsAllowed(Def def) => this.SettingsNew.IsAllowed(def);
        public bool IsAllowed(Def profile, MaterialDef material) => this.SettingsNew.IsAllowed(profile, material);
    }
}

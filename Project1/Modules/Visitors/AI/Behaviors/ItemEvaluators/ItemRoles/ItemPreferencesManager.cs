using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public partial class ItemPreferencesManager : Inspectable, ISaveable, ISerializableNew<ItemPreferencesManager> //IItemPreferencesManager
    {
        static List<ItemRoleDef> _flatItemRolesList;
        static Dictionary<ItemRoleContextDef, List<ItemRoleDef>> ContextToItemRolesMap = [];

        Control _gui;

        readonly Actor Actor;
        readonly Dictionary<ItemRoleDef, (Entity item, int score)> cache = [];

        readonly Dictionary<int, ItemBias> ItemBiases = [];
        readonly Queue<Entity> notScannedYet = [];
        readonly Dictionary<ItemRoleDef, ItemPreference> PreferencesNew = [];
        readonly Dictionary<int, int> TempIgnore = [];
        readonly HashSet<int> ToDiscard = [];

        public ItemPreferencesManager()
        {

        }
        public ItemPreferencesManager(Actor actor)
        {
            this.Actor = actor;
        }

        private void enqueueNewSpawnedItem(EntitySpawnedEvent e)
        {
            if (e.Entity is Tool && !this.TempIgnore.ContainsKey(e.Entity.RefId))
                this.notScannedYet.Enqueue(e.Entity);
        }
        static List<ItemRoleDef> GenerateItemRolesAll()
        {
            var flat = new List<ItemRoleDef>();
            foreach (var rDef in Def.GetDefs<ItemRoleDef>())
            {
                if (!ContextToItemRolesMap.TryGetValue(rDef.Context, out var list))
                    ContextToItemRolesMap[rDef.Context] = list = [];
                list.Add(rDef);
                flat.Add(rDef);
            }
            return flat;
        }

        Control GetGui()
        {
            var table = new TableObservable<ItemPreference>()
                .AddColumn("role", 128, p => new Label(p.Role))
                .AddColumn("item", 128, p => new Label(() => p.Item?.DebugName ?? "none", () => p.Item?.Select()))
                .AddColumn("score", 64, p => new Label(() => p.Score.ToString()))
                ;//.Bind(this.PreferencesView);
            var box = new ScrollableBoxNewNew(table.RowWidth, table.RowHeight * 16, ScrollModes.Vertical)
                .AddControls(table)
                .ToWindow($"{this.Actor.Name}'s Item Preferences");
            return box;
        }

        static void Init()
        {
            GenerateItemRolesAll();
        }

        private void ScanOne()
        {
            var jobs = this.Actor.GetJobs();
            var item = notScannedYet.Dequeue();
            if (this.Actor.Map != item.Map)
                return;
            var roles = this.Evaluate(item);
            if (!roles.Any())
                return;
            var finalRoles = roles
                    .Where(r => this.GetExistingPreference(r.role).score is int existingScore && r.score > existingScore);

            foreach (var r in finalRoles)
            {
                if (cache.TryGetValue(r.role, out var existing))
                {
                    if (r.score > existing.score)
                        cache[r.role] = (item, r.score);
                }
                else
                    cache.Add(r.role, (item, r.score));
            }
        }
        private bool StillValid(Entity i)
        {
            return i.ExistsOn(this.Actor.Map);
        }


        private void UpdateBiases()
        {
            List<int> toRemove = [];

            foreach (var (key, bias) in this.ItemBiases)
                if (bias.Tick() == 0)
                    toRemove.Add(key);

            foreach (var key in toRemove)
                this.ItemBiases.Remove(key);
        }

        private void UpdateTempIgnore()
        {
            List<int> toRemove = [];

            foreach (var (key, cooldown) in this.TempIgnore)
                if (cooldown == 0)
                    toRemove.Add(key);
                else
                    this.TempIgnore[key] = cooldown - 1;

            foreach (var key in toRemove)
            {
                this.TempIgnore.Remove(key);

                var item = this.Actor.Map.World.GetEntity(key);
                if (item.ExistsOn(this.Actor.Map))
                    this.notScannedYet.Enqueue(item);
            }
        }

        static List<ItemRoleDef> FlatItemRolesList => _flatItemRolesList ??= GenerateItemRolesAll();

        bool IsScanning => notScannedYet.Count > 0;


        internal void Commit(ItemRoleDef role, Entity item, int score)
        {
            if (!this.PreferencesNew.TryGetValue(role, out var pref))
            {
                pref = new(role);// { Role = context, Item = item, Score = score };
                this.PreferencesNew.Add(role, pref);
            }
            Entity oldItem = pref.Item;
            int oldScore = pref.Score;
            pref.Item = item;
            pref.Score = score;
            //item.Ownership.Owner = this.Actor;

            Packets.SyncDeltas(this.Actor, [(role, oldItem, item, score)]);
            this.cache.Remove(role);
        }
        internal IEnumerable<(ItemRoleDef role, int score)> Evaluate(Entity item)
        {
            foreach (var role in FlatItemRolesList)
            {
                var score = role.Worker.GetInventoryScore(this.Actor, item, role);
                if (this.ItemBiases.TryGetValue(item.RefId, out var bias))
                    score += bias.Value;
                if (score > 0)
                    yield return (role, score);
            }
        }
        internal (ItemRoleDef role, int score) FindBestRole(Entity item)
        {
            var allRoles = this.Evaluate(item);
            return allRoles.OrderByDescending(i => i.score).FirstOrDefault();

        }
        internal (Entity item, int score) GetExistingPreference(ItemRoleDef role)
        {
            if (this.PreferencesNew.TryGetValue(role, out var existing))
                return (existing.Item, existing.Score);
            return (null, 0);
        }

        internal Entity GetExistingPreference(ItemRoleDef role, out int score)
        {
            if (this.PreferencesNew.TryGetValue(role, out var existing))
            {
                score = existing.Score;
                return existing.Item;
            }
            score = 0;
            return null;
        }
        internal IEnumerable<(ItemRoleDef role, Entity item, int score)> GetPotential()
        {
            if (this.IsScanning)
            {
                ScanOne();
                yield break;
            }
            var toRemove = new List<ItemRoleDef>();
            foreach (var (con, (i, score)) in this.cache)
            {
                if (!StillValid(i))
                    toRemove.Add(con);
                else
                    yield return (con, i, score);
            }
            foreach (var r in toRemove)
                this.cache.Remove(r);

        }

        internal IEnumerable<(ItemRoleDef role, Entity item, int score)> GetPotentialAll()
        {
            if (notScannedYet.Count == 0)
                yield break;
            var jobs = this.Actor.GetJobs();
            var dic = new Dictionary<ItemRoleDef, (Entity item, int score)>();
            while (notScannedYet.Count > 0)
            {
                var item = notScannedYet.Dequeue();
                if (this.Actor.Map != item.Map)
                    continue;
                var roles = this.Evaluate(item);
                if (!roles.Any())
                    continue;
                var finalRoles = roles
                    .Where(r => this.GetExistingPreference(r.role, out var existingScore) is var existing && r.score > existingScore);

                foreach (var r in finalRoles)
                {
                    if (dic.TryGetValue(r.role, out var existing))
                    {
                        if (r.score > existing.score)
                            dic[r.role] = (item, r.score);
                    }
                    else
                        dic.Add(r.role, (item, r.score));
                }
            }

            foreach (var (context, pref) in dic)
                yield return (context, pref.item, pref.score);
        }
        internal void RemovePreference(ItemRoleDef tag)
        {
            this.PreferencesNew[tag].Clear();
        }

        public IEnumerable<Entity> GetJunk()
        {
            this.Validate();
            var actor = this.Actor;
            var net = actor.Net;
            var items = actor.Inventory.GetItems();
            foreach (var i in this.ToDiscard.ToArray())
            {
                var item = net.World.GetEntity(i);
                if (!items.Contains(item))
                {
                    this.RemoveJunk(item);
                    continue;
                }
                yield return item;
            }
        }


        public Control GetListControl(Entity entity)
        {
            var p = this.GetPreference(entity);
            return new Label(p) { HoverText = $"[{this.Actor.Name}] prefers [{entity.Name}] for [{p}]" };
        }
        public Def GetPreference(Entity item)
        {
            return this.PreferencesNew.Values.FirstOrDefault(p => p.Item == item)?.Role.Context;
        }
        public Entity GetPreference(Def context)
        {
            throw new Exception();
            //return this.GetPreference(RegistryByContext[context]);
        }

        //public int GetScore(Def context, Entity item)
        //{
        //    return RegistryByContext[context].Score(this.Actor, item);
        //}
        public IEnumerable<Entity> GetUselessItems(IEnumerable<Entity> entity)
        {
            var items = this.Actor.Inventory.GetItems();
            foreach (var i in items)
                if (!this.IsUseful(i))
                    yield return i;
        }

        public void HandleItem(Entity item)
        {
            foreach (var pref in this.PreferencesNew.Values)
            {
                var role = pref.Role;
                var score = role.Worker.GetInventoryScore(this.Actor, item, role);
                if (score < 0)
                    continue;
                if (score > pref.Score)
                {
                    pref.Item = item;
                    pref.Score = score;
                    return; // TODO check 
                }
            }
            if (!this.IsUseful(item))
                this.ToDiscard.Add(item.RefId);
        }
        public bool IsPreference(Entity item)
        {
            return this.PreferencesNew.Values.Any(p => item == p.Item);
        }
        public bool IsUseful(Entity item)
        {
            if (item.Def == ItemDefOf.Coins)
                return true;
            if (this.PreferencesNew.Values.Any(p => p.Item == item))
                return true;
            return false;
        }

        public void ModifyBias(Entity entity, int value)
        {
            if (!this.ItemBiases.TryGetValue(entity.RefId, out var bias))
            {
                bias = new ItemBias(entity, value);
                this.ItemBiases.Add(entity.RefId, bias);
            }
            else
                bias.Value += value;
        }
        public void OnDespawn(MapBase oldMap)
        {
            this.notScannedYet.Clear();
            oldMap.Events.Unsubscribe(this);
        }

        public void OnForcedDrop(Entity item)
        {
            this.ModifyBias(item, -200);
            this.TempIgnore[item.RefId] = (int)Ticks.FromSeconds(10);

            List<ItemPreference> toRemove = [];
            foreach (var (context, preference) in this.PreferencesNew)
                if (preference.ItemRefId == item.RefId)
                    toRemove.Add(preference);
            foreach (var r in toRemove)
                this.PreferencesNew.Remove(r.Role);

            Packets.SyncDeltas(this.Actor, [.. toRemove.Select(r => (r.Role, r.Item, (Entity)null, 0))]);

            foreach (var i in this.Actor.Map.GetEntities<Tool>())
                if (i != item)
                    this.notScannedYet.Enqueue(i);
        }
        public void OnMapLoaded()
        {
            this.Actor.Map.Events.ListenTo<EntitySpawnedEvent>(enqueueNewSpawnedItem);
            foreach (var i in this.Actor.Map.GetEntities<Tool>())
                this.notScannedYet.Enqueue(i);
        }
        public void OnSpawn(MapBase newMap)
        {
            foreach (var i in newMap.GetEntities<Tool>())
                this.notScannedYet.Enqueue(i);
            newMap.Events.ListenTo<EntitySpawnedEvent>(enqueueNewSpawnedItem);
        }
        public void RemoveJunk(Entity item)
        {
            this.ToDiscard.Remove(item.RefId);
        }

        public void ResetPreferences()
        {
            var items = this.Actor.Inventory.GetItems();
            foreach (var i in items)
                this.HandleItem(i);
        }

        public void ResolveReferences()
        {
        }

        public void Tick()
        {
            this.UpdateBiases();
            this.UpdateTempIgnore();
        }
        public void Validate()
        {
            this.ResetPreferences();
        }

        public Control Gui => this._gui ??= this.GetGui();

        [EnsureStaticCtorCall]
        static class Packets
        {
            static readonly int pSyncPrefsAll;

            static Packets()
            {
                pSyncPrefsAll = Registry.PacketHandlers.Register(Receive);
            }

            private static void Receive(INetEndpoint net, Packet pck)
            {
                if (net is Server)
                    throw new Exception();
                var r = pck.PacketReader;

                var actor = net.World.GetEntity<Actor>(r.ReadInt32());
                var manager = actor.ItemPreferences;

                // read deltas
                var length = r.ReadInt32();
                for (int i = 0; i < length; i++)
                {
                    var role = r.ReadDef<ItemRoleDef>();
                    var olditem = (r.ReadInt32() is int oldid && oldid > 0) ? actor.Map.World.GetEntity(oldid) : null;
                    var newitem = (r.ReadInt32() is int newid && newid > 0) ? actor.Map.World.GetEntity(newid) : null;
                    var score = r.ReadInt32();
                    manager.ApplyDelta(role, olditem, newitem, score);
                }
            }

            public static void SyncDeltas(Actor actor, (ItemRoleDef role, Entity oldItem, Entity newItem, int score)[] deltas)
            {
                var w = (actor.Net as Server).BeginPacket(pSyncPrefsAll);
                w.Write(actor.RefId);
                w.Write(deltas.Length);
                for (int i = 0; i < deltas.Length; i++)
                {
                    var (role, olditem, newitem, score) = deltas[i];
                    w.Write(role);
                    w.Write(olditem?.RefId ?? -1);
                    w.Write(newitem?.RefId ?? -1);
                    w.Write(score);
                }
            }
        }

        #region ISaveable implementations
        public ISaveable Load(SaveTag tag)
        {
            tag.TryGetTag("Preferences", pt =>
            {
                foreach (var p in pt.LoadList<ItemPreference>())
                {
                    var existing = this.PreferencesNew[p.Role];
                    existing.CopyFrom(p);
                    //this.PreferencesView.Add(existing);
                }
            });

            return this;
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(this.PreferencesNew.Values.Where(p => p.Item is not null).Save("Preferences"));
            return tag;
        }
        #endregion
        #region ISerializableNew implementations
        public static ItemPreferencesManager Create(IDataReader r) => new ItemPreferencesManager().Read(r);

        public ItemPreferencesManager Read(IDataReader r)
        {
            //this.PreferencesNew.ReadFromFlat(r, Def.GetDef<ItemRoleDef>, ItemPreference.Create);
            //var flat = new List<ItemPreference>().Read(r);
            this.PreferencesNew.FromValues(new List<ItemPreference>().Read(r), p => p.Role);
            return this;
        }

        public void Write(IDataWriter w)
        {
            w.Write(this.PreferencesNew.Values);
        }
        #endregion
    }

    //[EnsureStaticCtorCall]
    //partial class ItemPreferencesManager : Inspectable, IItemPreferencesManager, ISaveable, ISerializableNew
    //{

    //    static readonly Dictionary<GearType, ItemRoleWorker> ItemRolesGear = [];
    //    static readonly Dictionary<JobDef, ItemRoleWorker> ItemRolesTool = [];
    //    static readonly Dictionary<IItemPreferenceContext, ItemRoleWorker> RegistryByContext = [];
    //    static readonly Dictionary<string, ItemRoleWorker> RegistryByName = [];

    //    Control _gui;

    //    readonly Actor Actor;
    //    readonly Dictionary<IItemPreferenceContext, (Entity item, int score)> cache = [];

    //    readonly Dictionary<int, ItemBias> ItemBiases = [];
    //    readonly Queue<Entity> notScannedYet = [];

    //    readonly Dictionary<IItemPreferenceContext, ItemPreference> PreferencesNew = [];
    //    readonly ObservableCollection<ItemPreference> PreferencesObs = [];
    //    readonly Dictionary<int, int> TempIgnore = [];
    //    readonly HashSet<int> ToDiscard = [];

    //    // TODO: item like/dislike registry
    //    static ItemPreferencesManager()
    //    {
    //        Init();
    //    }
    //    public ItemPreferencesManager(Actor actor)
    //    {
    //        this.Actor = actor;
    //        this.PopulateRoles();

    //        this.PreferencesObs.CollectionChanged += this.PreferencesObs_CollectionChanged;
    //    }

    //    private void enqueueNewSpawnedItem(EntitySpawnedEvent e)
    //    {
    //        if (e.Entity is Tool && !this.TempIgnore.ContainsKey(e.Entity.RefId))
    //            this.notScannedYet.Enqueue(e.Entity);
    //    }

    //    static void GenerateItemRolesGear()
    //    {
    //        var geardefs = GearType.Dictionary.Values;
    //        foreach (var g in geardefs)
    //            ItemRolesGear.Add(g, new ItemRoleGearWorker(g));
    //    }
    //    static void GenerateItemRolesTools()
    //    {
    //        var defs = Def.Database.Values.OfType<JobDef>();
    //        foreach (var d in defs)
    //            ItemRolesTool.Add(d, new ItemRoleToolWorker(d));
    //    }
    //    Control GetGui()
    //    {
    //        var table = new TableObservable<ItemPreference>()
    //            .AddColumn("role", 128, p => new Label(p.Role.Context))
    //            .AddColumn("item", 128, p => new Label(() => p.Item?.DebugName ?? "none", () => p.Item?.Select()))
    //            .AddColumn("score", 64, p => new Label(() => p.Score.ToString()))
    //            .Bind(this.PreferencesObs);
    //        var box = new ScrollableBoxNewNew(table.RowWidth, table.RowHeight * 16, ScrollModes.Vertical)
    //            .AddControls(table)
    //            .ToWindow($"{this.Actor.Name}'s Item Preferences");
    //        return box;
    //    }
    //    private Entity GetPreference(ItemRoleWorker role)
    //    {
    //        return this.PreferencesNew[role.Context].Item;
    //    }
    //    static void Init()
    //    {
    //        GenerateItemRolesGear();
    //        GenerateItemRolesTools();

    //        // TODO no need to initialize each actor's preferences with empty roles?
    //        // TODO find way to avoid checking every role of the same type for items that are invalid for said role type, for example don't check all tool roles for non-tool items
    //        foreach (var r in ItemRolesTool.Values.Concat(ItemRolesGear.Values))
    //        {
    //            RegistryByName[r.ToString()] = r;
    //            RegistryByContext[r.Context] = r;
    //        }
    //    }

    //    private void PopulateRoles()
    //    {
    //        foreach (var r in ItemRolesTool.Values.Concat(ItemRolesGear.Values))
    //            this.PreferencesNew.Add(r.Context, new(r));
    //    }

    //    private void PreferencesObs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    //    {
    //        if (this.Actor.Net is Server)
    //            Packets.Sync(this.Actor.Net, this.Actor, e.OldItems, e.NewItems);
    //    }

    //    private void ScanOne()
    //    {
    //        var jobs = this.Actor.GetJobs();
    //        var item = notScannedYet.Dequeue();
    //        if (this.Actor.Map != item.Map)
    //            return;
    //        var roles = this.Evaluate(item);
    //        if (!roles.Any())
    //            return;
    //        var finalRoles = roles
    //                .Where(r => jobs.Any(j => j.Enabled && j.Def == r.role))
    //                .Where(r => this.GetPreference(r.role, out var existingScore) is var existing && r.score > existingScore);

    //        foreach (var r in finalRoles)
    //        {
    //            if (cache.TryGetValue(r.role, out var existing))
    //            {
    //                if (r.score > existing.score)
    //                    cache[r.role] = (item, r.score);
    //            }
    //            else
    //                cache.Add(r.role, (item, r.score));
    //        }
    //    }
    //    private bool StillValid(Entity i)
    //    {
    //        return i.ExistsOn(this.Actor.Map);
    //    }

    //    void SyncAddPref(ItemPreference pref)
    //    {
    //        if (this.Actor.Net is not Client)
    //            throw new Exception();
    //        var existing = this.PreferencesNew[pref.Role.Context];
    //        existing.CopyFrom(pref);
    //        existing.ResolveReferences(this.Actor);
    //        if (!this.PreferencesObs.Contains(existing))
    //            this.PreferencesObs.Add(existing);
    //    }
    //    void SyncRemovePref(ItemPreference pref)
    //    {
    //        if (this.Actor.Net is not Client)
    //            throw new Exception();
    //        var existing = this.PreferencesNew[pref.Role.Context];
    //        existing.Clear();
    //        if (this.PreferencesObs.Contains(existing))
    //            this.PreferencesObs.Remove(existing);
    //    }

    //    private void UpdateBiases()
    //    {
    //        List<int> toRemove = [];

    //        foreach (var (key, bias) in this.ItemBiases)
    //            if (bias.Tick() == 0)
    //                toRemove.Add(key);

    //        foreach (var key in toRemove)
    //            this.ItemBiases.Remove(key);
    //    }

    //    private void UpdateTempIgnore()
    //    {
    //        //var keys = this.TempIgnore.Keys.ToList();
    //        //foreach (var key in keys)
    //        //{
    //        //    if (this.TempIgnore[key] <= 0)
    //        //        this.TempIgnore.Remove(key);
    //        //    else
    //        //        this.TempIgnore[key]--;
    //        //}

    //        List<int> toRemove = [];

    //        foreach (var (key, cooldown) in this.TempIgnore)
    //            if (cooldown == 0)
    //                toRemove.Add(key);
    //            else
    //                this.TempIgnore[key] = cooldown - 1;

    //        foreach (var key in toRemove)
    //        {
    //            this.TempIgnore.Remove(key);

    //            var item = this.Actor.Map.World.GetEntity(key);
    //            if (item.ExistsOn(this.Actor.Map))
    //                this.notScannedYet.Enqueue(item);
    //        }
    //    }

    //    static IEnumerable<ItemRoleWorker> AllRoles => ItemRolesGear.Values.Concat(ItemRolesTool.Values);
    //    bool IsScanning => notScannedYet.Count > 0;

    //    //public bool AddPreference(Entity item)
    //    //{
    //    //    var scored = AllRoles
    //    //        .Select(r => (r, r.Score(this.Actor, item)))
    //    //        .Where(rs => rs.Item2 > -1)
    //    //        .OrderByDescending(rs => rs.Item2);
    //    //    if (!scored.Any())
    //    //        return false;
    //    //    var bestRole = scored.First();
    //    //    var pref = this.PreferencesNew[bestRole.r.Context];
    //    //    pref.Item = item;
    //    //    pref.Score = bestRole.Item2;
    //    //    if (!this.PreferencesObs.Contains(pref))
    //    //        this.PreferencesObs.Add(pref);
    //    //    return true;
    //    //}
    //    public void AddPreference(IItemPreferenceContext context, Entity item, int score)
    //    {
    //        var pref = this.PreferencesNew[context];
    //        pref.Item = item;
    //        pref.Score = score;
    //        //item.Ownership.Owner = this.Actor;
    //        if (this.PreferencesObs.Contains(pref))
    //            this.PreferencesObs.Remove(pref);
    //        this.PreferencesObs.Add(pref); // HACK to trigger observable syncing
    //        this.cache.Remove(context);
    //    }

    //    public static ISerializableNew Create(IDataReader r) => new ItemPreference().Read(r);
    //    public IEnumerable<(IItemPreferenceContext role, int score)> Evaluate(Entity item)
    //    {
    //        foreach (var pref in this.PreferencesNew.Values)
    //        {
    //            var role = pref.Role;
    //            var score = role.Score(this.Actor, item);
    //            if (this.ItemBiases.TryGetValue(item.RefId, out var bias))
    //                score += bias.Value;
    //            if (score > 0)
    //                yield return (role.Context, score);
    //        }
    //    }
    //    public (IItemPreferenceContext role, int score) FindBestRole(Entity item)
    //    {
    //        var allRoles = this.Evaluate(item);
    //        return allRoles.OrderByDescending(i => i.score).FirstOrDefault();

    //        //ItemPreference bestPreference = null;
    //        //int bestScore = -1;
    //        //foreach (var pref in this.PreferencesNew.Values)
    //        //{
    //        //    var role = pref.Role;
    //        //    var score = role.Score(this.Actor, item);
    //        //    if (score > bestScore)
    //        //    {
    //        //        bestPreference = pref;
    //        //        bestScore = score;
    //        //    }
    //        //}
    //        //return (bestPreference?.Role.Context, bestScore);
    //    }

    //    public void ForceDrop(Entity item)
    //    {
    //        this.ModifyBias(item, -200);
    //        this.TempIgnore[item.RefId] = (int)Ticks.FromSeconds(10);

    //        List<IItemPreferenceContext> prevRoles = [];
    //        foreach (var (context, preference) in this.PreferencesNew)
    //            if (preference.ItemRefId == item.RefId)
    //            {
    //                preference.Clear();
    //                prevRoles.Add(preference.Role.Context);
    //            }

    //        foreach (var i in this.Actor.Map.GetEntities<Tool>())
    //            if (i != item)
    //                this.notScannedYet.Enqueue(i);
    //    }

    //    public IEnumerable<Entity> GetJunk()
    //    {
    //        this.Validate();
    //        var actor = this.Actor;
    //        var net = actor.Net;
    //        var items = actor.Inventory.GetItems();
    //        foreach (var i in this.ToDiscard.ToArray())
    //        {
    //            var item = net.World.GetEntity(i) as Entity;
    //            if (!items.Contains(item))
    //            {
    //                this.RemoveJunk(item);
    //                continue;
    //            }
    //            yield return item;
    //        }
    //    }


    //    public Control GetListControl(Entity entity)
    //    {
    //        var p = this.GetPreference(entity);
    //        return new Label(p) { HoverText = $"[{this.Actor.Name}] prefers [{entity.Name}] for [{p}]" };
    //    }
    //    public IEnumerable<(IItemPreferenceContext role, Entity item, int score)> GetPotential()
    //    {
    //        if (this.IsScanning)
    //        {
    //            ScanOne();
    //            yield break;
    //        }
    //        var toRemove = new List<IItemPreferenceContext>();
    //        foreach (var (context, (i, score)) in this.cache)
    //        {
    //            if (!StillValid(i))
    //                toRemove.Add(context);
    //            else
    //                yield return (context, i, score);
    //        }
    //        foreach (var role in toRemove)
    //            this.cache.Remove(role);

    //    }

    //    public IEnumerable<(IItemPreferenceContext role, Entity item, int score)> GetPotentialAll()
    //    {
    //        if (notScannedYet.Count == 0)
    //            yield break;
    //        var jobs = this.Actor.GetJobs();
    //        var dic = new Dictionary<IItemPreferenceContext, (Entity item, int score)>();
    //        while (notScannedYet.Count > 0)
    //        {
    //            var item = notScannedYet.Dequeue();
    //            if (this.Actor.Map != item.Map)
    //                continue;
    //            var roles = this.Evaluate(item);
    //            if (!roles.Any())
    //                continue;
    //            var finalRoles = roles
    //                .Where(r => jobs.Any(j => j.Enabled && j.Def == r.role))
    //                .Where(r => this.GetPreference(r.role, out var existingScore) is var existing && r.score > existingScore);

    //            foreach (var r in finalRoles)
    //            {
    //                if (dic.TryGetValue(r.role, out var existing))
    //                {
    //                    if (r.score > existing.score)
    //                        dic[r.role] = (item, r.score);
    //                }
    //                else
    //                    dic.Add(r.role, (item, r.score));
    //            }
    //        }

    //        foreach (var (context, pref) in dic)
    //            yield return (context, pref.item, pref.score);
    //    }
    //    public IItemPreferenceContext GetPreference(Entity item)
    //    {
    //        return this.PreferencesNew.Values.FirstOrDefault(p => p.Item == item)?.Role.Context;
    //    }
    //    public Entity GetPreference(IItemPreferenceContext context)
    //    {
    //        return this.GetPreference(RegistryByContext[context]);
    //    }

    //    public Entity GetPreference(IItemPreferenceContext context, out int score)
    //    {
    //        var p = this.PreferencesNew[context];
    //        score = p.Score;
    //        return p.Item;
    //    }

    //    public int GetScore(IItemPreferenceContext context, Entity item)
    //    {
    //        return RegistryByContext[context].Score(this.Actor, item);
    //    }
    //    public IEnumerable<Entity> GetUselessItems(IEnumerable<Entity> entity)
    //    {
    //        var items = this.Actor.Inventory.GetItems();
    //        foreach (var i in items)
    //            if (!this.IsUseful(i))
    //                yield return i;
    //    }

    //    public void HandleItem(Entity item)
    //    {
    //        foreach (var pref in this.PreferencesNew.Values)
    //        {
    //            var role = pref.Role;
    //            var score = role.Score(this.Actor, item);
    //            if (score < 0)
    //                continue;
    //            if (score > pref.Score)
    //            {
    //                pref.Item = item;
    //                pref.Score = score;
    //                return; // TODO check 
    //            }
    //        }
    //        if (!this.IsUseful(item))
    //            this.ToDiscard.Add(item.RefId);
    //    }
    //    public bool IsPreference(Entity item)
    //    {
    //        return this.PreferencesNew.Values.Any(p => item == p.Item);
    //    }
    //    public bool IsUseful(Entity item)
    //    {
    //        if (item.Def == ItemDefOf.Coins)
    //            return true;
    //        if (this.PreferencesNew.Values.Any(p => p.Item == item))
    //            return true;
    //        return false;
    //    }

    //    public ISaveable Load(SaveTag tag)
    //    {
    //        tag.TryGetTag("Preferences", pt =>
    //        {
    //            foreach (var p in pt.LoadList<ItemPreference>())
    //            {
    //                var existing = this.PreferencesNew[p.Role.Context];
    //                existing.CopyFrom(p);
    //                this.PreferencesObs.Add(existing);
    //            }
    //        });

    //        return this;
    //    }

    //    public void ModifyBias(Entity entity, int value)
    //    {
    //        if (!this.ItemBiases.TryGetValue(entity.RefId, out var bias))
    //        {
    //            bias = new ItemBias(entity, value);
    //            this.ItemBiases.Add(entity.RefId, bias);
    //        }
    //        else
    //            bias.Value += value;
    //    }
    //    public void OnDespawn(MapBase oldMap)
    //    {
    //        this.notScannedYet.Clear();
    //        oldMap.Events.Unsubscribe(this);
    //    }
    //    public void OnMapLoaded()
    //    {
    //        this.Actor.Map.Events.ListenTo<EntitySpawnedEvent>(enqueueNewSpawnedItem);

    //        // HACK TO JUMP START THE SCANNING SYSTEM
    //        foreach (var i in this.Actor.Map.GetEntities<Tool>())
    //            this.notScannedYet.Enqueue(i);
    //    }
    //    public void OnSpawn(MapBase newMap)
    //    {
    //        foreach (var i in newMap.GetEntities<Tool>())
    //            this.notScannedYet.Enqueue(i);
    //        newMap.Events.ListenTo<EntitySpawnedEvent>(enqueueNewSpawnedItem);
    //    }

    //    public ISerializableNew Read(IDataReader r)
    //    {
    //        foreach (var p in this.PreferencesNew)
    //            p.Value.Read(r);
    //        foreach (var p in this.PreferencesNew.Where(t => t.Value.Score > 0))
    //            this.PreferencesObs.Add(p.Value);
    //        return this;
    //    }
    //    public void RemoveJunk(Entity item)
    //    {
    //        this.ToDiscard.Remove(item.RefId);
    //    }

    //    public void RemovePreference(IItemPreferenceContext tag)
    //    {
    //        this.PreferencesNew[tag].Clear();
    //    }

    //    public void ResetPreferences()
    //    {
    //        var items = this.Actor.Inventory.GetItems();
    //        foreach (var i in items)
    //            this.HandleItem(i);
    //    }

    //    public void ResolveReferences()
    //    {
    //        foreach (var p in this.PreferencesObs)
    //            p.ResolveReferences(this.Actor);
    //    }

    //    public SaveTag Save(string name = "")
    //    {
    //        var tag = new SaveTag(SaveTag.Types.Compound, name);
    //        tag.Add(this.PreferencesNew.Values.Where(p => p.Item is not null).Save("Preferences"));
    //        return tag;
    //    }

    //    public void Tick()
    //    {
    //        this.UpdateBiases();
    //        this.UpdateTempIgnore();
    //    }
    //    public void Validate()
    //    {
    //        this.ResetPreferences();
    //    }

    //    public void Write(IDataWriter w)
    //    {
    //        foreach (var r in this.PreferencesNew.Values)
    //            r.Write(w);
    //    }

    //    public Control Gui => this._gui ??= this.GetGui();
    //    public IEnumerable<ItemPreference> Preferences => this.PreferencesObs;

    //    [EnsureStaticCtorCall]
    //    static class Packets
    //    {
    //        static readonly int pSyncPrefsAll;

    //        static Packets()
    //        {
    //            pSyncPrefsAll = Registry.PacketHandlers.Register(Receive);
    //        }

    //        private static void Receive(INetEndpoint net, Packet pck)
    //        {
    //            if (net is Server)
    //                throw new Exception();
    //            var r = pck.PacketReader;

    //            var actor = net.World.GetEntity<Actor>(r.ReadInt32());
    //            var prefs = actor.ItemPreferences as ItemPreferencesManager;
    //            var oldItems = new List<ItemPreference>().Read(r);
    //            var newItems = new List<ItemPreference>().Read(r);

    //            foreach (var p in oldItems)
    //                prefs.SyncRemovePref(p);
    //            foreach (var p in newItems)
    //                prefs.SyncAddPref(p);
    //        }

    //        internal static void Sync(NetEndpoint net, Actor actor, System.Collections.IList oldItems, System.Collections.IList newItems)
    //        {
    //            var w = net.BeginPacket(pSyncPrefsAll);

    //            w.Write(actor.RefId);

    //            if (oldItems is null)
    //                w.Write(0);
    //            else
    //                oldItems.Cast<ItemPreference>().ToList().WriteNew(w);

    //            if (newItems is null)
    //                w.Write(0);
    //            else
    //                newItems.Cast<ItemPreference>().ToList().WriteNew(w);
    //        }
    //    }
    //}
}

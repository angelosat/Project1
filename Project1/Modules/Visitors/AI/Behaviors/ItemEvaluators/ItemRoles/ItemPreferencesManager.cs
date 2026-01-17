using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace Start_a_Town_
{
    public partial class ItemPreferencesManager : Inspectable, ISaveable, ISerializableNew<ItemPreferencesManager> //IItemPreferencesManager
    {
        static List<ItemRoleDef> _flatItemRolesList;
        static readonly Dictionary<ItemRoleContextDef, List<ItemRoleDef>> ContextToItemRolesMap = [];

        Control _gui;

        readonly Actor Actor;
        readonly Dictionary<ItemRoleDef, (Entity item, int score)> cache = [];

        readonly Dictionary<int, ItemBias> ItemBiases = [];
        readonly Queue<Entity> notScannedYet = [];
        readonly Dictionary<ItemRoleDef, ItemPreference> PrefsInternal = [];

        readonly ReadOnlyDictionary<ItemRoleDef, ItemPreference> PreferencesNew;
        readonly Dictionary<int, int> TempIgnore = [];
        readonly HashSet<int> ToDiscard = [];

        ItemPreferencesManager()
        {
            //Dictionary<ItemRoleDef, ItemPreference> _preferences = [];
            foreach (var role in Def.GetDefs<ItemRoleDef>())
                this.PrefsInternal[role] = new ItemPreference(role);
            this.PreferencesNew = new(this.PrefsInternal);
        }

        public ItemPreferencesManager(Actor actor) : this()
        {
            this.Actor = actor;
        }

        private void EnqueueNewSpawnedItem(EntitySpawnedEvent e)
        {
            //if (e.Entity is Tool && !this.TempIgnore.ContainsKey(e.Entity.RefId))
            if (!this.TempIgnore.ContainsKey(e.Entity.RefId))
                this.notScannedYet.Enqueue(e.Entity);
        }
        

        Control GetGui()
        {
            var table = new Table<ItemPreference>()
                .AddColumn("role", 128, p => new Label(p.Role.Label))
                //.AddColumn("item", 128, p => new Label(() => p.Item?.DebugName ?? "none", () => p.Item?.Select()))
                .AddColumn("item", 64, p => new Label(() => p.Item?.Label ?? "none", () => p.Item?.Select()))
                .AddColumn("score", 32, p => new Label(() => p.InventoryScore.ToString()));
            table.AddItems(this.PreferencesNew.Values);
                
            var box = new ScrollableBoxNewNewNew(table, table.RowWidth, table.RowHeight * 16, ScrollModes.Vertical)
                .ToWindow($"{this.Actor.Name}'s Item Preferences");
            return box;
            


            throw new Exception();
            //var table = new TableObservable<ItemPreference>()
            //    .AddColumn("role", 128, p => new Label(p.Role))
            //    .AddColumn("item", 128, p => new Label(() => p.Item?.DebugName ?? "none", () => p.Item?.Select()))
            //    .AddColumn("score", 64, p => new Label(() => p.InventoryScore.ToString()))
            //    ;//.Bind(this.PreferencesView);
            //var box = new ScrollableBoxNewNew(table.RowWidth, table.RowHeight * 16, ScrollModes.Vertical)
            //    .AddControls(table)
            //    .ToWindow($"{this.Actor.Name}'s Item Preferences");
            //return box;
        }

        static void Init()
        {
            GenerateItemRolesAll();
        }

        private void EvaluateOne()
        {
            if (notScannedYet.Count == 0)
                return;
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
        bool IsScanning => notScannedYet.Count > 0;

        internal void UpdatePref(ItemRoleDef role, Entity item, int score)
        {
            var pref = this.PrefsInternal[role];
            pref.Update(item, score);
            this.PrefsInternal[role] = pref;
        }
        internal void Commit(ItemRoleDef role, Entity item, int score)
        {
            var pref = this.PreferencesNew[role];
            Entity oldItem = pref.Item;
            int oldScore = pref.InventoryScore;
            this.UpdatePref(role, item, score);
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
                return (existing.Item, existing.InventoryScore);
            return (null, 0);
        }

        internal Entity GetExistingPreference(ItemRoleDef role, out int score)
        {
            if (this.PreferencesNew.TryGetValue(role, out var existing))
            {
                score = existing.InventoryScore;
                return existing.Item;
            }
            score = 0;
            return null;
        }
        internal IEnumerable<(ItemRoleDef role, Entity item, int score)> GetPotential()
        {
            if (this.IsScanning)
            {
                //ScanOne();
                yield break;
            }
            if (this.cache.Count == 0)
                yield break;
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
            //return this.PreferencesNew.Values.FirstOrDefault(p => p.Item == item)?.Role.Context;
            return this.PreferencesNew.Values.FirstOrDefault(p => p.Item == item).Role?.Def; // if itempreferences are struct, then the default returned will have role == null
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
            //foreach (var pref in this.PreferencesNew.Values)
            //{
            //    var role = pref.Role;
            //    var score = role.Worker.GetInventoryScore(this.Actor, item, role);
            //    if (score < 0)
            //        continue;
            //    if (score > pref.InventoryScore)
            //    {
            //        pref.Item = item;
            //        pref.InventoryScore = score;
            //        return; // TODO check 
            //    }
            //}
            foreach (var (role, pref) in this.PrefsInternal)
            {
                var score = role.Worker.GetInventoryScore(this.Actor, item, role);
                if (score < 0)
                    continue;
                if (score > pref.InventoryScore)
                {
                    this.UpdatePref(role, item, score);
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
            if (item.Def == ItemDefOf.Coins) // HACK
                return true;
            if (this.PreferencesNew.Values.Any(p => p.Item == item && p.InventoryScore > 0))
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

        public void ForceDrop(Entity item)
        {
            this.ModifyBias(item, -200);
            this.TempIgnore[item.RefId] = (int)Ticks.FromSeconds(10);

            List<ItemPreference> toSync = [];
            foreach (var (context, preference) in this.PreferencesNew)
                if (preference.Item == item)
                {
                    //preference.Item = null;
                    //preference.InventoryScore = 0;
                    this.UpdatePref(context, null, 0);
                    toSync.Add(preference);
                }
            //foreach (var r in toRemove)
            //    this.PreferencesNew.Remove(r.Role);

            Packets.SyncDeltas(this.Actor, [.. toSync.Select(r => (r.Role, r.Item, (Entity)null, 0))]);

            foreach (var i in this.Actor.Map.Entities)
                if (i != item)
                    this.notScannedYet.Enqueue(i);
        }
        public IEnumerable<(Entity item, int score)> GetItemsBySituationalScore(Actor actor, Func<Entity, bool> filter)
        {
            var potential = this.PreferencesNew.Values.Where(p => p.Item != null && filter(p.Item));
            // TODO: For large inventories, consider replacing SortedDictionary with a simple List<(Entity, int)> + Sort()
            // to reduce allocations and overhead. Current approach is fine for typical small inventories.
            var byScore = new SortedDictionary<int, List<(Entity, int)>>(Comparer<int>.Create((a, b) => b.CompareTo(a)));
            foreach (var pref in potential)
            {
                var score = pref.Role.Worker.GetSituationalScore(actor, pref.Item, pref.Role);
                if (!byScore.TryGetValue(score, out var list))
                    byScore[score] = list = [];
                list.Add((pref.Item, score));
            }
            foreach (var (score, list) in byScore)
                foreach (var item in list)
                    yield return item;
        }
        public IEnumerable<ItemPreference> GetItemsBySituationalScoreNew(Actor actor, Func<Entity, bool> filter)
        {
            // Collect all valid preferences with their computed score
            var scoredList = this.PrefsInternal.Values
                .Where(p => p.Item != null && filter(p.Item))
                .Select(p => (pref: p, score: p.Role.Worker.GetSituationalScore(actor, p.Item, p.Role)))
                .ToList();

            // Sort descending by score
            scoredList.Sort((a, b) => b.score.CompareTo(a.score));

            // Yield only the ItemPreference part
            foreach (var entry in scoredList)
                yield return entry.pref;
        }
        public int GetTotalSituationalScoreFor(Entity item)
        {
            var relevantRoles = this.PrefsInternal.Where(pref => pref.Value.Item == item);
            int total = 0;
            foreach(var (role, pref) in relevantRoles)
                total += role.GetSituationalScore(this.Actor, item);
            return total;
        }
        //public void OnMapLoaded()
        //{
        //    this.Actor.Map.Events.ListenTo<EntitySpawnedEvent>(enqueueNewSpawnedItem);
        //    foreach (var i in this.Actor.Map.GetEntities<Tool>())
        //        this.notScannedYet.Enqueue(i);
        //}
        public void OnSpawn(MapBase newMap)
        {
            foreach (var i in newMap.Entities)
                this.notScannedYet.Enqueue(i);
            newMap.Events.ListenTo<EntitySpawnedEvent>(EnqueueNewSpawnedItem);
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

        public void Tick()
        {
            this.EvaluateOne();
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
                    //var olditem = (r.ReadInt32() is int oldid && oldid > 0) ? actor.Map.World.GetEntity(oldid) : null;
                    //var newitem = (r.ReadInt32() is int newid && newid > 0) ? actor.Map.World.GetEntity(newid) : null;
                    var olditemid = r.ReadInt32();
                    var newitemid = r.ReadInt32();
                    var olditem = olditemid > 0 ? actor.Map.World.GetEntity(olditemid) : null;
                    var newitem = newitemid > 0 ? actor.Map.World.GetEntity(newitemid) : null;
                    var score = r.ReadInt32();
                    //manager.ApplyDelta(role, olditem, newitem, score);
                    manager.UpdatePref(role, newitem, score);
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
                foreach (var p in pt.LoadListNew<ItemPreference>())
                {
                    var existing = this.PreferencesNew[p.Role];
                    existing.CopyFrom(p);
                    //this.PreferencesView.Add(existing);
                }
            });

            return this;
        }
        internal void ResolveReferences() 
        {
            foreach (var i in this.Actor.Map.Entities)
                this.notScannedYet.Enqueue(i);
            this.Actor.Map.Events.ListenTo<EntitySpawnedEvent>(EnqueueNewSpawnedItem);
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
            this.PreferencesNew.Sync(r);
            return this;
        }

        public void Write(IDataWriter w)
        {
            this.PreferencesNew.Sync(w);
        }
        #endregion
    }
}

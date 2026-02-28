using System;
using System.Collections.Generic;
using System.Linq;
using Project1.Framework;
using Project1.Framework.UI;
using Project1.Framework.Serialization;
using Project1.Framework.Events;
using Project1.Framework.Helpers;
using Project1.Core.Entities;
using Project1.Core.Towns.AI.Behaviors.ItemEvaluators.ItemRoles;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Core.Towns.Duties;

namespace Project1.Core
{
    [EnsureStaticCtorCall]
    public partial class ItemPreferencesManager : Inspectable, ISaveable, ISerializableNew<ItemPreferencesManager>
    {
        static List<ItemRoleDef> _flatItemRolesList;
        static readonly Dictionary<ItemRoleContextDef, List<ItemRoleDef>> ContextToItemRolesMap = [];

        Control _gui;

        readonly Actor Actor;
        readonly Dictionary<ItemRoleDef, (Entity item, int score)> PreCommitScanCache = [];

        readonly Dictionary<int, ItemBias> ItemBiases = [];
        readonly Queue<Entity> notScannedYet = [];
        readonly Dictionary<ItemRoleDef, ItemPreference> PrefsInternal = [];
        readonly Dictionary<Entity, List<ItemPreference>> ItemsToPrefs = [];
        readonly Dictionary<int, int> TempIgnore = [];
        readonly HashSet<int> ToDiscard = [];

        public ItemPreferencesManager(Actor actor)
        {
            this.Actor = actor;
        }
        private void EnqueueNewSpawnedItem(EntitySpawnedEvent e)
        {
            if (!this.TempIgnore.ContainsKey(e.Entity.RefId))
                this.notScannedYet.Enqueue(e.Entity);
        }
        Control GetGui()
        {
            var table = new Table<ItemPreference>()
                .AddColumn("role", 128, p => new Label(p.Role.LabelReadable))
                .AddColumn("item", 64, p => new Label(() => p.Item?.LabelReadable ?? "none", () => p.Item?.Select()))
                .AddColumn("score", 32, p => new Label(() => p.InventoryScore.ToString()));
            table.AddItems(this.PrefsInternal.Values);

            this.PrefUpdated += addOrUpdate;
            this.PrefRemoved += table.RemoveItem;

            void addOrUpdate(ItemPreference pref)
            {
                if (table.GetControlFromItem(pref) is Control control) control.Invalidate(true);
                else table.AddItem(pref);
            }
            
            var box = new ScrollableBoxNewNewNew(table, table.RowWidth, 200, ScrollModes.Vertical)
                .ToWindow($"{this.Actor.Name}'s Item Preferences");
            box.HideAction = () =>
            {
                this.PrefUpdated -= table.AddItem;
                this.PrefRemoved -= table.RemoveItem;
            };
            return box;
        }

        private void EvaluateOne()
        {
            if (notScannedYet.Count == 0)
                return;
            var jobs = this.Actor.ActiveDuties;
            var item = notScannedYet.Dequeue();
            if (this.Actor.Map != item.Map)
                return;
            var roles = this.Evaluate(item);
            
            foreach (var (role, score) in roles)
            {
                if (this.PreCommitScanCache.TryGetValue(role, out var cached) && score <= cached.score &&
                    this.PrefsInternal.TryGetValue(role, out var committed) && score <= committed.InventoryScore)
                    continue;

                this.PreCommitScanCache[role] = (item, score);
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
        event Action<ItemPreference> PrefUpdated;
        event Action<ItemPreference> PrefRemoved;
        internal void UpdatePref(ItemRoleDef role, Entity item, int score)
        {
            if (score <= 0)
            {
                if(this.PrefsInternal.TryGetValue(role, out var existing))
                    this.PrefRemoved?.Invoke(existing);
                this.PrefsInternal.Remove(role);
                return;
            }
            var pref = new ItemPreference(role, item, score);
            this.PrefsInternal[role] = pref;
            this.PrefUpdated?.Invoke(pref);
        }
        internal void Commit(ItemRoleDef role, Entity item, int score)
        {
            if (this.PrefsInternal.TryGetValue(role, out var oldPref))
            {
                var oldItem = oldPref.Item;
                int oldScore = oldPref.InventoryScore;
                Packets.SyncDeltas(this.Actor, [(role, oldItem, item, score)]);
                return;
            }
            var pref = new ItemPreference(role, item, score);
            this.PrefsInternal[role] = pref;
            if (!this.ItemsToPrefs.TryGetValue(item, out var list))
                this.ItemsToPrefs[item] = list = [];
            list.Add(pref);
            Packets.SyncDeltas(this.Actor, [(role, null, item, score)]);
            this.PreCommitScanCache.Remove(role);
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
        internal Entity GetExistingPreference(ItemRoleDef role, out int score)
        {
            if (this.PrefsInternal.TryGetValue(role, out var existing))
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
            if (this.PreCommitScanCache.Count == 0)
                yield break;
            var toRemove = new List<ItemRoleDef>();
            foreach (var (con, (i, score)) in this.PreCommitScanCache)
            {
                if (!StillValid(i))
                    toRemove.Add(con);
                else
                    yield return (con, i, score);
            }
            foreach (var r in toRemove)
                this.PreCommitScanCache.Remove(r);
        }

        internal IEnumerable<(ItemRoleDef role, Entity item, int score)> GetPotentialAll()
        {
            if (notScannedYet.Count == 0)
                yield break;
            var jobs = this.Actor.ActiveDuties;
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
            return this.PrefsInternal.Values.FirstOrDefault(p => p.Item == item).Role?.Def; 
            // if itempreferences are struct, then the default returned will have role == null
        }
        public Entity GetPreference(Def context)
        {
            return null;
        }
        public IEnumerable<Entity> GetUselessItems(IEnumerable<Entity> entity)
        {
            var items = this.Actor.Inventory.GetItems();
            foreach (var i in items)
                if (!this.IsUseful(i))
                    yield return i;
        }

        public void HandleItem(Entity item)
        {
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
            return this.PrefsInternal.Values.Any(p => item == p.Item);
        }
        public bool IsUseful(Entity item)
        {
            if (item.Def == ItemDefOf.Coins) // HACK
                return true;
            if (this.ItemsToPrefs.ContainsKey(item))
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

            if (this.ItemsToPrefs.TryGetValue(item, out var prefs))
                foreach (var pref in prefs)
                {
                    this.PrefsInternal.Remove(pref.Role);
                    toSync.Add(pref);
                }
            this.ItemsToPrefs.Remove(item);

            Packets.SyncDeltas(this.Actor, [.. toSync.Select(r => (r.Role, r.Item, (Entity)null, 0))]);

            foreach (var i in this.Actor.Map.Entities)
                if (i != item)
                    this.notScannedYet.Enqueue(i);
        }
        public IEnumerable<(Entity item, int score)> GetItemsBySituationalScore(Actor actor, Func<Entity, bool> filter)
        {
            var potential = this.ItemsToPrefs
                    .Where(e => filter(e.Key))
                    .SelectMany(e => e.Value);

            // TODO: For large inventories, consider replacing SortedDictionary with a simple List<(Entity, int)> + Sort()
            // to reduce allocations and overhead. Current approach is fine for typical small inventories.
            var scored = potential
                .Select(pref => (pref.Item, pref.Role.Worker.GetSituationalScore(actor, pref.Item, pref.Role)))
                .Where(t => t.Item2 != 0)
                .ToList();
            scored.Sort((a, b) => b.Item2.CompareTo(a.Item2));
            foreach (var t in scored)
                yield return t;
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
            foreach (var (pref, score) in scoredList)
                yield return pref;
        }
        public int GetTotalSituationalScoreFor(Entity item)
        {
            var relevantRoles = this.PrefsInternal.Where(pref => pref.Value.Item == item);
            int total = 0;
            foreach(var (role, pref) in relevantRoles)
                total += role.GetSituationalScore(this.Actor, item);
            return total;
        }
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
                    var olditemid = r.ReadInt32();
                    var newitemid = r.ReadInt32();
                    var olditem = olditemid > 0 ? actor.Map.World.GetEntity(olditemid) : null;
                    var newitem = newitemid > 0 ? actor.Map.World.GetEntity(newitemid) : null;
                    var score = r.ReadInt32();
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
                    this.PrefsInternal.Add(p.Role, p);
                }
            });
            this.BuildItemsToPrefsCache();
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
            tag.Add(this.PrefsInternal.Values.Save("Preferences"));
            return tag;
        }
        #endregion
        #region ISerializableNew implementations
        public static ItemPreferencesManager Create(IDataReader r) => new ItemPreferencesManager().Read(r);
        public ItemPreferencesManager()
        {
            
        }
        public ItemPreferencesManager Read(IDataReader r)
        {
            r.ReadValuesWithInferredKeys(this.PrefsInternal, r => r.Role);
            this.BuildItemsToPrefsCache();
            return this;
        }
        void BuildItemsToPrefsCache()
        {
            this.ItemsToPrefs.Clear();
            foreach(var pref in this.PrefsInternal.Values)
            {
                if(!this.ItemsToPrefs.TryGetValue(pref.Item, out var roleList))
                    this.ItemsToPrefs[pref.Item] = roleList = [];
                roleList.Add(pref);
            }
        }
        public void Write(IDataWriter w)
        {
            w.WriteValues(this.PrefsInternal);
        }
        #endregion
    }
}

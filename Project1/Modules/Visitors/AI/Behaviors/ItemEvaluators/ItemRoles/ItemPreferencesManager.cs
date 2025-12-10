using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    partial class ItemPreferencesManager : Inspectable, IItemPreferencesManager, ISaveable, ISerializableNew
    {
        // TODO: item like/dislike registry
        static ItemPreferencesManager()
        {
            Init();
        }
        static void Init()
        {
            GenerateItemRolesGear();
            GenerateItemRolesTools();

            // TODO no need to initialize each actor's preferences with empty roles?
            // TODO find way to avoid checking every role of the same type for items that are invalid for said role type, for example don't check all tool roles for non-tool items
            foreach (var r in ItemRolesTool.Values.Concat(ItemRolesGear.Values))
            {
                RegistryByName[r.ToString()] = r;
                RegistryByContext[r.Context] = r;
            }
        }
        static readonly Dictionary<string, ItemRole> RegistryByName = new();
        static readonly Dictionary<IItemPreferenceContext, ItemRole> RegistryByContext = new();

        static readonly Dictionary<GearType, ItemRole> ItemRolesGear = [];
        static readonly Dictionary<JobDef, ItemRole> ItemRolesTool = [];

        readonly Dictionary<int, ItemBias> ItemBiases = [];

        readonly Queue<Entity> notScannedYet = new();

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

        static void GenerateItemRolesGear()
        {
            var geardefs = GearType.Dictionary.Values;
            foreach (var g in geardefs)
                ItemRolesGear.Add(g, new ItemRoleGear(g));
        }
        static void GenerateItemRolesTools()
        {
            var defs = Def.Database.Values.OfType<JobDef>();
            foreach (var d in defs)
                ItemRolesTool.Add(d, new ItemRoleTool(d));
        }

        static IEnumerable<ItemRole> AllRoles => ItemRolesGear.Values.Concat(ItemRolesTool.Values);

        readonly Dictionary<IItemPreferenceContext, ItemPreference> PreferencesNew = new();
        readonly ObservableCollection<ItemPreference> PreferencesObs = new();
        readonly HashSet<int> ToDiscard = new();
        public IEnumerable<ItemPreference> Preferences => this.PreferencesObs;

        readonly Actor Actor;
        public ItemPreferencesManager(Actor actor)
        {
            this.Actor = actor;
            this.PopulateRoles();

            this.PreferencesObs.CollectionChanged += this.PreferencesObs_CollectionChanged;
        }

        public void Tick()
        {
            this.UpdateBiases();
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

        private void PreferencesObs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (this.Actor.Net is Server)
                Packets.Sync(this.Actor.Net, this.Actor, e.OldItems, e.NewItems);
        }

        private void PopulateRoles()
        {
            foreach (var r in ItemRolesTool.Values.Concat(ItemRolesGear.Values))
                this.PreferencesNew.Add(r.Context, new(r));
        }
        public (IItemPreferenceContext role, int score) FindBestRole(Entity item)
        {
            var allRoles = this.FindAllRoles(item);
            return allRoles.OrderByDescending(i => i.score).FirstOrDefault();

            //ItemPreference bestPreference = null;
            //int bestScore = -1;
            //foreach (var pref in this.PreferencesNew.Values)
            //{
            //    var role = pref.Role;
            //    var score = role.Score(this.Actor, item);
            //    if (score > bestScore)
            //    {
            //        bestPreference = pref;
            //        bestScore = score;
            //    }
            //}
            //return (bestPreference?.Role.Context, bestScore);
        }
        public IEnumerable<(IItemPreferenceContext role, int score)> FindAllRoles(Entity item)
        {
            foreach (var pref in this.PreferencesNew.Values)
            {
                var role = pref.Role;
                var score = role.Score(this.Actor, item);
                if (score > 0)
                    yield return (role.Context, score);
            }
        }

        public void HandleItem(Entity item)
        {
            foreach (var pref in this.PreferencesNew.Values)
            {
                var role = pref.Role;
                var score = role.Score(this.Actor, item);
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
        public IItemPreferenceContext GetPreference(Entity item)
        {
            return this.PreferencesNew.Values.FirstOrDefault(p => p.Item == item)?.Role.Context;
        }
        private Entity GetPreference(ItemRole role)
        {
            return this.PreferencesNew[role.Context].Item;
        }

        public Entity GetPreference(IItemPreferenceContext context, out int score)
        {
            var p = this.PreferencesNew[context];
            score = p.Score;
            return p.Item;
        }
        public Entity GetPreference(IItemPreferenceContext context)
        {
            return this.GetPreference(RegistryByContext[context]);
        }

        public void ResetPreferences()
        {
            var items = this.Actor.Inventory.GetItems();
            foreach (var i in items)
                this.HandleItem(i);
        }
        public IEnumerable<Entity> GetUselessItems(IEnumerable<Entity> entity)
        {
            var items = this.Actor.Inventory.GetItems();
            foreach (var i in items)
                if (!this.IsUseful(i))
                    yield return i;
        }
        public bool IsUseful(Entity item)
        {
            if (item.Def == ItemDefOf.Coins)
                return true;
            if (this.PreferencesNew.Values.Any(p => p.Item == item))
                return true;
            return false;
        }
        public void Validate()
        {
            this.ResetPreferences();
        }

        public IEnumerable<Entity> GetJunk()
        {
            this.Validate();
            var actor = this.Actor;
            var net = actor.Net;
            var items = actor.Inventory.GetItems();
            foreach (var i in this.ToDiscard.ToArray())
            {
                var item = net.World.GetEntity(i) as Entity;
                if (!items.Contains(item))
                {
                    this.RemoveJunk(item);
                    continue;
                }
                yield return item;
            }
        }
        public void RemoveJunk(Entity item)
        {
            this.ToDiscard.Remove(item.RefId);
        }
        public bool AddPreference(Entity item)
        {
            var scored = AllRoles
                .Select(r => (r, r.Score(this.Actor, item)))
                .Where(rs => rs.Item2 > -1)
                .OrderByDescending(rs => rs.Item2);
            if (!scored.Any())
                return false;
            var bestRole = scored.First();
            var pref = this.PreferencesNew[bestRole.r.Context];
            pref.Item = item;
            pref.Score = bestRole.Item2;
            if (!this.PreferencesObs.Contains(pref))
                this.PreferencesObs.Add(pref);
            return true;
        }
        public void AddPreference(IItemPreferenceContext context, Entity item, int score)
        {
            var pref = this.PreferencesNew[context];
            pref.Item = item;
            pref.Score = score;
            //item.Ownership.Owner = this.Actor;
            if (this.PreferencesObs.Contains(pref))
                this.PreferencesObs.Remove(pref);
            this.PreferencesObs.Add(pref); // HACK to trigger observable syncing
        }

        public void RemovePreference(IItemPreferenceContext tag)
        {
            this.PreferencesNew[tag].Clear();
        }
        public bool IsPreference(Entity item)
        {
            return this.PreferencesNew.Values.Any(p => item == p.Item);
        }

        public int GetScore(IItemPreferenceContext context, Entity item)
        {
            return RegistryByContext[context].Score(this.Actor, item);
        }

        Control _gui;
        private Action _unsubscribe;

        public Control Gui => this._gui ??= this.GetGui();
        Control GetGui()
        {
            var table = new TableObservable<ItemPreference>()
                .AddColumn("role", 128, p => new Label(p.Role.Context))
                .AddColumn("item", 128, p => new Label(() => p.Item?.DebugName ?? "none", () => p.Item?.Select()))
                .AddColumn("score", 64, p => new Label(() => p.Score.ToString()))
                .Bind(this.PreferencesObs);
            var box = new ScrollableBoxNewNew(table.RowWidth, table.RowHeight * 16, ScrollModes.Vertical)
                .AddControls(table)
                .ToWindow($"{this.Actor.Name}'s Item Preferences");
            return box;
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(this.PreferencesNew.Values.Where(p => p.Item is not null).Save("Preferences"));
            return tag;
        }

        public ISaveable Load(SaveTag tag)
        {
            tag.TryGetTag("Preferences", pt =>
            {
                foreach (var p in pt.LoadList<ItemPreference>())
                {
                    var existing = this.PreferencesNew[p.Role.Context];
                    existing.CopyFrom(p);
                    this.PreferencesObs.Add(existing);
                }
            });

            return this;
        }

        public void Write(IDataWriter w)
        {
            foreach (var r in this.PreferencesNew.Values)
                r.Write(w);
        }

        public ISerializableNew Read(IDataReader r)
        {
            foreach (var p in this.PreferencesNew)
                p.Value.Read(r);
            foreach (var p in this.PreferencesNew.Where(t => t.Value.Score > 0))
                this.PreferencesObs.Add(p.Value);
            return this;
        }

        void SyncAddPref(ItemPreference pref)
        {
            if (this.Actor.Net is not Client)
                throw new Exception();
            var existing = this.PreferencesNew[pref.Role.Context];
            existing.CopyFrom(pref);
            existing.ResolveReferences(this.Actor);
            if (!this.PreferencesObs.Contains(existing))
                this.PreferencesObs.Add(existing);
        }
        void SyncRemovePref(ItemPreference pref)
        {
            if (this.Actor.Net is not Client)
                throw new Exception();
            var existing = this.PreferencesNew[pref.Role.Context];
            existing.Clear();
            if (this.PreferencesObs.Contains(existing))
                this.PreferencesObs.Remove(existing);
        }

        public void ResolveReferences()
        {
            foreach (var p in this.PreferencesObs)
                p.ResolveReferences(this.Actor);
        }

        
        public Control GetListControl(Entity entity)
        {
            var p = this.GetPreference(entity);
            return new Label(p) { HoverText = $"[{this.Actor.Name}] prefers [{entity.Name}] for [{p}]" };
        }

        public static ISerializableNew Create(IDataReader r) => new ItemPreference().Read(r);

        public IEnumerable<(IItemPreferenceContext role, Entity item, int score)> GetPotential()
        {
            if (notScannedYet.Count == 0)
                yield break;
            var jobs = this.Actor.GetJobs();
            var dic = new Dictionary<IItemPreferenceContext, (Entity item, int score)>();
            while(notScannedYet.Count > 0)
            {
                var item = notScannedYet.Dequeue();
                if (this.Actor.Map != item.Map)
                    continue;
                var roles = this.FindAllRoles(item);
                if (!roles.Any())
                    continue;
                var finalRoles = roles
                    .Where(r => jobs.Any(j => j.Enabled && j.Def == r.role))
                    .Where(r => this.GetPreference(r.role, out var existingScore) is var existing && r.score > existingScore);


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

            foreach (var vk in dic)
                yield return (vk.Key, vk.Value.item, vk.Value.score);
        }
        public void OnMapLoaded()
        {
            // HACK TO JUMP START THE SCANNING SYSTEM
            foreach (var i in this.Actor.Map.GetEntities<Tool>())
                this.notScannedYet.Enqueue(i);
        }
        public void OnSpawn(MapBase newMap)
        {
            foreach (var i in newMap.GetEntities<Tool>())
                this.notScannedYet.Enqueue(i);
            newMap.Events.ListenTo<EntitySpawnedEvent>(enqueueNewSpawnedItem);
        }
        public void OnDespawn(MapBase oldMap)
        {
            this.notScannedYet.Clear();
            oldMap.Events.Unsubscribe(this);
        }
        private void enqueueNewSpawnedItem(EntitySpawnedEvent e)
        {
            this.notScannedYet.Enqueue(e.Entity);
        }

        [EnsureStaticCtorCall]
        static class Packets
        {
            static readonly int pSyncPrefsAll;
            static Packets()
            {
                pSyncPrefsAll = Registry.PacketHandlers.Register(Receive);
            }

            internal static void Sync(NetEndpoint net, Actor actor, System.Collections.IList oldItems, System.Collections.IList newItems)
            {
                var w = net.BeginPacket(pSyncPrefsAll);

                w.Write(actor.RefId);

                if (oldItems is null)
                    w.Write(0);
                else
                    oldItems.Cast<ItemPreference>().ToList().WriteNew(w);

                if (newItems is null)
                    w.Write(0);
                else
                    newItems.Cast<ItemPreference>().ToList().WriteNew(w);
            }

            private static void Receive(INetEndpoint net, Packet pck)
            {
                if (net is Server)
                    throw new Exception();
                var r = pck.PacketReader;

                var actor = net.World.GetEntity<Actor>(r.ReadInt32());
                var prefs = actor.ItemPreferences as ItemPreferencesManager;
                var oldItems = new List<ItemPreference>().Read(r);
                var newItems = new List<ItemPreference>().Read(r);
              
                foreach (var p in oldItems)
                    prefs.SyncRemovePref(p);
                foreach (var p in newItems)
                    prefs.SyncAddPref(p);
            }
        }
    }
}

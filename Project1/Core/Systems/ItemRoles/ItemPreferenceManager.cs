using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Events;
using Project1.Core.Helpers;
using Project1.Core.Networking;
using Project1.Core.Simulation;
using Project1.Core.Systems.Inventory;
using Project1.Framework;
using Project1.Framework.Events;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace Project1.Core.Systems.ItemRoles;

public sealed class ItemPreferenceManager : EventOwner, ISaveable, ISerializableNew<ItemPreferenceManager>
{
    protected override StaticWorld World => field ??= (StaticWorld)this.Actor.World;

    event Action<ItemPreference> PrefUpdated;

    event Action<ItemPreference> PrefRemoved;

    readonly ChangeNotifier Notifier = new();

    readonly Actor Actor;

    readonly Dictionary<ItemRoleDef, (Entity item, int score)> PreCommitScanCache = [];

    readonly Dictionary<EntityRefId, ItemBias> ItemBiases = [];

    public Control Gui => field ??= this.GetGui();

    readonly Dictionary<ItemRoleDef, ItemPreference> PrefsInternal = [];

    readonly Dictionary<EntityRefId, List<ItemPreference>> ItemsToPrefs = [];

    readonly Dictionary<EntityRefId, int> TempIgnore = [];

    readonly HashSet<EntityRefId> ToDiscard = [];
   
    Entity Resolve(EntityRefId id)
        => this.Actor.World.Resolve(id);

    public ItemPreferenceManager(Actor actor)
    {
        this.Actor = actor;
    }

    Window GetGui()
    {
        var table = new Table<ItemPreference>()
            .AddColumn("role", 128, p => new Label(p.Role.LabelReadable))
            //.AddColumn("item", 64, p => new Label(() => p.Item?.LabelReadable ?? "none", () => p.Item?.Select()))
            .AddColumn("item", 64, p => new Label(() => this.Actor.World.Resolve(p.ItemRefId)?.LabelReadable ?? "none", () => this.Actor.World.Resolve(p.ItemRefId)?.Select()))
            .AddColumn("score", 32, p => new Label(() => p.InventoryScore.ToString()));
        table.AddItems(this.PrefsInternal.Values);

        this.PrefUpdated += addOrUpdate;
        this.PrefRemoved += table.RemoveItem;

        void addOrUpdate(ItemPreference pref)
        {
            if (table.GetControlFromItem(pref) is Control control) 
                control.Invalidate(true);
            else 
                table.AddItem(pref);
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

    internal void TryPreCommit(Entity item, ItemEvaluation result)
    {
        if (this.IsIgnored(item))
            return;

        foreach (var (role, score) in result.Roles)
            if (this.Compare(role, score))
                this.PreCommitScanCache[role] = (item, score);
    }

    private bool Compare(ItemRoleDef role, int score)
    {
        if (!this.PreCommitScanCache.TryGetValue(role, out var cached) || score > cached.score)
            return true;
        if (!this.PrefsInternal.TryGetValue(role, out var committed) || score > committed.InventoryScore)
            return true;
        return false;
    }

    public bool Wants(Entity item)
    {
        //var result = this.EvaluateAndRegister(item);
        if (this.IsIgnored(item))
            return false;

        var result = ItemEvaluationSystem.GetOrEvaluate(this.Actor, item);
        foreach (var (role, score) in result.Roles)
        {
            if (this.Compare(role, score))
                return true;
        }
        return false;
    }

    private bool StillValid(Entity i)
    {
        return i.ExistsOn(this.Actor.Map);
    }

    private void UpdateBiases()
    {
        List<EntityRefId> toRemove = [];

        foreach (var (key, bias) in this.ItemBiases)
            if (bias.Tick() == 0)
                toRemove.Add(key);

        foreach (var key in toRemove)
            this.ItemBiases.Remove(key);
    }

    private void UpdateTempIgnore()
    {
        List<EntityRefId> toRemove = [];

        foreach (var (key, cooldown) in this.TempIgnore)
            if (cooldown == 0)
                toRemove.Add(key);
            else
                this.TempIgnore[key] = cooldown - 1;

        foreach (var key in toRemove)
        {
            this.TempIgnore.Remove(key);

            var item = this.Actor.Map.World.Resolve(key);
            if (item.ExistsOn(this.Actor.Map))
            {
                throw new NotImplementedException();
                //this.TryEnqueue(item);
            }
        }
    }

    internal bool IsIgnored(Entity item)
        => this.TempIgnore.ContainsKey(item.RefId);


    internal bool TryCommit(Entity item)
    {
        if (this.IsIgnored(item))
            return false;

        if (item.World is null)
            throw new InvalidOperationException($"Tried to evaluate disposed item {item}");
        //var results = this.EvaluateAndRegister(item);
        var results = ItemEvaluationSystem.GetOrEvaluateAndRegister(this.Actor, item);
        bool commited = false;
        // commit item for roles where it scores higher than existing items
        foreach (var (Role, Score) in results.Roles)
        {
            if (this.PrefsInternal.TryGetValue(Role, out var existing) && existing.InventoryScore >= Score)
                continue;
            commited = true;
            this.Commit(Role, item, Score);
        }
        return commited;
    }
    
    internal void Commit(ItemRoleDef role, Entity item, int score)
    {
        EntityRefId oldItemId = EntityRefId.Null;
        if (this.PrefsInternal.TryGetValue(role, out var oldPref))
        {
            oldItemId = oldPref.ItemRefId;
            //var oldItem = this.Actor.World.Resolve(oldItemId);
            //this.ItemsToPrefs.Remove(oldItem);
            var existingList = this.ItemsToPrefs[oldItemId];
            existingList.RemoveAll(p => p.Role == oldPref.Role);
            if (existingList.Count == 0)
                this.ItemsToPrefs.Remove(oldItemId);
        }
        var pref = new ItemPreference(role, item, score);
        this.PrefsInternal[role] = pref;
        if (!this.ItemsToPrefs.TryGetValue(item.RefId, out var list))
            this.ItemsToPrefs[item.RefId] = list = [];
        list.Add(pref);
        this.Notifier.Notify();
        //Packets_ItemPrefs.SyncDeltas(this.Actor, [(role, oldItem, item, score)]);
        this.Actor.World.Events.Post(new ItemPrefsDeltaEvent(this.Actor, [(role, oldItemId, item.RefId, score)]));
        this.PreCommitScanCache.Remove(role);
    }
  
    internal void ApplyPreferenceDelta(ItemRoleDef role, EntityRefId itemId, int score)
    {
        if (this.PrefsInternal.TryGetValue(role, out var existing))
        {
            this.RemoveFromItemsToPrefs(existing);

            if (score <= 0)
            {
                this.PrefsInternal.Remove(role);
                this.PrefRemoved?.Invoke(existing);
                this.Notifier.Notify();
                return;
            }
        }
        else if (score <= 0)
            //return;
            throw new InvalidOperationException(
                $"Cannot remove preference for role '{role}' because no preference exists.");
        if (itemId == EntityRefId.Null)
            throw new InvalidCastException(
                $"Tried to assign a preference with a null {nameof(itemId)} for role '{role}' with score '{score}'.");
        var pref = new ItemPreference(role, itemId, score);
        this.PrefsInternal[role] = pref;
        if (!this.ItemsToPrefs.TryGetValue(itemId, out var list))
            this.ItemsToPrefs[itemId] = list = [];

        list.Add(pref);

        this.PrefUpdated?.Invoke(pref);
        this.Notifier.Notify();
    }

    void RemoveFromItemsToPrefs(ItemPreference pref)
    {
        //var item = this.Resolve(pref.ItemRefId);
        if (!this.ItemsToPrefs.TryGetValue(pref.ItemRefId, out var list))
            return;

        list.RemoveAll(p => p.Role == pref.Role);

        if (list.Count == 0)
            this.ItemsToPrefs.Remove(pref.ItemRefId);
    }

    internal int GetExistingScore(ItemRoleDef role)
        => this.PrefsInternal.TryGetValue(role, out var pref) ? pref.InventoryScore : 0;

    [Obsolete]
    internal (ItemRoleDef role, int score) FindBestRole(Entity item)
    {
        throw new NotImplementedException();

        //var allRoles = this.EvaluateWithoutRegistering(item);
        //return allRoles.OrderByDescending(i => i.score).FirstOrDefault();
    }

    internal Entity? GetExistingPreference(ItemRoleDef role, out int score)
    {
        if (this.PrefsInternal.TryGetValue(role, out var existing))
        {
            score = existing.InventoryScore;
            //return existing.Item;
            return this.Resolve(existing.ItemRefId);
        }
        score = 0;
        return null;
    }

    internal IEnumerable<(ItemRoleDef role, Entity item, int score)> TryGetPotential()
    {
        //if (this.IsScanning)
        //{
        //    yield break;
        //}
        foreach (var v in this.GetPotentialInt())
            yield return v;
    }

    internal IEnumerable<(ItemRoleDef role, Entity item, int score)> GetPotentialInt()
    {
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
        // if there's been invalidations, rescan map
        //if (toRemove.Count > 0)
        //{
        //    foreach (var item in this.Actor.Map.Entities)
        //        this.TryEnqueue(item);
        //}
        foreach (var r in toRemove)
            this.PreCommitScanCache.Remove(r);
    }

    private void RemovePreferencesForItem(EntityRefId itemId)
    {
        if (!this.ItemsToPrefs.TryGetValue(itemId, out var prefs))
            return;

        List<(ItemRoleDef role, EntityRefId oldItem, EntityRefId newItem, int score)> deltas = [];

        foreach (var pref in prefs)
        {
            this.PrefsInternal.Remove(pref.Role);
            this.PrefRemoved?.Invoke(pref);

            deltas.Add((pref.Role, itemId, EntityRefId.Null, 0));
        }

        this.ItemsToPrefs.Remove(itemId);

        this.Notifier.Notify();
        this.Actor.World.Events.Post(new ItemPrefsDeltaEvent(this.Actor, [.. deltas]));
    }

    public IEnumerable<Entity> GetJunk()
    {
        //this.Validate();
        var actor = this.Actor;
        var net = actor.Net;
        var items = actor.Inventory.GetItems();
        foreach (var i in this.ToDiscard.ToArray())
        {
            var item = net.World.Resolve(i);
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
        return new LabelNew(()=>
        {
            return this.GetPreference(entity);
        })
            .InvalidateOn(this.Notifier);
    }
    // TODO decide how to display multiple roles for one item
    public ItemRoleDef? GetPreference(Entity item)
    {
        //var pref = this.PrefsInternal.Values.FirstOrDefault(p => p.Item == item);
        var pref = this.PrefsInternal.Values.FirstOrDefault(p => p.ItemRefId == item.RefId);
        return pref.Role;
        // if itempreferences are struct, then the default returned will have role == null
    }
    //// TODO decide how to display multiple roles for one item
 

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

    public bool IsPreference(Entity item)
    {
        //return this.PrefsInternal.Values.Any(p => item == p.Item);
        return this.PrefsInternal.Values.Any(p => item.RefId == p.ItemRefId);
    }

    public bool IsUseful(Entity item)
    {
        if (this.ItemsToPrefs.ContainsKey(item.RefId))
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

    public void ForceDrop(Entity item)
    {
        this.ModifyBias(item, -200);
        this.TempIgnore[item.RefId] = (int)Ticks.FromSeconds(10);
        this.RemovePreferencesForItem(item.RefId);
    }

    internal void DiscardPotential(Entity item)
    {
        this.ModifyBias(item, -200);

        foreach(var key in this.PreCommitScanCache.Keys.ToArray())
        {
            if (this.PreCommitScanCache[key].item == item)
                this.PreCommitScanCache.Remove(key);
        }

        // TODO sync precommit cache?

        //foreach (var i in this.Actor.Map.Entities)
        //    if (i != item)
        //        this.TryEnqueue(i);
    }

    public IEnumerable<(Entity item, int score)> GetItemsBySituationalScore(Actor actor, Func<Entity, bool> filter)
    {
        // TODO: maybe filter out items that are not in inventory??
        var potential = this.ItemsToPrefs
                .Where(e => filter(this.Resolve(e.Key)))
                .SelectMany(e => e.Value);

        var scored = potential
            //.Select(pref => (pref.Item, pref.Role.Worker.GetSituationalScore(actor, pref.Item, pref.Role)))
            .Select(pref => (this.Resolve(pref.ItemRefId), pref.Role.Worker.GetSituationalScore(actor, this.Resolve(pref.ItemRefId))))
            .Where(t => t.Item2 != 0)
            .ToList();
        scored.Sort((a, b) => b.Item2.CompareTo(a.Item2));
        foreach (var t in scored)
            yield return t;
    }

    internal IEnumerable<EquipStatusDecision> GetItemsBySituationalScoreNewNew(Actor actor, Func<Entity, bool> filter)
    {
        var potential = this.ItemsToPrefs
                .Select(entry => (item: this.Resolve(entry.Key), scores: entry.Value))
                .Where(e => filter(e.item))
                .Select(e => (e.item, sum: e.scores.Sum(p => p.Role.Worker.GetSituationalScore(actor, e.item))));

        foreach (var t in potential.OrderByDescending(a => a.sum))
            yield return new(t.item.RefId, t.sum);
    }

    public int GetTotalSituationalScoreFor(Entity item)
    {
        //var relevantRoles = this.PrefsInternal.Where(pref => pref.Value.Item == item);
        var relevantRoles = this.PrefsInternal.Where(pref => pref.Value.ItemRefId == item.RefId);
        int total = 0;
        foreach(var (role, pref) in relevantRoles)
            total += role.Worker.GetSituationalScore(this.Actor, item);
        return total;
    }

    public void RemoveJunk(Entity item)
    {
        this.ToDiscard.Remove(item.RefId);
    }

    public void Tick()
    {
        this.UpdateBiases();
        this.UpdateTempIgnore();
    }

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

    internal void OnAttachedToWorld()
    {
        // TODO dont do this because this will get called
        // whether the actor and/or the item are in seperate maps/overworld locations
        //this.Actor.World.Events.ListenTo<ItemOwnerChangedEvent>(HandleItemOwnerChanged);
        this.Actor.World.Events.ListenTo<InventoryItemAddedEvent>(OnInventoryItemAdded);
        this.Actor.World.Events.ListenTo<EntityDisposedEvent>(OnEntityDisposed);
        // TODO initial inventory scan
        //this.InitialInventoryScan();
    }

    internal void OnDispose()
        => this.Actor.World.Events.Unsubscribe(this);

    private void OnEntityDisposed(EntityDisposedEvent e)
    {
        if (e.Entity == this.Actor)
            return;

        if (this.Actor.Net?.IsClient == true)
            return;

        this.RemovePreferencesForItem(e.Entity.RefId);
    }

    private void OnInventoryItemAdded(InventoryItemAddedEvent e)
    {
        if (this.Actor.Net.IsClient)
            return;
        if (e.Actor != this.Actor)
            return;
        //this.EnqueueItemInt(e.Item);
        this.TryCommit(e.Item);
    }

    public SaveTag Save(string name = "")
    {
        var tag = new SaveTag(SaveTag.Types.Compound, name);
        tag.Add(this.PrefsInternal.Values.Save("Preferences"));
        return tag;
    }

    public static ItemPreferenceManager Create(IDataReader r) => new ItemPreferenceManager().Read(r);

    public ItemPreferenceManager()
    {
        
    }

    public void Write(IDataWriter w)
    {
        w.WriteValues(this.PrefsInternal);
    }

    public ItemPreferenceManager Read(IDataReader r)
    {
        r.ReadValuesWithInferredKeys(this.PrefsInternal, r => r.Role);
        this.BuildItemsToPrefsCache();
        return this;
    }

    void BuildItemsToPrefsCache()
    {
        this.ItemsToPrefs.Clear();
        foreach (var pref in this.PrefsInternal.Values)
        {
            //if(!this.ItemsToPrefs.TryGetValue(pref.Item, out var roleList))
            //    this.ItemsToPrefs[pref.Item] = roleList = [];
            //var resolved = this.Resolve(pref.ItemRefId);
            if (!this.ItemsToPrefs.TryGetValue(pref.ItemRefId, out var roleList))
                this.ItemsToPrefs[pref.ItemRefId] = roleList = [];
            roleList.Add(pref);
        }
    }
}

using Start_a_Town_.Net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public partial class ItemPreferencesManager
    {
        public sealed class ItemPreference : Inspectable, ISaveable, ISerializableNew<ItemPreference>
        {
            internal ItemRoleDef Role;
            int _itemRefId;
            public int ItemRefId
            {
                get => this.Item?.RefId ?? this._itemRefId;
                private set { this._itemRefId = value; }
            }
            public Entity Item;
            public int InventoryScore;

            public ItemPreference()
            {

            }
            internal ItemPreference(ItemRoleDef role)
            {
                this.Role = role;
            }
            public void CopyFrom(ItemPreference pref)
            {
                if (this.Role != pref.Role)
                    throw new Exception();
                this.Item = pref.Item;
                this.ItemRefId = pref.ItemRefId;
                this.InventoryScore = pref.InventoryScore;
            }
            public override string ToString()
            {
                return $"{Role}" + (this.Item is not null ? $":{this.Item.DebugName}:{InventoryScore}" : "");
            }

            public void Write(IDataWriter w)
            {
                w.Write(this.Role.ToString());
                w.Write(this.ItemRefId);
                w.Write(this.InventoryScore);
            }

            public ItemPreference Read(IDataReader r)
            {
                this.Role = r.ReadDef<ItemRoleDef>();// RegistryByName[r.ReadString()];
                this.ItemRefId = r.ReadInt32();
                this.InventoryScore = r.ReadInt32();
                return this;
            }

            public SaveTag Save(string name = "")
            {
                var tag = new SaveTag(SaveTag.Types.Compound, name);
                this.Role.ToString().Save(tag, "Role");
                this.ItemRefId.Save(tag, "ItemRefId");
                this.InventoryScore.Save(tag, "Score");
                return tag;
            }

            public ISaveable Load(SaveTag tag)
            {
                //this.Role = RegistryByName[(string)tag["Role"].Value];
                this.Role = tag.LoadDef<ItemRoleDef>("Role");
                this.ItemRefId = (int)tag["ItemRefId"].Value;
                this.InventoryScore = (int)tag["Score"].Value;
                return this;
            }

            internal void Clear()
            {
                this.Item = null;
                this.ItemRefId = 0;
                this.InventoryScore = 0;
            }

            internal void ResolveReferences(Actor actor)
            {
                this.Item = actor.World.GetEntity<Entity>(this.ItemRefId);
                this.Refresh(actor);
            }

            void Refresh(Actor actor)
            {
                if (actor.Net is Client)
                    return;
                this.InventoryScore = this.Role.Worker.GetInventoryScore(actor, this.Item, this.Role);
            }

            public static ItemPreference Create(IDataReader r) => new ItemPreference().Read(r);
        }
        public void SyncPrefs(System.Collections.Generic.ICollection<ItemPreference> oldItems, System.Collections.Generic.ICollection<ItemPreference> newItems)
        {
            foreach (var old in oldItems)
            {
                if (!this.PreferencesNew.TryGetValue(old.Role, out var existing))
                    throw new Exception();
                this.PreferencesNew.Remove(old.Role);
            }
            foreach (var newPref in newItems)
            {
                if (!this.PreferencesNew.TryGetValue(newPref.Role, out var existing))
                {
                    existing = new(newPref.Role);
                    this.PreferencesNew[newPref.Role] = existing;
                }
                existing.CopyFrom(newPref);
                existing.ResolveReferences(this.Actor);
            }
        }
        internal void ApplyDelta(ItemRoleDef role, Entity olditem, Entity newitem, int score)
        {
            if (newitem is null)
                this.PreferencesNew.Remove(role);
            else
            {
                if (!this.PreferencesNew.TryGetValue(role, out var pref))
                    pref = new(role);
                pref.Item = newitem;
                pref.InventoryScore = score;
                this.PreferencesNew[role] = pref;
            }
        }
        public IEnumerable<(Entity item, int score)> GetItemsBySituationalScore(Actor actor, Func<Entity, bool> filter)
        {
            var potential = this.PreferencesNew.Values.Where(p => filter(p.Item));
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
    }
}

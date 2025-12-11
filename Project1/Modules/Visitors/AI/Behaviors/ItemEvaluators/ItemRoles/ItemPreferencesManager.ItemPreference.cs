using Start_a_Town_.Net;
using System;

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
            public int Score;

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
                this.Score = pref.Score;
            }
            public override string ToString()
            {
                return $"{Role}" + (this.Item is not null ? $":{this.Item.DebugName}:{Score}" : "");
            }

            public void Write(IDataWriter w)
            {
                w.Write(this.Role.ToString());
                w.Write(this.ItemRefId);
                w.Write(this.Score);
            }

            public ItemPreference Read(IDataReader r)
            {
                this.Role = r.ReadDef<ItemRoleDef>();// RegistryByName[r.ReadString()];
                this.ItemRefId = r.ReadInt32();
                this.Score = r.ReadInt32();
                return this;
            }

            public SaveTag Save(string name = "")
            {
                var tag = new SaveTag(SaveTag.Types.Compound, name);
                this.Role.ToString().Save(tag, "Role");
                this.ItemRefId.Save(tag, "ItemRefId");
                this.Score.Save(tag, "Score");
                return tag;
            }

            public ISaveable Load(SaveTag tag)
            {
                //this.Role = RegistryByName[(string)tag["Role"].Value];
                this.Role = tag.LoadDef<ItemRoleDef>("Role");
                this.ItemRefId = (int)tag["ItemRefId"].Value;
                this.Score = (int)tag["Score"].Value;
                return this;
            }

            internal void Clear()
            {
                this.Item = null;
                this.ItemRefId = 0;
                this.Score = 0;
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
                this.Score = this.Role.Worker.GetInventoryScore(actor, this.Item, this.Role);
            }

            public static ItemPreference Create(IDataReader r) => new ItemPreference().Read(r);
        }
        public void SyncPrefs(System.Collections.Generic.ICollection<ItemPreference> oldItems, System.Collections.Generic.ICollection<ItemPreference> newItems)
        {
            foreach(var old in oldItems)
            {
                if (!this.PreferencesNew.TryGetValue(old.Role, out var existing))
                    throw new Exception();
                this.PreferencesNew.Remove(old.Role);
            }
            foreach(var newPref in newItems)
            { 
                if(!this.PreferencesNew.TryGetValue(newPref.Role, out var existing))
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
                pref.Score = score;
                this.PreferencesNew[role] = pref;
            }
            //if (olditem is not null)
            //{
            //    if (!this.PreferencesNew.TryGetValue(role, out var pref))
            //        throw new Exception();
            //    if (newitem is not null)
            //    {
            //        pref.Item = newitem;
            //        pref.Score = score;
            //    }
            //    else
            //        this.PreferencesNew.Remove(role);
            //}
            //else
            //{
            //    if (this.PreferencesNew.ContainsKey(role))
            //        throw new Exception();
            //    if (newitem is null)
            //        throw new Exception();
            //    this.PreferencesNew[role] = new(role) { Item = newitem, Score = score };
            //}
        }
    }
}

using Start_a_Town_.Net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public partial class ItemPreferencesManager
    {
        //public sealed class ItemPreference : Inspectable, ISaveable, IDictionarySyncable<ItemRoleDef, ItemPreference>// ISerializableNew<ItemPreference>, ICopyable<ItemPreference>, IKeyable<ItemRoleDef>
        public struct ItemPreference : ISaveable, ISaveableNewNew<ItemPreference>, IDictionarySyncable<ItemRoleDef, ItemPreference>// ISerializableNew<ItemPreference>, ICopyable<ItemPreference>, IKeyable<ItemRoleDef>
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
            internal void Update(Entity item, int score)
            {
                this.Item = item;
                this.InventoryScore = score;
            }
            public ItemPreference CopyFrom(ItemPreference source)
            {
                if (this.Role != source.Role)
                    throw new Exception();
                this.Item = source.Item;
                this.ItemRefId = source.ItemRefId;
                this.InventoryScore = source.InventoryScore;
                return this;
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
            static public ItemPreference Create(SaveTag tag) => (ItemPreference)new ItemPreference().Load(tag);

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
            public void Set(Entity entity, int score)
            {
                this.Item = entity;
                this.InventoryScore = score;
            }

            public ItemRoleDef GetKey() => this.Role;
            public bool ShouldCopy() => this.Item is not null;
        }
        
        internal void ApplyDelta(ItemRoleDef role, Entity olditem, Entity newitem, int score)
        {
            if (newitem is null)
                this.PreferencesNew[role].Set(null, 0);
            else
                this.PreferencesNew[role].Set(newitem, score);
        }
        
    }
}

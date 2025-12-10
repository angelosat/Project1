using Start_a_Town_.Net;
using System;
using System.IO;

namespace Start_a_Town_
{
    partial class ItemPreferencesManager
    {
        public sealed class ItemPreference : Inspectable, ISaveable, ISerializableNew
        {
            public ItemRole Role;
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
            public ItemPreference(ItemRole role)
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
                return $"{Role}"+ (this.Item is not null ? $":{this.Item.DebugName}:{Score}" : "");
            }

            public void Write(IDataWriter w)
            {
                w.Write(this.Role.ToString());
                w.Write(this.ItemRefId);
                w.Write(this.Score);
            }

            public ISerializableNew Read(IDataReader r)
            {
                this.Role = RegistryByName[r.ReadString()];
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
                this.Role = RegistryByName[(string)tag["Role"].Value];
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
                this.Score = this.Role.Score(actor, this.Item);
            }

            public static ISerializableNew Create(IDataReader r) => new ItemPreference().Read(r);
        }
    }
}

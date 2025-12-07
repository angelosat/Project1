using Start_a_Town_.UI;
using System.IO;
using System.Runtime.CompilerServices;

namespace Start_a_Town_
{
    public class EntityEffectWrapper : ISaveableNew, ISerializableNew
    {
        internal EffectDef Def;
        EntityEffectWrapper()
        {
            
        }
        public EntityEffectWrapper(EffectDef def)
        {
            this.Def = def;
        }

        public static ISaveableNew Create(SaveTag tag)
        {
            var e = new EntityEffectWrapper();
            //tag.TryGetTagValue<string>("Def", t => e.Def = Start_a_Town_.Def.GetDef<EffectDef>(t));
            tag.TryLoadDef("Def", ref e.Def);
            return e;
        }

        public static ISerializableNew Create(IDataReader r)
        {
            var e = new EntityEffectWrapper();
            //e.Def = Start_a_Town_.Def.GetDef<EffectDef>(r.ReadString());
            e.Def = r.ReadDef<EffectDef>();
            return e;
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.Def.Save(tag, "Def");
            return tag;
        }

        public void Write(BinaryWriter w)
        {
            this.Def.Write(w);
        }

        public Control GetGui()
        {
            return new Label($"Effect: {this.Def.Name}");
        }
    }
}

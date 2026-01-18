using Start_a_Town_.UI;
using System.IO;

namespace Start_a_Town_
{
    public record EntityEffectWrapper(EffectDef Def, Def Target, float Value) : ISaveableNew, ISerializableNew<EntityEffectWrapper>
    {
        //internal EffectDef Def;
        //internal Def Target;
        //internal float Value;
        //EntityEffectWrapper()
        //{
            
        //}
        //public EntityEffectWrapper(EffectDef def)
        //{
        //    this.Def = def;
        //}
        internal void Start(Actor actor) => this.Def.Worker.OnStart(actor, this);
        internal void Finish(Actor actor) => this.Def.Worker.OnFinish(actor, this);
        
        public static ISaveableNew Create(SaveTag tag)
        {
            var def = tag.LoadDef<EffectDef>("Def");
            var target = tag.LoadDef<Def>("Target");
            var value = tag.LoadSingle("Value");
            return new EntityEffectWrapper(def, target, value);
            //var e = new EntityEffectWrapper();
            ////tag.TryGetTagValue<string>("Def", t => e.Def = Start_a_Town_.Def.GetDef<EffectDef>(t));
            //tag.TryLoadDef("Def", ref e.Def);
            //return e;
        }

        public static EntityEffectWrapper Create(IDataReader r)
        {
            var def = r.ReadDef<EffectDef>();
            var target = r.ReadDef();
            var value = r.ReadSingle();
            return new(def, target, value);
            //var e = new EntityEffectWrapper();
            //e.Def = r.ReadDef<EffectDef>();
            //return e;
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.Def.Save(tag, "Def");
            this.Target.Save(tag, "Target");
            this.Value.Save(tag, "Value");
            return tag;
        }

        public Control GetGui()
        {
            return new Label($"Effect: {this.Def.Name}");
        }

        public void Write(IDataWriter w)
        {
            w.Write(this.Def);
            w.Write(this.Target);
            w.Write(this.Value);
        }

        public EntityEffectWrapper Read(IDataReader r) => throw new System.Exception();// new EntityEffectWrapper().Read(r);
    }
}

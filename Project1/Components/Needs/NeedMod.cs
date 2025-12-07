using System.IO;

namespace Start_a_Town_
{
    public class NeedMod : ISerializable, ISaveable
    {
        public EffectDef Def;
        public float RateMod;
        public float ValueMod;
        public NeedMod()
        {

        }
        /// <summary>
        /// Creates a modifier that adjusts the associated metric each tick.
        /// The Rate is applied per tick; duration is managed by the effect that uses this modifier.
        /// </summary>
        /// <param name="valuePerTick">Signed value to apply per tick.</param>
        public NeedMod(EffectDef needLetDef, float valuePerTick)
        {
            this.Def = needLetDef;
            this.RateMod = valuePerTick;
            //this.RateMod = rate;
            //this.ValueMod = value;
        }

        public override string ToString()
        {
            //return string.Format("{0}: ValueMod: {1:+#;-#;0} RateMod: {2:+#;-#;0}", this.Def.Name, this.ValueMod, this.RateMod);
            return $"{this.Def.Name}: ValueMod: {this.ValueMod:+#;-#;0} RateMod: {this.RateMod:+#;-#;0}";
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(this.Def.Name.Save("Def"));
            tag.Add(this.RateMod.Save("RateMod"));
            tag.Add(this.ValueMod.Save("ValueMod"));

            return tag;
        }

        public ISaveable Load(SaveTag tag)
        {
            tag.LoadDef<EffectDef>("Def");
            tag.TryGetTagValueOrDefault<float>("RateMod", out this.RateMod);
            tag.TryGetTagValueOrDefault<float>("ValueMod", out this.ValueMod);

            return this;
        }

        public void Write(BinaryWriter w)
        {
            w.Write(this.Def.Name);
            w.Write(this.RateMod);
            w.Write(this.ValueMod);
        }

        public ISerializable Read(IDataReader r)
        {
            this.Def = r.ReadDef<EffectDef>();
            this.RateMod = r.ReadSingle();
            this.ValueMod = r.ReadSingle();
            return this;
        }
    }
}

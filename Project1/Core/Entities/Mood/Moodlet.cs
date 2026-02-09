using Microsoft.Xna.Framework;
using Project1.Core.Base;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Interfaces;
using Project1.Framework;
using Project1.Framework.Interfaces;
using Project1.Framework.Serialization;
using Project1.Framework.UI;

namespace Project1.Core.Entities.Mood
{
    public sealed class Moodlet : ISaveable, ISerializableNew<Moodlet>, INamed
    {
        public enum Modes { Finite, Indefinite }

        public MoodletDef Def { get; private set; }
        int TicksRemaining;

        public string Name => this.Def.Name;

        public bool Tick(Actor parent)
        {
            if (!this.Def.Condition(parent))
                return false;
            if (this.Def.Mode == Modes.Indefinite)
                return true;
            
            this.TicksRemaining--;
            return this.TicksRemaining > 0;
        }
        public Moodlet()
        {
            
        }
        public Moodlet(int ticks = 0)
        {
            this.TicksRemaining = ticks;
        }

        public Moodlet(MoodletDef moodletDef)
        {
            this.Def = moodletDef;
            this.TicksRemaining = this.Def.Duration;
        }

        public Control GetUI()
        {
            return new Label(string.Format("{0} {1}", this.Def.Description, this.Def.Value.ToString("+#;-#;0"))) {
                TextColorFunc = () => this.Def.Value < 0 ? Color.Red : Color.Lime,
                HoverFunc = () => this.Def.Mode == Modes.Finite ? string.Format("{0} remaining", (this.TicksRemaining / Ticks.PerSecond).ToString(" #0.##s")) : ""
            };
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(this.Def.Name.Save("Def"));
            tag.Add(this.TicksRemaining.Save("TicksRemaining"));
            return tag;
        }

        public ISaveable Load(SaveTag tag)
        {
            tag.TryGetTagValue<string>("Def", t => this.Def = Core.Def.GetDef<MoodletDef>(t));
            tag.TryGetTagValueOrDefault<int>("TicksRemaining", out this.TicksRemaining);
            return this;
        }

        public void Write(IDataWriter w)
        {
            w.Write(this.Def.Name);
            w.Write(this.TicksRemaining);
        }

        public Moodlet Read(IDataReader r)
        {
            this.Def = Core.Def.GetDef<MoodletDef>(r.ReadString());
            this.TicksRemaining = r.ReadInt32();
            return this;
        }

        public static Moodlet Create(IDataReader r)
        {
            return new Moodlet().Read(r);
        }
    }
}

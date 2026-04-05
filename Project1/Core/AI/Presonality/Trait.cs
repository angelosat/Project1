using Project1.Core.Helpers;
using Project1.Core.Screens;
using Project1.Core.UI;
using Project1.Framework;
using Project1.Framework.Input;
using Project1.Framework.Interfaces;
using Project1.Framework.Serialization;
using Project1.Framework.UI;

namespace Project1.Core.AI.Personality
{
    public sealed class Trait : Inspectable, ISaveableNewNew<Trait>, IDefWrapper<TraitDef>, ISerializableNew<Trait>, IProgressBar, INamed, IListable
    {
        public float Percentage
        {
            get => this.Value / MaxDefault;
            set => this.Value = (MaxDefault - MinDefault) * value;
        }

        public TraitDef TraitDef;
        public TraitDef Def => this.TraitDef;
        public string Name => this.TraitDef.Name;
        public override string LabelReadable => this.Value >= 0 ? this.TraitDef.NamePositive : this.TraitDef.NameNegative;
        public const float MinDefault = -100;
        public const float MaxDefault = 100;
        public const float ValueRange = 100;
        public float Value;
        public float Normalized => this.Value / ValueRange;  //unsigned. do i want this?
        public float Min => MinDefault;
        public float Max => MaxDefault;
        public Trait()
        {
            
        }
        public Trait(TraitDef def)
        {
            this.TraitDef = def;
        }
        public override string ToString()
        {
            return $"{this.TraitDef.Name}: {this.Value}";
        }

        public Control GetListControlGui()
        {
            var box = new Panel() { AutoSize = true, BackgroundStyle = BackgroundStyle.TickBox };
            var bar = new BarSigned() { Object = this, TextFunc = () => this.LabelReadable, HoverFunc = () => $"{this.TraitDef.Name}: {this.Value} ({this.LabelReadable})\n{this.TraitDef.Description.Wrap(TooltipManager.Width)}" 
              
            };
            //bar.LeftClickAction = overrideVal;
            box.AddControls(bar);
            //return box;
            return bar;

            //void overrideVal()
            //{
            //    if (!InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
            //        return;
            //    "todo: request trait change from server".ToConsole();
            //    var xPosClicked = bar.ScreenLocation.X + bar.Width - UIManager.MouseScaled.X;
            //    var center = bar.ScreenLocation.X + bar.Width / 2;
            //    var val = (xPosClicked - center) * 2;
            //    Ingame.Instance.Events.Post(new PlayerChangeTraitValue())
            //    return;
            //}
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, this.TraitDef.Name);
            tag.SaveDef("Def", this.TraitDef);
            tag.Add(this.Value.Save("Value"));
            return tag;
        }

        public static Trait Create(SaveTag tag)
        {
            var trait = new Trait();
            trait.TraitDef = tag.LoadDef<TraitDef>("Def");
            tag.TryGetTagValueOrDefault("Value", out trait.Value);
            return trait;
        }
        public void Write(IDataWriter w)
        {
            w.Write(this.TraitDef);
            w.Write(this.Value);
        }

        public Trait Read(IDataReader r)
        {
            this.TraitDef = r.ReadDef<TraitDef>();
            this.Value = r.ReadSingle();
            return this;
        }

        public static Trait Create(IDataReader r)
        {
            return new Trait().Read(r);
        }

        public Trait Load(SaveTag tag)
        {
            this.Value = tag.LoadSingle("Value");
            return this;
        }
    }
}

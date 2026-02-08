using Microsoft.Xna.Framework;
using Project1.Framework.UI;
using System;

namespace Project1.Core.WorldGen
{
    public class TerraformerProperty
    {
        public string Name, Format;
        public readonly float DefaultValue;
        public float Min;
        public float Max;
        public float Step;

        float _value;
        public float Value
        {
            get => this._value;
            set
            {
                var v = Math.Round(value / this.Step) * this.Step;
                this._value = (float)Math.Max(this.Min, Math.Min(this.Max, v));
            }
        }
        public TerraformerProperty(string name, float value, float min, float max, float step = 1, string format = "")
        {
            if (step <= 0)
                throw new ArgumentException();
            this.Name = name;
            this.Min = min;
            this.Max = max;
            this.Step = step;
            this.Value = this.DefaultValue = value;
            this.Format = format;
        }
        public Control GetGui()
        {
            return new GroupBox() { BackgroundColor = Color.SlateGray * .5f }.AddControlsHorizontally(
                SliderNew.CreateWithLabelNew(this.Name, () => this.Value, v => this.Value = v, 100, this.Min, this.Max, this.Step, "##0%"),
                IconButton.CreateSmall(Icon.Replace, ResetValue));

        }
        public void ResetValue() => this.Value = this.DefaultValue;

    }
}

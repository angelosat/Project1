using Microsoft.Xna.Framework;
using Project1.Core.Animations;
using Project1.Framework.UI;

namespace Project1.Core.Entities.Stats
{
    sealed class StatContribution(Entity owner, StatDef def, BoneDef source)
    {
        internal enum ContributionType { Additive, Multiplicative }

        internal ContributionType CType;
        internal readonly StatDef Def = def;
        float? _value;
        internal float Value => this._value ??= this.Def.CalculateFor(this.Owner);
        internal void SetValue(float value) => this._value = value;
        internal readonly BoneDef Source = source;
        internal Entity Owner = owner;
        internal string Label => $"{this.Def.LabelReadable}: {this.Value.ToString(this.Def.StringFormat)}";
        internal Control CreateGui() =>
            new Label($"{this.Label} ({this.Source.LabelReadable})") { TextColorFunc = () => this.Value > 0 ? Color.Lime : Color.Red };//: {this.Owner.Body.FindBone(this.Source).Material.Label} x{this.Owner.Quality.Multiplier:0.00} from {this.Owner.Quality.Label} Quality)") { TextColorFunc = () => this.Value > 0 ? Color.Lime : Color.Red };
    }
}
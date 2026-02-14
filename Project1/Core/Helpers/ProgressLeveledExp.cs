using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;

namespace Project1.Core.Helpers
{
    public class ProgressLeveledExp : Progress
    {
        readonly int BaseAmountToLevel;
        public ProgressLeveledExp(int amountToLevelBase, int level)
        {
            this.BaseAmountToLevel = amountToLevelBase;
            this.Level = level;
        }
        int _level = 1;
        public int Level 
        {
            get => this._level;
            set
            {
                if (value == this.Level)
                    return;
                this.Value = 0;
                this._level = value;
                this.Max = this.GetNextLvlProgress(value);
            }
        }
        int GetNextLvlProgress(int currentLvl) => (int)Math.Pow(2, currentLvl - 1) * this.BaseAmountToLevel;
        public void AddValue(float v)
        {
            const int debugMultiplier = 10;
            v *= debugMultiplier;
            if (this.Value + v < this.Max)
            {
                this.Value += v;
                return;
            }
            var remaining = this.Value + v;
            int levelsGained = 0;
            int nextLvlXp = (int)this.Max;
            do
            {
                remaining -= nextLvlXp;
                nextLvlXp = GetNextLvlProgress(this.Level + levelsGained++);
            } while (remaining >= nextLvlXp);
            this.Level += levelsGained;
            this.Max = GetNextLvlProgress(this.Level);
            this.Value = remaining;
        }
        public Control GetControl()
        {
            var box = new GroupBox();
            box.AddControlsBottomLeft(
                new Label() { TextFunc = () => $"Current Level: {this.Level}" },
                new Label() { TextFunc = () => $"Next Level: {this.Value:0} / {this.Max:0}" });
            return box;
        }
        protected override void SaveExtra(SaveTag tag)
        {
            this.Level.Save(tag, "Level");
        }
        protected override void LoadExtra(SaveTag tag)
        {
            tag.TryGetTagValue<int>("Level", v => this.Level = v);
        }
        protected override void WriteExtra(IDataWriter w)
        {
            w.Write(this.Level);
        }
        protected override void ReadExtra(IDataReader r)
        {
            this.Level = r.ReadInt32();
        }
    }
}

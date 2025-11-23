using System;
using System.IO;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class ProgressLeveledExp : Progress
    {
        readonly int BaseAmountToLevel;
      
        public ProgressLeveledExp(int amountToLevelBase, int level)
        {
            this.BaseAmountToLevel = amountToLevelBase;
            this.Level = level;
        }

        //public int Level { get; private set; } = 1;
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
      
        //public override float Max { get => GetAmountRequired(this.Level + 1); }

        //public override float Value
        //{
        //    set
        //    {
        //        if (value >= this.Max)
        //        {
        //            value -= this.Max;
        //            this.Level++;
        //        }
        //        base.Value = value;
        //    }
        //}

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

            //var box = new GroupBox();
            //box.AddControlsBottomLeft(
            //    new Label() { TextFunc = () => $"Current Level: {this.Level}" },
            //    new Label() { TextFunc = () => $"Next Level: {(int)this.Value} / {(int)this.Max}" });
            //return box;
        }

        protected override void SaveExtra(SaveTag tag)
        {
            this.Level.Save(tag, "Level");
        }
        protected override void LoadExtra(SaveTag tag)
        {
            tag.TryGetTagValue<int>("Level", v => this.Level = v);
        }
        protected override void WriteExtra(BinaryWriter w)
        {
            w.Write(this.Level);
        }
        protected override void ReadExtra(BinaryReader r)
        {
            this.Level = r.ReadInt32();
        }
    }

    public class ProgressLeveledExpOld : Progress
    {
        public int Level { get; private set; } = 1;
        
        public void SetLevel(int level)
        {
            if (level == this.Level)
                return;
            this.Value = 0;
            this.Level = level;
        }
        readonly int BaseAmountToLevel;

        public ProgressLeveledExpOld(int amountToLevelBase, int level)
        {
            this.BaseAmountToLevel = amountToLevelBase;
            this.Level = level;
        }

        public override float Max { get => GetAmountRequired(this.Level + 1) ; }

        float GetAmountRequired(int level)
        {
            if (level == 0)
                return BaseAmountToLevel;
            return level * BaseAmountToLevel + GetAmountRequired(level - 1);
        }

        public override float Value
        {
            set
            {
                if (value >= this.Max)
                {
                    value -= this.Max;
                    this.Level++;
                }
                base.Value = value;
            }
        }

        public Control GetControl()
        {
            var box = new GroupBox();
            box.AddControlsBottomLeft(
                new Label() { TextFunc = () => $"Current Level: {this.Level}" },
                new Label() { TextFunc = () => $"Next Level: {(int)this.Value} / {(int)this.Max}"});
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
        protected override void WriteExtra(BinaryWriter w)
        {
            w.Write(this.Level);
        }
        protected override void ReadExtra(BinaryReader r)
        {
            this.Level = r.ReadInt32();
        }
    }
}

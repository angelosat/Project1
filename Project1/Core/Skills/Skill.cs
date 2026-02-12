using System;
using Microsoft.Xna.Framework;
using Project1.Framework;
using Project1.Framework.Interfaces;
using Project1.Framework.UI;
using Project1.Framework.Serialization;
using Project1.Framework.Helpers;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Entities.Stats;


namespace Project1.Core.Skills
{
    public partial class Skill : Inspectable, ISaveableNewNew<Skill>, IDefWrapper<SkillDef>, ISerializableNew<Skill>, INamed, IListable
    {
        public SkillsComponent Comp;
        public SkillDef SkillDef;
        public SkillDef Def => this.SkillDef;
        int _level = 1;
        public int Level
        {
            get => this._level;
            set
            {
                this._level = Math.Max(1, value);
                this.LvlProgress.SetMax(GetNextLvlXp(this._level));
            }
        }
        public readonly ProgressInt LvlProgress = new(10);
        const int XpToLevelBase = 10;
        public Skill() { }
        public Skill(SkillsComponent owner, SkillDef def)
        {
            this.Comp = owner;
            this.SkillDef = def;
        }

        public int XpToLevel => (int)this.LvlProgress.Max;
        public float CurrentXP => this.LvlProgress.Value;
        public string Name => this.SkillDef.LabelReadable;
        public override string LabelReadable => this.Name;

        static int GetNextLvlXp(int currentLvl) => (int)Math.Pow(2, currentLvl - 1) * XpToLevelBase;
        static int GetLevel(int xp) => (int)(Math.Log2(xp / XpToLevelBase) + 1);
      
        internal void Award(int v)
        {
            var actor = this.Comp.Owner;
            const int debugMultiplier = 5;
            v *= debugMultiplier;
            if (this.LvlProgress.Value + v < this.LvlProgress.Max)
            {
                this.LvlProgress.ApplyDelta(v);
                actor.Map.Events.Post(new SkillAdjustedEvent(actor as Actor, this));
                return;
            }
            var remaining = this.LvlProgress.Value + v;
            int levelsGained = 0;
            int nextLvlXp = (int)this.LvlProgress.Max;
            do
            {
                remaining -= nextLvlXp;
                nextLvlXp = GetNextLvlXp(this.Level + levelsGained++);
            } while (remaining >= nextLvlXp);
            this.Level += levelsGained;
            this.LvlProgress.SetMax(GetNextLvlXp(this.Level));
            this.LvlProgress.SetValue(remaining);
            actor.Net.ConsoleBox.Write(Log.Entry.Notification(actor, " has reached Level ", this.Level," in ", this, "!"));
            actor.Map.Events.Post(new SkillAdjustedEvent(actor as Actor, this));
            actor.Map.Events.Post(new SkillLevelUpEvent(actor as Actor, this));

        }
        public void SetValue(int level, int xp)
        {
            var oldLevel = this.Level;
            var actor = this.Comp.Owner;
            this.Level = level;
            this.LvlProgress.SetValue(xp);
            actor.Map.Events.Post(new SkillAdjustedEvent(actor as Actor, this));
            if (this.Level != oldLevel)
                actor.Map.Events.Post(new SkillLevelUpEvent(actor as Actor, this));
        }
        public Control GetListControlGui()
        {
            var label = new Bar(this.LvlProgress)
            {
                Width = 200,
                TextFunc = () => $"{this.SkillDef.LabelReadable}: {this.Level}",
                TooltipFunc = (t) =>
                {
                    t.AddControlsBottomLeft(
                        new Label(this.SkillDef.Description),
                        new Label() { TextFunc = () => $"Current Level: {this.Level}" },
                        new Label() { TextFunc = () => $"Experience: {this.CurrentXP} / {this.XpToLevel}" });
                    foreach(var interaction in StatSystem.GetAffectedInteractionsFor(this.Def))
                        t.AddControlsBottomLeft(new Label($"Improves {interaction.LabelReadable} efficiency by {this.Level}%") { TextColorFunc = () => Color.Lime });
                }
            };
            return label;
        }

        public void Write(IDataWriter w)
        {
            w.Write(this.SkillDef);
            w.Write(this.Level);
            w.Write(this.LvlProgress.Value);
        }

        public Skill Read(IDataReader r)
        {
            this.SkillDef = r.ReadDef<SkillDef>();
            this.Level = r.ReadInt32();
            this.LvlProgress.SetValue(r.ReadInt32());
            return this;
        }
       
        public override string ToString()
        {
            return $"{this.SkillDef.LabelReadable}: {this.Level} ({this.CurrentXP} / {this.XpToLevel})";
        }

        public static Skill Create(IDataReader r) => new Skill().Read(r);

        public static Skill Create(SaveTag tag)
        {
            var skill = new Skill();
            skill.SkillDef = tag.LoadDef<SkillDef>("Def");
            //tag.TryGetTagValueOrDefault("Level", out skill._level);
            skill.Level = tag.LoadInt("Level");
            skill.LvlProgress.SetMax(GetNextLvlXp(skill.Level));
            skill.LvlProgress.SetValue((int)tag["Progress"].Value);
            return skill;
        }
        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, this.Name);
            tag.SaveDef("Def", this.Def);
            tag.Add(this.Level.Save("Level"));
            tag.Add(this.LvlProgress.Value.Save("Progress"));
            return tag;
        }
       
    }
}

using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.UI;
using System;

namespace Start_a_Town_
{
    public partial class Skill : Inspectable, ISaveableNewNew<Skill>, IDefWrapper<SkillDef>, ISerializableNew<Skill>, INamed, IListable
    {
        public NpcSkillsComponent Comp;
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
        public Skill(NpcSkillsComponent owner, SkillDef def)
        {
            this.Comp = owner;
            this.SkillDef = def;
        }

        public int XpToLevel => (int)this.LvlProgress.Max;
        public float CurrentXP => this.LvlProgress.Value;
        public string Name => this.SkillDef.Label;
        public override string Label => this.Name;

        //static int GetNextLvlXpTest1(int currentLvl) => (int)Math.Pow(XpToLevelBase, currentLvl + 1);
        //static int GetNextLvlXpTest2(int currentLvl) => currentLvl > 0 ? (int)Math.Pow(XpToLevelBase, currentLvl) * (XpToLevelBase - 1) : XpToLevelBase;
        //static int GetNextLvlXpTest3(int currentLvl) => (currentLvl + 1) * XpToLevelBaseNew + (currentLvl == 0 ? 0 : GetNextLvlXpTest3(currentLvl - 1));
        static int GetNextLvlXp(int currentLvl) => (int)Math.Pow(2, currentLvl - 1) * XpToLevelBase;
        static int GetLevel(int xp) => (int)(Math.Log2(xp / XpToLevelBase) + 1);

        //static public void Init(Hud hud)
        //{
        //    hud.RegisterEventHandler(Components.Message.Types.SkillIncrease, OnSkillIncrease);
        //}
        //static void OnSkillIncrease(GameEvent a)
        //{
        //    var actor = a.Parameters[0] as GameObject;
        //    var skill = (Skill)a.Parameters[1];
        //    FloatingText.Create(actor, $"{skill.SkillDef.Label} increased!", ft => ft.Font = UIManager.FontBold);
        //}
        internal void Award(int v)
        {
            var actor = this.Comp.Owner;

            //for (int i = 0; i < 20; i++)
            //    GetNextLvlXp(i).ToConsole();
            const int debugMultiplier = 5;// 00;
            v *= debugMultiplier;
            if (this.LvlProgress.Value + v < this.LvlProgress.Max)
            {
                //this.LvlProgress.Value += v;
                this.LvlProgress.Add(v);
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
            //this.LvlProgress.Max = GetNextLvlXp(this.Level);
            //this.LvlProgress.Value = remaining;
            this.LvlProgress.SetMax(GetNextLvlXp(this.Level));
            this.LvlProgress.SetValue(remaining);
            actor.Net.ConsoleBox.Write(Log.Entry.Notification(actor, " has reached Level ", this.Level," in ", this, "!"));
            //actor.Net.EventOccured((int)Message.Types.SkillIncrease, actor, this);
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
        static Skill()
        {
            //for (int i = 1; i < 10; i++)
            //{
            //    $"{i}: {GetNextLvlXp(i)}".ToConsole();
            //}
        }
      
        public Control GetListControlGui()
        {
            var label = new Bar(this.LvlProgress)// Label()
            {
                Width = 200,
                TextFunc = () => $"{this.SkillDef.Label}: {this.Level}",
                TooltipFunc = (t) =>
                {
                    t.AddControlsBottomLeft(
                        new Label(this.SkillDef.Description),
                        new Label() { TextFunc = () => $"Current Level: {this.Level}" },
                        new Label() { TextFunc = () => $"Experience: {this.CurrentXP} / {this.XpToLevel}" });
                }
            };
            return label;
        }

        

       
        public void Write(IDataWriter w)
        {
            w.Write(this.SkillDef);
            w.Write(this.Level);
            //this.LvlProgress.Write(w);
            w.Write(this.LvlProgress.Value);
        }

        public Skill Read(IDataReader r)
        {
            this.SkillDef = r.ReadDef<SkillDef>();
            this.Level = r.ReadInt32();
            //this.LvlProgress.Max = GetNextLvlXp(this.Level);
            this.LvlProgress.SetValue(r.ReadInt32());
            return this;
        }
        //public Skill Clone()
        //{
        //    return new Skill(this.Def) { LvlProgress = new Progress(this.LvlProgress), Level = this.Level };
        //}
        public override string ToString()
        {
            return $"{this.SkillDef.Label}: {this.Level} ({this.CurrentXP} / {this.XpToLevel})";
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

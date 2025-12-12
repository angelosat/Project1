using Start_a_Town_.Components;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System;

namespace Start_a_Town_
{
    public class Skill : Inspectable, ISaveable, ISerializableNew<Skill>, INamed, IListable
    {
        public NpcSkillsComponent Container;
        public SkillDef Def;
        int _level = 1;
        public int Level
        {
            get => this._level;
            set
            {
                this._level = value;
                this.LvlProgress.Max = GetNextLvlXp(value);
            }
        }
        Progress LvlProgress = new();
        const int XpToLevelBase = 10;
        Skill() { }
        public Skill(SkillDef def)
        {
            this.Def = def;
        }

        public int XpToLevel => (int)this.LvlProgress.Max;
        public float CurrentXP => this.LvlProgress.Value;
        public string Name => this.Def.Label;
        public override string Label => this.Name;

        //static int GetNextLvlXpTest1(int currentLvl) => (int)Math.Pow(XpToLevelBase, currentLvl + 1);
        //static int GetNextLvlXpTest2(int currentLvl) => currentLvl > 0 ? (int)Math.Pow(XpToLevelBase, currentLvl) * (XpToLevelBase - 1) : XpToLevelBase;
        //static int GetNextLvlXpTest3(int currentLvl) => (currentLvl + 1) * XpToLevelBaseNew + (currentLvl == 0 ? 0 : GetNextLvlXpTest3(currentLvl - 1));
        static int GetNextLvlXp(int currentLvl) => (int)Math.Pow(2, currentLvl - 1) * XpToLevelBase;

        static public void Init(Hud hud)
        {
            hud.RegisterEventHandler(Components.Message.Types.SkillIncrease, OnSkillIncrease);
        }
        static void OnSkillIncrease(GameEvent a)
        {
            var actor = a.Parameters[0] as GameObject;
            var skill = (Skill)a.Parameters[1];
            FloatingText.Create(actor, $"{skill.Def.Label} increased!", ft => ft.Font = UIManager.FontBold);
        }
        internal void Award(float v)
        {
            //for (int i = 0; i < 20; i++)
            //    GetNextLvlXp(i).ToConsole();
            const int debugMultiplier = 10;
            v *= debugMultiplier;
            if (this.LvlProgress.Value + v < this.LvlProgress.Max)
            {
                this.LvlProgress.Value += v;
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
            this.LvlProgress.Max = GetNextLvlXp(this.Level);
            this.LvlProgress.Value = remaining;
            var actor = this.Container.Parent;
            actor.Net.ConsoleBox.Write(Log.Entry.Notification(actor, " has reached Level ", this.Level," in ", this, "!"));
            actor.Net.EventOccured((int)Message.Types.SkillIncrease, actor, this);
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
                TextFunc = () => $"{this.Def.Label}: {this.Level}",
                TooltipFunc = (t) =>
                {
                    t.AddControlsBottomLeft(
                        new Label(this.Def.Description),
                        new Label() { TextFunc = () => $"Current Level: {this.Level}" },
                        new Label() { TextFunc = () => $"Experience: {this.CurrentXP} / {this.XpToLevel}" });
                }
            };
            return label;
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, this.Name);
            tag.Add(this.Level.Save("Level"));
            tag.Add(this.LvlProgress.Value.Save("Progress"));
            return tag;
        }

        public ISaveable Load(SaveTag tag)
        {
            tag.TryGetTagValueOrDefault("Level", out this._level);
            this.LvlProgress.Max = GetNextLvlXp(this.Level);
            this.LvlProgress.Value = (float)tag["Progress"].Value;
            return this;
        }

        public void Write(IDataWriter w)
        {
            w.Write(this.Def);
            w.Write(this.Level);
            //this.LvlProgress.Write(w);
            w.Write(this.LvlProgress.Value);
        }

        public Skill Read(IDataReader r)
        {
            this.Def = r.ReadDef<SkillDef>();
            this.Level = r.ReadInt32();
            //this.LvlProgress.Max = GetNextLvlXp(this.Level);
            this.LvlProgress.Value = r.ReadInt32();
            return this;
        }
        public Skill Clone()
        {
            return new Skill(this.Def) { LvlProgress = new Progress(this.LvlProgress), Level = this.Level };
        }
        public override string ToString()
        {
            return $"{this.Def.Label}: {this.Level} ({this.CurrentXP} / {this.XpToLevel})";
        }

        public static Skill Create(IDataReader r) => new Skill().Read(r);

        

        [EnsureStaticCtorCall]
        internal static class Packets
        {
            static int _pTypeIdModifySkill;

            static Packets()
            {
                _pTypeIdModifySkill = Registry.PacketHandlers.Register(Receive);
            }
            internal static void Send(Actor actor, SkillDef skill, int delta)
            {
                var server = actor.Net as Server;
                server.BeginPacket(_pTypeIdModifySkill)
                    .Write(actor.RefId)
                    .Write(skill)
                    .Write(delta);
            }
            private static void Receive(NetEndpoint endpoint, Packet packet)
            {
                var client = endpoint as Client;
                var r = packet.PacketReader;
                var actor = client.World.GetEntity<Actor>(r.ReadInt32());
                var skill = r.ReadDef<SkillDef>();
                var delta = r.ReadInt32();

                actor.Skills[skill].Award(delta);
            }
        }
    }
}

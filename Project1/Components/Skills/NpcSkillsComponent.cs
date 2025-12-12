using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.UI;
using System;
using System.IO;
using System.Linq;
using static Start_a_Town_.GlobalVars;

namespace Start_a_Town_
{
    public class NpcSkillsComponent : EntityComponent, IGui
    {
        public readonly Skill[] SkillsNew;
        static public Panel UI = new Panel(new Rectangle(0, 0, 500, 400));

        public override object Clone()
        {
            return new NpcSkillsComponent(this.SkillsNew.ToArray());
        }
        public NpcSkillsComponent(ItemDef def)
        {
            var defs = def.ActorProperties.Skills;
            this.SkillsNew = new Skill[defs.Length];
            for (int i = 0; i < defs.Length; i++)
            {
                this.SkillsNew[i] = new Skill(defs[i]) { Container = this };
            }
        }
        public NpcSkillsComponent()
        {

        }
        public NpcSkillsComponent(params Skill[] skills)
        {
            var count = skills.Length;
            this.SkillsNew = new Skill[count];
            for (int i = 0; i < count; i++)
            {
                var newSkill = skills[i].Clone();
                newSkill.Container = this;
                this.SkillsNew[i] = newSkill;
            }
        }
        public NpcSkillsComponent(params SkillDef[] defs)
        {
            this.SkillsNew = new Skill[defs.Length];
            for (int i = 0; i < defs.Length; i++)
            {
                this.SkillsNew[i] = new Skill(defs[i]);
            }
        }

        public override string Name { get; } = "Npc Skills";

        [InspectorHidden]
        internal Skill this[SkillDef skill] => this.GetSkill(skill);
      
        public Control GetCreationGui()
        {
            var table = new TableScrollableCompact<Skill>()
                .AddColumn(null, "name", 80, s => new Label(s.Def.Label), 0)
                .AddColumn(null, "value", 16, s => new Label() { TextFunc = () => s.Level.ToString() }, 0);

            table.AddItems(this.SkillsNew);
            return table;
        }
        public void NewGui(GroupBox box)
        {
            ListBoxNoScroll GuiList = new();
            GuiList.AddItems(this.SkillsNew);
            box.AddControls(GuiList);
        }
        internal Skill GetSkill(SkillDef skill)
        {
            return this.SkillsNew.First(s => s.Def == skill);
        }
        
        public NpcSkillsComponent Randomize()
        {
            var range = 10;
            var average = range / 2;
            
            var values = RandomHelper.NextNormalsBalanced(this.SkillsNew.Length);
            for (int i = 0; i < this.SkillsNew.Length; i++)
            {
                var skill = this.SkillsNew[i];
                skill.Level = (int)(average * (1 + values[i]));
            }
            return this;
        }

        internal override void SaveExtra(SaveTag tag)
        {
            this.SkillsNew.SaveImmutable(tag, "Skills");
        }
        internal override void LoadExtra(SaveTag tag)
        {
            this.SkillsNew.TryLoadImmutable(tag, "Skills");
        }
        public override void Write(IDataWriter w)
        {
            this.SkillsNew.WriteNew(w);
        }
        public override void Read(IDataReader r)
        {
            this.SkillsNew.Read(r);
        }
        internal void AwardAndSync(SkillDef skill, float amount)
        {
            this[skill].Award(amount);
            Skill.Packets.Send(this.Parent as Actor, skill, amount);
        }
        public class Props : ComponentProps
        {
            public override Type CompClass => typeof(NpcSkillsComponent);
            public SkillDef[] Items;
            public Props(params SkillDef[] defs)
            {
                this.Items = defs;
            }
        }
    }
}

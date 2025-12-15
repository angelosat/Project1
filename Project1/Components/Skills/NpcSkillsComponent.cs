using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.UI;
using System.Linq;

namespace Start_a_Town_
{
    public class NpcSkillsComponent : EntityComp, IGui
    {
        public Skill[] SkillsNew;
        static public Panel UI = new Panel(new Rectangle(0, 0, 500, 400));

        public NpcSkillsComponent()
        {

        }
        internal override void CopyFrom(EntityComp source)
        {
            var comp = source as NpcSkillsComponent;
            this.SkillsNew = new Skill[comp.SkillsNew.Length];
            for (int i = 0; i < comp.SkillsNew.Length; i++)
                this.SkillsNew[i] = new Skill(comp.SkillsNew[i].Def);
            this.Randomize();
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
            this.SkillsNew.Write(w);
        }
        public override void Read(IDataReader r)
        {
            this.SkillsNew.Read(r);
        }
        internal void AwardAndSync(SkillDef skill, int amount)
        {
            Adjust(skill, amount);
            Skill.Packets.Send(this.Owner as Actor, skill, amount);
        }

        public void Adjust(SkillDef skill, float amount)
        {
            this[skill].Award(amount);
        }
        public new class Spec : Spec<NpcSkillsComponent>
        {
            public SkillDef[] Items;
            public Spec(params SkillDef[] defs)
            {
                this.Items = defs;
            }
            protected override void ApplyDefaultsTo(NpcSkillsComponent comp)
            {
                comp.SkillsNew = new Skill[this.Items.Length];
                for (int i = 0; i < this.Items.Length; i++)
                    comp.SkillsNew[i] = new Skill(this.Items[i]);
            }
        }
    }
}

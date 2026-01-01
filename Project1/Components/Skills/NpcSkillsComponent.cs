using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.UI;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    public class NpcSkillsComponent : EntityComp, IGui
    {
        public Dictionary<SkillDef, Skill> SkillsNew = [];
        static public Panel UI = new Panel(new Rectangle(0, 0, 500, 400));

        public NpcSkillsComponent()
        {

        }
        internal override void CopyFrom(EntityComp source)
        {
            var comp = source as NpcSkillsComponent;
            //this.SkillsNew = new Skill[comp.SkillsNew.Length];
            //for (int i = 0; i < comp.SkillsNew.Length; i++)
            //    this.SkillsNew[i] = new Skill(this, comp.SkillsNew[i].SkillDef);

            foreach (var s in comp.SkillsNew.Values)
                this.Add(s.SkillDef);
            this.Randomize();
        }
        public IEnumerable<Skill> All => this.SkillsNew.Values;
        public void Add(SkillDef def)
        {
            this.SkillsNew.Add(def, new Skill(this, def) { Comp = this });
        }
        internal override void Resolve()
        {
            foreach (var s in this.SkillsNew.Values)
                s.Comp = this;
        }
        public override string Name { get; } = "Npc Skills";

        [InspectorHidden]
        internal Skill this[SkillDef skill] => this.GetSkill(skill);
      
        public Control GetCreationGui()
        {
            var table = new TableScrollableCompact<Skill>()
                .AddColumn(null, "name", 80, s => new Label(s.SkillDef.Label), 0)
                .AddColumn(null, "value", 16, s => new Label() { TextFunc = () => s.Level.ToString() }, 0);

            table.AddItems(this.SkillsNew.Values);
            return table;
        }
        public void NewGui(GroupBox box)
        {
            ListBoxNoScroll GuiList = new();
            GuiList.AddItems(this.SkillsNew.Values);
            box.AddControls(GuiList);
        }
        internal Skill GetSkill(SkillDef skill)
        {
            //return this.SkillsNew.First(s => s.Def == skill);
            return this.SkillsNew[skill];
        }
        
        public NpcSkillsComponent Randomize()
        {
            var range = 10;
            var average = range / 2;
            var snapshot = this.SkillsNew.Values.ToList();
            var values = RandomHelper.NextNormalsBalanced(snapshot.Count);
            for (int i = 0; i < snapshot.Count; i++)
            {
                var skill = snapshot[i];
                skill.Level = (int)(average * (1 + values[i]));
            }
            return this;
        }

        internal override void SaveExtra(SaveTag tag)
        {
            //this.SkillsNew.SaveImmutable(tag, "Skills");
            tag.SaveDefWrappers("Skills", this.SkillsNew);
        }
        internal override void LoadExtra(SaveTag tag)
        {
            tag.LoadDefWrappers("Skills", this.SkillsNew);
            this.Resolve();
        }
        public override void Write(IDataWriter w)
        {
            w.WriteValues(this.SkillsNew);
        }
        public override void Read(IDataReader r)
        {
            r.ReadDefWrappers(this.SkillsNew);
            this.Resolve();
        }
        
        public void Increase(SkillDef skill, int amount)
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
                //comp.SkillsNew = new Skill[this.Items.Length];
                //for (int i = 0; i < this.Items.Length; i++)
                //    comp.SkillsNew[i] = new Skill(comp, this.Items[i]);

                foreach (var s in this.Items)
                    comp.Add(s);
            }
        }
    }
    internal class SkillIncreaseEvent(Actor actor, SkillDef skill, int delta) : IEventPayload
    {
        public readonly Actor Actor = actor;
        public readonly SkillDef Skill = skill;
        public readonly int Delta = delta;
    }
}

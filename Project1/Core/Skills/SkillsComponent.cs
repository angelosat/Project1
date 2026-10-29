using Microsoft.Xna.Framework;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Skills;

public class SkillsComponent : EntityComp, IGui
{
    public override EntityCompDef CompDef => EntityCompDefOf.Skills;
    public Dictionary<SkillDef, SkillRuntime> SkillsNew = [];
    static public Panel UI = new Panel(new Rectangle(0, 0, 500, 400));

    public SkillsComponent()
    {

    }
    internal override void CopyFrom(EntityComp source)
    {
        var comp = source as SkillsComponent;

        foreach (var s in comp.SkillsNew.Values)
            this.Add(s.SkillDef);
        this.Randomize();
    }
    public IEnumerable<SkillRuntime> All => this.SkillsNew.Values;
    public void Add(SkillDef def)
    {
        this.SkillsNew.Add(def, new SkillRuntime(this, def) { Comp = this });
    }
    internal override void Resolve()
    {
        foreach (var s in this.SkillsNew.Values)
            s.Comp = this;
    }
    public override string Name { get; } = "Npc Skills";

    [InspectorHidden]
    internal SkillRuntime this[SkillDef skill] => this.GetSkill(skill);
  
    public Control GetCreationGui()
    {
        var table = new TableScrollableCompact<SkillRuntime>()
            .AddColumn(null, "name", 80, s => new Label(s.SkillDef.LabelReadable), 0)
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
    internal SkillRuntime GetSkill(SkillDef skill)
        => this.SkillsNew[skill];
    internal int GetLevel(SkillDef skill)
        => this.SkillsNew[skill].Level;

    public SkillsComponent Randomize()
    {
        var range = 10;
        var average = range / 2;
        var snapshot = this.SkillsNew.Values.ToList();
        var values = RandomHelperAI.NextNormalsBalanced(snapshot.Count);
        for (int i = 0; i < snapshot.Count; i++)
        {
            var skill = snapshot[i];
            skill.Level = 1 + (int)(average * (1 + values[i]));
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
        //tag.LoadDefWrappers("Skills", this.SkillsNew);
        // re initialize from profile in case of new/removed skills
        foreach (var skilldef in (this.Owner.Profile as ActorDnaDef).Skills)
            this.Add(skilldef);

        // actually load in previous saved values
        var saved = tag.LoadDefWrappers<SkillDef, SkillRuntime>("Skills");
        foreach (var s in saved)
            if(this.SkillsNew.ContainsKey(s.Key))
                this.SkillsNew[s.Key] = s.Value;
        this.Resolve();
    }
    public override void Write(IDataWriter w)
    {
        w.WriteValues(this.SkillsNew);
    }
    public override void Read(IDataReader r)
    {
        //r.ReadDefWrappers(this.SkillsNew);
        var skillsLoaded = r.ReadList<SkillRuntime>();//.Where(s => s.Def is not null);
        this.SkillsNew = skillsLoaded.Where(s=>s.Def is not null).ToDictionary(s => s.Def);
        //foreach(var skill in skillsLoaded)
        //{
        //    if (this.SkillsNew.ContainsKey(skill.Def))
        //        this.SkillsNew[skill.Def] = skill;
        //}
        this.Resolve();
    }
    
    public void Increase(SkillDef skill, int amount)
    {
        this[skill].Award(amount);
    }

    internal void SetValue(SkillDef skill, int level, int xp)
        => this[skill].SetValue(level, xp);
        
    internal void ApplyXp(SkillDef skill, int xp)
    {
        var actor = this.Owner as Actor;
        var s = this.SkillsNew[skill];
        var result = s.AwardInt(xp);
        actor.World.Events.Post(new SkillAdjustedEvent(actor, s));
        if(result == SkillRuntime.SkillXpAwardResult.LevelUp)
            actor.World.Events.Post(new SkillLevelUpEvent(actor, s));
    }

    //public override IEnumerable<(StatDef target, float value)> GetModifiers()
    //{
    //    foreach(var (stat, tooluse) in WorkSystem.StatToTooluse)
    //    {
    //        var skill = tooluse.Skill;
    //        yield return (stat, this.SkillsNew[skill].Level);
    //    }
    //}

    //public override float? GetStatMod(StatDef target)
    ////=> this.SkillsNew[WorkSystem.StatToTooluse[target].Skill].Level;
    //{
    //    if (WorkSystem.TryGetStatToToolUse(target, out var capability))
    //        return this.SkillsNew[capability.Skill].Level;
    //    return null;
    //}

    public new class Spec : Spec<SkillsComponent>
    {
        public SkillDef[] Items;
        public Spec(params SkillDef[] defs)
        {
            this.Items = defs;
        }
        protected override void ApplyDefaultsTo(SkillsComponent comp)
        {
            //comp.SkillsNew = new Skill[this.Items.Length];
            //for (int i = 0; i < this.Items.Length; i++)
            //    comp.SkillsNew[i] = new Skill(comp, this.Items[i]);

            foreach (var s in this.Items)
                comp.Add(s);
        }
    }
}


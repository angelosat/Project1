using Project1.Core.AI;
using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Needs;

public sealed class NeedsComp : EntityComp<NeedsComp.Spec>, IGui
{
    public override EntityCompDef CompDef => EntityCompDefOf.Needs;
    public override string Name { get; } = "Needs";
       
    public Dictionary<NeedDef, NeedRuntime> NeedsNew = [];

    internal override void CopyFrom(EntityComp source)
    {
        var c = (NeedsComp)source;
        foreach(var n in c.NeedsNew.Values)
            this.NeedsNew.Add(n.NeedDef, new NeedRuntime(this.Owner as Actor, n.NeedDef));
    }
    public NeedsComp()
    {
    }
    public NeedRuntime AddNeed(NeedDef def)
    {
        var need = new NeedRuntime(this.Owner as Actor, def);
        this.NeedsNew.Add(def, need);
        return need;
    }
    public void Add(params NeedDef[] defs)
    {
        foreach (var d in defs)
            this.AddNeed(d);
    }
    public void Remove(params NeedDef[] defs)
    {
        foreach (var d in defs)
            this.NeedsNew.Remove(d);
    }
    public int GetDeficit(NeedDef def)
        => this.NeedsNew[def].Deficit;
    public override void Tick()
    {
        foreach (var n in this.NeedsNew.Values)
            n.Tick();
    }
    public override void TickOffMap()
        => this.Tick();
    internal override void Resolve()
    {
        foreach (var n in this.NeedsNew.Values)
            n.Owner = this.Owner as Actor;
    }
    static public NeedRuntime ModifyNeed(GameObject actor, NeedDef type, int value)
    {
   
        var need = actor.GetNeed(type);
        need.ApplyDelta(value);
        return need;
    }
    public void GetUI(GameObject parent, Control container)
    {
        var box = new GroupBox();

        var byCategory = this.NeedsNew.Values.GroupBy(n => n.NeedDef.CategoryDef);
        foreach (var cat in byCategory)
        {
            var panel = new PanelLabeled(cat.Key.LabelReadable) { Location = box.BottomLeft };
            foreach (var n in cat)
            {
                var ui = n.GetUI(parent);
                ui.Location = panel.Controls.BottomLeft;
                panel.AddControls(ui);
            }
            box.AddControls(panel);
        }
        container.AddControls(box);
    }
    public void GetUI(Control container)
    {
        var box = new GroupBox();

        var byCategory = this.NeedsNew.Values.GroupBy(n => n.NeedDef.CategoryDef);
        foreach (var cat in byCategory)
        {
            var panel = new PanelLabeled(cat.Key.LabelReadable) { Location = box.BottomLeft };
            foreach (var n in cat)
            {
                var ui = n.GetUI(this.Owner);
                ui.Location = panel.Controls.BottomLeft;
                panel.AddControls(ui);
            }
            box.AddControls(panel);
        }
        container.AddControls(box);
    }
    public override void Write(IDataWriter w)
    {
        w.WriteValues(this.NeedsNew);
    }
    public override void Read(IDataReader r)
    {
        r.ReadDefWrappers(this.NeedsNew);
        this.Resolve();
    }
    internal override void SaveExtra(SaveTag tag)
    {
        tag.SaveValues(this.NeedsNew, "Needs");
    }
    internal override void LoadExtra(SaveTag tag)
    {
        tag.LoadDefWrappers("Needs", this.NeedsNew);
        this.Resolve();
    }
    void Rebuild()
    {
        this.NeedsNew.Clear();
        var profile = this.Owner.Profile as ActorDnaDef;
        var profileneeds = profile.Needs;
        var role = this.Owner.GetComponent<AIComp>().Meta;
        var roleneeds = role.Def.Needs;
        var allneeds = profileneeds.Concat(roleneeds);
        foreach (var n in allneeds)
            this.Add(n);
    }
    public void NewGui(GroupBox box)
    {
        var byCategory = this.NeedsNew.Values.GroupBy(n => n.NeedDef.CategoryDef);
        foreach (var cat in byCategory)
        {
            var panel = new PanelLabeled(cat.Key.LabelReadable) { Location = box.BottomLeft };
            foreach (var n in cat)
            {
                var ui = n.GetUI(this.Owner);
                ui.Location = panel.Controls.BottomLeft;
                panel.AddControls(ui);
            }
            box.AddControls(panel);
        }
    }
    public int GetValue(NeedDef need)
        => this.NeedsNew[need].Value;
    public float GetPercentage(NeedDef need)
        => this.NeedsNew[need].Percentage;

    internal void SetPercentage(NeedDef adventuring, float percentage)
    {
        var need = this.NeedsNew[adventuring];
        need.SetValue((int)(need.Max * percentage), this.Owner);
        this.Owner.World?.Events.Post(new ActorNeedOverridenEvent(this.Owner as Actor, need.Def, need.Value));
    }

    internal void ApplyAccumulatorDelta(NeedDef need, float delta)
        //=> this.NeedsNew[need].Accumulator += delta;
        => this.NeedsNew[need].ApplyAccumulatorDelta(delta);

    public new class Spec: Spec<NeedsComp>
    {
        public NeedDef[] Needs;
        public Spec(params NeedDef[] defs)
        {
            this.Needs = defs;
        }
        protected override void ApplyDefaultsTo(NeedsComp comp)
        {
            if (this.Needs != null)
            {
                comp.Add(this.Needs);
            }
        }
    }
}
using Project1.Core.Animations;
using Project1.Core.Helpers;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Entities.Stats;

class StatsComp : EntityComp, IGuiNew
{
    public override EntityCompDef CompDef => EntityCompDefOf.Stats;
    readonly Dictionary<BoneDef, List<StatContribution>> ContributionsBySource = [];
    readonly Dictionary<StatDef, List<StatContribution>> ContributionsByStat = [];
    internal void Bake(StatDef def, BoneDef source)
    {
        this.Register(def, source);
    }
    public float this[StatDef def]
    {
        get
        {
            if (!this.ContributionsByStat.TryGetValue(def, out var list))
                return 0;
            return list.Sum(c => c.Value);
        }
    }

    public new class Spec : Spec<StatsComp> { }
    public override string Name { get; } = "StatsNew";
    readonly Dictionary<StatDef, List<StatNewModifier>> Modifiers = new();
    internal List<StatNewModifier> GetModifiers(StatDef statNewDef)
    {
        this.Modifiers.TryGetValue(statNewDef, out var item);
        return item ?? new List<StatNewModifier>();
    }
    public void AddModifier(StatNewModifier mod)
    {
        if (this.Modifiers.TryGetValue(mod.Def.Source, out var existing))
            existing.Add(mod);
        else
            this.Modifiers[mod.Def.Source] = new List<StatNewModifier>() { mod };
    }
    public bool RemoveModifier(StatNewModifier mod)
    {
        if (this.Modifiers.TryGetValue(mod.Def.Source, out var existing))
        {
            if (existing.Remove(mod))
            {
                if (!existing.Any())
                    this.Modifiers.Remove(mod.Def.Source);
                return true;
            }
        }
        return false;
    }

    internal override void GetInterface(GameObject gameObject, Control box)
    {
        var gui = GUITable ??= new TableScrollableCompact<StatDef>()
            .AddColumn("name", "", 128, a => new Label(a.LabelReadable) { HoverText = a.Description })
            .AddColumn("value", "", (int)UIManager.Font.MeasureString("###").X, a => new Label(() => a.CalculateFor(this.Owner).ToString()));
        gui.ClearItems();
        gui.AddItems(StatDef.NpcStatPackage);
        box.AddControlsBottomLeft(gui);
    }
    TableScrollableCompact<StatDef> GUITable;
    public override GroupBox GetGUI()
    {
        var gui = GUITable ??= new TableScrollableCompact<StatDef>()
            .AddColumn("name", "", 64, a => new Label(a.LabelReadable), 1)
            .AddColumn("value", "", (int)UIManager.Font.MeasureString("###").X, a => new Label(() => a.CalculateFor(this.Owner).ToString()));
        gui.ClearItems();
        gui.AddItems(StatDef.NpcStatPackage);
        return GUITable;
    }
    public Control CreateControl()
    {
        //var gui = new TableScrollableCompact<StatDef>()
        //    .AddColumn("name", "", 64, a => new Label(a.LabelReadable))
        //    .AddColumn("value", "", (int)UIManager.Font.MeasureString("###").X, a => new Label(() => a.CalculateFor(this.Owner).ToString()));
        var gui = new Table<StatDef>()
            .AddColumn("name", 96, a => new Label(a.LabelReadable), 1)
            //.AddColumn("value", (int)UIManager.Font.MeasureString("###").X, a => new Label(() => a.CalculateFor(this.Owner).ToString()));
            .AddColumn("divider", 8, s => new Label(""))
            .AddColumn("value", 96, a => new Label(() => a.CalculateFor(this.Owner).ToString()));
        gui.AddItems(StatDef.NpcStatPackage);
        return gui;
    }

    public override void OnTooltipCreated(Control tooltip)
    {
        foreach (var (source, list) in this.ContributionsByStat)
            foreach (var stat in list)
                tooltip.AddControlsBottomLeft(stat.CreateGui());
    }
    internal override void CopyFrom(EntityComp source)
    {
        var comp = (StatsComp)source;
        foreach (var (bone, list) in comp.ContributionsBySource)
        {
            var newlist = new List<StatContribution>();
            this.ContributionsBySource[bone] = newlist;
            foreach (var stat in list)
                newlist.Add(new StatContribution(this.Owner, stat.Def, bone));
        }
    }
    void Register(StatDef def, BoneDef source, float? value = null)
    {
        var c = new StatContribution(this.Owner, def, source);
        if (value.HasValue)
            c.SetValue(value.Value);
        if (!this.ContributionsBySource.TryGetValue(source, out var list))
            this.ContributionsBySource[source] = list = [];
        if(!this.ContributionsByStat.TryGetValue(def, out var liststat))
            this.ContributionsByStat[def] = liststat = [];
        liststat.Add(c);
        list.Add(c);
    }
    public override void Write(IDataWriter w)
    {
        w.Write(this.ContributionsBySource.Count);
        foreach (var (bone, list) in this.ContributionsBySource)
        {
            w.Write(bone);
            w.Write(list.Count);
            foreach (var s in list)
            {
                w.Write(s.Def);
                w.Write(s.Value);
            }
        }
    }
    public override void Read(IDataReader r)
    {
        var bonecount = r.ReadInt32();
        for (int i = 0; i < bonecount; i++)
        {
            var bone = r.ReadDef<BoneDef>();
            var count = r.ReadInt32();
            for (int j = 0; j < count; j++)
            {
                var def = r.ReadDef<StatDef>();
                var value = r.ReadSingle();
                this.Register(def, bone, value);
            }
        }
    }

    
}
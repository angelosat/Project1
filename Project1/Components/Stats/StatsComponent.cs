using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using System;

namespace Start_a_Town_.Components
{
    class StatContribution(GameObject owner, StatDef def, BoneDef source)
    {
        enum ContributionType { Additive, Multiplicative, Override }
        internal readonly StatDef Def = def;
        float? _value;
        internal float Value => this._value ??= this.Def.CalculateFor(this.Owner);
        internal void SetValue(float value) => this._value = value;
        internal readonly BoneDef Source = source;
        internal GameObject Owner = owner;
        internal string Label => $"{this.Def.Label}: {this.Value.ToString(this.Def.StringFormat)}";
        internal Control CreateGui() => new Label($"{this.Label} ({this.Source.Label}: {this.Owner.Body[this.Source].Material.Label})") { TextColorFunc = () => this.Value > 0 ? Color.Lime : Color.Red };
    }
    class StatsComponent : EntityComp
    {
        readonly Dictionary<BoneDef, List<StatContribution>> Contributions = [];
        internal void Bake(StatDef def, BoneDef source)
        {
            if (!this.Contributions.TryGetValue(source, out var list))
                this.Contributions[source] = list = [];
            list.Add(new StatContribution(this.Owner, def, source));
        }
        //void RefreshAll()
        //{
        //    foreach (var (source, stats) in this.Contributions)
        //        foreach (var stat in stats)
        //            stat.Refresh(this.Owner);
        //}
        //internal override void ResolveReferencesNew()
        //{
        //    this.RefreshAll();
        //}
        public new class Spec : Spec<StatsComponent> { }
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
        internal override void GetInterface(GameObject gameObject, UI.Control box)
        {
            var gui = GUITable ??= new TableScrollableCompact<StatDef>()
                .AddColumn("name", "", 128, a => new Label(a.Label) { HoverText = a.Description })
                .AddColumn("value", "", (int)UIManager.Font.MeasureString("###").X, a => new Label(() => a.CalculateFor(this.Owner).ToString()));
            gui.ClearItems();
            gui.AddItems(StatDef.NpcStatPackage);
            box.AddControlsBottomLeft(gui);
        }
        TableScrollableCompact<StatDef> GUITable;
        public override GroupBox GetGUI()
        {
            var gui = GUITable ??= new TableScrollableCompact<StatDef>()
                .AddColumn("name", "", 64, a => new Label(a.Label))
                .AddColumn("value", "", (int)UIManager.Font.MeasureString("###").X, a => new Label(() => a.CalculateFor(this.Owner).ToString()));
            gui.ClearItems();
            gui.AddItems(StatDef.NpcStatPackage);
            return GUITable;
        }
        public override void OnTooltipCreated(GameObject parent, Control tooltip)
        {
            foreach (var (source, list) in this.Contributions)
                foreach (var stat in list)
                    tooltip.AddControlsBottomLeft(stat.CreateGui());
        }
        internal override void CopyFrom(EntityComp source)
        {
            var comp = (StatsComponent)source;
            foreach (var (bone, list) in comp.Contributions)
            {
                var newlist = new List<StatContribution>();
                this.Contributions[bone] = newlist;
                foreach (var stat in list)
                    newlist.Add(new StatContribution(this.Owner, stat.Def, bone));
            }
        }
        public override void Write(IDataWriter w)
        {
            w.Write(this.Contributions.Count);
            foreach (var (bone, list) in this.Contributions)
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
                var list = new List<StatContribution>();
                var bone = r.ReadDef<BoneDef>();
                this.Contributions[bone] = list;
                var count = r.ReadInt32();
                for (int j = 0; j < count; j++)
                {
                    var def = r.ReadDef<StatDef>();
                    var stat = new StatContribution(this.Owner, def, bone);
                    stat.SetValue(r.ReadSingle());
                    list.Add(stat);
                }
            }
        }
        public override object Clone()
        {
            return new StatsComponent();
        }
    }
}

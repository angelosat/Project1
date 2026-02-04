using Microsoft.Xna.Framework;
using Project1.Framework.Animations;
using Project1.Framework.Entities;
using Project1.Framework.UI;
using Start_a_Town_;
using Start_a_Town_.UI;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Framework.Stats
{

    class StatContribution(GameObject owner, StatDef def, BoneDef source)//, StatContribution.ContributionType cType)
    {
        internal enum ContributionType { Additive, Multiplicative }

        internal ContributionType CType;// = cType;
        internal readonly StatDef Def = def;
        float? _value;
        internal float Value => this._value ??= this.Def.CalculateFor(this.Owner);
        internal void SetValue(float value) => this._value = value;
        internal readonly BoneDef Source = source;
        internal GameObject Owner = owner;
        internal string Label => $"{this.Def.Label}: {this.Value.ToString(this.Def.StringFormat)}";
        internal Control CreateGui() =>
            new Label($"{this.Label} ({this.Source.Label})") { TextColorFunc = () => this.Value > 0 ? Color.Lime : Color.Red };//: {this.Owner.Body.FindBone(this.Source).Material.Label} x{this.Owner.Quality.Multiplier:0.00} from {this.Owner.Quality.Label} Quality)") { TextColorFunc = () => this.Value > 0 ? Color.Lime : Color.Red };
    }
    class StatsComponent : EntityComp
    {
        readonly Dictionary<BoneDef, List<StatContribution>> ContributionsBySource = [];
        readonly Dictionary<StatDef, List<StatContribution>> ContributionsByStat = [];
        internal void Bake(StatDef def, BoneDef source)
        {
            this.Register(def, source);
            //if (!this.ContributionsBySource.TryGetValue(source, out var list))
            //    this.ContributionsBySource[source] = list = [];
            //list.Add(new StatContribution(this.Owner, def, source));
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
        public float this[StatDef def]
        {
            get
            {
                if (!this.ContributionsByStat.TryGetValue(def, out var list))
                    return 0;
                return list.Sum(c => c.Value);
            }
        }
        //public float GetStat(StatDef stat)
        //{
        //    //float total = 0;
        //    //foreach (var (bone, list) in this.ContributionsBySource)
        //    //    foreach (var c in list)
        //    //        if (c.Def == stat)
        //    //            total += c.Value;
        //    //return total;

        //    if (!this.ContributionsByStat.TryGetValue(stat, out var list))
        //        return 0;
        //    return list.Sum(c => c.Value);
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
        internal override void GetInterface(GameObject gameObject, Control box)
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
            //tooltip.AddControlsBottomLeft(new Label("by source"));
            //foreach (var (source, list) in this.ContributionsBySource)
            //    foreach (var stat in list)
            //        tooltip.AddControlsBottomLeft(stat.CreateGui());
            //tooltip.AddControlsBottomLeft(new Label("by stat"));
            foreach (var (source, list) in this.ContributionsByStat)
                foreach (var stat in list)
                    tooltip.AddControlsBottomLeft(stat.CreateGui());
        }
        internal override void CopyFrom(EntityComp source)
        {
            var comp = (StatsComponent)source;
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
                //var list = new List<StatContribution>();
                var bone = r.ReadDef<BoneDef>();
                //this.ContributionsBySource[bone] = list;
                var count = r.ReadInt32();
                for (int j = 0; j < count; j++)
                {
                    var def = r.ReadDef<StatDef>();
                    //var stat = new StatContribution(this.Owner, def, bone);
                    var value = r.ReadSingle();
                    //stat.SetValue();
                    //list.Add(stat);
                    this.Register(def, bone, value);
                }
            }
        }
        public override object Clone()
        {
            return new StatsComponent();
        }
    }
}

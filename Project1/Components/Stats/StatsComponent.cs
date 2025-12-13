using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    class StatsComponent : EntityComp
    {
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
                .AddColumn("value", "", (int)UIManager.Font.MeasureString("###").X, a => new Label(() => a.GetValue(this.Parent).ToString()));
            gui.ClearItems();
            gui.AddItems(StatDef.NpcStatPackage);
            box.AddControlsBottomLeft(gui);
        }
        TableScrollableCompact<StatDef> GUITable;
        public override GroupBox GetGUI()
        {
            var gui = GUITable ??= new TableScrollableCompact<StatDef>()
                .AddColumn("name", "", 64, a => new Label(a.Label))
                .AddColumn("value", "", (int)UIManager.Font.MeasureString("###").X, a => new Label(() => a.GetValue(this.Parent).ToString()));
            gui.ClearItems();
            gui.AddItems(StatDef.NpcStatPackage);
            return GUITable;
        }

        public override object Clone()
        {
            return new StatsComponent();
        }
    }
}

using Project1.Framework.Interfaces;
using Project1.Framework.UI;
using Start_a_Town_.UI;

namespace Project1.Framework.Skills
{
    public struct ToolUse : IListable
    {
        public readonly ToolUseDef Def;
        public readonly int Effectiveness;
        readonly string _label;
        public ToolUse(ToolUseDef def, int efficiency)
        {
            this.Def = def;
            this.Effectiveness = 1;// efficiency;
            this._label = $"{this.Def.Label}: {this.Effectiveness}";
        }

        public string Label => this._label;

        public Control GetListControlGui()
        {
            return new Label(this.Def);
        }
    }
}

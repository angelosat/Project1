using Project1.Core.Interfaces;
using Project1.Core.UI;
using Project1.Core.UI;

namespace Project1.Core.Tools
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

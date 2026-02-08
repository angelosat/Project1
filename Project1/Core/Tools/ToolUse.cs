using Project1.Framework.UI;

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
            this._label = $"{this.Def.LabelReadable}: {this.Effectiveness}";
        }

        public string LabelReadable => this._label;

        public Control GetListControlGui()
        {
            return new Label(this.Def);
        }
    }
}
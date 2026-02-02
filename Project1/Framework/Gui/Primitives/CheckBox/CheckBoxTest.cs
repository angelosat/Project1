using System;

namespace Start_a_Town_.UI
{
    internal class CheckBoxTest : GroupBox
    {
        readonly CheckBoxIcon Icon;
        readonly Label Label;
        public CheckBoxTest(string label, Func<bool> checkedGetter, Action tickAction) : this(() => label, checkedGetter, tickAction) { }
        public CheckBoxTest(Func<string> labelGetter, Func<bool> checkedGetter, Action tickAction)
        {
            this.Label = new(labelGetter) { Active = false };
            this.Icon = new(checkedGetter, tickAction);
            this.AddControlsHorizontally(this.Icon, this.Label);
            this.Controls.AlignCenterHorizontally();
        }
    }
}

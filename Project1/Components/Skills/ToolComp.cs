using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public class ToolComp : EntityComp<ToolComp.Spec>
    {
        public new class Spec : Spec<SpriteComp>
        {
            public readonly ToolUseDef ToolUse;
            public Spec(ToolUseDef toolUse)
            {
                this.ToolUse = toolUse;
            }
        }
        public override string Name { get; } = "Tool";
        
        public ToolUseDef ToolUse;
        public ToolProfileDef ToolDef;
        readonly List<ToolUseDef> Skills = new();
     
        public ToolComp()
        {

        }
        
        public ToolComp(params ToolUseDef[] skills)
        {

        }
        public ToolComp Initialize(params ToolUseDef[] skills)
        {
            return this;
        }
        public ToolUseDef Skill { get { return this.Skills.FirstOrDefault(); } }
        
     
        public override string ToString()
        {
            if (this.Skills.Count == 0)
                return "";
            string text = "";
            foreach (var item in this.Skills)
                text += item.Name + "\n";
            return text.TrimEnd('\n');
        }

        public override void OnTooltipCreated(GameObject parent, Control tooltip)
        {
            tooltip.AddControlsBottomLeft(this.GetUI(parent));
        }
        GroupBox GetUI(GameObject parent)
        {
            var box = new GroupBox();
            box.AddControlsBottomLeft(new Label(this.ToolUse));
                //box.AddControlsBottomLeft(ToolUseDef.GetUI(ability.Value.Def.ID, ability.Value.Effectiveness));
            return box;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;
using Start_a_Town_.Components;
using System.IO;

namespace Start_a_Town_
{
    public class ToolAbilityComponent : EntityComponent
    {
        public override string Name { get; } = "Tool";
        public override object Clone()
        {
            return new ToolAbilityComponent(this.Props);
        }
        public ToolProps Props;
        readonly List<ToolUseDef> Skills = new();
        public ToolAbilityComponent(ToolProps props)
        {
            this.Props = props;
        }
        public ToolAbilityComponent()
        {

        }
        
        public ToolAbilityComponent(params ToolUseDef[] skills)
        {

        }
        public ToolAbilityComponent Initialize(params ToolUseDef[] skills)
        {
            return this;
        }

        /// <summary>
        /// load sprites here on in the Tool.ObjectLoaded method?
        /// </summary>
        //public override void OnObjectLoaded(GameObject parent)
        //{
        //    if (this.Props is null)
        //        return;
        //    this.Parent.Body.Sprite = this.Props.SpriteHandle;
        //    this.Parent.Body[BoneDef.EquipmentHead].Sprite = this.Props.SpriteHead;
        //}
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
            box.AddControlsBottomLeft(new Label(this.Props.ToolUse));
                //box.AddControlsBottomLeft(ToolUseDef.GetUI(ability.Value.Def.ID, ability.Value.Effectiveness));
            return box;
        }

        public override void Write(IDataWriter w)
        {
            w.Write(this.Props is not null); // HACK for loading legacy items which lack Props
            this.Props?.Write(w);
        }
        public override void Read(IDataReader r)
        {
            if (r.ReadBoolean()) // HACK for loading legacy items which lack Props
                this.Props = Def.GetDef<ToolProps>(r);
        }

        internal override void SaveExtra(SaveTag tag)
        {
            this.Props?.Name.Save(tag, "Props");
        }

        internal override void LoadExtra(SaveTag tag)
        {
            tag.TryLoadDef("Props", ref this.Props);
        }
    }
}

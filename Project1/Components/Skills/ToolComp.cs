using Start_a_Town_.Components;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.Linq;

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
        ToolProfileDef Profile => this.Owner.Profile as ToolProfileDef;
        public ToolUseDef ToolUse;
        public ToolProfileDef ToolDef;
        readonly List<ToolUseDef> Skills = [];
        float? baseSpeed, baseWork;
        public float BaseSpeed => this.baseSpeed ??= this.CalculateBaseSpeed();
        public float BaseWork => this.baseWork ??= this.CalculateBaseWorkAmount();
        internal override void ResolveReferencesNew()
        {
            ToolSystem.BakeStats(this.Owner);
            this.RefreshStats();
        }

        private void RefreshStats()
        {
            this.baseSpeed = null;
            this.baseWork = null;
        }
        float CalculateBaseSpeed()
        {
            var tool = this.Owner;
            var material = tool.GetMaterial(BoneDefOf.ToolHandle);
            var aa = 20f; // what is this?
            var density = Math.Max(aa, material.Density); // in case for some reason the material is air
            var total = aa / density;
            total *= tool.Quality.Multiplier;
            total = StatDefOf.ToolSpeed.Worker.CalculateStat(this.Owner);
            return total;
        }
        float CalculateBaseWorkAmount()
        {
            var tool = this.Owner;
            var material = tool.GetMaterial(BoneDefOf.ToolHead);
            return material.Density * tool.Quality.Multiplier;
        }
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

        internal override void CopyFrom(EntityComp source)
        {
            var comp = (ToolComp)source;
            this.ToolUse = comp.ToolUse;
            this.ToolDef = comp.ToolDef;
            this.baseSpeed = comp.baseSpeed;
            this.baseWork = comp.baseWork;
            foreach (var sk in comp.Skills)
                this.Skills.Add(sk);
        }
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
            box.AddControlsBottomLeft(new Label($"Speed: {this.BaseSpeed:0.00}"));
            box.AddControlsBottomLeft(new Label($"{this.Profile.ToolUse.Label} Effectiveness: {this.BaseWork:0}"));
            box.AddControlsBottomLeft(new Label(StatSystem.ToolToInteraction[this.Profile.ToolUse]));
            //box.AddControlsBottomLeft(ToolUseDef.GetUI(ability.Value.Def.ID, ability.Value.Effectiveness));
            return box;
        }

        internal float? GetWorkValue(ToolUseDef toolUse)
        {
            if (this.Profile.ToolUse != toolUse)
                return null;
            return this.BaseWork;
        }
        //public override void Write(IDataWriter w)
        //{
        //    w.Write(this.baseSpeed);
        //    w.Write(this.baseWork);
        //}
        //public override void Read(IDataReader r)
        //{
        //    this.baseSpeed = r.ReadSingle();
        //    this.baseWork = r.ReadSingle();
        //}
    }
}

using Project1.Core.Animations;
using Project1.Core.Entities;
using Project1.Core.Entities.Stats;
using Project1.Core.Stats;
using Project1.Core.Systems.Quality;
using Project1.Framework.Helpers;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;

namespace Project1.Core.Systems.Tools;

public sealed class ToolComp : EntityComp<ToolComp.Spec>
{
    public override EntityCompDef CompDef => EntityCompDefOf.Tool;
    public new class Spec : Spec<SpriteComp>
    {
        public readonly ToolUseDef ToolUse;
        public Spec(ToolUseDef toolUse)
        {
            this.ToolUse = toolUse;
        }
    }
    public override string Name { get; } = "Tool";
    GearProfileDef Profile => this.Owner.Profile as GearProfileDef;
    public ToolUseDef ToolUse;
    public GearProfileDef ToolDef;
    readonly List<ToolUseDef> Skills = [];
    float? baseSpeed, baseWork;
    public float BaseSpeed => this.baseSpeed ??= this.CalculateBaseSpeed();
    public float BaseWork => this.baseWork ??= this.CalculateBaseWorkAmount();
    internal override void ResolveReferencesNew()
    {
        ToolSystem.BakeStats(this.Owner);
        this.RefreshStats();

        //this.Owner.SetName($"{this.Owner.Body.FindBone(BoneDefOf.ToolHead).Material.LabelReadable} {this.Owner.Profile.LabelReadable} (Handle: {this.Owner.Body.FindBone(BoneDefOf.ToolHandle).Material.LabelReadable})");
        this.Owner.SetName($"{this.Owner.Body.Material.LabelReadable} {this.Owner.Profile.LabelReadable}");
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
        total *= tool.QualityComp.Tier.Multiplier;
        total = StatDefOf.ToolSpeed.Worker.CalculateStat(this.Owner);
        return total;
    }

    float CalculateBaseWorkAmount()
    {
        var tool = this.Owner;
        var material = tool.GetMaterial(BoneDefOf.ToolHead);
        return material.Density * tool.QualityComp.Tier.Multiplier;
    }

    //public ToolComp()
    //{

    //}
    
    //public ToolComp(params ToolUseDef[] skills)
    //{

    //}

    //public ToolComp Initialize(params ToolUseDef[] skills)
    //{
    //    return this;
    //}

    //public ToolUseDef Skill { get { return this.Skills.FirstOrDefault(); } }

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

    public override void OnTooltipCreated(Control tooltip)
    {
        tooltip.AddControlsBottomLeft(this.GetUI());
    }

    GroupBox GetUI()
    {
        var box = new GroupBox();
        //box.AddControlsBottomLeft(new Label(this.ToolUse));
        //box.AddControlsBottomLeft(new Label($"Speed: {this.BaseSpeed:0.00}"));
        //box.AddControlsBottomLeft(new Label($"{this.Profile.ToolUse.LabelReadable} Effectiveness: {this.BaseWork:0}"));
        //box.AddControlsBottomLeft(new Label(StatSystem.ToolToInteraction[this.Profile.ToolUse]));
        
        return box;
    }

    internal override IEnumerable<Control> GetTooltipControls()
    {
        var profile = this.Owner.Profile as GearProfileDef;
        var slot = profile.Role.Slot;
        yield return new LabelNew($"Slot: {slot.LabelReadable}");
        foreach (var stat in this.Profile.Role.Stats)
        {
            var value = stat.Worker.CalculateStat(this.Owner);
            yield return new LabelNew($"{stat}: {value}");
        }
    }

    internal float? GetWorkValue(ToolUseDef toolUse)
    {
        if (this.Profile.ToolUse != toolUse)
            return null;
        return this.BaseWork;
    }

    public override void Randomize(GameObject parent, RandomThreaded random)
        => ToolSystem.Randomize(this.Owner);
}
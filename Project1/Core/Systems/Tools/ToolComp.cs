using Microsoft.Xna.Framework;
using Project1.Core.Animations;
using Project1.Core.Entities;
using Project1.Core.Entities.Stats;
using Project1.Core.Helpers;
using Project1.Core.Stats;
using Project1.Core.Systems.Gear;
using Project1.Core.Systems.Quality;
using Project1.Framework;
using Project1.Framework.Helpers;
using Project1.Framework.Serialization;
using Project1.Framework.UI;
using System;
using System.Collections.Generic;
using System.Linq;

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
    //public ToolUseDef ToolUse;
    public GearProfileDef ToolDef;
    readonly List<ToolUseDef> Skills = [];
    public readonly Dictionary<StatBonusDef, float> ActorEnhancements = [];
    readonly Dictionary<StatBonusDef, float> ItemEnhancements = [];
    public readonly Dictionary<StatDef, float> Modifiers = [];
    float? baseSpeed, baseWork;
    public float BaseSpeed => this.baseSpeed ??= this.CalculateBaseSpeed();
    public float BaseWork => this.baseWork ??= this.CalculateBaseWorkAmount();
    internal override void ResolveReferencesNew()
    {
        ToolSystem.BakeStats(this.Owner);
        this.RefreshStats();

        //this.Owner.SetName($"{this.Owner.Body.FindBone(BoneDefOf.ToolHead).Material.LabelReadable} {this.Owner.Profile.LabelReadable} (Handle: {this.Owner.Body.FindBone(BoneDefOf.ToolHandle).Material.LabelReadable})");
        //this.Owner.SetName($"{this.Owner.Body.Material.LabelReadable} {this.Owner.Profile.LabelReadable}");
        //ToolSystem.Validate(this.Owner);
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
        //this.ToolUse = comp.ToolUse;
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
        if (profile.Damage is DamageDef damage)
            //yield return new LabelNew($"Damage: {damage.LabelReadable}");
            yield return new LabelNew($"{this.BaseWork} {damage.LabelReadable} damage");
        if(profile.ToolUses is not null)
            foreach(var use in profile.ToolUses)
                yield return new LabelNew($"{this.BaseWork} {use.LabelReadable} effectiveness");

        foreach (var stat in this.Profile.Role.Stats)
        {
            var value = stat.Worker.CalculateStat(this.Owner);
            //yield return new LabelNew($"{stat}: +{value}");
            //yield return new LabelNew($"+{StatSystem.Format(stat, value)} {stat.LabelReadable}");
            yield return new LabelNew($"+{StatSystem.Format(stat, value)} {stat.Worker.Format(stat, this.Owner)}");
        }

        foreach(var (def, value) in this.ActorEnhancements)
        {
            yield return new LabelNew($"+{value:#} {def.TargetDef.LabelReadable}") { TextColorFunc = () => value > 0 ? Color.Lime : Color.Red };
        }

        foreach(var (stat, value) in this.Modifiers)
            yield return new LabelNew($"+{value:#} {stat.LabelReadable}") { TextColorFunc = () => value > 0 ? Color.Lime : Color.Red };
    }

    internal float? GetWorkValue(ToolUseDef toolUse)
    {
        if (this.Profile.ToolUse != toolUse)
            return null;
        return this.BaseWork;
    }

    public void AddActorEnhancement(StatBonusDef targetDef, float value)
        => this.ActorEnhancements.Add(targetDef, value);

    public void AddItemEnhancement(StatBonusDef targetDef, float value)
        => this.ItemEnhancements.Add(targetDef, value);

    public override void Randomize(GameObject parent, RandomThreaded random)
        => ToolSystem.Randomize(this.Owner);

    public override void Write(IDataWriter w)
    {
        //w.Write(this.ActorEnhancements.Count);
        //foreach(var b in this.ActorEnhancements)
        //{
        //    w.Write(b.Key);
        //    w.Write(b.Value);
        //}
        w.Write(this.ActorEnhancements.Keys.ToArray());
        w.Write(this.Modifiers.Keys.ToArray());
        w.Write(this.Modifiers.Values);
    }

    public override void Read(IDataReader r)
    {
        //var count = r.ReadInt32();
        //for (int i = 0; i < count; i++)
        //{
        //    var def = r.ReadDef<StatBonusDef>();
        //    var value = r.ReadSingle();
        //    this.ActorEnhancements.Add(def, value);
        //}

        var actorBonuses = r.ReadListDef<StatBonusDef>();
        foreach(var b in actorBonuses)
            this.ActorEnhancements.Add(b, b.Worker.Calculate(this.Owner));
        var statMods = r.ReadListDef<StatDef>();
        var statModsValues = r.ReadListSingle();
        var dic = statMods.Zip(statModsValues);
        //foreach (var mod in statMods)
        //    this.Modifiers[mod] = -1;
        foreach (var (stat, value) in dic)
            this.Modifiers[stat] = value;
    }

    internal override void PostLoad()
    {
        foreach (var mod in this.Modifiers.Keys)
            this.Modifiers[mod] = this.Owner.Quality.RankEffective;
    }

    internal override void SaveExtra(SaveTag tag)
    {
        tag.SaveDefs("ActorBonuses", this.ActorEnhancements.Keys.ToArray());
    }

    internal override void LoadExtra(SaveTag tag)
    {
        if (tag.TryLoadDefs<StatBonusDef>("ActorBonuses", out var list))
            foreach (var b in list)
            {
                this.ActorEnhancements.Add(b, b.Worker.Calculate(this.Owner));
                //b.Worker.Apply(this.Owner, b);
            }
    }
}
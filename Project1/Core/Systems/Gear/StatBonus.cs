using Project1.Core.Attributes;
using Project1.Core.Entities;
using Project1.Core.Resources;
using Project1.Core.Stats;
using Project1.Core.Systems.Tools;
using Project1.Framework;
using System;

namespace Project1.Core.Systems.Gear;

public class StatBonusDef(string name, Def targetDef) : Def(name)
{
    public readonly Def TargetDef = targetDef;
    public string Label;
    public Type WorkerType;
    public StatBonusWorker Worker => field ??= Activator.CreateInstance(this.WorkerType, this) as StatBonusWorker;
}

[EnsureStaticCtorCall]
static public class StatBonusDefOf
{
    static readonly public StatBonusDef Strength = new("Strength", AttributeDefOf.Strength) { Label = "the Bear", WorkerType = typeof(StatBonusWorker_Actor) };
    static readonly public StatBonusDef Intelligence = new("Intelligence", AttributeDefOf.Intelligence) { Label = "the Owl", WorkerType = typeof(StatBonusWorker_Actor) };
    static readonly public StatBonusDef Dexterity = new("Dexterity", AttributeDefOf.Dexterity) { Label = "the Tiger", WorkerType = typeof(StatBonusWorker_Actor) };
    static readonly public StatBonusDef Sturdy = new("Sturdy", StatDefOf.Armor) { Label = "Sturdy", WorkerType = typeof(StatBonusWorker_Item) };
    static readonly public StatBonusDef Durable = new("Durable", ResourceDefOf.Durability) { Label = "Durable", WorkerType = typeof(StatBonusWorker_Item) };
    static StatBonusDefOf()
    {
        Def.Register(typeof(StatBonusDefOf));
    }
}

public abstract class StatBonusWorker(StatBonusDef def)
{
    protected StatBonusDef Def = def;
    protected abstract string Format(Entity item);
    public abstract void ApplyNew(Entity item);
    public void Apply(Entity item)
    {
        var comp = item.GetComponent<ToolComp>();
        comp.AddActorEnhancement(this.Def, 5f);
        item.SetName(this.Format(item));
    }        
}

public class StatBonusWorker_Item(StatBonusDef def) : StatBonusWorker(def)
{
    public override void ApplyNew(Entity item)
    {
        var comp = item.GetComponent<ToolComp>();
        comp.AddItemEnhancement(this.Def, 5f);
        item.SetName(this.Format(item));
    }

    protected override string Format(Entity item)
    => $"{this.Def.Label} {item.Name}";
}

public class StatBonusWorker_Actor(StatBonusDef def) : StatBonusWorker(def)
{
    public override void ApplyNew(Entity item)
    {
        var comp = item.GetComponent<ToolComp>();
        comp.AddActorEnhancement(this.Def, 5f);
        item.SetName(this.Format(item));
    }
    protected override string Format(Entity item)
    => $"{item.Name} of {this.Def.Label}";
}

//public class StatBonusWorker_Suffix(StatBonusDef def) : StatBonusWorker(def)
//{
//    protected override string Format(Entity item)
//        => $"{item.Name} {this.Def.Label}";
//}

//public class StatBonusWorker_Prefix(StatBonusDef def) : StatBonusWorker(def)
//{
//    protected override string Format(Entity item)
//        => $"{this.Def.Label} {item.Name}";
//}

abstract class ItemEnhancement
{
    public abstract void Apply(Entity item);
}

class ItemEnhancement_Weight
{
    public void Apply(Entity item)
    {
        item.Physics.AddWeightModifier(.5f);
    }
}

class ItemEnhancement_Durability
{
    public void Apply(Entity item)
    { 
        var dur = item.Resources.View(ResourceDefOf.Durability);
        dur.Max += 7;
    }
}

//abstract class StatBonusWorker_Item(Def targetDef)
//{
//    protected Def TargetDef = targetDef;
//    public abstract void Apply(Entity item);

//}

//class StatBonusWorker_Item_Resource(Def targetDef) : StatBonusWorker_Item(targetDef)
//{
//    public override void Apply(Entity item)
//    {
//        var dur = item.Resources.View((ResourceDef)this.TargetDef);
//        dur.Max += 7;
//    }
//}
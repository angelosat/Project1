using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Tools;
using Project1.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Systems.Gear;

public class StatBonusDef(string name, Def targetDef, Type workerType) : Def($"{name}:{targetDef?.LabelReadable}")
{
    public readonly Def TargetDef = targetDef;
    public string Label;
    public Type WorkerType = workerType;
    public StatBonusWorker Worker => field ??= Activator.CreateInstance(this.WorkerType) as StatBonusWorker;
}

[EnsureStaticCtorCall]
static public class StatBonusDefOf
{
    //static readonly public StatBonusDef Strength = new("Strength", AttributeDefOf.Strength) { Label = "the Bear", WorkerType = typeof(StatBonusWorker_Actor) };
    //static readonly public StatBonusDef Intelligence = new("Intelligence", AttributeDefOf.Intelligence) { Label = "the Owl", WorkerType = typeof(StatBonusWorker_Actor) };
    //static readonly public StatBonusDef Dexterity = new("Dexterity", AttributeDefOf.Dexterity) { Label = "the Tiger", WorkerType = typeof(StatBonusWorker_Actor) };
    //static readonly public StatBonusDef Sturdy = new("Sturdy", StatDefOf.Armor) { Label = "Sturdy", WorkerType = typeof(StatBonusWorker_Item) };
    //static readonly public StatBonusDef Durable = new("Durable", ResourceDefOf.Durability) { Label = "Durable", WorkerType = typeof(StatBonusWorker_Item) };
    static StatBonusDefOf()
    {
        //Def.Register(typeof(StatBonusDefOf));

        var contexts = Def.Get<StatBonusContextDef>();

        var toRegister = new List<StatBonusDef>();
        foreach (var contextDef in contexts)
            //toRegister.AddRange(Def.Get(contextDef.TargetDefType).Select(d => new StatBonusDef(contextDef.LabelReadable, d, contextDef.WorkerType)));
            toRegister.AddRange(contextDef.Worker.GetTargetDefs().Select(d => new StatBonusDef(contextDef.LabelReadable, d, contextDef.WorkerType)));
        foreach (var role in toRegister)
            Def.Register(role);
        return;
    }
}

public class StatBonusContextDef(string name, /*Type targetDefType, */Type workerType) : Def(name)
{
    //public Type TargetDefType = targetDefType;
    public Type WorkerType = workerType;
    public StatBonusWorker Worker => field ??= (StatBonusWorker)Activator.CreateInstance(this.WorkerType);
}

[EnsureStaticCtorCall]
static public class StatBonusContextDefOf
{
    static public readonly StatBonusContextDef Attribute = new("Attribute", /*typeof(AttributeDef), */typeof(StatBonusWorker_Actor_Attribute));
    static public readonly StatBonusContextDef Resource = new("Resource", /*typeof(ResourceDef), */typeof(StatBonusWorker_Actor_Resource));
    static public readonly StatBonusContextDef Trait = new("Trait", /*typeof(TraitDef), */typeof(StatBonusWorker_Actor_Trait));

    static StatBonusContextDefOf()
    {
        Def.Register(typeof(StatBonusContextDefOf));
    }
}

public abstract class StatBonusWorker()//StatBonusDef def)
{
    //protected StatBonusDef Def = def;
    protected abstract string Format(Entity item, StatBonusDef def);
    public abstract void ApplyNew(Entity item, StatBonusDef def);
    public void Apply(Entity item, StatBonusDef def)
    {
        var comp = item.GetComponent<ToolComp>();
        //comp.AddActorEnhancement(this.Def, 5f);
        item.SetName(this.Format(item, def));
    }
    public virtual IEnumerable<Def> GetTargetDefs() => [];
}
//public abstract class StatBonusWorker_ActorNew : StatBonusWorker
//{
//    public virtual IEnumerable<Def> GetTargetDefs() => [];
//}
//public class StatBonusWorker_Actor_Attribute(StatBonusDef def) : StatBonusWorker_Actor(def)
public class StatBonusWorker_Actor_Attribute : StatBonusWorker_Actor
{
    public override IEnumerable<Def> GetTargetDefs()
        => ActorDnaDefOf.Npc.Attributes;
}
public class StatBonusWorker_Actor_Resource : StatBonusWorker_Actor
{
    public override IEnumerable<Def> GetTargetDefs()
        => ActorDnaDefOf.Npc.Resources;
}
public class StatBonusWorker_Actor_Trait : StatBonusWorker_Actor
{
    public override IEnumerable<Def> GetTargetDefs()
        => ActorDnaDefOf.Npc.Traits;
}
public class StatBonusWorker_Item : StatBonusWorker
{
    public override void ApplyNew(Entity item, StatBonusDef def)
    {
        var comp = item.GetComponent<ToolComp>();
        comp.AddItemEnhancement(def, 5f);
        item.SetName(this.Format(item, def));
    }

    protected override string Format(Entity item, StatBonusDef def)
    => $"{def.Label} {item.Name}";
}

public class StatBonusWorker_Actor : StatBonusWorker
{
    public override void ApplyNew(Entity item, StatBonusDef def)
    {
        var comp = item.GetComponent<ToolComp>();
        comp.AddActorEnhancement(def, 5f);
        item.SetName(this.Format(item, def));
    }
    protected override string Format(Entity item, StatBonusDef def)
    => $"{item.Name} of {def.TargetDef.LabelReadable}";
}
//public class StatBonusWorker_Item(StatBonusDef def) : StatBonusWorker(def)
//{
//    public override void ApplyNew(Entity item)
//    {
//        var comp = item.GetComponent<ToolComp>();
//        comp.AddItemEnhancement(this.Def, 5f);
//        item.SetName(this.Format(item));
//    }

//    protected override string Format(Entity item)
//    => $"{this.Def.Label} {item.Name}";
//}

//public class StatBonusWorker_Actor(StatBonusDef def) : StatBonusWorker(def)
//{
//    public override void ApplyNew(Entity item)
//    {
//        var comp = item.GetComponent<ToolComp>();
//        comp.AddActorEnhancement(this.Def, 5f);
//        item.SetName(this.Format(item));
//    }
//    protected override string Format(Entity item)
//    => $"{item.Name} of {this.Def.TargetDef.LabelReadable}";
//}

using Project1.Core.Entities.Stats;
using Project1.Core.Systems.Gear;
using Project1.Core.Systems.Stats;
using Project1.Framework.Events;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Entities.Actors;

public sealed class ActorStatTracker(Actor actor)
{
    readonly Actor Actor = actor;

    //readonly Dictionary<StatBonusDef, float> Bonuses = [];

    Dictionary<StatDef, StatRuntime> Stats => field ??= StatSystem.ActorStats.ToDictionary(a => a, a => new StatRuntime(a));

    //bool BonusesDirty = true, StatsDirty = true;

    //public void MarkBonusesDirty()
    //{
    //    this.BonusesDirty = true;
    //    foreach (var runtime in this.Stats.Values)
    //        runtime.MarkDirty();
    //}

    //public void MarkStatsDirty() => this.StatsDirty = true;
   
    public IEnumerable<StatRuntime> GetAllStatRuntimes()
        => this.Stats.Values;

    public IEnumerable<StatDef> GetAllStatDefs()
        => this.Stats.Keys;

    internal void MarkDirty(StatDef stat)
        => this.Stats[stat].MarkDirty();

    //public void ModifyBonus(StatDef def, float delta)
    //{
    //    this.Stats[def].Mod += delta;
    //    this.MarkStatsDirty();
    //}

    public float GetFinal(StatDef def)
    {
        var runtime = this.Stats[def];
        if (runtime.Dirty)
        {
            ResolveBonus(def, runtime);
            ResolveValue(def, runtime);
            runtime.Dirty = false;
        }
        return runtime.Value + runtime.Mod;
    }

    private void ResolveValue(StatDef def, StatRuntime runtime)
        => runtime.Value = def.BaseValue + def.Worker.CalculateActorStat(def, this.Actor);

    private void ResolveBonus(StatDef def, StatRuntime runtime)
        => runtime.Mod = this.Actor.Effects.GetStatMod(def);

    internal string ResolveAndFormat(StatDef def)
    {
        var runtime = this.Stats[def];
        var value = this.GetFinal(def);
        return $"{StatSystem.Format(def, value)}"; // +{StatSystem.Format(def, runtime.Bonus)}";
    }

    //internal string Breakdown(StatDef def)
    //{
    //    var runtime = this.Stats[def];
    //    return $"{def.LabelReadable}: {StatSystem.Format(def, runtime.Value)} (base) + {StatSystem.Format(def, runtime.Mod)} (bonus)";
    //}

    internal string Breakdown(StatDef def)
    {
        this.GetFinal(def);
        var runtime = this.Stats[def];

        return $"{def.LabelReadable}: {StatSystem.Format(def, runtime.Value)} (base) + {StatSystem.Format(def, runtime.Mod)} (bonus)";
    }

    internal ChangeNotifier GetNotifier(StatDef def)
        => this.Stats[def].Notifier;
}

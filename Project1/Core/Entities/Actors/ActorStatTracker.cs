using Project1.Core.Entities.Stats;
using Project1.Core.Systems.Gear;
using Project1.Core.Systems.Stats;
using Project1.Framework.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project1.Core.Entities.Actors;

public sealed class ActorStatTracker(Actor actor)
{
    readonly Actor Actor = actor;
    readonly Dictionary<StatBonusDef, float> Bonuses = [];
    readonly Dictionary<StatDef, float> BonusesNew = [];
    Dictionary<StatDef, StatRuntime> Stats => field ??= StatSystem.ActorStats.ToDictionary(a => a, a => new StatRuntime(a));
    bool BonusesDirty = true, StatsDirty = true;
    public bool IsStatsDirty => this.StatsDirty;
    public void MarkBonusesDirty()
    {
        this.BonusesDirty = true;
        foreach (var runtime in this.Stats.Values)
            runtime.MarkDirty();
    }
    public void MarkStatsDirty() => this.StatsDirty = true;
    public IEnumerable<(StatDef, float)> GetAllStats()
    {
        if (this.StatsDirty)
            this.RebuildStats();
        foreach (var (stat, runtime) in this.Stats)
            yield return (stat, runtime.Value);
    }
    public IEnumerable<StatRuntime> GetAllStatRuntimes()
        => this.Stats.Values;
    public IEnumerable<StatDef> GetAllStatDefs()
    => this.Stats.Keys;

    public void AddBonus(StatBonusDef def, float val)
    {
        this.Bonuses.TryGetValue(def, out var current);
        this.Bonuses[def] = current + val;

        //this.Values[def] += val;
        this.MarkBonusesDirty();
    }

    internal void MarkDirty(StatDef stat)
        => this.Stats[stat].MarkDirty();

    public void ModifyBonus(StatDef def, float delta)
    {
        this.Stats[def].Bonus += delta;
        this.MarkStatsDirty();
    }

    public void Clear()
    {
        this.Bonuses.Clear();
        this.MarkBonusesDirty();
    }
    public bool TryGetBonus(StatBonusDef def, out float val)
    {
        if (this.BonusesDirty)
            this.RebuildBonuses();
        return this.Bonuses.TryGetValue(def, out val);
    }

    void RebuildBonuses()
    {
        this.BonusesDirty = false;
        this.Bonuses.Clear();
        Dictionary<StatDef, float> temp = [];
        foreach (var comp in this.Actor.Components.Values)
            foreach (var (bonus, value) in comp.GetBonuses())
            {
                this.Bonuses[bonus] = this.Bonuses.GetValueOrDefault(bonus) + value;
                //temp[bonus.TargetStatDef] = temp.GetValueOrDefault(bonus.TargetStatDef) + value;
            }
        foreach (var comp in this.Actor.Components.Values)
            foreach (var (bonus, value) in comp.GetModifiers())
            {
                temp[bonus] = temp.GetValueOrDefault(bonus) + value;
                //temp[bonus.TargetStatDef] = temp.GetValueOrDefault(bonus.TargetStatDef) + value;
            }
        foreach (var (stat, runtime) in this.Stats)
        {
            if(temp.TryGetValue(stat, out var mod))
                runtime.Value = mod;
                //runtime.Bonus = mod;
        }
    }

    public float Get(StatDef def)
    {
        var runtime = this.Stats[def];
        //var bonus = this.BonusesNew.GetValueOrDefault(def);
        //return runtime.Resolve(this.Actor);
        if (runtime.Dirty)
        {
            var bonus = this.Actor.Components.Values.Sum(c => c.GetStatMod(def));
            runtime.Bonus = bonus;
            runtime.Resolve(this.Actor);
        }
        return runtime.Value;// * (def.Type == StatDef.Types.Percentile ? .01f : 1f);

        //if (runtime.Dirty)
        //{
        //    runtime.Dirty = false;
        //    runtime.Value = def.BaseValue + runtime.Def.Worker.CalculateStatActor(runtime.Def, this.Actor);
        //    //foreach (var comp in this.Actor.Components.Values)
        //    //    runtime.Value = comp.GetStatMod(def) ?? runtime.Value;
        //}
        //return runtime.Value * (def.Type == StatDef.Types.Percentile ? .01f : 1f);

    }

    internal string ResolveAndFormat(StatDef def)
    {
        var runtime = this.Stats[def];
        var value = this.Get(def);
        var formatted = StatSystem.Format(def, value);
        return formatted + $"{runtime.Bonus}";
    }

    internal ChangeNotifier GetNotifier(StatDef def)
        => this.Stats[def].Notifier;

    void RebuildStats()
    {
        this.StatsDirty = false;
        this.RebuildBonuses();
        //foreach (var (stat, runtime) in this.Stats)
        //    runtime.Value = stat.CalculateFor(this.Actor);
    }

    internal void Remove(StatDef def, float value)
    {
        this.Stats[def].Value -= value;
    }
    internal void Add(StatDef def, float value)
    {
        this.Stats[def].Value += value;
    }

   
}

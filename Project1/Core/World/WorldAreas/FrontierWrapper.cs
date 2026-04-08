using Project1.Core.Entities;
using Project1.Core.Entities.Actors;
using Project1.Core.Skills;
using Project1.Framework.Helpers;
using System;
using System.Collections.Generic;

namespace Project1.Core.World.WorldAreas;

public class FrontierWrapper
{
    public readonly FrontierDef Def;
    List<Entity> Treasures = [];
    public FrontierWrapper(FrontierDef def)
    {
        this.Def = def;
    }

    internal void AddTreasure(Entity entity)
    {
        this.Treasures.Add(entity);
    }
    internal bool TryFindTreasure(Random rand, Actor actor, out Entity treasure)
    {
        if(this.Treasures.Count == 0)
        {
            treasure = null;
            return false;
        }
        var exploration = actor.Skills.GetLevel(SkillDefOf.Exploration);
        var chance = 10 + exploration;
        var roll = rand.Roll100(chance);
        if (!roll)
        {
            treasure = null;
            return false;
        }
        treasure = this.Treasures.SelectRandom(rand);
        this.Treasures.Remove(treasure);
        return true;
    }
    internal void Tick(Actor actor)
    {
        // roll encounter
        // roll random loot
        // etc
    }
}

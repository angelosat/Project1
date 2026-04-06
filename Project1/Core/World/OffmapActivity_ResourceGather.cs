using Project1.Core.Entities.Actors;
using Project1.Core.Systems.Materials;
using Project1.Core.World.WorldAreas;
using Project1.Framework.Helpers;
using System.Collections.Generic;

namespace Project1.Core.World;

//internal sealed class OffmapActivity_ResourceGather : OffmapActivity
//{
//    //static List<MaterialTypeDef> AvailableTypes = [MaterialTypeDefOf.Wood, MaterialTypeDefOf.Metal]; 
//    static List<MaterialRefinementDef> AvailableLoot = [MaterialRefinementDefOf.Logs, MaterialRefinementDefOf.Ore]; 
//    internal override void Tick(FrontierWrapper frontier, Actor actor)
//    {
//        var tier = frontier.Def.Tier;
//        var random = actor.World.Random;
//        //var selectedType = AvailableTypes.SelectRandom(random);
//        var selectedRef = AvailableLoot.SelectRandom(random);
//        var selectedMat = MaterialSystem.ByTierAndType(tier, selectedRef);
//        var skill = selectedMat.Type.GatheringSkill;
//        var item = MaterialSystem.Create(selectedRef, selectedMat, 1);
//        actor.Inventory.Insert(item);
//        actor.Skills.ApplyXp(skill, 10);
//    }
//}

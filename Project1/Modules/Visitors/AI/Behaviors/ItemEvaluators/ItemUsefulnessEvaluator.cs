using System;
using System.Linq;

namespace Start_a_Town_.AI
{
    static public class ItemUsefulnessEvaluator
    {
        static public int EvaluateTool(Actor actor, Entity item)
        {
            var tool = item.Def.ToolProperties;
            if (tool == null)
                return 0;
            var ability = tool.Ability;
            var similarOwnedItem = actor.Inventory.FindItems((Entity item) => item.Def.ToolProperties?.Ability.Def == ability.Def).FirstOrDefault();
            var benefit = ability.Effectiveness - (similarOwnedItem?.Def.ToolProperties.Ability.Effectiveness ?? 0); // TODO get total effeciency (multiplied by item level/quality?
            return benefit;
        }

        static public int EvaluateApparel(Actor actor, Entity item)
        {
            var apparel = item.Def.ApparelProperties;
            if (apparel == null)
                return 0;
            int benefit = 0;
            var similarOwnedItem = actor.Inventory.FindItems((Entity item) => item.Def.ApparelProperties?.GearType == apparel.GearType).FirstOrDefault();
            benefit = apparel.ArmorValue - (similarOwnedItem?.Def.ApparelProperties.ArmorValue ?? 0);
            return benefit;
        }
        static Func<Actor, Entity, int>[] Evaluators = { EvaluateTool, EvaluateApparel };
        static public int Evaluate(Actor actor, Entity Item)
        {
            var score = Evaluators.Sum(e => e(actor, Item));
            return score;
        }
    }
}

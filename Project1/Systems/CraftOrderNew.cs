using System.Collections.Generic;

namespace Start_a_Town_
{
    internal class CraftOrderNew
    {
        public enum CraftMode
        {
            FixedAmount,       // Craft X times
            StockpileLimit,    // Craft until stockpile has at least X
            Infinite           // Craft forever
        }

        public CraftMode Mode;
        public int Amount; // X for FixedAmount or StockpileLimit, ignored for Infinite

        public bool Enabled;

        public EntityCreationRequest Target { get; init; }
        //public int Quantity { get; init; }

        // Explicit actor restriction
        public HashSet<int> AllowedActors = [];

        // Minimum skill requirement
        public int SkillFilter;

        public SkillDef Skill { get; init; }

        // Optional input constraints
        public Dictionary<MaterialTypeDef, int> RequiredInputs = [];

        public CraftOrderNew(MaterialMappingDef mapping)
        {
            this.Skill = mapping.MaterialType.SkillToRefine;
            //this.Target = new EntityCreationRequest(stage: mapping.Process)
        }

        public bool CanActorPerform(Actor actor)
        {
            if (!this.Enabled) return false;
            if (this!.AllowedActors.Contains(actor.RefId)) return false;
            if (actor.Skills.GetSkill(this.Skill).Level < SkillFilter) return false;
            return true;
        }

        public bool ShouldQueue(int currentStock)
        {
            return Mode switch
            {
                CraftMode.FixedAmount => Amount > 0,
                CraftMode.StockpileLimit => currentStock < Amount,
                CraftMode.Infinite => true,
                _ => false
            };
        }
    }
}

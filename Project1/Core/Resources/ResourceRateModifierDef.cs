using Project1.Core.Entities;
using Project1.Core.Stats;
using Project1.Core.Base;
using System;

namespace Project1.Core.Resources
{
    public class ResourceRateModifierDef : Def
    {
        public enum Types { Permanent, Finite }
        public Types Type;
        Func<GameObject, float> Function;
        public ResourceDef Source;
        public int BaseDurationInTicks = 0;
        public ResourceRateModifierDef(string name, ResourceDef source) : base(name)
        {
           
            this.Source = source;
        }
        public float GetRateMod(GameObject parent)
        {
            return this.Function(parent);
        }
        static public readonly ResourceRateModifierDef HaulingStaminaDrain = new("HaulingStaminaDrain", ResourceDefOf.Stamina) { 
            Function = 
                actor =>
                {
                    var val = StatDefOf.Encumberance.CalculateFor(actor);
                    var factor = .1f;
                    val *= factor;
                    return -val;
                },
            Type = Types.Permanent
        };
    }
}

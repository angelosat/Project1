using Microsoft.Xna.Framework;
using Project1.Core.Entities.Actors;
using Project1.Core.Resources;
using Project1.Core.Systems.Effects;

namespace Project1.Core.Systems.Magic;

//public sealed record Effect_DamageResource : EntityEffectWorker
//{
//    public override EffectDef Def => EffectDefOf.;

//    protected override void OnStart(Actor actor, EntityEffectRuntime wrapper)
//    {
//        var resource = ResourceDefOf.Health;
//        actor.Resources.ApplyDelta(resource, -(int)wrapper.Budget.Value);
//    }

//    internal override Color GetTint(EntityEffectRuntime runtime)
//    => ((SpellSchoolDef)runtime.Context).Color;

//    //internal override string GetString(EntityEffectRuntime runtime)
//    //    => $"Damages {base.GetString(target, magnitude)} instantly";
//}

using System;

namespace Start_a_Town_
{
    class InteractionDepart : Interaction
    {
        public override void Perform()
        {
            var a = this.Actor;
            var area = OffsiteAreaDefOf.Forest; //TODO store target visitor area in the visitorproperites class when the decision to depart occurs and fetch it from there
            a.VisitOffsiteArea(area);
            a.Despawn();
            a.Net.Report($"{a.Name} has departed!");
        }
        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}

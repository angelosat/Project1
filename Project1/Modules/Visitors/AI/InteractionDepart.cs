using System;
namespace Start_a_Town_
{
    class InteractionDepartLogic : InteractionLogic
    {
        internal override void OnStart(Interaction i)
        {
            var a = i.Actor;
            var area = FrontierDefOf.Forest; //TODO store target visitor area in the visitorproperites class when the decision to depart occurs and fetch it from there
            var world = a.World as StaticWorld;
            world.Space.Enter(a);
            i.Finish();
        }
    }
    class InteractionDepart : Interaction
    {
        public InteractionDepart()
        {
        }
        public override void Perform()
        {
            var a = this.Actor;
            var area = FrontierDefOf.Forest; //TODO store target visitor area in the visitorproperites class when the decision to depart occurs and fetch it from there
            var world = a.World as StaticWorld;
            world.Space.Enter(a);
        }
    }
}

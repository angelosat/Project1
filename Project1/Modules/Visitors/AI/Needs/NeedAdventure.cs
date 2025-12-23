namespace Start_a_Town_
{
    public class NeedAdventure : NeedWorker
    {
        protected override void TickExtra(Need need)
        {
            need.Value = (need.Owner.World.CurrentTick - (need.Owner as Actor).AI.Meta.LastMapTransitionTick) / 100;
        }
    }
}

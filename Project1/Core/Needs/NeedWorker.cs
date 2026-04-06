namespace Project1.Core.Needs;

public abstract class NeedWorker
{
    public void Tick(NeedRuntime need)
    {
        need.ApplyAccumulatorDelta(-need.Def.DecayTicksPerUnit);
        var delta = need.AccumulatorNew.Flush();
        if (delta != 0)
            need.ApplyDelta(delta);
    }
    protected virtual void TickExtra(NeedRuntime need) { }
}

namespace Project1.Core.AI.Thought;

internal class ThoughtItemEvaluatorVisitor : ThoughtProcess
{
    public override void Tick(AIState state)
    {
        //var manager = state.ItemPreferences;
    }
}
internal class ThoughtItemEvaluatorTownMember : ThoughtProcess
{
    public override void Tick(AIState state)
    {
        var manager = state.ItemPreferences;
        manager.EvaluateOne();
    }
}

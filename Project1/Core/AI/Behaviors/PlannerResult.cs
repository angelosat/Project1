namespace Project1.Core.AI.Behaviors
{
    public struct PlannerResult
    {
        public readonly static PlannerResult Empty = new(null, null);

        public Plan Plan;
        public Planner Source;

        public PlannerResult(Plan plan, Planner source)
        {
            this.Plan = plan;
            this.Source = source;
        }
    }
}

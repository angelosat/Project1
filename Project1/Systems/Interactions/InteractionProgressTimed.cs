namespace Start_a_Town_
{
    internal class InteractionProgressInstant : IInteractionProgressHandler
    {
        public void Tick(Interaction interaction) => interaction.Progress.Complete();
        public void AddProgress(Interaction interaction, int progress) { }
        public bool IsFinished(Interaction interaction) => interaction.Progress.IsFinished;
        public float GetProgressPercentage(Interaction interaction) => 1f;
    }
    internal class InteractionProgressFirstContact : IInteractionProgressHandler
    {
        public void Tick(Interaction interaction) { }
        public void AddProgress(Interaction interaction, int progress) => interaction.Progress.Complete();                 
        public bool IsFinished(Interaction interaction) => interaction.Progress.IsFinished;
        public float GetProgressPercentage(Interaction interaction) => 1f;
    }
    internal class InteractionProgressTimed : IInteractionProgressHandler
    {
        public float GetProgressPercentage(Interaction interaction) => interaction.Progress.Percentage;
        public void Tick(Interaction interaction) => interaction.Progress.ApplyDelta(1);// interaction.AddProgress(1);
        public void AddProgress(Interaction interaction, int progress) { }
        public bool IsFinished(Interaction interaction) => interaction.Progress.IsFinished;
    }
    internal class InteractionProgressTool : IInteractionProgressHandler
    {
        public float GetProgressPercentage(Interaction interaction) => interaction.Progress.Percentage;
        public void Tick(Interaction interaction) { }
        public void AddProgress(Interaction interaction, int progress) => interaction.Progress.ApplyDelta(progress); //AddProgress(progress);// 
        public bool IsFinished(Interaction interaction) => interaction.Progress.IsFinished;
    }
    internal class InteractionProgressToolExternal : IInteractionProgressHandler
    {
        public float GetProgressPercentage(Interaction interaction) => interaction.Context.ProgressPercentage;
        public void Tick(Interaction interaction) { }
        public void AddProgress(Interaction interaction, int progress) => interaction.Def.Logic.ApplyWork(interaction.Context, progress);
        public bool IsFinished(Interaction interaction) => interaction.Context.ProgressPercentage >= 1;
    }
    internal class InteractionProgressPassive : IInteractionProgressHandler
    {
        public float GetProgressPercentage(Interaction interaction) => interaction.Context.ProgressPercentage;
        public void Tick(Interaction interaction) { }
        public void AddProgress(Interaction interaction, int progress) { }
        public bool IsFinished(Interaction interaction) => interaction.Context.ProgressPercentage >= 1;
    }
}

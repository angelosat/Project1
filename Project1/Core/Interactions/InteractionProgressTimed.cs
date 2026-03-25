namespace Project1.Core.Interactions
{
    public static class InteractionProgressHandlers
    {
        public static readonly IInteractionProgressHandler Instant = new InteractionProgressInstant();
        public static readonly IInteractionProgressHandler FirstContact = new InteractionProgressFirstContact();
        public static readonly IInteractionProgressHandler Timed = new InteractionProgressTimed();
        public static readonly IInteractionProgressHandler Internal = new InteractionProgressInternal();
        public static readonly IInteractionProgressHandler External = new InteractionProgressContextual();
        public static readonly IInteractionProgressHandler Passive = new InteractionProgressPassive();

        sealed class InteractionProgressInstant : IInteractionProgressHandler
        {
            public void Tick(Interaction interaction) => interaction.Progress.Complete();
            public void AddProgress(Interaction interaction, int progress) { }
            public bool IsFinished(Interaction interaction) => interaction.Progress.IsFinished;
            public float GetProgressPercentage(Interaction interaction) => 1f;
        }
        sealed class InteractionProgressFirstContact : IInteractionProgressHandler
        {
            public void Tick(Interaction interaction) { }
            public void AddProgress(Interaction interaction, int progress) => interaction.Progress.Complete();
            public bool IsFinished(Interaction interaction) => interaction.Progress.IsFinished;
            public float GetProgressPercentage(Interaction interaction) => 1f;
        }
        sealed class InteractionProgressTimed : IInteractionProgressHandler
        {
            public float GetProgressPercentage(Interaction interaction) => interaction.Progress.Percentage;
            public void Tick(Interaction interaction) => interaction.Progress.ApplyDelta(1);// interaction.AddProgress(1);
            public void AddProgress(Interaction interaction, int progress) { }
            public bool IsFinished(Interaction interaction) => interaction.Progress.IsFinished;
        }
        sealed class InteractionProgressInternal : IInteractionProgressHandler
        {
            public float GetProgressPercentage(Interaction interaction) => interaction.Progress.Percentage;
            public void Tick(Interaction interaction) { }
            public void AddProgress(Interaction interaction, int progress) => interaction.Progress.ApplyDelta(progress); //AddProgress(progress);// 
            public bool IsFinished(Interaction interaction) => interaction.Progress.IsFinished;
        }
        sealed class InteractionProgressContextual : IInteractionProgressHandler
        {
            public float GetProgressPercentage(Interaction interaction) => interaction.Context.ProgressPercentage;
            public void Tick(Interaction interaction) 
            {
                if (interaction.Actor.Net.IsClient)
                    return;
                interaction.Def.Logic.OnTick(interaction);
            }
            public void AddProgress(Interaction interaction, int progress)// => interaction.Def.Logic.ApplyWork(interaction.Context, progress);
            {
                if (interaction.Actor.Net.IsClient)
                    return;
                interaction.Def.Logic.ApplyWork(interaction.Context, progress);
            }
            public bool IsFinished(Interaction interaction) => interaction.Context.ProgressPercentage >= 1;
        }
        sealed class InteractionProgressPassive : IInteractionProgressHandler
        {
            public float GetProgressPercentage(Interaction interaction) => interaction.Context.ProgressPercentage;
            public void Tick(Interaction interaction) { }
            public void AddProgress(Interaction interaction, int progress) { }
            public bool IsFinished(Interaction interaction) => interaction.Context.ProgressPercentage >= 1;
        }
    }
}

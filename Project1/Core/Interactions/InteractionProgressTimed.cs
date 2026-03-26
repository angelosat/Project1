namespace Project1.Core.Interactions
{
    public static class InteractionProgressHandlers
    {
        public static readonly IInteractionProgressHandler Instant = new InteractionProgressInstant();
        public static readonly IInteractionProgressHandler FirstContact = new InteractionProgressFirstContact();
        public static readonly IInteractionProgressHandler Timed = new InteractionProgressTimed();
        public static readonly IInteractionProgressHandler Internal = new InteractionProgressInternal();
        public static readonly IInteractionProgressHandler External = new InteractionProgressContextual();
        public static readonly IInteractionProgressHandler ExternalFull = new InteractionProgressFullyExternal();
        public static readonly IInteractionProgressHandler Passive = new InteractionProgressPassive();

        sealed class InteractionProgressInstant : IInteractionProgressHandler
        {
            public void Tick(Interaction interaction) => interaction.Progress.Complete();
            public void AddProgressFromToolSwing(Interaction interaction, int progress) { }
            public bool IsFinished(Interaction interaction) => interaction.Progress.IsFinished;
            public float GetProgressBarPercentage(Interaction interaction) => 1f;
        }
        sealed class InteractionProgressFirstContact : IInteractionProgressHandler
        {
            public void Tick(Interaction interaction) { }
            public void AddProgressFromToolSwing(Interaction interaction, int progress) => interaction.Progress.Complete();
            public bool IsFinished(Interaction interaction) => interaction.Progress.IsFinished;
            public float GetProgressBarPercentage(Interaction interaction) => 1f;
        }
        sealed class InteractionProgressTimed : IInteractionProgressHandler
        {
            public float GetProgressBarPercentage(Interaction interaction) => interaction.Progress.Percentage;
            //public void Tick(Interaction interaction) => interaction.Progress.ApplyDelta(1);// interaction.AddProgress(1);
            public void Tick(Interaction interaction)
            {
                interaction.Progress.ApplyDelta(1);// interaction.AddProgress(1);
                if (interaction.Actor.Net.IsServer)
                    interaction.Def.Logic.OnTick(interaction);
            }
            public void AddProgressFromToolSwing(Interaction interaction, int progress) { }
            public bool IsFinished(Interaction interaction) => interaction.Progress.IsFinished;
        }
        sealed class InteractionProgressInternal : IInteractionProgressHandler
        {
            public float GetProgressBarPercentage(Interaction interaction) => interaction.Progress.Percentage;
            public void Tick(Interaction interaction) { }
            public void AddProgressFromToolSwing(Interaction interaction, int progress) => interaction.Progress.ApplyDelta(progress); //AddProgress(progress);// 
            public bool IsFinished(Interaction interaction) => interaction.Progress.IsFinished;
        }
        sealed class InteractionProgressContextual : IInteractionProgressHandler
        {
            public float GetProgressBarPercentage(Interaction interaction) => interaction.Context.ProgressBarPercentage;
            public void Tick(Interaction interaction) 
            {
                if (interaction.Actor.Net.IsClient)
                    return;
                interaction.Def.Logic.OnTick(interaction);
            }
            public void AddProgressFromToolSwing(Interaction interaction, int progress)// => interaction.Def.Logic.ApplyWork(interaction.Context, progress);
            {
                if (interaction.Actor.Net.IsClient)
                    return;
                interaction.Def.Logic.ApplyWork(interaction.Context, progress);
            }
            public bool IsFinished(Interaction interaction) => interaction.Context.ProgressBarPercentage >= 1;
        }
        sealed class InteractionProgressPassive : IInteractionProgressHandler
        {
            public float GetProgressBarPercentage(Interaction interaction) => interaction.Context.ProgressBarPercentage;
            public void Tick(Interaction interaction) { }
            public void AddProgressFromToolSwing(Interaction interaction, int progress) { }
            public bool IsFinished(Interaction interaction) => interaction.Context.ProgressBarPercentage >= 1;
        }
        sealed class InteractionProgressFullyExternal : IInteractionProgressHandler
        {
            // purely for visual feedback
            public float GetProgressBarPercentage(Interaction interaction) => interaction.Progress.Percentage;
            public void Tick(Interaction interaction)
            {
                //if (interaction.Actor.Net.IsClient)
                //    return;
                interaction.Def.Logic.OnTick(interaction);
            }
            public void AddProgressFromToolSwing(Interaction interaction, int progress) { }
            public bool IsFinished(Interaction interaction)
            {
                if (interaction.Actor.Net.IsClient)
                    return false;
                return interaction.Def.Logic.IsFinished(interaction);
            }
        }
    }
}

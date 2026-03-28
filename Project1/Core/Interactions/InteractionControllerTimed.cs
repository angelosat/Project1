namespace Project1.Core.Interactions
{
    public static class InteractionControllers
    {
        public static readonly IInteractionController Instant = new InteractionControllerInstant();
        public static readonly IInteractionController FirstContact = new InteractionControllerFirstContact();
        public static readonly IInteractionController Timed = new InteractionControllerTimed();
        public static readonly IInteractionController Internal = new InteractionControllerInternal();
        public static readonly IInteractionController External = new InteractionControllerContextual();
        public static readonly IInteractionController ExternalFull = new InteractionControllerFullyExternal();
        public static readonly IInteractionController Passive = new InteractionControllerPassive();

        sealed class InteractionControllerInstant : IInteractionController
        {
            public void Tick(Interaction interaction) => interaction.Progress.Complete();
            public void AddProgressFromToolSwing(Interaction interaction, int progress) { }
            public bool IsFinished(Interaction interaction) => interaction.Progress.IsFinished;
            public float GetProgressBarPercentage(Interaction interaction) => 1f;
        }
        sealed class InteractionControllerFirstContact : IInteractionController
        {
            public void Tick(Interaction interaction) { }
            public void AddProgressFromToolSwing(Interaction interaction, int progress) => interaction.Progress.Complete();
            public bool IsFinished(Interaction interaction) => interaction.Progress.IsFinished;
            public float GetProgressBarPercentage(Interaction interaction) => 1f;
        }
        sealed class InteractionControllerTimed : IInteractionController
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
        sealed class InteractionControllerInternal : IInteractionController
        {
            public float GetProgressBarPercentage(Interaction interaction) => interaction.Progress.Percentage;
            public void Tick(Interaction interaction) { }
            public void AddProgressFromToolSwing(Interaction interaction, int progress) => interaction.Progress.ApplyDelta(progress); //AddProgress(progress);// 
            public bool IsFinished(Interaction interaction) => interaction.Progress.IsFinished;
        }
        sealed class InteractionControllerContextual : IInteractionController
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
        sealed class InteractionControllerPassive : IInteractionController
        {
            public float GetProgressBarPercentage(Interaction interaction) => interaction.Context.ProgressBarPercentage;
            public void Tick(Interaction interaction) { }
            public void AddProgressFromToolSwing(Interaction interaction, int progress) { }
            public bool IsFinished(Interaction interaction) => interaction.Context.ProgressBarPercentage >= 1;
        }
        sealed class InteractionControllerFullyExternal : IInteractionController
        {
            // purely for visual feedback
            public float GetProgressBarPercentage(Interaction i) => i.Context.GetPercentage(i);
            public void Tick(Interaction i)
            {
                var logic = i.Def.Logic;
                logic.OnTick(i);
            }
            public void AddProgressFromToolSwing(Interaction interaction, int progress) { }
            public bool IsFinished(Interaction interaction)
                => false;
        }
    }
}

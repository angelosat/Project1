namespace Project1.Core.Interactions
{
   
    public interface IInteractionController
    {
        bool IsFinished(Interaction interaction);
        float GetProgressBarPercentage(Interaction interaction);
        void Tick(Interaction interaction);
        void AddProgressFromToolSwing(Interaction interaction, int progress);
    }
}

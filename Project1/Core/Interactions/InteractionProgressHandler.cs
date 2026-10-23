namespace Project1.Core.Interactions
{
   
    public interface IInteractionController
    {
        bool IsFinished(Interaction interaction);
        float GetProgressBarPercentage(Interaction interaction);
        void Tick(Interaction interaction);
        void AddWorkFromToolSwing(Interaction interaction, int progress);
    }
}

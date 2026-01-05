namespace Start_a_Town_
{
    public interface IInteractionProgressHandler
    {
        bool IsFinished(Interaction interaction);
        float GetProgressPercentage(Interaction interaction);
        void Tick(Interaction interaction);
        void AddProgress(Interaction interaction, int progress);
    }
}

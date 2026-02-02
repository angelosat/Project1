namespace Start_a_Town_
{
    public interface IPowerSource
    {
        void ConsumePower(MapBase map, float amount);
        bool HasAvailablePower(float amount);
        float GetRemaniningPower();
    }
}

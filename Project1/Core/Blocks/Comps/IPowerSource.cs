using Project1.Core.Simulation;

namespace Project1.Core
{
    public interface IPowerSource
    {
        void ConsumePower(MapBase map, float amount);
        bool HasAvailablePower(float amount);
        float GetRemaniningPower();
    }
}

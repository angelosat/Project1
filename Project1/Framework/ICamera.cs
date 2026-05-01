namespace Project1.Framework;

public interface ICamera
{
    double RotCos { get; }
    double RotSin { get; }

    void RotateClockwise();
    void RotateCounterClockwise();
    void RotationReset();
    void ZoomDecrease();
    void ZoomIncrease();
    void ZoomReset();
}

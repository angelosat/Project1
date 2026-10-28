namespace Project1.Core.World.PointsOfInterest;

internal class PoISecret(int keyFrame)
{
    internal readonly int Keyframe = keyFrame;
    bool Discovered;
    internal bool IsDiscovered => Discovered;
    internal void Discover() => Discovered = true;
}

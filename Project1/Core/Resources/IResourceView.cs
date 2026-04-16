using Project1.Framework.Serialization;

namespace Project1.Core.Resources;

public interface IResourceView
{
    public ResourceDef Def { get; }
    float Value { get; set; }
    float Percentage { get; set; }
    float Max { get; set; }
    int TicksPerRecoverOne { get; set; }
    ResourceThreshold CurrentThreshold { get; }
    void ApplyDelta(float delta);
    float GetThresholdValue(int index);

    IResourceView Write(IDataWriter w);
    IResourceView Read(IDataReader r);
}

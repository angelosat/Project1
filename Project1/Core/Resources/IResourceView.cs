using Project1.Framework.Serialization;

namespace Project1.Core.Resources;

public interface IResourceView
{
    public ResourceDef Def { get; }
    int Value { get; set; }
    float Percentage { get; set; }
    int Max { get; set; }
    int TicksPerRecoverOne { get; set; }
    ResourceThreshold CurrentThreshold { get; }
    void ApplyDelta(int delta);
    void ApplyAccumulatorRateDelta(float delta);
    void ApplyAccumulatorDelta(float delta);
    float GetThresholdValue(int index);

    IResourceView Write(IDataWriter w);
    IResourceView Read(IDataReader r);
}

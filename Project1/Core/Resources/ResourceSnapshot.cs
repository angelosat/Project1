namespace Project1.Core.Resources;

public readonly record struct ResourceSnapshot(ResourceDef Def, float Value, float Max)
{
    public float Percentage => this.Value / this.Max;
}

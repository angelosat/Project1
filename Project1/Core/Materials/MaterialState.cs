namespace Project1.Core.Materials
{
    public struct MaterialState
    {
        static readonly public MaterialState Gas = new();
        static readonly public MaterialState Solid = new();
        static readonly public MaterialState Liquid = new();
    }
}

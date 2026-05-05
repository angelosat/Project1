using Microsoft.Xna.Framework;

namespace Project1.Core.Simulation.Lighting;

/// <summary>
///  TODO convert sun field to vector4 as well
/// </summary>
public class LightToken
{
    public Vector3 Global;
    public Vector4 Sun, Block;

    public LightToken(Vector3 global, Vector4 sun, Vector4 block)
    {
        this.Global = global;
        this.Sun = sun;
        this.Block = block;
    }
}

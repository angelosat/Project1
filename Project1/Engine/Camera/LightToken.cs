using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    /// <summary>
    ///  TODO convert sun field to vector4 as well
    /// </summary>
    public class LightToken
    {
        public Vector3 Global;
        public Color Sun;
        public Vector4 Block;

        public LightToken(Vector3 global, Color sun, Vector4 block)
        {
            this.Global = global;
            this.Sun = sun;
            this.Block = block;
        }
    }
}

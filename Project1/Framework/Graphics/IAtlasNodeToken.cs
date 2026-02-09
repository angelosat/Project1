using Microsoft.Xna.Framework;

namespace Project1.Framework.Graphics
{
    public abstract class IAtlasNodeToken : Inspectable
    {
        public Vector2 TopLeftUV, TopRightUV, BottomLeftUV, BottomRightUV;
        public Rectangle Rectangle;
        public AtlasBase Atlas;
    }
}

using Microsoft.Xna.Framework;
using Project1.Core.Base;

namespace Project1.Core.Graphics
{
    public abstract class IAtlasNodeToken : Inspectable
    {
        public Vector2 TopLeftUV, TopRightUV, BottomLeftUV, BottomRightUV;
        public Rectangle Rectangle;
        public AtlasBase Atlas;
    }
}

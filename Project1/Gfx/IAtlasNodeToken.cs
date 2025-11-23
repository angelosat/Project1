using Microsoft.Xna.Framework;

namespace Start_a_Town_.Graphics
{
    public abstract class IAtlasNodeToken : Inspectable
    {
        public Vector2 TopLeftUV, TopRightUV, BottomLeftUV, BottomRightUV;
        public Rectangle Rectangle;
        public AtlasBase Atlas;
    }
}

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Components
{
    /// <summary>
    /// TODO: optimize
    /// </summary>
    struct Shadow
    {
        public GameObject Parent;
        public Vector3 Global;
        public float Alpha;
        const int ShadowVisibilityRange = 4;

        public Shadow(GameObject parent, Vector3 global)
        {
            this.Parent = parent;
            this.Global = global;
            this.Alpha = Math.Max(0, 1 - (parent.Global.Z - global.Z) / ShadowVisibilityRange);
        }
        public void Draw(MySpriteBatch sb, MapBase map, Camera camera)
        {
            if (camera.IsCompletelyHiddenByFog(this.Global.Z))
                return;
            float dn = this.Global.GetDrawDepth(map, camera);
            Vector2 pos = camera.GetScreenPosition(this.Global).ToFloored();
            Sprite.Shadow.Draw(sb, pos, Color.White * this.Alpha, 0, Sprite.Shadow.OriginGround, camera.Zoom*Parent.Body.Scale, SpriteEffects.None, dn);
        }
    }
}

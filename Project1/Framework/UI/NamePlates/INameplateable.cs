using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Project1.Framework.Rendering;
using Project1.Framework.WorldGen;

namespace Start_a_Town_.UI
{
    /// <summary>
    /// string Name { get; set; }
    /// Vector3 Global { get; set; }
    /// Rectangle GetBounds(Camera camera);
    /// Color GetNameplateColor();
    /// void NameplateInit(Nameplate plate);
    /// </summary>
    public interface INameplateable
    {
        //Rectangle Bounds { get; set; }
        string Name { get; }
        Vector3 Global { get; }
        MapBase Map { get; }
        Rectangle GetScreenBounds(Camera camera);
        Color GetNameplateColor();
        void OnNameplateCreated(Nameplate plate);
        void DrawNameplate(SpriteBatch sb, Rectangle viewport, Nameplate plate);
    }
}

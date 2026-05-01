using Microsoft.Xna.Framework;
using Project1.Core.Screens;
using Project1.Core.Simulation;

namespace Project1.Core.UI.NamePlates;

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
    Rectangle GetScreenBounds(MapViewport viewport);
    Color GetNameplateColor();
    void OnNameplateCreated(Nameplate plate);
    //void DrawNameplate(SpriteBatch sb, Rectangle viewport, Nameplate plate);
}

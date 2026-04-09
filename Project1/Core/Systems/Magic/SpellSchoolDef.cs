using Microsoft.Xna.Framework;

namespace Project1.Core.Systems.Magic;

public sealed class SpellSchoolDef(string name, Color color) : Def(name)
{
    public readonly Color Color = color;
}

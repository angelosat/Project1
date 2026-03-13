using Project1.Core.Graphics;

namespace Project1.Core.Systems.Consumables
{
    public class ConsumableDef(string name, string verb, Sprite sprite) : Def(name)
    {
        public string Verb = verb;
        public Sprite Sprite = sprite;
    }
}

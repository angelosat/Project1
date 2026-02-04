using Project1.Framework.Attributes;
using Project1.Framework.Entities;

namespace Project1.Core.Attributes
{
    class AttributeDexterity : AttributeWorker
    {
        public AttributeDexterity(AttributeDef def) : base(def)
        {
        }

        public override void Tick(GameObject obj, AttributeRuntime attributeStat)
        {
        }
    }
}

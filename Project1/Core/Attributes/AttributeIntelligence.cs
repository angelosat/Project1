using Project1.Core.Entities;

namespace Project1.Core.Attributes
{
    class AttributeIntelligence : AttributeWorker
    {
        public AttributeIntelligence(AttributeDef def) : base(def)
        {
        }

        public override void Tick(GameObject obj, AttributeRuntime attributeStat)
        {
        }
    }
}

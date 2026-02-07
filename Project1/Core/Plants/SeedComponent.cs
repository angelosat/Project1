using Project1.Core.Entities;
using Project1.Core.Interfaces;
using Project1.Core.UI;

namespace Project1.Core.Plants
{
    class SeedComponent : EntityComp
    {
        public override string Name { get; } = "Seed";

        public int Level = 1;

        public SeedComponent()
        {
        }
        public SeedComponent(SeedComponent toCopy)
        {
            this.Level = toCopy.Level;
        }

        public override object Clone()
        {
            return new SeedComponent(this);
        }

        internal override void GetSelectionInfo(IUISelection info, GameObject parent)
        {
            info.AddInfo(new Label() { TextFunc = () => string.Format("Grows into: {0}", this.Owner.Profile.Label) });
        }

        public new class Props : Spec<SeedComponent> { }
        
    }
}

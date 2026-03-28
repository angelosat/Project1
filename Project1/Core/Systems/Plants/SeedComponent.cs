using Project1.Framework.UI;
using Project1.Core.Entities;

namespace Project1.Core.Systems.Plants
{
    sealed class SeedComponent : EntityComp
    {
        public override EntityCompDef CompDef => EntityCompDefOf.Seed;
        public override string Name { get; } = "Seed";

        public int Level = 1;

        public SeedComponent()
        {
        }
        public SeedComponent(SeedComponent toCopy)
        {
            this.Level = toCopy.Level;
        }
        //internal override void GetSelectionInfo(IUISelection info, GameObject parent)
        //{
        //    info.AddInfo(new Label() { TextFunc = () => string.Format("Grows into: {0}", this.Owner.Profile.LabelReadable) });
        //}

        public new class Spec : Spec<SeedComponent> { }
        
    }
}

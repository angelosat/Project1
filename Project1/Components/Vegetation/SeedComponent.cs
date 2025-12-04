using Start_a_Town_.Components;
using Start_a_Town_.UI;
using System;
using System.IO;

namespace Start_a_Town_
{
    class SeedComponent : EntityComponent
    {
        public override string Name { get; } = "Seed";

        public int Level = 1;
        public PlantProperties Plant;

        public SeedComponent()
        {
        }
        public SeedComponent(SeedComponent toCopy)
        {
            this.Plant = toCopy.Plant;
            this.Level = toCopy.Level;
        }

        public void SetPlant(PlantProperties props)
        {
            /// do all this here or do it wherever this is called from?
            this.Plant = props;
            //this.Parent.Name = $"{props.Name} {this.Parent.Def.Label}";
            //this.Parent.Name = $"{props.Label} {props.SeedsName}";
        }

        public override object Clone()
        {
            return new SeedComponent(this);
        }

        internal override void GetSelectionInfo(IUISelection info, GameObject parent)
        {
            info.AddInfo(new Label() { TextFunc = () => string.Format("Grows into: {0}", this.Plant.Name) });
        }

        internal override void SaveExtra(SaveTag tag)
        {
            this.Plant.Save(tag, "Plant");
        }
        internal override void LoadExtra(SaveTag tag)
        {
            this.Plant = tag.LoadDef<PlantProperties>("Plant");
        }
        public override void Write(BinaryWriter w)
        {
            this.Plant.Write(w);
        }
        public override void Read(IDataReader r)
        {
            this.Plant = Def.GetDef<PlantProperties>(r.ReadString());
        }

        public class Props : ComponentProps
        {
            public override Type CompClass => typeof(SeedComponent);
        }
    }
}

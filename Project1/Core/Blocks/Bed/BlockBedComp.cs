using Microsoft.Xna.Framework;
using Project1.Framework.Entities.Actors;
using System;

namespace Start_a_Town_
{
    internal class BlockBedComp : BlockEntityComp
    {
        internal new class Spec : BlockEntityComp.Spec
        {
            public override Type CompType => typeof(BlockBedComp);

            public override BlockEntityComp CreateComp() => new BlockBedComp();
        }
        public enum Types { Citizen, Visitor };
        public Types Type = Types.Citizen;
        
        public override string Name => "Bed";
        public bool Occupied => this.CurrentOccupant != EntityRefId.Null;
        public EntityRefId CurrentOccupant = EntityRefId.Null;
        public Actor Owner;
        internal override void Initialize()
        {
            this.Parent.Name = "Bed";
        }
        internal Color GetColorFromType()
        {
            return this.Type switch
            {
                Types.Citizen => Color.White,
                Types.Visitor => Color.Cyan,
                _ => throw new Exception(),
            };
        }

    }
}

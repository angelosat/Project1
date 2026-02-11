using System;
using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Blocks.Comps;
using Project1.Core.Entities.Actors;
using Project1.Core.Helpers.Structs;

namespace Project1.Core
{
    internal class BlockBedComp : BlockComp
    {
        internal new class Spec : BlockComp.Spec
        {
            public override Type CompType => typeof(BlockBedComp);

            public override BlockComp CreateComp() => new BlockBedComp();
        }
        public override BlockCompDef CompDef => BlockCompDefOf.Bed;
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

using Project1.Core.Entities.Actors;
using Project1.Core.Helpers;
using Project1.Core.Systems.Materials;
using Project1.Core.World.Frontiers;
using Project1.Framework.Serialization;

namespace Project1.Core.World.Actions
{
    internal sealed class WorldActionData_HarvestResource(FrontierRuntime frontier, Actor actor) : WorldActionData(frontier, actor)
    {
        internal required MaterialRefinementDef Refinement;
        internal required MaterialDef Material;

        public override void Write(IDataWriter w)
        {
            w.Write(this.Refinement);
            w.Write(this.Material);
        }
        public override ISerializable Read(IDataReader r)
        {
            this.Refinement = r.ReadDef<MaterialRefinementDef>();
            this.Material = r.ReadDef<MaterialDef>();
            return this;
        }
    }
}

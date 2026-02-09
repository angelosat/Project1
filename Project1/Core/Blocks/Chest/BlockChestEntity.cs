using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Simulation;
using Project1.Core.Inventory;
using Project1.Core.Entities;
using Project1.Framework.Serialization;
using Project1.Framework;

namespace Project1.Core
{
    partial class BlockChest
    {
        public class BlockChestEntity : BlockEntity
        {
            public Container Container;


            public BlockChestEntity(BlockDef def, IntVec3 originGlobal, int capacity)
                : base(def, originGlobal)
            {
                this.Container = new Container(capacity) { Name = "Container" };
            }

            public override GameObjectSlot GetChild(string containerName, int slotID)
            {
                return this.Container.GetSlot(slotID);
            }

            public override void OnRemoved(MapBase map, IntVec3 global)
            {
                foreach(var slot in this.Container.GetNonEmpty())
                {
                    map.Net.PopLoot(slot.Object, global, Vector3.Zero);
                }
            }
            protected override void AddSaveData(SaveTag tag)
            {
                tag.Add(new SaveTag(SaveTag.Types.Compound, "Container", this.Container.Save()));
            }
            protected override void LoadExtra(SaveTag tag)
            {
                tag.TryGetTag("Container", t => this.Container.Load(t));
            }
            protected override void WriteExtra(IDataWriter io)
            {
                this.Container.Write(io);
            }
            protected override void ReadExtra(IDataReader io)
            {
                this.Container.Read(io);
            }
        }
    }
}
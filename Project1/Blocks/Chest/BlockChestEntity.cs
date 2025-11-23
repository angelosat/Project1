using Microsoft.Xna.Framework;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    partial class BlockChest
    {
        public class BlockChestEntity : BlockEntity
        {
            public Container Container;


            public BlockChestEntity(IntVec3 originGlobal, int capacity)
                : base(originGlobal)
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
            protected override void WriteExtra(System.IO.BinaryWriter io)
            {
                this.Container.Write(io);
            }
            protected override void ReadExtra(System.IO.BinaryReader io)
            {
                this.Container.Read(io);
            }
        }
    }
}

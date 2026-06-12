using Project1.Core.Gear;
using Project1.Core.Systems.Gear;

namespace Project1.Core.Systems.Tools;

public class GearRoleDef(string name, GearSlotDef slot, BoneStructureDef bones) : Def(name)
{
    public readonly GearSlotDef Slot = slot;
    public readonly BoneStructureDef Bones = bones;
}

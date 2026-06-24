using Project1.Core.Entities.Stats;
using Project1.Core.Gear;
using Project1.Core.Systems.Gear;

namespace Project1.Core.Systems.Tools;

public class GearRoleDef(string name, GearSlotDef slot, BoneStructureDef bones, StatDef[] stats) : Def(name)
{
    public readonly GearSlotDef Slot = slot;
    public readonly BoneStructureDef Bones = bones;
    public readonly StatDef[] Stats = stats;
}

using Project1.Core.Animations;

#nullable enable

namespace Project1.Core.Systems.Gear;

public class BoneStructureDef(string name, Bone body, params BoneDef[] bones) : Def(name)
{
    public readonly BoneDef[] Bones = bones;
    public Bone Body = body;
}

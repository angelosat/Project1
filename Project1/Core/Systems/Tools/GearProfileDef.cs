using Project1.Core.Animations;
using Project1.Core.Entities;
using Project1.Core.Entities.Stats;
using Project1.Core.Gear;
using Project1.Core.Graphics;
using Project1.Core.Legacy;
using Project1.Core.Legacy.Storage.New;
using Project1.Core.Skills;
using Project1.Core.Systems.Gear;
using System.Collections.Generic;

namespace Project1.Core.Systems.Tools;

public class GearProfileDef(string name, GearSlotDef slot, BoneStructureDef bones) : Def(name), IItemDefVariator
{
    public GearSlotDef Slot = slot;
    public BoneStructureDef Bones = bones;
    public ToolUseDef ToolUse;
    public DamageDef Damage;
    public Sprite SpriteHandle, SpriteHead;
    public readonly Dictionary<BoneDef, Sprite> BoneSprites;
    public SkillDef Skill;
    public string Description;
   
    public StorageFilterNewNew GetFilter()
    {
        return new(this.LabelReadable, ItemDefOf.Tool, this);
    }
}

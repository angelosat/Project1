using System;

namespace Start_a_Town_
{
    public abstract class ItemCreationArgs;
    internal class ToolSystem : IItemCreationSystem
    {
        Entity Create(ToolProfileDef profile, MaterialDef handleMaterial, MaterialDef headMaterial)
        {
            var item = (Entity)Activator.CreateInstance(ItemDefOf.Tool.ItemClass);
            item.ToolComponent.ToolDef = profile;
            item.Body[BoneDefOf.ToolHandle].Sprite = profile.SpriteHandle;
            item.Body[BoneDefOf.ToolHandle].Material = handleMaterial;

            item.Body[BoneDefOf.ToolHead].Sprite = profile.SpriteHead;
            item.Body[BoneDefOf.ToolHead].Material = headMaterial;

            return item;
        }

        public Entity Create(Def def, ItemCreationArgs args)
        {
            if (def is not ToolProfileDef profile)
                throw new InvalidOperationException($"{nameof(ToolSystem)} received wrong profile");

            if (args is not Args a)
                throw new InvalidOperationException($"{nameof(ToolSystem)} received wrong args");

            var item = ItemDefOf.Tool.Create();

            item.ToolComponent.ToolDef = profile;

            var handle = item.Body.FindBone(BoneDefOf.ToolHandle);
            handle.Sprite = profile.SpriteHandle;
            handle.Material = a.HandleMaterial;

            var head = item.Body.FindBone(BoneDefOf.ToolHandle);
            head.Sprite = profile.SpriteHead;
            head.Material = a.HeadMaterial;

            //item.Body[BoneDefOf.ToolHandle].Sprite = profile.SpriteHandle;
            //item.Body[BoneDefOf.ToolHandle].Material = a.HandleMaterial;

            //item.Body[BoneDefOf.ToolHead].Sprite = profile.SpriteHead;
            //item.Body[BoneDefOf.ToolHead].Material = a.HeadMaterial;
            return item;
        }
        public class Args(MaterialDef handleMaterial, MaterialDef headMaterial) : ItemCreationArgs
        {
            public readonly MaterialDef HandleMaterial = handleMaterial, HeadMaterial = headMaterial;
        }
    }
}

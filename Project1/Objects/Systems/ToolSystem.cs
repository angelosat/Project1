using System;

namespace Start_a_Town_
{
    public abstract class ItemCreationArgs;
    internal class ToolSystem// : IItemCreationSystem
    {
        static public Entity Create(ToolProfileDef def, MaterialDef handleMaterial, MaterialDef headMaterial)
        {
            if (def is not ToolProfileDef profile)
                throw new InvalidOperationException($"{nameof(ToolSystem)} received wrong profile");

            var item = ItemDefOf.Tool.Create();
            item.Initialize();

            item.ToolComponent.ToolDef = profile;

            var handle = item.Body.FindBone(BoneDefOf.ToolHandle);
            handle.Sprite = profile.SpriteHandle;
            handle.Material = headMaterial;

            var head = item.Body.FindBone(BoneDefOf.ToolHead);
            head.Sprite = profile.SpriteHead;
            head.Material = handleMaterial;

            item.Name = profile.Label;


            return item;
        }

        internal static Entity Create(EntityCreationRequest req)
        {
            return Create(req.Template as ToolProfileDef, req.MaterialBindings[BoneDefOf.ToolHandle], req.MaterialBindings[BoneDefOf.ToolHead]);
        }

        public Entity Create(Def def, ItemCreationArgs args)
        {
            if (def is not ToolProfileDef profile)
                throw new InvalidOperationException($"{nameof(ToolSystem)} received wrong profile");

            if (args is not Args a)
                throw new InvalidOperationException($"{nameof(ToolSystem)} received wrong args");

            var item = ItemDefOf.Tool.Create();
            item.Initialize();

            item.ToolComponent.ToolDef = profile;

            var handle = item.Body.FindBone(BoneDefOf.ToolHandle);
            handle.Sprite = profile.SpriteHandle;
            handle.Material = a.HandleMaterial;

            var head = item.Body.FindBone(BoneDefOf.ToolHead);
            head.Sprite = profile.SpriteHead;
            head.Material = a.HeadMaterial;

            item.Name = profile.Label;


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

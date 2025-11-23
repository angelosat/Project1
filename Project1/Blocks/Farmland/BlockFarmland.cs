using Microsoft.Xna.Framework;
using Start_a_Town_.Graphics;

namespace Start_a_Town_
{
    partial class BlockFarmland : Block
    {
        public override bool IsMinable => true;
        readonly AtlasDepthNormals.Node.Token[] Textures;
        public BlockFarmland()
            : base("Farmland")
        {
            this.Textures = new AtlasDepthNormals.Node.Token[2];
            this.Textures[0] = Atlas.Load("blocks/farmland", BlockDepthMap, NormalMap);
            this.Textures[1] = Atlas.Load("blocks/farmlandSowed", BlockDepthMap, NormalMap);
            this.Variations.Add(this.Textures[0]);
        }

        public override AtlasDepthNormals.Node.Token GetToken(int variation, int orientation, int cameraRotation, byte data)
        {
            return this.Textures[data];
        }

        public static bool IsSeeded(byte data)
        {
            return data == 1;
        }
        
        public override void OnDrop(GameObject actor, GameObject dropped, TargetArgs target, int amount = -1)
        {
            if (dropped.HasComponent<SeedComponent>())
                Plant(actor.Map, target.Global, dropped);
            else
                base.OnDrop(actor, dropped, target, amount);
        }
        static public void Plant(MapBase map, Vector3 global, GameObject obj)
        {
            var plantdef = obj.GetComponent<SeedComponent>().Plant;
            var plant = plantdef.CreatePlant();
            plant.SyncInstantiate(map.Net);
            plant.SyncSpawn(map, global.Above());
            //var placer = new BlockSoil.Placer();
            //placer.Place(map, global);
            Block.Place(BlockDefOf.Soil, map, global, map.GetCell(global).Material, 0, 0, 0);
            map.Town.ZoneManager.GetZoneAt(global)?.Invalidate();
            obj.StackSize--;
        }
    }
}

using Microsoft.Xna.Framework;
using Project1.Core.Blocks;
using Project1.Core.Entities;
using Project1.Core.Simulation;
using Project1.Core.Systems.Plants;
using Project1.Framework;
using Project1.Framework.Graphics;

namespace Project1.Core
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
        
        public override bool TryConsume(GameObject actor, GameObject dropped, IntVec3 global, int amount = -1)
        {
            if (dropped.HasComponent<SeedComponent>())
            {
                Plant(actor.Map, global, dropped);
                return true;
            }
            return false;
        }
        static public void Plant(MapBase map, IntVec3 global, GameObject obj)
        {
            var plantdef = obj.Profile as PlantSpeciesDef;
            var plant = plantdef.Create(PlantStageDefOf.Plant);
            map.World.Register(plant);
            map.Spawn(plant, global.Above, Vector3.Zero);
            MapEdit.Paint(MapEditContext.Simulation, map, [global], BlockDefOf.Soil, map.GetCell(global).Material, 0, 0, 0);
            //map.Town.ZoneManager.GetZoneAt(global).MarkDirty();
            obj.Consume(1);
        }
    }
}
